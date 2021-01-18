﻿namespace AllInSkateChallenge.Features.Statistics
{
    using System.Collections.Generic;

    public class EventStatisticsViewModel
    {
        public SkaterStatisticsModel LongestTotalDistance { get; set; }

        public SkaterStatisticsModel LongestSingleDistance { get; set; }

        public SkaterStatisticsModel ShortestSingleDistance { get; set; }

        public SkaterStatisticsModel MostJourneys { get; set; }

        public List<StatisticsItemModel> SkateSessions { get; set; }

        public List<StatisticsItemModel> SkateDistances { get; set; }

        public decimal TotalMiles { get; set; }

        public int TotalSkateSessions { get; set; }

        public decimal MilesByStrava { get; set; }

        public decimal MilesByManual { get; set; }

        public int JourneysByStrava { get; set; }

        public int JourneysByManual { get; set; }

        public List<CheckPointStatisticsModel> CheckPoints { get; set; }
    }
}