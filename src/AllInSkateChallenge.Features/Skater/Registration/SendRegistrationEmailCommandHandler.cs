﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AllInSkateChallenge.Features.Data.Static;
using AllInSkateChallenge.Features.Framework.Routing;
using AllInSkateChallenge.Features.Services.Email;

using MediatR;

using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;

namespace AllInSkateChallenge.Features.Skater.Registration
{
    public class SendRegistrationEmailCommandHandler : IRequestHandler<SendRegistrationEmailCommand>
    {
        private readonly IViewToStringRenderer viewToStringRenderer;

        private readonly IAbsoluteUrlHelper absoluteUrlHelper;

        private readonly IEmailSender emailSender;

        private readonly ICheckPointRepository checkPointRepository;

        private readonly ILogger<SendRegistrationEmailCommandHandler> logger;

        public SendRegistrationEmailCommandHandler(
            IViewToStringRenderer viewToStringRenderer, 
            IAbsoluteUrlHelper absoluteUrlHelper, 
            IEmailSender emailSender,
            ICheckPointRepository checkPointRepository,
            ILogger<SendRegistrationEmailCommandHandler> logger)
        {
            this.viewToStringRenderer = viewToStringRenderer;
            this.absoluteUrlHelper = absoluteUrlHelper;
            this.emailSender = emailSender;
            this.checkPointRepository = checkPointRepository;
            this.logger = logger;
        }

        public async Task<Unit> Handle(SendRegistrationEmailCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var startPoint = checkPointRepository.Get().OrderBy(x => x.Distance).FirstOrDefault();

                var emailModel = new RegistrationEmailModel
                {
                    LogoUrl = absoluteUrlHelper.Get("/images/AllInSkateChallengeBannerSmall.png"),
                    EmailConfirmationUrl = request.EmailConfirmationUrl,
                    SiteUrl = absoluteUrlHelper.Get("/"),
                    LogMilesUrl = absoluteUrlHelper.Get("/skater/skate-log"),
                    SponsorLogoUrl = absoluteUrlHelper.Get("/images/SkateEverywhereLogo.png"),
                    StartingPostCard = absoluteUrlHelper.Get(startPoint?.Image),
                    FromSkateEverywhere = request.FromSkateEverywhere
                };

                var emailBody = await viewToStringRenderer.RenderPartialToStringAsync("~/Views/Email/RegistrationEmail.cshtml", emailModel);

                await emailSender.SendEmailAsync(request.Email, "ALL IN Skate Challenge Registration", emailBody);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to progress updates when saving mileage entries", request);
            }

            return Unit.Value;
        }
    }
}
