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
        services.AddSingleton<DialogService>();
        services.AddTransient<AuthHeaderHandler>();

        services.AddHttpClient("BookingCareApi", (sp, client) =>
            {
                var baseUrl = configuration.GetValue<string>("Api:BaseUrl")
                    ?? throw new InvalidOperationException("Missing Api:BaseUrl configuration for WinForms client");
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
