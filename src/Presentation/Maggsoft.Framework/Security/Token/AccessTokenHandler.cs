using Maggsoft.Framework.Mapper;
using Maggsoft.Framework.Security.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Maggsoft.Framework.Security.Token
{
    public class AccessTokenHandler : IAccessTokenHandler
    {
        private readonly ApiTokenOptions _tokenOptions;
        private readonly IHttpContextAccessor _context;

        public AccessTokenHandler(IOptions<ApiTokenOptions> tokenOptions, IHttpContextAccessor context)
        {
            _tokenOptions = tokenOptions.Value;
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public AccessTokenDto CreateAccessToken(object user)
        {
            //Token Süresi
            var accessTokenExpiration = DateTime.UtcNow.AddDays(_tokenOptions.AccessTokenExpiration);
            var securityKey = SingHandler.GetSecurityKey(_tokenOptions.SecurityKey);

            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            var jwtSecurityToken = new JwtSecurityToken(
                expires: accessTokenExpiration,
                notBefore: DateTime.UtcNow,
                claims: GetClaim(user),
                signingCredentials: signingCredentials
            );

            var handler = new JwtSecurityTokenHandler();
            var token = handler.WriteToken(jwtSecurityToken);


            var accessToken = new AccessTokenDto
            {
                Token = token,
                RefreshToken = CreateRefreshToke(),
                Expiration = accessTokenExpiration,
                User = AutoMapperConfiguration.Mapper.Map<object>(user)
            };

            return accessToken;
        }

        public object GetAccessToken()
        {
            string authorizationHeader = _context.HttpContext.Request.Headers["Authorization"];

            if (authorizationHeader != null)
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var token = authorizationHeader.Split(" ")[1];
                var parsedToken = tokenHandler.ReadJwtToken(token);

                var user = parsedToken.Claims.Where(c => c.Type == ClaimTypes.UserData).Select(p => p.Value)
                    .FirstOrDefault();
                if (string.IsNullOrEmpty(user))
                {
                    throw new ArgumentNullException($"user");
                }

                return JsonConvert.DeserializeObject<object>(user);
            }

            throw new ArgumentNullException($"accountnumber");
        }

        #region private

        private string CreateRefreshToke()
        {
            var numberBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(numberBytes);
                return Convert.ToBase64String(numberBytes);
            }
        }

        private IEnumerable<Claim> GetClaim(object userRequest)
        {
            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.UserData, JsonConvert.SerializeObject(userRequest)),
            };

            return claims;
        }

        #endregion
    }
}
