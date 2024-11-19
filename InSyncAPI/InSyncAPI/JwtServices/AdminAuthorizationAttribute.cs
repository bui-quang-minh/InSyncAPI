using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using Serilog.Events;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace InSyncAPI.JwtServices
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class AdminAuthorizationAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _requiredRole = "admin";

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Lấy token từ Header Authorization
            var authorizationHeader = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            {
                context.Result = new Microsoft.AspNetCore.Mvc.ForbidResult();
                return;
            }

            var token = authorizationHeader.Substring("Bearer ".Length);

            try
            {
                // Tạo TokenValidationParameters để xác thực
                var validationParameters = await GetValidationParametersFromJWKS();

                var handler = new JwtSecurityTokenHandler();
                var principal = handler.ValidateToken(token, validationParameters, out var validatedToken);

                // Kiểm tra vai trò (role)
                var roleClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);

                if (roleClaim == null || roleClaim.Value != _requiredRole)
                {
                    context.Result = new Microsoft.AspNetCore.Mvc.ForbidResult();
                    return;
                }
            }
            catch (SecurityTokenException)
            {
                context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedResult();
                return;
            }
            catch (Exception)
            {
                context.Result = new Microsoft.AspNetCore.Mvc.ForbidResult();
                return;
            }

            // Nếu kiểm tra qua, cho phép tiếp tục
            await Task.CompletedTask;
        }

        private static async Task<TokenValidationParameters> GetValidationParametersFromJWKS()
        {
            var builder = new ConfigurationBuilder()
                                              .SetBasePath(Directory.GetCurrentDirectory())
                                              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            IConfigurationRoot configuration = builder.Build();
            string issuer = configuration.GetSection("JwtSetting:Issuer").Value;
            string audience = configuration.GetSection("JwtSetting:Audience").Value;
            string jwksUrl = configuration.GetSection("JwtSetting:JWKS").Value;

            using var client = new HttpClient();
            var jwksJson = await client.GetStringAsync(jwksUrl);
            var jwks = new JsonWebKeySet(jwksJson);
            var signingKeys = jwks.GetSigningKeys();

            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKeys = signingKeys,
                ClockSkew = TimeSpan.Zero
            };
        }
    }
}
