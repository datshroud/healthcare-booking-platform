using System;
using System.Windows.Forms;
using System.Net.Http;
using BookingCareManagement.WinForms.Clients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BookingCareManagement.WinForms;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        using var host = CreateHostBuilder().Build();
        var MainForm = host.Services.GetRequiredService<MainForm>();
        System.Windows.Forms.Application.Run(MainForm);
    }

    private static IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddHttpClient<DoctorApiClient>((sp, client) =>
                {
                    var baseUrl = context.Configuration.GetValue<string>("Api:BaseUrl");
                    if (string.IsNullOrWhiteSpace(baseUrl))
                    {
                        throw new InvalidOperationException("Api:BaseUrl configuration is required for the WinForms client.");
                    }

                    client.BaseAddress = new Uri(baseUrl);
                })
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    return new HttpClientHandler
                    {
                        // Trust the ASP.NET Core developer certificate so HTTPS calls succeed during local development.
                        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    };
                });
                services.AddTransient<MainForm>();
            });
    }
}