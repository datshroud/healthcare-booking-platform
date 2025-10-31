using BookingCareManagement.Application.Features.Auth.Commands;
using BookingCareManagement.Infrastructure.Identity;
using BookingCareManagement.Infrastructure.Persistence;
using BookingCareManagement.Infrastructure.Persistence.Seed;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
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
try
{
    using var con = new SqlConnection(cs);
    await con.OpenAsync();
    Console.WriteLine("SQL connected OK");
}
catch (Exception ex)
{
    Console.WriteLine("SQL connect failed: " + ex.Message);
    throw;
}

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

app.MapGet("/", () => Results.Redirect("/login"));

app.Run();
