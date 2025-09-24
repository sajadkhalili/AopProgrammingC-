namespace WebApplication7.Controllers;

public class DatabaseLogHandler : ILogHandler
{
    public void Handle ( string className, string methodName, string callerClass, string callerMethod )
    {
        // ????? ?? ??????? (EF Core / Dapper)
        Console.WriteLine($"[DB] Log: {className}.{methodName} <- {callerClass}.{callerMethod}");
    }
}
