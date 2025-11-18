using BookingCareManagement.Application.Abstractions;
using BookingCareManagement.Application.Features.Auth.Commands;
using BookingCareManagement.Application.Features.Customers.Commands;
using BookingCareManagement.Application.Features.Customers.Queries;
using BookingCareManagement.Application.Features.Doctors.Commands;
using BookingCareManagement.Application.Features.Doctors.Queries;
using BookingCareManagement.Application.Features.Specialties.Commands;
using BookingCareManagement.Application.Features.Specialties.Queries;
using BookingCareManagement.Domain.Abstractions;
using BookingCareManagement.Infrastructure.Identity;
using BookingCareManagement.Infrastructure.Persistence;
using BookingCareManagement.Infrastructure.Persistence.Repositories;
using BookingCareManagement.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeAreaFolder("Admin", "/", "AdminOnly");
    options.Conventions.AuthorizeAreaFolder("Doctor", "/", "DoctorOrAbove");
    options.Conventions.AuthorizeAreaFolder("Customer", "/", "CustomerOrAbove");

    // options.Conventions.AddAreaPageRoute("Customer", "/Reviews/Reviews", "/reviews");

    // options.Conventions.AllowAnonymousToAreaPage("Customer", "/Reviews/Reviews");

});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("DoctorOrAbove", policy => policy.RequireRole("Admin", "Doctor"));
    options.AddPolicy("CustomerOrAbove", policy => policy.RequireRole("Admin", "Doctor", "Customer"));

});



builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<RegisterHandler>();
builder.Services.AddScoped<LoginHandler>();
builder.Services.AddScoped<RefreshTokenHandler>();

builder.Services.Configure<GoogleOAuthSettings>(builder.Configuration.GetSection("GoogleOAuth"));

builder.Services.AddCors(o =>
    o.AddPolicy("spa", p => p
        .WithOrigins("https://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
    )
);

// ... các services.Add... khác
builder.Services.AddHttpContextAccessor(); // Cần cho FileStorageService
//builder.Services.AddScoped<IFileStorageService, FileStorageService>(); // Đăng ký dịch vụ file

// Đăng ký Repository
builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();

// Đăng ký Handler
builder.Services.AddScoped<GetAllDoctorsQueryHandler>();

builder.Services.AddScoped<ISpecialtyRepository, SpecialtyRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<CreateDoctorCommandHandler>(); // Đăng ký handler mới

builder.Services.AddScoped<GetDoctorByIdQueryHandler>();
builder.Services.AddScoped<UpdateDoctorCommandHandler>();
builder.Services.AddScoped<DeleteDoctorCommandHandler>();

builder.Services.AddScoped<GetAllSpecialtiesQueryHandler>();
builder.Services.AddScoped<CreateSpecialtyCommandHandler>();
builder.Services.AddScoped<UpdateSpecialtyCommandHandler>();
builder.Services.AddScoped<DeleteSpecialtyCommandHandler>();

builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<GetAllCustomersQueryHandler>();
builder.Services.AddScoped<CreateCustomerCommandHandler>();
builder.Services.AddScoped<UpdateCustomerCommandHandler>();
builder.Services.AddScoped<DeleteCustomerCommandHandler>();

// Register Invoice repository
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();

var app = builder.Build();

/* BẠN NÊN XÓA HOẶC COMMENT KHỐI NÀY LẠI
var rewrites = new RewriteOptions()
    .AddRedirect("^calendar/?$", "dashboard")
    .AddRedirect("^appointments/?$", "dashboard")
    .AddRedirect("^doctors/?$", "dashboard")
    .AddRedirect("^customers/?$", "dashboard")
    .AddRedirect("^specialties/?$", "dashboard")
    .AddRedirect("^finance/?$", "dashboard");

app.UseRewriter(rewrites);
*/


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
var cs = builder.Configuration.GetConnectionString("DefaultConnection");
// try
// {
//     using var con = new SqlConnection(cs);
//     await con.OpenAsync();
//     Console.WriteLine("SQL connected OK");
// }
// catch (Exception ex)
// {
//     Console.WriteLine("SQL connect failed: " + ex.Message);
//     throw;
// }


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(scope.ServiceProvider);
}

app.MapGet("/_routes", (IEnumerable<EndpointDataSource> sources) =>
{
    var patterns = sources
        .SelectMany(s => s.Endpoints)
        .OfType<RouteEndpoint>()
        .Select(e => e.RoutePattern.RawText);
    return string.Join("\n", patterns.OrderBy(x => x));
});

app.UseCors("spa");

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.MapControllers();


app.Run();