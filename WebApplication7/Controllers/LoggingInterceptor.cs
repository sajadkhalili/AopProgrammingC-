using Castle.DynamicProxy;
using System.Diagnostics;
using System.Reflection;

namespace WebApplication7.Controllers;
using Castle.DynamicProxy;
using System.Diagnostics;
using System.Text.Json;

public class LoggingInterceptor : IInterceptor
{
   // private readonly LoggingDbContext _db;
    private readonly LoggingOptions _options;

    public LoggingInterceptor (/* LoggingDbContext db,*/ LoggingOptions options )
    {
      //  _db=db;
        _options=options;
    }

    public void Intercept ( IInvocation invocation )
    {
        var start = DateTime.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        var argsJson = JsonSerializer.Serialize(invocation.Arguments);
        var caller = _options.EnableCallerTracking ? GetCaller() : new Caller();
        //"N/A";

        invocation.Proceed();

        if (invocation.Method.ReturnType==typeof(Task))
        {
            invocation.ReturnValue=InterceptAsync((Task)invocation.ReturnValue,
                invocation, argsJson, caller, start, stopwatch);
        }
        else if (invocation.Method.ReturnType.IsGenericType&&
                 invocation.Method.ReturnType.GetGenericTypeDefinition()==typeof(Task<>))
        {
            var resultType = invocation.Method.ReturnType.GenericTypeArguments[0];
            var method = GetType()
                .GetMethod(nameof(InterceptAsyncGeneric), System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)
                .MakeGenericMethod(resultType);
            invocation.ReturnValue=method.Invoke(this, new object[] { invocation, argsJson, caller, start, stopwatch });
        }
        else
        {
            stopwatch.Stop();
            SaveLog(invocation.Method.Name, argsJson, invocation.ReturnValue, caller, start, DateTime.UtcNow, stopwatch.Elapsed);
        }
    }

    private async Task InterceptAsync ( Task task, IInvocation invocation, string argsJson, Caller caller, DateTime start, Stopwatch stopwatch )
    {
        await task.ConfigureAwait(false);
        stopwatch.Stop();
        SaveLog(invocation.Method.Name, argsJson, null, caller, start, DateTime.UtcNow, stopwatch.Elapsed);
    }

    private async Task<T> InterceptAsyncGeneric<T> ( IInvocation invocation, string argsJson, Caller caller, DateTime start, Stopwatch stopwatch )
    {
        var task = (Task<T>)invocation.ReturnValue;
        var result = await task.ConfigureAwait(false);
        stopwatch.Stop();
        SaveLog(invocation.Method.Name, argsJson, result, caller, start, DateTime.UtcNow, stopwatch.Elapsed);
        return result;
    }

    private void SaveLog ( string methodName, string argsJson, object result, Caller caller, DateTime start, DateTime end, TimeSpan duration )
    {
        var log = new MethodLog
        {
            MethodName=methodName,
            Caller=caller,
            RequestJson=argsJson,
            ResponseJson=JsonSerializer.Serialize(result),
            StartTime=start,
            EndTime=end,
            DurationMs=duration.TotalMilliseconds
        };

        //_db.MethodLogs.Add(log);
        //_db.SaveChanges();

        Console.WriteLine($"📝 {methodName} called by {caller.ClassName} {caller.MethodName} in {log.DurationMs} ms");
    }

    private Caller GetCaller ()
    {
        var stack = new StackTrace();
        var ew = stack.GetFrames();
        var frame = stack.GetFrames()
            .Skip(3)
            .FirstOrDefault(f =>
                f.GetMethod().DeclaringType!=null&&
                !f.GetMethod().DeclaringType.FullName.StartsWith("Castle"));

        return new Caller()
        {
            ClassName =frame?.GetMethod().DeclaringType?.Name,
                MethodName =frame?.GetMethod().Name

        };

     //   return $"{frame?.GetMethod().DeclaringType?.Name}.{frame?.GetMethod().Name}";
    }
}

public class LoggingInterceptor2 : IInterceptor
    {
        private readonly ILogHandler _logHandler;

        public LoggingInterceptor2 ( ILogHandler logHandler )
        {
            _logHandler=logHandler;
        }

        public void Intercept ( IInvocation invocation )
        {
            var className = invocation.TargetType.Name;
            var methodName = invocation.Method.Name;

            var stack = new StackTrace();
            var callerFrame = stack.GetFrame(2);
            var callerMethod = callerFrame?.GetMethod();

            var callerClass = callerMethod?.DeclaringType?.Name??"UnknownClass";
            var callerMethodName = callerMethod?.Name??"UnknownMethod";

            _logHandler.Handle(className, methodName, callerClass, callerMethodName);

            invocation.Proceed();
        }

}

