using System;
using System.Windows.Forms;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BookingCareManagement.WinForms.Startup;
using BookingCareManagement.WinForms.Shared.State;
using BookingCareManagement.WinForms.Areas.Account.Forms;

namespace BookingCareManagement.WinForms;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        using var host = CreateHostBuilder().Build();

        // Load previous session from storage
        var services = host.Services;
        var session = services.GetRequiredService<SessionState>();
        var storage = services.GetRequiredService<IAuthStorage>();

        // Try to restore persisted tokens
        var persisted = storage.Load();
        if (persisted is not null)
        {
            session.AccessToken = persisted.AccessToken;
            session.RefreshToken = persisted.RefreshToken;
            session.CurrentUserId = persisted.UserId;
        }

        // If not authenticated, show login form first
        if (!session.IsAuthenticated)
        {
            using var login = services.GetRequiredService<LoginForm>();
            var result = login.ShowDialog();
            if (result != DialogResult.OK)
            {
                // user cancelled login
                return;
            }
        }

        // Run main shell
        System.Windows.Forms.Application.Run(new MainForm());
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
                services.AddWinFormsInfrastructure(context.Configuration);

                // Forms
                services.AddTransient<LoginForm>();
            });
    }
}