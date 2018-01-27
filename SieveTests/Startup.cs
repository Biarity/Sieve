using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sieve.Models;
using Sieve.Services;
using SieveTests.Entities;
using SieveTests.Services;

namespace SieveTests
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
            services.AddMvc();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("TestSqlServer")));

            services.Configure<SieveOptions>(Configuration.GetSection("Sieve"));


            //services.AddScoped<ISieveProcessor, SieveProcessor>();
            services.AddScoped<ISieveCustomSortMethods<Post>, SieveCustomSortMethodsOfPosts>();
            services.AddScoped<ISieveCustomFilterMethods<Post>, SieveCustomFilterMethodsOfPosts>();
            services.AddScoped<ISieveProcessor<Post>, SieveProcessor<Post>>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
