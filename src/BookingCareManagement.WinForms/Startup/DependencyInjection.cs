using System;
using System.Net.Http;
using BookingCareManagement.WinForms.Shared.Http;
using BookingCareManagement.WinForms.Shared.Services;
using BookingCareManagement.WinForms.Shared.State;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookingCareManagement.WinForms.Startup;

public static class DependencyInjection
{
    public static IServiceCollection AddWinFormsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(SessionState.CreateUnauthenticated());
        services.AddSingleton<IAuthStorage, FileAuthStorage>();
        services.AddSingleton<DialogService>();
        services.AddTransient<AuthHeaderHandler>();
        // API client services
        services.AddSingleton<AuthService>();

        services.AddHttpClient("BookingCareApi", (sp, client) =>
            {
                // Prefer configuration, then environment variable, then fallback to known deployed URL.
                var baseUrl = configuration["Api:BaseUrl"]
                              ?? Environment.GetEnvironmentVariable("API_BASE_URL")
                              ?? "https://healthcare-booking-dzhba4dmdjagcdbq.southeastasia-01.azurewebsites.net";

                client.BaseAddress = new Uri(baseUrl);
            })
            .AddHttpMessageHandler<AuthHeaderHandler>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });

        services.AddAdminArea();
        services.AddDoctorArea();
        services.AddCustomerArea();
        services.AddAccountArea();
        services.AddPublicArea();

        return services;
    }
}
