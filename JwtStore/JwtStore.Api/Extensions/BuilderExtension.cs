using System.Text;
using JwtStore.Core;
using JwtStore.Infra.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace JwtStore.Api.Extensions;

public static class BuilderExtension
{
    public static void AddConfiguration(this WebApplicationBuilder builder)
    {
        Configuration.Database.ConnectionString = builder.Configuration.GetSection("ConnectionStrings:DefaultConnection").Value ?? string.Empty;
        Configuration.Secrets.ApiKey = builder.Configuration.GetSection("Secrets:ApiKey").Value ?? string.Empty;
        Configuration.Secrets.JwtPrivateKey = builder.Configuration.GetSection("Secrets:JwtPrivateKey").Value ?? string.Empty;
        Configuration.Secrets.PasswordSaltKey = builder.Configuration.GetSection("Secrets:PasswordSaltKey").Value ?? string.Empty;
        Configuration.SendGrid.ApiKey = builder.Configuration.GetSection("SendGrid:ApiKey").Value ?? string.Empty;
        Configuration.Email.DefaultFromName = builder.Configuration.GetSection("Email:DefaultFromName").Value ?? string.Empty;
        Configuration.Email.DefaultFromEmail = builder.Configuration.GetSection("Email:DefaultFromEmail").Value ?? string.Empty;
    }
    
    public static void AddDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<AppDbContext>(x =>
            x.UseSqlServer(
                Configuration.Database.ConnectionString,
                b => b.MigrationsAssembly("JwtStore.Api")));
    }
    
    public static void AddJwtAuthentication(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration.Secrets.JwtPrivateKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });
        builder.Services.AddAuthorization();
    }

    public static void AddMediator(this WebApplicationBuilder builder)
    {
        builder.Services.AddMediatR(x
            => x.RegisterServicesFromAssembly(typeof(Configuration).Assembly));
    }
}