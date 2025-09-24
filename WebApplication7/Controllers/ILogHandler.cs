namespace WebApplication7.Controllers;

public interface ILogHandler
{
    void Handle ( string className, string methodName, string callerClass, string callerMethod );
}

