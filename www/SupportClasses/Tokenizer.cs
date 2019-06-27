using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Microsoft.Extensions.Configuration;

namespace www.SupportClasses
{
    public class Tokenizer
    {
        private readonly string secret;
        private readonly IJwtEncoder encoder;

        public Tokenizer(IConfiguration configuration)
        {
            secret = configuration.GetValue<string>("KeyString");
            encoder = new JwtEncoder(new HMACSHA256Algorithm(), new JsonNetSerializer(), new JwtBase64UrlEncoder());
        }

        public string GenerateToken(AuthUser user)
        {
            var token = encoder.Encode(user, secret);
            return token;
        }
    }

    public class AuthUser : IIdentity
    {
        public string AuthenticationType => "Nancy";
        public bool IsAuthenticated { get; }
        public string Name { get; }
        public string Login { get; }
        public Guid Id { get; }
        public double exp { get; }

        public AuthUser(string name, string login, Guid id)
        {
            Name = name;
            Login = login;
            Id = id;
            IsAuthenticated = true;
            exp = UnixEpoch.GetSecondsSince(DateTimeOffset.Now.AddMinutes(5));
        }
    }
}
