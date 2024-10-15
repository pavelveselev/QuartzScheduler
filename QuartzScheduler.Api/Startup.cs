using System;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using QuartzScheduler.Api.Converters;
using QuartzScheduler.Api.Filters;
using QuartzScheduler.Api.Mapper;
using QuartzScheduler.Api.Quartz;
using QuartzScheduler.Api.Workers;
using QuartzScheduler.Common.Services;
using QuartzScheduler.Common.Validators;
using QuartzScheduler.Core.Mapper;
using QuartzScheduler.Core.Quartz.Jobs;
using QuartzScheduler.Core.Services;
using QuartzScheduler.Core.Validators;
using QuartzScheduler.Dal;

namespace QuartzScheduler.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .AddJsonOptions(options =>
                    options.JsonSerializerOptions.Converters.Add(new TimeSpanToStringConverter()));

            services.AddMvc(options => options.Filters.Add(typeof(ApiExceptionFilter)));

            services.AddApiVersioning(conf =>
            {
                conf.ApiVersionSelector = new CurrentImplementationApiVersionSelector(conf);
                conf.AssumeDefaultVersionWhenUnspecified = true;
                conf.ReportApiVersions = true;
                conf.ApiVersionReader = new UrlSegmentApiVersionReader();
            });

            services.AddVersionedApiExplorer(opt =>
            {
                opt.GroupNameFormat = "'v'VVV";
                opt.SubstituteApiVersionInUrl = true;
            });

            // Worker service
            services.AddHostedService<QuartzSchedulerHostedService>();

            // Mapper
            services.AddAutoMapper(
                typeof(ApiMapperProfile),
                typeof(CoreMappingProfile));

            // Validators
            services.AddTransient<ICronModelValidator, CronModelValidator>();

            var connectionString = Configuration.GetConnectionString(SchedulerDbContext.SchedulerDbConnString);
            var quartzSettings = new NameValueCollection
            {
                { "quartz.scheduler.instanceName", "ScreenshotsScheduler" },
                { "quartz.scheduler.instanceId", "AUTO" },
                { "quartz.serializer.type", "Json" }
            };

            if (!string.IsNullOrEmpty(connectionString))
            {
                Console.WriteLine($"Database: {GetDbName(connectionString)}");

                quartzSettings.Add("quartz.jobStore.type", "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz");
                quartzSettings.Add("quartz.jobStore.driverDelegateType", "Quartz.Impl.AdoJobStore.SqlServerDelegate, Quartz");
                quartzSettings.Add("quartz.jobStore.useProperties", "true");
                quartzSettings.Add("quartz.jobStore.dataSource", "default");
                quartzSettings.Add("quartz.dataSource.default.connectionString", connectionString);
                quartzSettings.Add("quartz.dataSource.default.provider", "SqlServer");
                quartzSettings.Add("quartz.jobStore.clustered", "false");
            }

            // Add Quartz services
            services.AddSingleton<IJobFactory, JobFactory>();
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>(x => new StdSchedulerFactory(quartzSettings));

            services.AddTransient<SendNotificationJob>();

            services.AddTransient<IScheduleJobService, ScheduleJobService>();
            services.AddTransient<IScheduleObjectService, ScheduleObjectService>();
            services.AddTransient<INotificationClient, HttpNotificationClient>();

            services.AddQuartz(q =>
            {
                services.AddQuartzHostedService(options =>
                {
                    // when shutting down we want jobs to complete gracefully
                    options.WaitForJobsToComplete = true;
                });
            });

            if (string.IsNullOrEmpty(connectionString))
            {
                services.AddSingleton<IExternalIdRepository, ExternalIdMemoryStore>();
            }
            else
            {
                services.AddScoped<IExternalIdRepository, ExternalIdDbStore>();
                services.AddScoped<SchedulerDbContext>(_ => new SchedulerDbContext(connectionString));
                services.AddHostedService<JobsToObjectsCleanerWorker>();
            }

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "QuartzSchedule API V1", Version = "v1" });
                c.SwaggerDoc("v2", new OpenApiInfo { Title = "QuartzSchedule API V2", Version = "v2" });

                c.MapType<TimeSpan>(() => new OpenApiSchema
                {
                    Type = "string",
                    Example = new OpenApiString(TimeSpanToStringConverter.Format)
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "QuartzSchedule API V1");
                c.SwaggerEndpoint("/swagger/v2/swagger.json", "QuartzSchedule API V2");
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            var connectionString = Configuration.GetConnectionString(SchedulerDbContext.SchedulerDbConnString);
            if (!string.IsNullOrEmpty(connectionString))
            {
                MigrateDb();
            }
        }

        private void MigrateDb()
        {
            try
            {
                string connectionString = Configuration.GetConnectionString(SchedulerDbContext.SchedulerDbConnString);

                var model = new SchedulerDbContext(connectionString);

                model.Database.Migrate();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static string GetDbName(string connectionString) =>
            string.Join(';', connectionString.Split(';').Where(x => x.Contains("Server") || x.Contains("Database")));
    }
}
