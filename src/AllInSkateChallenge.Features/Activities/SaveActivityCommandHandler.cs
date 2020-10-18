﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AllInSkateChallenge.Features.Data;
using AllInSkateChallenge.Features.Data.Entities;
using AllInSkateChallenge.Features.Skater;

using MediatR;

using Microsoft.Extensions.Logging;

namespace AllInSkateChallenge.Features.Activities
{
    public class SaveActivityCommandHandler : IRequestHandler<SaveActivityCommand, SaveActivityCommandResult>
    {
        private readonly ApplicationDbContext context;

        private readonly ILogger<SaveActivityCommandHandler> logger;

        public SaveActivityCommandHandler(ApplicationDbContext context, ILogger<SaveActivityCommandHandler> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public async Task<SaveActivityCommandResult> Handle(SaveActivityCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var distance = request.Distance;
                switch (request.DistanceUnit)
                {
                    case DistanceUnit.Kilometres:
                        distance = distance * 0.621371M;
                        break;
                    case DistanceUnit.Metres:
                        distance = distance * 0.000621371M;
                        break;
                }

                var recordExists = !string.IsNullOrWhiteSpace(request.StavaActivityId) && context.SkateLogEntries.Any(x => x.StravaId.Equals(request.StavaActivityId));
                if (!recordExists)
                {
                    var entry = new SkateLogEntry { ApplicationUserId = request.Skater.Id, StravaId = request.StavaActivityId, DistanceInMiles = distance, Logged = request.StartDate ?? DateTime.Now };
                    context.SkateLogEntries.Add(entry);
                    await context.SaveChangesAsync();

                    return new SaveActivityCommandResult { WasSuccessful = true };
                }
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to save mileage entry", request);
            }

            return new SaveActivityCommandResult() { WasSuccessful = false };
        }
    }
}
