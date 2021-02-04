﻿using System.Collections.Generic;

using AllInSkateChallenge.Features.Data.Static;

using Microsoft.AspNetCore.Mvc.Rendering;

namespace AllInSkateChallenge.Features.LeaderBoard
{
    public class LeaderBoardViewComponentModel
    {
        public bool ShowFilter { get; set; }

        public SkateTarget FilterValue { get; set; }

        public List<SelectListItem> FilterItems { get; set; }

        public int? Limit { get; set; }

        public string LeaderBoardUrl { get; set; }
    }
}
