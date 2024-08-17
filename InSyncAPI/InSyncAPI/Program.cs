namespace InSyncAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();
            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            var summaries = new[]
            {
                "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
            };

            app.MapGet("/weatherforecast", (HttpContext httpContext) =>
            {
                var forecast = new[]
                {
                    new { id = 0, name = "Templates & Plan Features", price = "Price", projects = "Projects", scenarios = "Number of Scenarios", support = "Support", storage = "Storage", users = "Number of Users", apiAccess = "API Access" },
                    new { id = 1, name = "Starters", price = "Free", projects = "3", scenarios = "3", support = "Weekdays", storage = "5GB", users = "1", apiAccess = "No" },
                    new { id = 2, name = "Professional", price = "100$/Month", projects = "Unlimited", scenarios = "Unlimited", support = "24/7", storage = "Unlimited", users = "10", apiAccess = "Yes" },
                    new { id = 3, name = "Professional", price = "100$/Month", projects = "Unlimited", scenarios = "Unlimited", support = "24/7", storage = "Unlimited", users = "10", apiAccess = "Yes" }
                };

                httpContext.Response.Headers["Content-Type"] = "application/json";
                return Results.Json(forecast);
            })
            .WithName("GetWeatherForecast");



            app.Run();
        }
    }
}
