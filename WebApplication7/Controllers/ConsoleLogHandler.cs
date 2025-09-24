namespace WebApplication7.Controllers;

public class ConsoleLogHandler : ILogHandler
{
    public void Handle ( string className, string methodName, string callerClass, string callerMethod )
    {
        Console.WriteLine($"?? {className}.{methodName} called by {callerClass}.{callerMethod}");
    }
}