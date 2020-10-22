﻿using System.Threading.Tasks;

using AllInSkateChallenge.Features.Data;
using AllInSkateChallenge.Features.Data.Entities;
using AllInSkateChallenge.Features.Framework.Models;

using Microsoft.AspNetCore.Identity;

namespace AllInSkateChallenge.Features.Privacy
{
    public class PrivacyViewModelBuilder : PageViewModelBuilder<PrivacyViewModel>, IPrivacyViewModelBuilder
    {
        public PrivacyViewModelBuilder(ApplicationDbContext context, UserManager<ApplicationUser> userManager) : base(context, userManager)
        {
        }

        public override async Task<PageViewModel<PrivacyViewModel>> Build()
        {
            var model = await base.Build();
            model.PageTitle = "Privacy Policy";
            model.DisplayPageTitle = "Privacy Policy";

            return model;
        }
    }
}