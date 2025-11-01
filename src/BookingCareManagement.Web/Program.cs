using BookingCareManagement.Application.Features.Auth.Commands;
using BookingCareManagement.Infrastructure.Identity;
using BookingCareManagement.Infrastructure.Persistence;
using BookingCareManagement.Infrastructure.Persistence.Seed;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeAreaFolder("Admin", "/", "AdminOnly");
    options.Conventions.AuthorizeAreaFolder("Doctor", "/", "DoctorOrAbove");
    options.Conventions.AuthorizeAreaFolder("Customer", "/", "CustomerOrAbove");

    // options.Conventions.AddAreaPageRoute("Customer", "/Reviews/Reviews", "/my-bookings/reviews");

    // // Alias cho ReviewSuccess.cshtml -> /my-bookings/reviews/success
    // options.Conventions.AddAreaPageRoute("Customer", "/Reviews/ReviewSuccess", "/my-bookings/reviews/success");
});

builder.Services.AddAuthorization(options => {
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

var app = builder.Build();


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

app.MapGet("/_routes", (IEnumerable<EndpointDataSource> sources) =>
{
    return string.Join("\n", sources.SelectMany(s => s.Endpoints).Select(e => e.DisplayName));
});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(scope.ServiceProvider);
}

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
