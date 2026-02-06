using Hangfire;
using CorporateKnowledgeBase.Web.Filters;
using CorporateKnowledgeBase.Application.Common.Behaviors;
using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Infrastructure.Data;
using CorporateKnowledgeBase.Infrastructure.Identity;
using CorporateKnowledgeBase.Infrastructure.Services;
using CorporateKnowledgeBase.Infrastructure.Hubs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.AI;
using OllamaSharp;


namespace CorporateKnowledgeBase.Web;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // EF Core + SQL Server
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        // Identity
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 6;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // Cookie settings
        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.AccessDeniedPath = "/Account/Login";
        });

        // UserNameResolver DI
        builder.Services.AddScoped<IUserNameResolver, UserNameResolver>();

        // Authorization Checker DI
        builder.Services.AddScoped<IAuthorizationChecker, AuthorizationChecker>();

        // Wiki Link Resolver DI
        builder.Services.AddScoped<CorporateKnowledgeBase.Application.Common.Helpers.IWikiLinkResolver,
            CorporateKnowledgeBase.Application.Common.Helpers.WikiLinkResolver>();

        // Markdown Renderer DI
        builder.Services.AddSingleton<CorporateKnowledgeBase.Application.Common.Helpers.IMarkdownRenderer,
            CorporateKnowledgeBase.Application.Common.Helpers.MarkdownRenderer>();


        // IApplicationDbContext DI
        builder.Services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<ApplicationDbContext>());

        // PDF Export Service
        builder.Services.AddScoped<IPdfExportService, PdfExportService>();


        // HttpContextAccessor
        builder.Services.AddHttpContextAccessor();

        // SignalR
        builder.Services.AddSignalR();

        // Hangfire
        builder.Services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
        builder.Services.AddHangfireServer();
        builder.Services.AddScoped<IBackgroundJobService, BackgroundJobService>();


        // Notification Service
        builder.Services.AddScoped<INotificationService, NotificationService>();

        // Ollama AI — Local LLM (default: llama3.2 3B, embedding: nomic-embed-text)
        // Alternative models: phi3, llama3.2:1b (lightweight), mistral, gemma2
        var ollamaUri = new Uri("http://localhost:11434");
        builder.Services.AddChatClient(new OllamaApiClient(ollamaUri, "llama3.2"));
        builder.Services.AddEmbeddingGenerator(
            new OllamaApiClient(ollamaUri, "nomic-embed-text"));
        builder.Services.AddScoped<IAIAssistantService, AIAssistantService>();



        // MediatR
        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(
                typeof(IApplicationDbContext).Assembly,
                typeof(ApplicationDbContext).Assembly);
            cfg.AddOpenBehavior(typeof(AuditLogBehavior<,>));
        });



        builder.Services.AddControllersWithViews()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(
                    new System.Text.Json.Serialization.JsonStringEnumConverter());
            });

        var app = builder.Build();

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

        // SignalR Hub
        app.MapHub<NotificationHub>("/notificationHub");


        // Hangfire Dashboard (Admin only)
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = [new HangfireAuthorizationFilter()]
        });

        // Recurring Jobs
        RecurringJob.AddOrUpdate<IBackgroundJobService>(
            "clean-old-audit-logs",
            service => service.CleanOldAuditLogsAsync(90),
            Cron.Daily);

        RecurringJob.AddOrUpdate<IBackgroundJobService>(
            "clean-old-notifications",
            service => service.CleanOldNotificationsAsync(30),
            Cron.Daily);


        // Seed Data
        using (var scope = app.Services.CreateScope())
        {
            await SeedData.InitializeAsync(scope.ServiceProvider);
        }


        app.Run();
    }
}
