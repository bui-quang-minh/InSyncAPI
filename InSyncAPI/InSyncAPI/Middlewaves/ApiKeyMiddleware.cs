namespace InSyncAPI.Middlewaves
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ApiKeyMiddleware> _logger;
        public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration
            , ILogger<ApiKeyMiddleware> logger)
        {
            _next = next;
            _configuration = configuration;
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            var excludedEndpoints = _configuration.GetSection("ExcludedEndpoints").Get<List<string>>();

            // Lấy đường dẫn hiện tại
            var currentPath = context.Request.Path.Value;

            // Nếu endpoint nằm trong danh sách miễn kiểm tra, bỏ qua xác thực API Key
            if (excludedEndpoints != null && excludedEndpoints.Contains(currentPath, StringComparer.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Skipping API Key validation for path: {Path}", currentPath);
                await _next(context);
                return;
            } 

            var provideApiKey = context.Request.Headers[AuthConfig.ApiKey].FirstOrDefault();
            if (string.IsNullOrEmpty(provideApiKey))
            {
                _logger.LogWarning("API Key is missing - Status400BadRequest");
                await GenerateResponse(context, 400, "API Key is missing");
                return;
            }

            var isValid = IsValidApiKey(provideApiKey);
            if (!isValid)
            {
                _logger.LogWarning("Invalid API Key -  Status401Unauthorized");
                await GenerateResponse(context, 401, "Invalid API Key");
                return;
            }
            _logger.LogWarning("Confirm valid api key - Status200OkRequest");
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
