using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using JWT.Builder;
using Microsoft.Extensions.Configuration;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Extensions;
using Nancy.Responses;

namespace www.SupportClasses
{
    public class Authorization : IApplicationStartup
    {
        private readonly IConfiguration _config;
        private readonly string secret;
        public Authorization(IConfiguration configuration, Tokenizer tokenizer)
        {
            secret = configuration.GetValue<string>("KeyString");
        }

        //This method will run when Nancy starts up
        public void Initialize(IPipelines pipelines)
        {
            pipelines.BeforeRequest += BeforeRequest;
            pipelines.AfterRequest += AfterRequest;
        }

        public Response BeforeRequest(NancyContext context)
        {
            string strAuthJWT = null;

            //We look for the JWT in both cookies and the Authorization header

            if (context.Request.Cookies.ContainsKey("Authorization"))
                strAuthJWT = context.Request.Cookies["Authorization"];
            if (!string.IsNullOrEmpty(context.Request.Headers.Authorization))
                strAuthJWT = context.Request.Headers.Authorization;

            if (string.IsNullOrEmpty(strAuthJWT))
                return null;

            var payload = new JwtBuilder()
                .WithSecret(secret)
                .MustVerifySignature()
                .Decode<AuthUser>(strAuthJWT);

            context.CurrentUser = new ClaimsPrincipal(payload);

            return null;
        }

        public void AfterRequest(NancyContext context)
        {
            if (context.Response.StatusCode == HttpStatusCode.Unauthorized)
                context.Response = new RedirectResponse("/auth");
        }
    }
}
