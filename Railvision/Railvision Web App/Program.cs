using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RailVision.App.Models;
using TrainGenie.Hubs;
using TrainGenie.Models;
using TrainGenie.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddCors(options => {
    options.AddPolicy("SignalRPolicy", policy => {
        policy.WithOrigins("https://localhost:44357")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddScoped<IncidentService>();
builder.Services.AddSignalR();

// Session configuration
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.Name = "TrainGenie.Session";
});

// Database connection
builder.Services.AddScoped<SqlConnection>(_ =>
    new SqlConnection(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.SuppressXFrameOptionsHeader = false;
});

builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.Cookie.Name = "TrainGenie.Auth";
    });

builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

// Configure custom MIME type for GeoJSON
var contentTypeProvider = new FileExtensionContentTypeProvider();
contentTypeProvider.Mappings[".geojson"] = "application/geo+json";

// Static files setup
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = contentTypeProvider
});

// Configure HTTP pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// Middleware ordering is critical here
app.UseCors("SignalRPolicy");
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Improved authentication middleware with better SignalR handling
app.Use(async (context, next) =>
{
    var path = context.Request.Path;

    // Skip auth check for these paths
    var isExcludedPath = path.StartsWithSegments("/Account/Login") ||
                          path.StartsWithSegments("/Account/Register") ||
                          path.StartsWithSegments("/error") ||
                          path.StartsWithSegments("/notificationHub") ||
                          path.StartsWithSegments("/_blazor") || // For Blazor if used
                          path.StartsWithSegments("/css") ||
                          path.StartsWithSegments("/js") ||
                          path.StartsWithSegments("/lib");

    if (!isExcludedPath && context.Session.GetString("IsAuthenticated") != "true")
    {
        context.Response.Redirect("/Account/Login");
        return;
    }

    await next();
});

// CRITICAL FIX: Endpoint configuration using UseEndpoints
app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorPages();
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}"
    );
    endpoints.MapHub<IncidentHub>("/incidentHub");
});

// Diagnostic endpoints
app.MapGet("/test-session", (HttpContext context) =>
{
    context.Session.SetString("Test", "Session Works!");
    return context.Session.GetString("Test");
});

app.Run();