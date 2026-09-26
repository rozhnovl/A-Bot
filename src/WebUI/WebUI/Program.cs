using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using WebUI.Client.Pages;
using WebUI.Components;
using WebUI.Components.Account;
using WebUI.Data;
using WebUI.Hubs;
using WebUI.Esi;
using Microsoft.AspNetCore.SignalR;

using Microsoft.Extensions.Hosting;
using StackExchange.Redis; // Requires NuGet package


var builder = WebApplication.CreateBuilder(args);

// A writable, persistent key ring is required for ESI refresh-token rotation.
// Production deployments should point this at a private, backed-up directory.
var keyDirectory = builder.Configuration["DataProtection:KeysDirectory"];
if (string.IsNullOrWhiteSpace(keyDirectory))
    keyDirectory = Path.Combine(builder.Environment.ContentRootPath, ".data-protection-keys");
var dataProtection = builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keyDirectory));
if (OperatingSystem.IsWindows()) dataProtection.ProtectKeysWithDpapi();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.AddServiceDefaults();
builder.AddRedisClient(connectionName: "cache");

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
var productionConnection = builder.Configuration.GetConnectionString("ProductionConnection") ?? "Data Source=production-v2.db;Cache=Shared";
var productionConnectionBuilder = new SqliteConnectionStringBuilder(productionConnection);
if (!Path.IsPathRooted(productionConnectionBuilder.DataSource))
    productionConnectionBuilder.DataSource = Path.Combine(builder.Environment.ContentRootPath, productionConnectionBuilder.DataSource);
builder.Services.AddDbContextFactory<ProductionDbContext>(options => options.UseSqlite(productionConnectionBuilder.ToString()));
builder.Services.AddSingleton<ProductionService>();
builder.Services.Configure<EsiOptions>(builder.Configuration.GetSection(EsiOptions.SectionName));
builder.Services.AddHttpClient<EsiHttpClient>();
builder.Services.AddScoped<EsiSqliteStore>();
builder.Services.AddScoped<IEsiTokenStore>(sp => sp.GetRequiredService<EsiSqliteStore>());
builder.Services.AddScoped<IEsiSnapshotSink>(sp => sp.GetRequiredService<EsiSqliteStore>());
builder.Services.AddScoped<EsiSalesProjection>();
builder.Services.AddSingleton<EsiPlanningService>();
builder.Services.AddSingleton<EsiSyncHealth>();
builder.Services.AddSingleton<EsiOAuthState>();
builder.Services.AddHostedService<EsiIngestionService>();
builder.Services.AddSignalR();

var app = builder.Build();

// The local HTTPS/SSO onboarding needs an Identity store before registration.
// Keep production schema changes an explicit deployment operation.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync();
}


app.MapDefaultEndpoints();
app.MapProductionApi();
app.MapEsiEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(WebUI.Client._Imports).Assembly);
app.MapHub<ProductionHub>("/hubs/production");

var productionService = app.Services.GetRequiredService<ProductionService>();
var productionHub = app.Services.GetRequiredService<IHubContext<ProductionHub>>();
productionService.Changed += () => _ = productionHub.Clients.All.SendAsync("productionChanged", productionService.Snapshot());

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
