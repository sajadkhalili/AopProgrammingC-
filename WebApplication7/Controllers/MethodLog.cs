
namespace WebApplication7.Controllers
{
    internal class MethodLog
    {
        public string MethodName { get; set; }
        public Caller Caller { get; set; }
        public string RequestJson { get; set; }
        public string ResponseJson { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double DurationMs { get; set; }
    }

    internal class Caller
    {
        public string ClassName { get; set; }
        public string MethodName { get; set; }
    }
}