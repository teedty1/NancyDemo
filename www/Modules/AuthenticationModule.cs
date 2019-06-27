using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.V3.Pages.Account.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Nancy;
using Nancy.Cookies;
using Nancy.ModelBinding;
using Nancy.Security;
using Newtonsoft.Json;
using www.SupportClasses;

namespace www
{
    public class AuthenticationModule : Nancy.NancyModule
    {
        public AuthenticationModule(IConfiguration config, Tokenizer tokenizer) : base("/auth")
        {
            Get("/", x => View["_views/Login", new LoginStatus
            {
                Authenticated = false,
                ErrorMessage = "Please authenticate"
            }]);

            Post("/", x =>
            {
                var loginInfo = this.Bind<LoginData>();
                var status = new LoginStatus
                {
                    Authenticated = false,
                    JWT = null,
                    ErrorMessage = "Your username or password did not match our records"
                };
                var statusCode = HttpStatusCode.Unauthorized;

                if (loginInfo.Username == "user" && loginInfo.Password == "password1")
                {
                    var jwt = tokenizer.GenerateToken(new AuthUser(loginInfo.Username, loginInfo.Username, Guid.NewGuid()));
                    status = new LoginStatus
                    {
                        JWT = jwt,
                        Authenticated = true,
                        ErrorMessage = null
                    };
                    statusCode = HttpStatusCode.OK;
                }

                return View["_views/Login", status]
                    .WithStatusCode(statusCode)
                    .WithCookie(new NancyCookie("Authorization", status.JWT, DateTime.UtcNow.AddMinutes(5)));
            });

            Get("/secure", x =>
            {
                this.RequiresAuthentication();
                return View["_views/Secure"];
            });
        }
    }

    public class LoginData
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class LoginStatus
    {
        public bool Authenticated { get; set; }
        public string ErrorMessage { get; set; }
        public string JWT { get; set; }
    }
}