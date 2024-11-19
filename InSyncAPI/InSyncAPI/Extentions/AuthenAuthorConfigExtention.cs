using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace InSyncAPI.Extentions
{
    public static class AuthenAuthorConfigExtention
    {
        public static void ConfigAuthenAuthor(this WebApplicationBuilder builder)
        {
            builder.Services.AddAuthorization();
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                //new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
            }).AddJwtBearer(x =>
            {


                x.Authority = builder.Configuration.GetSection("JwtSetting:Issuer").Value;
                x.Audience = builder.Configuration.GetSection("JwtSetting:Audience").Value;
                x.RequireHttpsMetadata = true;
                x.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {

                    ValidIssuer = builder.Configuration.GetSection("JwtSetting:Issuer").Value,
                    ValidAudience = builder.Configuration.GetSection("JwtSetting:Audience").Value,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero,
                };

                x.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var client = context.HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>().CreateClient();
                        var jwksUrl = builder.Configuration.GetSection("JwtSetting:JWKS").Value; // Clerk JWKS URL

                        // Lấy JWKS từ Clerk
                        var jwks = await client.GetStringAsync(jwksUrl);

                        // Phân tích JWKS và cấu hình các key cho việc xác thực
                        var jsonWebKeySet = new JsonWebKeySet(jwks);
                        var keys = jsonWebKeySet.GetSigningKeys();
                        context.Options.TokenValidationParameters.IssuerSigningKeys = keys;

                    }
                };


            });

           
        }
    }
}
