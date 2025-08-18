using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.EntityFrameworkCore;
using rWCMS.Data;
using rWCMS.Filters;
using rWCMS.Repositories;
using rWCMS.Repositories.Interface;
using rWCMS.Services;
using rWCMS.Services.Interface;
using rWCMS.Utilities;

var builder = WebApplication.CreateBuilder(args);

// Add configuration from appsettings.json
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Add logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
    logging.SetMinimumLevel(LogLevel.Debug);
});

// Add services to the container
builder.Services.AddControllers(options =>
{
    // Add global exception filter to simplify error handling
    options.Filters.Add<GlobalExceptionFilter>();
});
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate();
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
});

builder.Services.AddDbContext<rWCMSDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IFileVersionRepository, FileVersionRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IWorkflowRepository, WorkflowRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<ISystemSettingRepository, SystemSettingRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<IFileVersionService, FileVersionService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IWorkflowService, WorkflowService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<SftpUtility>();

// Register ActiveDirectoryUtility
builder.Services.AddSingleton<ActiveDirectoryUtility>(sp =>
    new ActiveDirectoryUtility(builder.Configuration["ActiveDirectorySettings:DomainName"]));

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseHttpsRedirection();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();