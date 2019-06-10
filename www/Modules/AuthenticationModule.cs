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
using Nancy.ModelBinding;
using Nancy.Security;
using Newtonsoft.Json;

namespace www
{
    public class AuthenticationModule : Nancy.NancyModule
    {
        public AuthenticationModule(IConfiguration config) : base("/auth")
        {
            Get("/", x => View["_views/Login"]);
            Post("/", x =>
            {
                var loginInfo = this.Bind<LoginData>();

                if (loginInfo.Username == "user1" && loginInfo.Password == "passWord1")
                {
                    var jwt = generateJWT(config, loginInfo.Username);
                    return jwt;
                }

                return View["_views/Login"].WithStatusCode(HttpStatusCode.Unauthorized);
            });

            Get("/verify", x =>
            {
                this.RequiresAuthentication();
                return new
                {
                    //username = Context.CurrentUser.Identity.Name,
                    isAdmin = Context.CurrentUser.Claims.Any(c => c.Type == "Access" && c.Value == "Admin")
                };
            });
        }

        private string generateJWT(IConfiguration config, string username)
        {
            var now = DateTime.UtcNow;

            var claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, now.ToUniversalTime().ToString(), ClaimValueTypes.Integer64),
                new Claim("Access", "Admin")
            };

            var symmetricKeyAsBase64 = config.GetValue<string>("KeyString");
            var keyByteArray = Encoding.ASCII.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyByteArray);

            var jwt = new JwtSecurityToken(
                issuer: "NancyDemo",
                audience: "Websites",
                claims: claims,
                notBefore: now,
                expires: now.Add(TimeSpan.FromMinutes(10)),
                signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new
            {
                access_token = encodedJwt,
                expires_in = (int)TimeSpan.FromMinutes(10).TotalSeconds
            };

            return JsonConvert.SerializeObject(response, new JsonSerializerSettings { Formatting = Formatting.Indented });
        }
    }

    public class LoginData
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}