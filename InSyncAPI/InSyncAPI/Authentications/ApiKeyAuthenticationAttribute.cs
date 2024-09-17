using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace InSyncAPI.Authentications
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class ApiKeyAuthenticationAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {

            var isValid = IsValidApiKey(context.HttpContext);
            if (!isValid)
            {
                context.Result = new UnauthorizedObjectResult("Invalid Authentication");
                return;
            }

        }
        private bool IsValidApiKey(HttpContext context)
        {
            var provideApiKey = context.Request.Headers[AuthConfig.ApiKey].FirstOrDefault();
            var _configuration = context.RequestServices.GetRequiredService<IConfiguration>();

            if (string.IsNullOrEmpty(provideApiKey)) return false;
            var validApiKey = _configuration.GetValue<string>(AuthConfig.AuthSection);
            return string.Equals(validApiKey, provideApiKey, StringComparison.Ordinal);
        }
    }
}
