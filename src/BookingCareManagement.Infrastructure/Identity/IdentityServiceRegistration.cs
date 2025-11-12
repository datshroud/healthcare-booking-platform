using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BookingCareManagement.Application.Features.Auth.Commands;
using BookingCareManagement.Application.Features.Doctors.Commands;
using BookingCareManagement.Application.Features.Doctors.Queries;
using BookingCareManagement.Domain.Aggregates.User;
using BookingCareManagement.Domain.Abstractions;
using BookingCareManagement.Infrastructure.Identity.Jwt;
using BookingCareManagement.Infrastructure.Persistence;
using BookingCareManagement.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace BookingCareManagement.Infrastructure.Identity
{
    public static class IdentityServiceRegistration
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<ApplicationDBContext>(otp =>
            {
                var connectionString = config.GetConnectionString("DefaultConnection");
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException("Database connection string 'DefaultConnection' is missing or empty.");
                }

                otp.UseSqlServer(connectionString, sql =>
                {
                    sql.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                });
            });

            services.AddIdentity<AppUser, AppRole>(o =>
            {
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequireUppercase = false;
            }).AddEntityFrameworkStores<ApplicationDBContext>()
            .AddDefaultTokenProviders();

            services.Configure<JwtSettings>(config.GetSection("Jwt"));
            var jwtSection = config.GetSection("Jwt");
            var jwt = jwtSection.Get<JwtSettings>() ?? throw new InvalidOperationException("Jwt configuration is missing.");
            if (string.IsNullOrWhiteSpace(jwt.Key))
            {
                throw new InvalidOperationException("Jwt key is not configured.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key));

            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = key,
                    RoleClaimType = ClaimTypes.Role,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        if (ctx.Request.Cookies.TryGetValue("access_token", out var token))
                        {
                            ctx.Token = token;
                        }
                        return Task.CompletedTask;
                    },

                    OnAuthenticationFailed = ctx =>
                    {
                        Console.WriteLine("JWT failed: " + ctx.Exception.Message);
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

            services.AddScoped<RegisterHandler>();
            services.AddScoped<LoginHandler>();
            services.AddScoped<RefreshTokenHandler>();

            services.AddScoped<IDoctorRepository, DoctorRepository>();
            services.AddScoped<ISpecialtyRepository, SpecialtyRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<GetAllDoctorsQueryHandler>();
            services.AddScoped<GetDoctorByIdQueryHandler>();
            services.AddScoped<CreateDoctorCommandHandler>();
            services.AddScoped<UpdateDoctorCommandHandler>();
            services.AddScoped<DeleteDoctorCommandHandler>();
            return services;
        }
    }
}
