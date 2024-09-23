using InSync_Api.DependencyInjectService;
using InSync_Api.MapperProfile;
using InSyncAPI.Authentications;
using InSyncAPI.Extentions;
using Microsoft.EntityFrameworkCore;
using WebNewsAPIs.Extentions;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using DataAccess.ContextAccesss;

namespace InSyncAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();
            builder.ConfigAuthenAuthor();

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
                builder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().SetIsOriginAllowed((host) => true));
            });

            var app = builder.Build();

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

            app.Run();
        }
    }
}
