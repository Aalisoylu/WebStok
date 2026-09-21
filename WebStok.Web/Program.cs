using WebStok.Domain.Interfaces;
using WebStok.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using WebStok.DataAccess.Persistence;
using FluentValidation;
using WebStok.Business.DTOs;
using WebStok.Business.Interfaces;
using WebStok.Business.Services;
using WebStok.Business.Validators;
using WebStok.Business.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System.Threading.RateLimiting;
using WebStok.Web.Security;
using WebStok.Web.Filters;

var builder = WebApplication.CreateBuilder(args);


//
var databasePath = Path.Combine(
    builder.Environment.ContentRootPath,
    "App_Data");

Directory.CreateDirectory(databasePath);

var connectionString = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder
{
    DataSource = Path.Combine(databasePath, "webstok.db"),
    ForeignKeys = true
}.ToString();

builder.Services.AddDbContext<WebStokDbContext>(options =>
    options.UseSqlite(connectionString));

//



// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<AccessDeniedExceptionFilter>();
});


builder.Services.AddScoped(
    typeof(IRepository<>),
    typeof(Repository<>));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();




builder.Services.AddScoped<
    IValidator<CreateCategoryDto>,
    CreateCategoryValidator>();

builder.Services.AddScoped<ICategoryService, CategoryService>();


builder.Services.AddScoped<
    IValidator<CreateWarehouseDto>,
    CreateWarehouseValidator>();

builder.Services.AddScoped<IWarehouseService, WarehouseService>();



builder.Services.AddScoped<
    IValidator<CreateProductDto>,
    CreateProductValidator>();

builder.Services.AddScoped<IProductService, ProductService>();


builder.Services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();

builder.Services.AddScoped<IValidator<LoginDto>, LoginValidator>();

builder.Services.AddScoped<IAuthService, AuthService>();



builder.Services.Configure<Microsoft.AspNetCore.Builder.RequestLocalizationOptions>(
    options =>
    {
        options.SetDefaultCulture("tr-TR")
            .AddSupportedCultures("tr-TR")
            .AddSupportedUICultures("tr-TR");
    });



builder.Services.AddScoped<IInitialAdminService, InitialAdminService>();    



builder.Services.AddScoped<WebStokCookieEvents>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";

        options.Cookie.Name = "__Host-WebStok.Auth";
        options.Cookie.Path = "/";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;

        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = false;
        options.EventsType = typeof(WebStokCookieEvents);
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.AddPolicy("ManageInventory", policy =>
        policy.RequireAuthenticatedUser()
            .RequireRole(
                "SuperAdmin",
                "ITAdmin",
                "WarehouseSupervisor"));
});

builder.Services.AddAntiforgery(options =>
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("login", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});




builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ICurrentUser, CurrentUser>();

builder.Services.AddScoped<
    IValidator<StockReceiptDto>,
    StockReceiptValidator>();


builder.Services.AddScoped<IStockReceiptService, StockReceiptService>();
builder.Services.AddScoped<IStockBalanceService, StockBalanceService>();

builder.Services.AddScoped<
    IValidator<StockIssueDto>,
    StockIssueValidator>();
    
builder.Services.AddScoped<IStockIssueService, StockIssueService>();
builder.Services.AddScoped<IStockMovementService, StockMovementService>();
builder.Services.AddScoped<
    IValidator<StockTransferDto>,
    StockTransferValidator>();

builder.Services.AddScoped<IStockTransferService, StockTransferService>();

builder.Services.AddScoped<
    IValidator<StockReturnDto>,
    StockReturnValidator>();


builder.Services.AddScoped<IStockReturnService, StockReturnService>();    
var app = builder.Build();
app.UseRequestLocalization();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    context.Response.Headers["Cache-Control"] =
        "no-store, no-cache, must-revalidate";
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["Expires"] = "0";

    await next();
});

app.UseRouting();

app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

app.MapStaticAssets().AllowAnonymous();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();

    var initialAdminService =
        scope.ServiceProvider.GetRequiredService<IInitialAdminService>();

    var created = await initialAdminService.EnsureCreatedAsync(
        app.Configuration["Bootstrap:AdminPassword"]);

    if (created)
    {
        app.Logger.LogInformation(
            "İlk yönetici hesabı oluşturuldu. Kullanıcı adı: admin");
    }
}
app.Run();
