using Scrutor;
using System.Reflection;
using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;

namespace WebApplication7.Controllers
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInterceptedServices<TLogHandler> (
            this IServiceCollection services,
            params Type[] assemblyMarkerTypes )
            where TLogHandler : class, ILogHandler
        {
            services.AddSingleton<ProxyGenerator>();
            services.AddTransient<LoggingInterceptor>();
            services.AddTransient<ILogHandler, TLogHandler>();

            foreach (var marker in assemblyMarkerTypes)
            {
                // رجیستر کردن کلاس‌های Service به صورت عادی
                services.Scan(scan => scan
                    .FromAssemblies(marker.Assembly)
                    .AddClasses(classes => classes.Where(c => c.Name.EndsWith("UserService")))
                    .AsImplementedInterfaces()
                    .WithTransientLifetime()
                );
            }

            // دکوریت کردن همه اینترفیس‌های Service با ProxyGenerator
            services.DecorateInterfacesWithProxy<LoggingInterceptor>();

            return services;
        }

        // متد کمکی برای دکوریت کردن همه Interface ها با Proxy
        private static void DecorateInterfacesWithProxy<TInterceptor> ( this IServiceCollection services )
            where TInterceptor : class, IInterceptor
        {
            var proxyGeneratorDescriptor = services.FirstOrDefault(d => d.ServiceType==typeof(ProxyGenerator));
            if (proxyGeneratorDescriptor==null)
                throw new InvalidOperationException("ProxyGenerator must be registered");

            var interceptorDescriptor = services.FirstOrDefault(d => d.ServiceType==typeof(TInterceptor));
            if (interceptorDescriptor==null)
                throw new InvalidOperationException($"{typeof(TInterceptor).Name} must be registered");

            // ثبت decorator برای هر سرویس Interface
            var descriptors = services.Where(d => d.ServiceType.IsInterface&&d.ImplementationType!=null).ToList();

            foreach (var descriptor in descriptors)
            {
                services.Decorate(descriptor.ServiceType, ( inner, provider ) =>
                {
                    var generator = provider.GetRequiredService<ProxyGenerator>();
                    var interceptor = provider.GetRequiredService<TInterceptor>();

                    return generator.CreateInterfaceProxyWithTarget(descriptor.ServiceType, inner, interceptor);
                });
            }
        }
    }
}
