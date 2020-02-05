using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PortFreight.Data;
using PortFreight.Services;
using PortFreight.Services.Areas.Identity.Services;
using PortFreight.Services.Common;
using PortFreight.Services.EmailSender;
using PortFreight.Services.Models;
using PortFreight.Services.Validation;
using PortFreight.Services.User;
using StackExchange.Profiling.Storage;
using System;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.AspNetCore.CookiePolicy;
using PortFreight.Services.Export;
using PortFreight.Services.Interface;
using PortFreight.Services.MSD2;
using PortFreight.Services.SubmissionSearch;
using PortFreight.Services.Import;
using PortFreight.Services.ShipListBulkUpload;
using PortFreight.Services.OrganisationList;
using PortFreight.Services.Mapper;
using PortFreight.Services.Organisation;
using PortFreight.Services.MSD3;

namespace PortFreight.Web
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMiniProfiler(options =>
            {
                options.RouteBasePath = "/profiler";
                (options.Storage as MemoryCacheStorage).CacheDuration = TimeSpan.FromMinutes(60);

                options.SqlFormatter = new StackExchange.Profiling.SqlFormatters.InlineFormatter();
            }).AddEntityFramework();


            services.Configure<CloudStorageInit>(
                Configuration.GetSection("CloudStorage"));

            services.AddDbContext<UserDbContext>(options =>
                options.UseMySql(
                    Configuration.GetConnectionString("UsersConnection")));

            services.AddDbContext<PortFreightContext>(options =>
                options.UseMySql(
                    Configuration.GetConnectionString("DataConnection"),
                    mySqlOptions => mySqlOptions.EnableRetryOnFailure()));

            services.AddIdentity<PortFreightUser, IdentityRole>(config =>
                {
                    config.SignIn.RequireConfirmedEmail = true;
                    config.Password.RequireDigit = false;
                    config.Password.RequireNonAlphanumeric = false;
                    config.Password.RequireUppercase = false;
                    config.Password.RequireLowercase = false;
                    config.Password.RequiredLength = 12;
                })
                .AddEntityFrameworkStores<UserDbContext>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
                options.LoginPath = "/Identity/Account/Login";
                options.SlidingExpiration = true;
                options.Cookie.SameSite = SameSiteMode.Strict;
            });
            services.Configure<SecurityStampValidatorOptions>(options =>
            {
                options.ValidationInterval = TimeSpan.Zero;
            });

            services.AddAuthorization(ops =>
            {
                ops.AddPolicy("Admin", policy =>
                {
                    policy.RequireRole("Admin");
                });
            });

            services.AddMvc()

            .AddRazorPagesOptions(options =>
            {
                options.Conventions.AuthorizeFolder("/");
                options.Conventions.AllowAnonymousToPage("/Index");
                options.Conventions.AllowAnonymousToFolder("/Errors");
                options.Conventions.AddAreaPageRoute("Identity", "/Account/Login", "/Account/Login");
                options.Conventions.AuthorizeAreaFolder("Identity", "/Manage");
                options.Conventions.AuthorizeAreaFolder("Admin", "/", "Admin");
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
            .AddSessionStateTempDataProvider();

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.Strict;
            });
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(10);
                options.Cookie.SameSite = SameSiteMode.Strict;
            });

            services.AddHsts(options =>
            {
                var currentDate = DateTime.Now;
                options.Preload = true;
                options.MaxAge = currentDate.AddYears(1) - currentDate;
                options.IncludeSubDomains = true;
            });
            
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            services.Configure<AuthMessageSenderOptions>(Configuration.GetSection("AuthMessageSenderOptions"));

            services.AddSingleton(HtmlEncoder.Create(allowedRanges: new[] { UnicodeRanges.BasicLatin,
                                               UnicodeRanges.CjkUnifiedIdeographs }));

            services.AddSingleton<IApiKeyDataStore, ApiKeyDataStore>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<IApiKeyService, ApiKeyService>();
            services.AddTransient<IValidationDictionary, ModelStateAdapter>();
            services.AddTransient<ICargoPortValidateService, CargoPortValidateService>();

            services.AddScoped<IHelperService, HelperService>();
            services.AddScoped<IUserService, UserService>();
            services.AddTransient<IMsd2DataService, Msd2DataService>();
            services.AddTransient<ICsvExtract, CsvExtract>();
            services.AddTransient<ISubmissionSearch, SubmissionSearch>();
            services.AddTransient<IShipListBulkUpload, ShipListBulkUpload>();
            services.AddTransient<IFileUploadValidator, FileUploadValidator>();
            services.AddTransient<IFileUploadValidator, FileUploadValidator>();
            services.AddTransient<IOrganisationSearch, OrganisationSearch>();
            services.AddTransient<IOrganisation, Organisation>();
            services.AddTransient<IEntityToViewModelMapper, EntityToViewModelMapper>();
            services.AddTransient<IMsd3AgentDataService, Msd3AgentDataService>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseForwardedHeaders();
            app.UseXContentTypeOptions();
            app.UseReferrerPolicy(opts => opts.NoReferrer());
            app.UseXXssProtection(options => options.EnabledWithBlockMode());
            app.UseXfo(options => options.SameOrigin());

            app.UseCsp(opts => opts
            .BlockAllMixedContent()
            .StyleSources(s => s.Self())
            .StyleSources(s => s.UnsafeInline()
            .CustomSources("ajax.googleapis.com"))
            .FontSources(s => s.Self())
            .FormActions(s => s.Self())
            .FrameAncestors(s => s.Self())
            .ImageSources(s => s.Self())
            .ScriptSources(s => s.Self())
            .ScriptSources(s => s.UnsafeInline()
            .CustomSources("ajax.googleapis.com", "ajax.aspnetcdn.com"))
            );

            app.Use(async (context, next) =>
                {
                    if (context.Request.IsHttps || context.Request.Headers["X-Forwarded-Proto"] == Uri.UriSchemeHttps)
                    {
                        context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
                        await next();
                    }
                    else
                    {
                        string queryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : string.Empty;
                        var https = "https://" + context.Request.Host + context.Request.Path + queryString;
                        context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
                        context.Response.Redirect(https);
                    }

                });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseMiniProfiler();
            }
            else
            {
                app.UseStatusCodePagesWithReExecute("/Errors/{0}");
            }

			app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy(new CookiePolicyOptions
            {
                HttpOnly = HttpOnlyPolicy.Always,
                Secure = CookieSecurePolicy.Always,
                MinimumSameSitePolicy = SameSiteMode.Strict
            });
            app.UseAuthentication();
            app.UseSession();
            app.UseMvc();
        }
    }
}
