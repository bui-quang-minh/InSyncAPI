using InSyncAPI.Middlewaves;

namespace InSyncAPI.Extentions
{
    public static class AddMiddlewareExtention
    {
        public static void AddMiddlewareExtetion(this WebApplication app)
        {
            app.UseMiddleware<LoggingControllerMiddleware>();
        }
    }
}
