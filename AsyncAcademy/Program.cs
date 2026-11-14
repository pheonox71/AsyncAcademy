using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AsyncAcademy.Data;
using Microsoft.Extensions.DependencyInjection.Extensions;
using AsyncAcademy.Models;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

// Connection string comes from configuration (appsettings placeholder, user secrets, or
// ConnectionStrings__TITAN-CONNECTION-STRING). The committed placeholder is not a real login.
var connectionString = builder.Configuration.GetConnectionString("TITAN-CONNECTION-STRING");
if (string.IsNullOrWhiteSpace(connectionString) ||
    connectionString.Contains("YOUR_SQL_USER", StringComparison.Ordinal) ||
    connectionString.Contains("YOUR_SQL_PASSWORD", StringComparison.Ordinal))
{
    throw new InvalidOperationException(
        "Connection string 'TITAN-CONNECTION-STRING' is not set. Set ConnectionStrings:TITAN-CONNECTION-STRING with user secrets or the ConnectionStrings__TITAN-CONNECTION-STRING environment variable.");
}

builder.Services.AddDbContext<AsyncAcademyContext>(options =>
    options.UseSqlServer(connectionString));

// From Bash
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(300);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
//

// Stripe test key comes from configuration (user secrets Stripe:SecretKey, or Stripe__SecretKey).
var stripeSecretKey = builder.Configuration["Stripe:SecretKey"];
if (string.IsNullOrWhiteSpace(stripeSecretKey) ||
    stripeSecretKey.Contains("your_key_here", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException(
        "Stripe secret key is not set. Set Stripe:SecretKey with user secrets or the Stripe__SecretKey environment variable.");
}

StripeConfiguration.ApiKey = stripeSecretKey;



builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>(); // The Last Part

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Upon service creation, check logic for seeding an empty database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var SeedData = new SeedData(scope.ServiceProvider.GetRequiredService<AsyncAcademyContext>());
    SeedData.Initialize(services);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSession(); // From Bash

app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();