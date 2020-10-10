using System;

using AllInSkateChallenge.Features.Data;
using AllInSkateChallenge.Features.Data.Entities;
using AllInSkateChallenge.Features.Data.Static;
using AllInSkateChallenge.Features.Gravatar;
using AllInSkateChallenge.Features.Home;
using AllInSkateChallenge.Features.LeaderBoard;
using AllInSkateChallenge.Features.Services.Email;
using AllInSkateChallenge.Features.Skater;
using AllInSkateChallenge.Features.Skater.Progress;
using AllInSkateChallenge.Features.Skater.SkateLog;
using AllInSkateChallenge.Features.Skater.StravaImport;
using AllInSkateChallenge.Features.Strava;
using AllInSkateChallenge.Features.Strava.Webhook;
using AllInSkateChallenge.Features.Updates;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AllInSkateChallenge
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer( Configuration.GetConnectionString("DefaultConnection")));
            services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false).AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddAuthentication()
                    .AddStrava(options => 
                    { 
                        options.ClientId = Configuration["Strava:ClientId"]; 
                        options.ClientSecret = Configuration["Strava:ClientSecret"]; 
                        options.SaveTokens = true;
                        options.Scope.Add("read");
                        options.Scope.Add("activity:read");
                        options.Scope.Add("activity:read_all");
                    });

            services.AddControllersWithViews().AddNewtonsoftJson();
            services.AddRazorPages();

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;

                options.SignIn.RequireConfirmedAccount = false;
            });

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(15);

                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.SlidingExpiration = true;
            });

            // External Services
            services.Configure<EmailSettings>(options => Configuration.GetSection("EmailSettings").Bind(options));
            services.AddTransient<IEmailSender, EmailSenderService>();
            services.Configure<StravaSettings>(options => Configuration.GetSection("Strava").Bind(options));
            services.AddTransient<IStravaService, StravaService>();

            services.AddTransient<IGravatarResolver, GravatarResolver>();
            services.AddTransient<ILeaderBoardQuery, LeaderBoardQuery>();
            services.AddTransient<ILatestUpdatesQuery, LatestUpdatesQuery>();
            services.AddTransient<IHomePageViewModelBuilder, HomePageViewModelBuilder>();
            services.AddTransient<ISkaterProgressViewModelBuilder, SkaterProgressViewModelBuilder>();
            services.AddTransient<ISkaterLogViewModelBuilder, SkaterLogViewModelBuilder>();
            services.AddTransient<IStravaImportViewModelBuilder, StravaImportViewModelBuilder>();

            // Data
            services.AddTransient<ICheckPointRepository, CheckPointRepository>();
            services.AddTransient<ISkaterMileageEntriesRepository, SkaterMileageEntriesRepository>();
            services.AddTransient<ISummaryStatisticsRepository, SummaryStatisticsRepository>();
            services.AddTransient<IStravaIntegrationLogRepository, StravaIntegrationLogRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
