using Serilog;

namespace InSyncAPI.Extentions
{
    public static class LogServiceExtention
    {
        public static void AddLogServiceExtention(this WebApplicationBuilder builder)
        {
            Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()  // Mức độ log tối thiểu
                    .WriteTo.Console(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Debug)     // Ghi log ra console
                    .WriteTo.File("Logs/log-.txt",restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information, rollingInterval: RollingInterval.Day, shared : true) // Ghi log ra file, theo ngày
                    .CreateLogger();
            // Thay thế logging mặc định bằng Serilog
            builder.Host.UseSerilog();
        }
       
    }
}
