using System;
using System.Windows.Forms;
using System.Net.Http;
//using BookingCareManagement.WinForms.Clients;
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

        //using var host = CreateHostBuilder().Build();

        //// SỬA: Lấy MainForm thay vì Form1
        //var mainForm = host.Services.GetRequiredService<MainForm>();

        System.Windows.Forms.Application.Run(new MainForm());
    }

    //private static IHostBuilder CreateHostBuilder()
    //{
    //    return Host.CreateDefaultBuilder()
    //        .ConfigureAppConfiguration((context, config) =>
    //        {
    //            config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    //        })
    //        .ConfigureServices((context, services) =>
    //        {
    //            // 1. Đăng ký DoctorApiClient (Typed HTTP Client)
    //            services.AddHttpClient<DoctorApiClient>((sp, client) =>
    //            {
    //                var baseUrl = context.Configuration.GetValue<string>("Api:BaseUrl");
    //                if (string.IsNullOrWhiteSpace(baseUrl))
    //                {
    //                    throw new InvalidOperationException("Api:BaseUrl configuration is required for the WinForms client.");
    //                }

    //                client.BaseAddress = new Uri(baseUrl);
    //            })
    //            .ConfigurePrimaryHttpMessageHandler(() =>
    //            {
    //                return new HttpClientHandler
    //                {
    //                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    //                };
    //            });

    //            // 2. Đăng ký MainForm (Quan trọng!)
    //            // Phải đăng ký để DI có thể tạo Form và Inject DoctorApiClient
    //            services.AddTransient<MainForm>();
    //        });
    //}
}