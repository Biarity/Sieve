using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sieve.Models;
using Sieve.Sample.Entities;
using Sieve.Sample.Services;
using Sieve.Services;

namespace Sieve.Sample
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
            services.AddMvc(opts =>
            {
                opts.EnableEndpointRouting = false;
            });

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite("Data Source=.\\sieve.db"));

            services.Configure<SieveOptions>(Configuration.GetSection("Sieve"));

            services.AddScoped<ISieveCustomSortMethods, SieveCustomSortMethods>();
            services.AddScoped<ISieveCustomFilterMethods, SieveCustomFilterMethods>();
            services.AddScoped<ISieveProcessor, ApplicationSieveProcessor>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            PrepareDatabase(app);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }

        private static void PrepareDatabase(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.EnsureDeleted();
            dbContext.Database.Migrate();
        }
    }
}
