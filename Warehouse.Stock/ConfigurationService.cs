namespace Warehouse.Stock;
using MassTransit;
using System.Reflection;
public static class ConfigurationService
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddMassTransit(m=>
        {
            m.AddConsumers(Assembly.GetExecutingAssembly());
             m.UsingRabbitMq((ctx,cfg)=>
             {
                cfg.Host("localhost","/",c=>
                {
                    c.Username("guest");
                    c.Password("guest");
                });
                cfg.ConfigureEndpoints(ctx);
             });
        });
        return services;
    }
}