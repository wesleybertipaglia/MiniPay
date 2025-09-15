using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Notification.Application.Consumer;
using Notification.Core.Interface;
using Notification.Infrastructure.Service;
using Shared.Core.Interface;
using Shared.Infrastructure.Service;

Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        Services.AddScoped<ICacheService, RedisCacheService>();
        services.AddSingleton<IMessageConsumer, RabbitMqConsumer>();
        services.AddSingleton<IEmailService, ConsoleEmailService>();
        services.AddHostedService<EmailVerificationConsumer>();
        services.AddHostedService<EmailConfirmedConsumer>();
    })
    .Build()
    .Run();