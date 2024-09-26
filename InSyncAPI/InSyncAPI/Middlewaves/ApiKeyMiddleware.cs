namespace InSyncAPI.Middlewaves
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            var provideApiKey = context.Request.Headers[AuthConfig.ApiKey].FirstOrDefault();
            var isValid = IsValidApiKey(provideApiKey);
            if (!isValid)
            {
                await GenerateResponse(context, 401, "Invalid Authentication");
                return;
            }

            await _next(context);
        }

        private async Task GenerateResponse(HttpContext context, int codeResponse, string message)
        {
            context.Response.StatusCode = codeResponse;
            await context.Response.WriteAsync(message);
        }

        private bool IsValidApiKey(string provideApiKey)
        {
            if (string.IsNullOrEmpty(provideApiKey)) return false;
            var validApiKey = _configuration.GetValue<string>(AuthConfig.AuthSection);
            return string.Equals(validApiKey, provideApiKey, StringComparison.Ordinal);
        }
    }
}
