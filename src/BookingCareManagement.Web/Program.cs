using System.IO;
using System.Reflection;
using BookingCareManagement.Infrastructure.Identity;
using BookingCareManagement.Infrastructure.Persistence;
using BookingCareManagement.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

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


builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }
});

builder.Services.AddInfrastructure(builder.Configuration);

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
