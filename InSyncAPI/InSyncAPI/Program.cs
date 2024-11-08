using InSync_Api.DependencyInjectService;
using InSync_Api.MapperProfile;
using InSyncAPI.Authentications;
using InSyncAPI.Extentions;
using Microsoft.EntityFrameworkCore;
using WebNewsAPIs.Extentions;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using DataAccess.ContextAccesss;

using static System.Net.WebRequestMethods;
using Serilog;
using Stripe;
using Microsoft.Extensions.Configuration;

namespace InSyncAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            string[] urlOrigins = new string[]
            {
                "https://insync-theta.vercel.app/",
                "http://localhost:3000/",
                "https://www.insync.com.vn/" ,
                "https://insync-git-master-djao-duy-thais-projects.vercel.app/"
            };

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
                .SetIsOriginAllowed((host) => true));
            });
            StripeConfiguration.ApiKey = builder.Configuration.GetValue<string>("StripConfig:ApiKey");
            var app = builder.Build();
           
            // Ghi log các yêu cầu HTTP
            app.UseSerilogRequestLogging();
            // Add middleware in to system
            app.AddMiddlewareExtetion();
            // set up Apikey for skipe
           
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            // app.UseMiddleware<ApiKeyMiddleware>();
            app.UseAuthorization();
            app.UseAuthentication();
            app.UseCors("CORSPolicy");
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
