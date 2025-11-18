using System;
using System.Windows.Forms;
using System.Net.Http;
using BookingCareManagement.WinForms.Startup;
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

        // Lấy MainForm từ DI container
        var mainForm = host.Services.GetRequiredService<MainForm>();

        System.Windows.Forms.Application.Run(mainForm);
    }

    private static IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                // Đăng ký WinForms infrastructure (HTTP Client, Services, etc.)
                services.AddWinFormsInfrastructure(context.Configuration);
            });
    }
}