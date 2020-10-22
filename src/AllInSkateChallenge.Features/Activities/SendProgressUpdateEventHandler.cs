﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AllInSkateChallenge.Features.Data;
using AllInSkateChallenge.Features.Data.Static;
using AllInSkateChallenge.Features.Framework.Routing;
using AllInSkateChallenge.Features.Services.Email;
using AllInSkateChallenge.Features.Skater;

using MediatR.Pipeline;

using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;

namespace AllInSkateChallenge.Features.Activities
{
    public class SendProgressUpdateEventHandler : IRequestPostProcessor<SaveActivityCommand, SaveActivityCommandResult>
    {
        private readonly ApplicationDbContext context;

        private readonly ICheckPointRepository checkPointRepository;

        private readonly IViewToStringRenderer viewToStringRenderer;

        private readonly IAbsoluteUrlHelper absoluteUrlHelper;

        private readonly IEmailSender emailSender;

        private readonly ILogger<SaveActivityCommandHandler> logger;

        public SendProgressUpdateEventHandler(
            ApplicationDbContext context,
            ICheckPointRepository checkPointRepository,
            IViewToStringRenderer viewToStringRenderer,
            IAbsoluteUrlHelper absoluteUrlHelper,
            IEmailSender emailSender, 
            ILogger<SaveActivityCommandHandler> logger)
        {
            this.context = context;
            this.checkPointRepository = checkPointRepository;
            this.viewToStringRenderer = viewToStringRenderer;
            this.absoluteUrlHelper = absoluteUrlHelper;
            this.emailSender = emailSender;
            this.logger = logger;
        }

        public async Task Process(SaveActivityCommand request, SaveActivityCommandResult response, CancellationToken cancellationToken)
        {
            if (!response.WasSuccessful || !request.Skater.AcceptProgressNotifications) return;

            try
            {
                var milesThisSkate = request.Distance;
                switch (request.DistanceUnit)
                {
                    case DistanceUnit.Kilometres:
                        milesThisSkate = milesThisSkate * 0.621371M;
                        break;
                    case DistanceUnit.Metres:
                        milesThisSkate = milesThisSkate * 0.000621371M;
                        break;
                }

                var totalMiles = context.SkateLogEntries.Where(x => x.ApplicationUserId.Equals(request.Skater.Id)).Sum(x => x.DistanceInMiles);
                var previousMiles = totalMiles - milesThisSkate;
                var checkPoints = checkPointRepository.Get();
                var checkPointReached = checkPoints.Where(x => x.Distance >= previousMiles && x.Distance <= totalMiles).OrderByDescending(x => x.Distance).FirstOrDefault();

                if (checkPointReached != null && request.Skater.EmailConfirmed)
                {
                    var nextCheckPoint = checkPoints.Where(x => x.Distance > checkPointReached.Distance).OrderBy(x => x.Distance).FirstOrDefault();

                    var emailModel = new SkaterProgressEmailViewModel
                    {
                        LogoUrl = absoluteUrlHelper.Get("/images/AllInSkateChallengeBanner2.png"),
                        Skater = request.Skater,
                        CheckPoint = checkPointReached,
                        TotalMiles = totalMiles,
                        NextCheckPoint = nextCheckPoint
                    };
                    var emailBody = await viewToStringRenderer.RenderPartialToStringAsync("~/Views/Email/SkaterProgressEmail.cshtml", emailModel);

                    await emailSender.SendEmailAsync(request.Skater.Email, "ALL IN Skate Challenge Progress", emailBody);
                }
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to progress updates when saving mileage entries", request);
            }
        }
    }
}