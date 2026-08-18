using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SalaryWise.Data;
using SalaryWise.Repositories;
using SalaryWise.Services;

var builder = WebApplication.CreateBuilder(args);

try
{
    Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, "App_Data"));
}
catch { /* Ignore if IIS directory creation is restricted */ }

// ── Database ──────────────────────────────────────────────────────
var connStr = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (connStr.Contains("workstation id", StringComparison.OrdinalIgnoreCase) ||
        connStr.Contains("initial catalog", StringComparison.OrdinalIgnoreCase) ||
        connStr.Contains("data source=dbrobot", StringComparison.OrdinalIgnoreCase) ||
        connStr.Contains("Server=", StringComparison.OrdinalIgnoreCase))
    {
        options.UseSqlServer(connStr);
    }
    else
    {
        options.UseSqlite(connStr);
    }
});

// ── Identity ──────────────────────────────────────────────────────
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit          = true;
    options.Password.RequiredLength        = 6;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase      = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// ── MVC ───────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// ── Repositories ─────────────────────────────────────────────────
builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();
builder.Services.AddScoped<IInvestmentPlanRepository, InvestmentPlanRepository>();

// ── Services ──────────────────────────────────────────────────────
builder.Services.AddScoped<IAffordabilityService, AffordabilityService>();
builder.Services.AddScoped<IRecommendationEngine, RecommendationEngine>();
builder.Services.AddScoped<IProjectionService, ProjectionService>();
builder.Services.AddScoped<IFinancialHealthService, FinancialHealthService>();

var app = builder.Build();

// ── Seed database ────────────────────────────────────────────────
try
{
    await SeedData.InitializeAsync(app.Services);
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "An error occurred while seeding the database.");
}

// ── Middleware ────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// ── Routes ────────────────────────────────────────────────────────
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
