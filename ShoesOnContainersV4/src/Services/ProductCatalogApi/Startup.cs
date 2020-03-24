using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShoesOnContainers.Services.ProductCatalogApi.Data;
using Microsoft.EntityFrameworkCore;
using ShoesOnContainers.Services.ProductCatalogApi;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Swashbuckle.AspNetCore.Swagger;

namespace ProductCatalogApi
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

            services.Configure<CatalogSettings>(Configuration);
            var hostname = Environment.GetEnvironmentVariable("SQLSERVER_HOST") ?? "mssqlserver";
            var password = Environment.GetEnvironmentVariable("SA_PASSWORD") ?? "MyProduct!123";
           
            var connectionString = $"Server={hostname};Database=CatalogDb;User ID=sa;Password={password};";

           
            services.AddDbContext<CatalogDbContext>(options =>
            {
                options.UseSqlServer(connectionString,
                                     sqlServerOptionsAction: sqlOptions =>
                                     {
                                         sqlOptions.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
                                         //Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
                                         sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                                     });

                // Changing default behavior when client evaluation occurs to throw. 
                // Default in EF Core would be to log a warning when client evaluation is performed.
                options.ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning));
                //Check Client vs. Server evaluation: https://docs.microsoft.com/en-us/ef/core/querying/client-eval
            });

            services.AddSwaggerGen(options =>
            {
                options.DescribeAllEnumsAsStrings();
                options.SwaggerDoc("v1",new Swashbuckle.AspNetCore.Swagger.Info()
                {
                    Title = "ShoesOnContainers - ProductCatalog HTTP API",
                    Version = "v1",
                    Description = "The Catalog Microservice HTTP API. This is a Data-Driven/CRUD microservice sample",
                    TermsOfService = "Terms Of Service"
                });
            });
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });


            //services.AddDbContext<CatalogDbContext>(options =>
            //{
            //    options.UseSqlServer(connectionString);

            //});

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger().UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            } );
           
            app.UseCors("CorsPolicy");

            app.UseMvc();
        }
    }
}
