using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PortFreight.Data;
using PortFreight.Data.Entities;
using PortFreight.FileProcess.ASCII;
using PortFreight.FileProcess.Common;
using PortFreight.FileProcess.GESMES;
using PortFreight.Services;
using PortFreight.Services.Areas.Identity.Services;
using PortFreight.Services.Common;
using PortFreight.Services.FileProcess;
using PortFreight.Services.MSD1;
using System;
using System.IO;
using System.Threading.Tasks;
using PortFreight.Services.EmailSender;
using PortFreight.FileProcessor.Common;
using PortFreight.Services.Models;
using PortFreight.FileProcessor.ASCII;
using PortFreight.Services.MSD2;
using PortFreight.FileProcessor.GESMES;

namespace PortFreight.FileProcess
{
    public class FileOptions
    {
        public string AsciiBucket { get; set; }
        public string ArchivedAsciiBucket { get; set; }
        public string GesmesBucket { get; set; }
        public string ArchivedGesmesBucket { get; set; }
        public string PortGesmesBucket { get; set; }
        public string PortAsciiBucket { get; set; }
        public string MaritimeHelpdeskEmailAddress { get; set; }
        public string NoUserEmailAddress { get; set; }
    }

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
            services.AddDbContext<UserDbContext>(options =>
                options.UseMySql(Configuration.GetConnectionString("UsersConnection")));

            services.AddDefaultIdentity<PortFreightUser>(config =>
            {
                config.SignIn.RequireConfirmedEmail = true;
                config.Password.RequireDigit = false;
                config.Password.RequireNonAlphanumeric = false;
                config.Password.RequireUppercase = false;
            }).AddEntityFrameworkStores<UserDbContext>();

            services.AddDbContext<PortFreightContext>(options =>
                options.UseMySql(Configuration.GetConnectionString("DataConnection")));

            services.Configure<FileOptions>(Configuration.GetSection("CloudStorage"));
            services.Configure<AuthMessageSenderOptions>(Configuration.GetSection("AuthMessageSenderOptions"));

            services.AddTransient<IHelperService, HelperService>();
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<IFileProcessService, FileProcessService>();
            services.AddTransient<ICargoPortValidateService, CargoPortValidateService>();
            services.AddTransient<IMsd1DataService, Msd1DataService>();
            services.AddTransient<IMsd2DataService, Msd2DataService>();
            services.AddTransient<IMsd3DataService, Msd3DataService>();
            services.AddTransient<MSD1FileProcess>();
            services.AddTransient<MSD2FileProcess>();
            services.AddTransient<MSD3FileProcess>();
            services.AddTransient<Msd1FileProcess>();
            services.AddTransient<Msd2FileProcess>();
            services.AddTransient<Msd3FileProcess>();
            services.AddTransient<EmailNotification>();
            services.AddTransient<ASCIIFileProcess>();
            services.AddTransient<GESMESFileProcess>();
            services.AddTransient<GesmesHelpers>();
            services.AddTransient<FileProcessing>();
            services.AddTransient<Msd1Data>();
            services.AddTransient<Msd2>();
            services.AddTransient<Msd3>();
            services.AddTransient<Msd3agents>();
            services.AddTransient<Msd1CargoSummary>();
            services.AddTransient<ValidateMsdData>();
            services.AddTransient<FlatFile>();
            services.AddTransient<LogFileData>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, FileProcessing fileProcessing)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseMvc();
        }
    }
}
