using BusinessObjects.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InSyncAPI.JwtServices
{
    public static class AuthenAuthorJwtToken
    {
        private static readonly TimeSpan TokenLife = TimeSpan.FromMinutes(20);
        private static string GenerationToken(User user)
        {
            var builder = new ConfigurationBuilder()
                              .SetBasePath(Directory.GetCurrentDirectory())
                              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            IConfigurationRoot configuration = builder.Build();
            var tokenKey = configuration.GetSection("JwtSetting:Key").Value;
            var issuer = configuration.GetSection("JwtSetting:Issuer").Value;
            var auudience = configuration.GetSection("JwtSetting:Audience").Value;

            var clams = new List<Claim>()
            {
               
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));
            var keyScret = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var Token = new JwtSecurityToken(
                claims: clams,
                expires: DateTime.UtcNow.Add(TokenLife),
                issuer: issuer,
                audience: auudience,
                signingCredentials: keyScret
                );
            var jwt = new JwtSecurityTokenHandler().WriteToken(Token);
            return jwt;
        }




        private static IEnumerable<Claim> GetClaimsFromToken(string token)
        {
            var builder = new ConfigurationBuilder()
                             .SetBasePath(Directory.GetCurrentDirectory())
                             .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            IConfigurationRoot configuration = builder.Build();
            var tokenKey = configuration.GetSection("JwtSetting:Key").Value;
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(tokenKey);

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return principal.Claims;
            }
            catch (Exception)
            {
                return Enumerable.Empty<Claim>();
            }
        }
        //public static Dictionary<string, string> GetPropertyInTokenJwt(string token)
        //{
        //    var handler = new JwtSecurityTokenHandler();


        //    // validate the token and extract the claims
        //    try
        //    {
        //        var claims = GetClaimsFromToken(token);

        //        Dictionary<string, string> claimInToken = new Dictionary<string, string>();

        //        foreach (var claim in claims)
        //        {
        //            claimInToken.Add(claim.Type, claim.Value);
        //        }
        //        return claimInToken;
        //    }
        //    catch (SecurityTokenException ex)
        //    {
        //        Console.WriteLine("Invalid token: " + ex.Message);
        //    }
        //    return null;
        //}
        public static Dictionary<string, string> GetClaimInTokenJwt(string token, TokenValidationParameters validationParameters)
        {
            var handler = new JwtSecurityTokenHandler();

            try
            {
                
                var principal = handler.ValidateToken(token, validationParameters, out var validatedToken);

                
                if (!(validatedToken is JwtSecurityToken jwtToken) ||
                    !jwtToken.Header.Alg.Equals(SecurityAlgorithms.RsaSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenException("Invalid token signature algorithm.");
                }

                // Lấy danh sách claims từ token
                Dictionary<string, string> claimInToken = principal.Claims
                    .ToDictionary(claim => claim.Type, claim => claim.Value);

                return claimInToken;
            }
            catch (SecurityTokenException ex)
            {
                Console.WriteLine($"Invalid token: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }

            return null;
        }




        public static string GenerateTokenJWTEndCode(User user)
        {
            var code = GenerationToken(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            return code;
        }
        public static string GetJWTTokenDeEncode(string jwtTokenEncode)
        {
            return  Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(jwtTokenEncode));
        }
    }
}
