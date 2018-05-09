

namespace SamhashoService
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;
    using System.Web.Configuration;

    using Microsoft.IdentityModel.Tokens;

    public class Security
    {
        static readonly string key = WebConfigurationManager.AppSettings["securityKey"];
        public static string GenerateSecurityToken(string userId, string username, string email)
        {
            try
            {
                byte[] symmetricKey = Convert.FromBase64String(key);
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
                                          {
                                              Subject = new ClaimsIdentity(new[]
                                                                               {
                                                                                   new Claim(ClaimTypes.NameIdentifier, userId),
                                                                                   new Claim(ClaimTypes.Name, username), 
                                                                                   new Claim(ClaimTypes.Email, email) 
                                                                               }),
                                              SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(symmetricKey), SecurityAlgorithms.HmacSha256Signature)
                                          };

                SecurityToken stoken = tokenHandler.CreateToken(tokenDescriptor);
                string token = tokenHandler.WriteToken(stoken);

                return token;
            }
            catch (Exception exception)
            {
                ServiceHelper.LogException(exception, new Dictionary<string, string>(), ErrorSource.Security );
                return string.Empty;
            }
        }

        public static bool ValidateToken(string token, out List<Claim> claims)
        {
            try
            {
                claims = new List<Claim>();
                ClaimsPrincipal simplePrinciple = GetPrincipal(token);
                ClaimsIdentity identity = simplePrinciple.Identity as ClaimsIdentity;

                if (identity == null)
                    return false;

                if (!identity.IsAuthenticated)
                    return false;

                Claim userIdClaim = identity.FindFirst(ClaimTypes.NameIdentifier);
                Claim nameClaim = identity.FindFirst(ClaimTypes.Name);
                Claim emailClaim = identity.FindFirst(ClaimTypes.Email);
                claims = new List<Claim>
                                         {
                                             new Claim(ClaimTypes.NameIdentifier, userIdClaim.Value),
                                             new Claim(ClaimTypes.Email, emailClaim.Value),
                                             new Claim(ClaimTypes.Name, nameClaim.Value)
                                         };
                return !string.IsNullOrEmpty(userIdClaim.Value);
            }
            catch (Exception exception)
            {
                ServiceHelper.LogException(exception, new Dictionary<string, string>(), ErrorSource.Authentication);
                claims = new List<Claim>();
                return false;
            }
        }

        public static ClaimsPrincipal GetPrincipal(string token)
        {
            try
            {
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                JwtSecurityToken jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

                if (jwtToken == null)
                    return null;

                byte[] symmetricKey = Convert.FromBase64String(key);

                TokenValidationParameters validationParameters = new TokenValidationParameters()
                                               {
                                                   RequireExpirationTime = true,
                                                   ValidateIssuer = false,
                                                   ValidateAudience = false,
                                                   IssuerSigningKey = new SymmetricSecurityKey(symmetricKey)
                                               };

                SecurityToken securityToken;
                ClaimsPrincipal principal = tokenHandler.ValidateToken(token, validationParameters, out securityToken);

                return principal;
            }
            catch (Exception exception)
            {
                ServiceHelper.LogException(exception, new Dictionary<string, string>(), ErrorSource.Security );
                return null;
            }
        }
    }
}