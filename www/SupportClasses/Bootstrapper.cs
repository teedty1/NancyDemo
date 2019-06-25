using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Nancy;
using Nancy.Authentication.JwtBearer;
using Nancy.Bootstrapper;
using Nancy.Configuration;
using Nancy.Json;
using Nancy.TinyIoc;

namespace www.SupportClasses
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        private readonly IConfiguration _config;
        private readonly IServiceProvider _services;

        public Bootstrapper(IConfiguration configuration, IServiceProvider dotNetServices)
        {
            _config = configuration;
            _services = dotNetServices;
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);
            container.Register(_config);
            container.Register<IServiceProvider>(_services);
        }

        public override void Configure(INancyEnvironment environment)
        {
            base.Configure(environment);

            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

            environment.Json(retainCasing: true);
            environment.Views(
                runtimeViewUpdates: true,
                runtimeViewDiscovery: true
            );
            environment.Tracing(
                enabled: true,
                displayErrorTraces: true
            );

        }

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            var keyString = _config.GetValue<string>("KeyString");
            var keyByteArray = Encoding.ASCII.GetBytes(keyString);
            var signingKey = new SymmetricSecurityKey(keyByteArray);

            var tokenValidationParameters = new TokenValidationParameters
            {
                // The signing key must match!
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,

                // Validate the JWT Issuer (iss) claim
                ValidateIssuer = true,
                ValidIssuer = "NancyDemo",

                // Validate the JWT Audience (aud) claim
                ValidateAudience = true,
                ValidAudience = "Websites",

                // Validate the token expiry
                ValidateLifetime = true,

                ClockSkew = TimeSpan.Zero
            };

            var configuration = new JwtBearerAuthenticationConfiguration
            {
                TokenValidationParameters = tokenValidationParameters,
                Challenge = "Guest"//if not use this,default to Bearer
            };

            pipelines.EnableJwtBearerAuthentication(configuration);
        }
    }
}
