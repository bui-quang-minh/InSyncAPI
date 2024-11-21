using InSync_Api.DependencyInjectService;
using InSync_Api.MapperProfile;
using InSyncAPI.Extentions;
using Microsoft.EntityFrameworkCore;
using WebNewsAPIs.Extentions;
using DataAccess.ContextAccesss;
using static System.Net.WebRequestMethods;
using Serilog;
using Stripe;
using InSyncAPI.Middlewaves;



namespace InSyncAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // config url api InSensitive Route
            builder.Services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
                options.AppendTrailingSlash = false;

            });
            builder.AddLogServiceExtention();


            // Add services to the container.
            builder.Services.AddAuthorization();
            builder.ConfigAuthenAuthor();
            // Add services Log tot he container


            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            builder.Services.AddControllers();
            //inject dbContext into system
            builder.Services.AddDbContext<InSyncContext>(options =>
            {
                var connectString = builder.Configuration.GetConnectionString("InSyncConnectionString");
                options.UseSqlServer(connectString);
            });
            // inject service in system
            builder.Services.InjectService();
            // add profile of auto mapper
            builder.Services.AddAutoMapper(typeof(InSyncMapperProfile));
            builder.Services.ConfigOdata();
            // config cors 
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CORSPolicy", builder =>
                 builder
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowAnyOrigin()
               );
            });
            builder.WebHost.ConfigureKestrel(options =>
            {
                var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
                options.ListenAnyIP(int.Parse(port));
            });
            StripeConfiguration.ApiKey = "sk_test_51QFCAdIMZTDPn6rJ6b9TwEnsjZDXKs54CWdmuTF1hlovLoVpbZnRyCZwANFppTQd3hVfHpe1U0EQT9T3QdodSr5R00lgHOs4Tp";
            var app = builder.Build();

            // Ghi log các yêu cầu HTTP
            app.UseSerilogRequestLogging();
            // Add middleware in to system
            //app.AddMiddlewareExtetion();
            // set up Apikey for skipe

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseCors("CORSPolicy");
            app.UseMiddleware<ApiKeyMiddleware>();
            app.UseAuthorization();
            app.UseAuthentication();
           
            app.MapControllers();

            try
            {
                Log.Information("Starting web host");
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush(); // Đảm bảo tất cả log đã được ghi trước khi dừng ứng dụng
            }


        }
    }
}
