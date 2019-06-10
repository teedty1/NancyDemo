using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nancy.Owin;
using www.SupportClasses;

namespace www
{
    public class Startup
    {
        private readonly IConfiguration config;
        public Startup(IConfiguration configuration)
        {
            config = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseOwin(owin => owin.UseNancy(nancy => nancy.Bootstrapper = new Bootstrapper(config)));
        }
    }
}
