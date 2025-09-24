using Microsoft.AspNetCore.Mvc;

namespace WebApplication7.Controllers;

public class  MyClass
{
    private readonly IUserService service;

    public MyClass ( IUserService service )
    {
        this.service=service;
    }
    public void sajad()
    {

        service.Register("s");
    }
}
[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };
    private readonly MyClass myClass;
    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IUserService service;

    public WeatherForecastController( MyClass myClass,ILogger<WeatherForecastController> logger, IUserService service)
    {
        this.myClass=myClass;
        _logger = logger;
        this.service=service;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {

        myClass.sajad();
        service.Register("s");
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }
}
