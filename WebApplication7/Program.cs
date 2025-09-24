
using Castle.DynamicProxy;
using WebApplication7.Controllers;

namespace WebApplication7
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddScoped<MyClass>();
            // تنظیمات LoggingOptions از config
            var loggingOptions = new LoggingOptions();
            builder.Configuration.GetSection("LoggingInterceptor").Bind(loggingOptions);
            loggingOptions.EnableCallerTracking = true;
            builder.Services.AddSingleton(loggingOptions);

            // ثبت DbContext
            //builder.Services.AddDbContext<LoggingDbContext>(options =>
            //    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // ثبت Castle Proxy + Interceptor
            builder.Services.AddSingleton<ProxyGenerator>();
            builder.Services.AddScoped<LoggingInterceptor>();

            // ثبت سرویس با Proxy
            builder.Services.AddScoped<IUserService>(provider =>
            {
                var proxyGen = provider.GetRequiredService<ProxyGenerator>();
                var target = new UserService();
                var interceptor = provider.GetRequiredService<LoggingInterceptor>();
                return proxyGen.CreateInterfaceProxyWithTarget<IUserService>(target, interceptor);
            });

            //builder.Services.AddSingleton<ProxyGenerator>();
            //builder.Services.AddSingleton<LoggingInterceptor>();
            //builder.Services.AddSingleton<ILogHandler, ConsoleLogHandler>();

            //builder.Services.AddScoped<IUserService>(sp =>
            //{
            //    var generator = sp.GetRequiredService<ProxyGenerator>();
            //    var interceptor = sp.GetRequiredService<LoggingInterceptor>();
            //    var actual = new UserService(); // ?? sp.GetRequiredService<UserService>() ??? ???? ??? DI ????? ???
            //    return generator.CreateInterfaceProxyWithTarget<IUserService>(actual, interceptor);
            //});
            // Add services to the container.
            //   builder.Services.AddInterceptedServices<ConsoleLogHandler>(typeof(UserService));
            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
