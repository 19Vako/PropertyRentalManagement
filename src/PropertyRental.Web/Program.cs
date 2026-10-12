using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using PropertyRental.Domain.Identity;
using PropertyRental.Infrastructure.Data;
using PropertyRental.Infrastructure.Identity;
using PropertyRental.Web.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApplicantOnly", policy => policy.RequireRole(RoleNames.Applicant));
    options.AddPolicy("PropertyManagerOnly", policy => policy.RequireRole(RoleNames.PropertyManager));
});
builder.Services.AddScoped<IApplicationVisibilityService, ApplicationVisibilityService>();
builder.Services.AddScoped<IManagerDashboardService, ManagerDashboardService>();
builder.Services.AddScoped<IPropertyManagementService, PropertyManagementService>();
builder.Services.AddScoped<IRentalApplicationWorkflowService, RentalApplicationWorkflowService>();
builder.Services.AddScoped<ILeaseManagementService, LeaseManagementService>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Home/AccessDenied";
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Принудительно устанавливаем культуру en-US для корректной обработки точек в decimal и датах
var supportedCultures = new[] { new CultureInfo("en-US") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-US"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

if (app.Environment.IsDevelopment() &&
    app.Configuration.GetValue<bool>("DevelopmentSeed:RunOnStartup"))
{
    await DevelopmentDatabaseInitializer.InitializeAsync(app.Services, app.Configuration);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    
    string[] roles = ["Applicant", "Property Manager", "APPLICANT"];  
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

app.Run();