using BusinessObjects.Models;
using InSyncAPI.Controllers;

namespace InSyncAPI.Middlewaves
{
    public class LoggingControllerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingControllerMiddleware> _logger;
        private readonly List<string> listApiEndPoint = new List<string>
        {
            "/api/assets","/api/customerreviews",
            "/api/privacypolicys","/api/projects",
            "/api/scenarios","/api/subsciptionplans",
            "/api/terms","/api/tutorials","/api/users",
            "/api/usersubsciptions"
        };

        public LoggingControllerMiddleware(RequestDelegate next, ILogger<LoggingControllerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var pathRequest = context.Request.Path;
            var checkExistRequestPath = listApiEndPoint.FirstOrDefault(c => pathRequest.StartsWithSegments(c));

            // Ghi log trước khi xử lý yêu cầu
            if (checkExistRequestPath != null)
            {
                // Lấy thông tin endpoint
                var endpoint = context.GetEndpoint();
                var controllerAction = endpoint?.DisplayName; // Lấy tên controller và action

                _logger.LogInformation($"--------------- Incoming request : Method : ({context.Request.Method}) - Path : ({context.Request.Path}) ---------------");
                if (controllerAction != null)
                {
                    _logger.LogInformation($"--------------- Target controller/action: {controllerAction} ---------------");
                }
            }

            await _next(context); // Tiếp tục xử lý yêu cầu

            // Ghi log sau khi xử lý yêu cầu
            if (checkExistRequestPath != null)
            {
                // Ghi log thông tin phản hồi
                _logger.LogInformation($"--------------- Outgoing response: {context.Response.StatusCode} for {context.Request.Path} at {DateTime.UtcNow} ---------------");
            }
        }
    }
}
