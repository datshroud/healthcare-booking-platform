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
using BookingCareManagement.WinForms.Areas.Customer.Forms;

namespace BookingCareManagement.WinForms;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // Register UI-thread exception handlers BEFORE any Controls are created
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (s, e) =>
        {
            Console.Error.WriteLine("Unhandled UI thread exception:");
            Console.Error.WriteLine(e.Exception.ToString());
        };

        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            Console.Error.WriteLine("Unhandled domain exception:");
            if (e.ExceptionObject is Exception ex)
                Console.Error.WriteLine(ex.ToString());
            else
                Console.Error.WriteLine(e.ExceptionObject?.ToString());
        };

        // now proceed with host and login flow

        try
        {
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
            if (!string.IsNullOrWhiteSpace(persisted.Redirect))
            {
                session.ApplyRoleHint(persisted.Redirect);
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

            // Ensure a clean session on startup so the login always appears
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
            Console.WriteLine("[Program] Creating main shell...");
            var shell = ResolveShell(services, session);
            Console.WriteLine("[Program] Main shell created, starting Application.Run");
            Application.Run(shell);
            Console.WriteLine("[Program] Application.Run returned");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Fatal exception during application startup:");
            Console.Error.WriteLine(ex.ToString());
            Environment.ExitCode = 1;
        }
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
