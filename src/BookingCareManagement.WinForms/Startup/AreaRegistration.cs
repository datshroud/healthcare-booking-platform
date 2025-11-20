using BookingCareManagement.WinForms.Areas.Account.Controllers;
using BookingCareManagement.WinForms.Areas.Account.Forms;
using BookingCareManagement.WinForms.Areas.Account.ViewModels;
using BookingCareManagement.WinForms.Areas.Admin.Controls;
using BookingCareManagement.WinForms.Areas.Admin.Controllers;
using BookingCareManagement.WinForms.Areas.Admin.Forms;
using BookingCareManagement.WinForms.Areas.Admin.Services;
using BookingCareManagement.WinForms.Areas.Admin.ViewModels;
using BookingCareManagement.WinForms.Areas.Customer.Controllers;
using BookingCareManagement.WinForms.Areas.Customer.Forms;
using BookingCareManagement.WinForms.Areas.Customer.ViewModels;
using BookingCareManagement.WinForms.Areas.Doctor.Controllers;
using BookingCareManagement.WinForms.Areas.Doctor.Services;
using BookingCareManagement.WinForms.Areas.Doctor.Forms;
using BookingCareManagement.WinForms.Areas.Doctor.ViewModels;
using BookingCareManagement.WinForms.Areas.Public.Controllers;
using BookingCareManagement.WinForms.Areas.Public.Forms;
using BookingCareManagement.WinForms.Areas.Public.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using BookingCareManagement.WinForms.Areas.Customer.Services.Models;

namespace BookingCareManagement.WinForms.Startup;

public static class AreaRegistration
{
    public static IServiceCollection AddAdminArea(this IServiceCollection services)
    {
        services
            .AddSingleton<AdminDashboardViewModel>()
            .AddTransient<AdminDoctorApiClient>()
            .AddTransient<AdminDashboardApiClient>()
            .AddTransient<AdminSpecialtyApiClient>()
            .AddTransient<AdminInvoiceApiClient>()
            .AddTransient<AdminAppointmentsApiClient>()
            .AddTransient<AppointmentManagementControl>()
            .AddTransient<AppointmentEditorForm>()
            .AddTransient<SpecialtyManagementControl>()
            .AddTransient<DoctorManagementControl>()
            .AddTransient<InvoiceManagementControl>()
            .AddTransient<InvoiceEditorForm>()
            .AddTransient<DashboardForm>()
            .AddTransient<Calendar>()
            .AddTransient<AdminNavigationController>()
            .AddTransient<AdminShellForm>()
            // Register WinForms for Admin area so DI can inject API clients
            .AddTransient<Doctor>()
            .AddTransient<Specialty>()
            .AddTransient<CustomerService>()
            .AddTransient<Customer>();
        return services;
    }

    public static IServiceCollection AddDoctorArea(this IServiceCollection services)
    {
        services
            .AddSingleton<DoctorAppointmentsViewModel>()
            .AddTransient<DoctorAppointmentsApiClient>()
            .AddTransient<DoctorAppointmentsController>()
            .AddTransient<DoctorAppointmentsForm>();
        return services;
    }

    public static IServiceCollection AddCustomerArea(this IServiceCollection services)
    {
        services
            .AddSingleton<CustomerQueueViewModel>()
            .AddTransient<CustomerQueueController>()
            .AddTransient<CustomerQueueForm>()
            // Register customer-facing forms
            .AddTransient<Service>()
            .AddTransient<Bookings>()
            .AddTransient<MyBookingForm>()
            .AddTransient<CustomerBookingApiClient>();
        return services;
    }

    public static IServiceCollection AddAccountArea(this IServiceCollection services)
    {
        services
            .AddSingleton<LoginViewModel>()
            .AddTransient<AuthController>()
            .AddTransient<LoginForm>();
        return services;
    }

    public static IServiceCollection AddPublicArea(this IServiceCollection services)
    {
        services
            .AddSingleton<PublicContentViewModel>()
            .AddTransient<PublicContentController>()
            .AddTransient<PublicInformationForm>()
            .AddTransient<MainForm>();
        return services;
    }
}
