using System;
using System.Windows.Forms;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BookingCareManagement.WinForms.Startup;
using BookingCareManagement.WinForms.Shared.State;
using BookingCareManagement.WinForms.Areas.Account.Forms;
using BookingCareManagement.WinForms.Shared.Models;

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
            if (persisted.CookieAuthenticated)
            {
                session.MarkCookieAuthenticated();
            }
            if (!string.IsNullOrWhiteSpace(persisted.DisplayName) || !string.IsNullOrWhiteSpace(persisted.Email))
            {
                var cachedProfile = new UserProfileDto
                {
                    UserId = persisted.UserId ?? string.Empty,
                    FullName = persisted.DisplayName ?? string.Empty,
                    Email = persisted.Email ?? string.Empty,
                    IsAdmin = persisted.IsAdmin,
                    IsDoctor = persisted.IsDoctor,
                    Roles = persisted.Roles ?? Array.Empty<string>()
                };
                session.ApplyProfile(cachedProfile);
            }
        }
        //2 dòng clear để đảm bảo mỗi lần chạy chương trình đều bắt đầu từ form login
        storage.Clear();
        session.Clear();
        // If not authenticated, show login form first (designer form)
        if (!session.IsAuthenticated)
        {
            using var login = services.GetRequiredService<Login>();
            var result = login.ShowDialog();
            if (result != DialogResult.OK)
            {
                // user cancelled login
                return;
            }
        }

        // Run main shell
        Application.Run(ResolveShell(services, session));
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
                services.AddTransient<Login>();
                services.AddTransient<Register>();
            });
    }

    private static Form ResolveShell(IServiceProvider services, SessionState session)
    {
        return new MainForm(services);
    }
}
