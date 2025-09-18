using Notification.Application.Consumer;
using Notification.Core.Interface;
using Notification.Infrastructure.Service;
using Serilog;
using Shared.Core.Interface;
using Shared.Infrastructure.Service;
using StackExchange.Redis;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "MiniPay.Notification.Service")
    .WriteTo.Console()
    .WriteTo.File(
        path: $"logs/{DateTime.Now:yyyyMMdd-HHmmss}.log",
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);
    
    builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    {
        var configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
        return ConnectionMultiplexer.Connect(configuration);
    });


    builder.Logging.ClearProviders();
    builder.Logging.AddSerilog();
    
    builder.Services.AddScoped<ICacheService, RedisCacheService>();
    builder.Services.AddSingleton<IMessageConsumer, RabbitMqConsumer>();
    builder.Services.AddSingleton<IEmailService, ConsoleEmailService>();
    builder.Services.AddHostedService<EmailVerificationConsumer>();
    builder.Services.AddHostedService<EmailConfirmedConsumer>();
    builder.Services.AddHostedService<TransactionNotificationConsumer>();

    var host = builder.Build();
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}