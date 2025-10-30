using BookingCareManagement.Application.Features.Auth.Commands;
using BookingCareManagement.Infrastructure.Identity;
using BookingCareManagement.Infrastructure.Persistence.Seed;

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

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
await DbSeeder.SeedAsync(app.Services);

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
