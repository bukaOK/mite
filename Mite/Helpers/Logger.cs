using System;
using System.IO;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace Mite.Helpers
{
    public static class Logger
    {
        private const string LogFilePath = "~\\error.log";

        public static void WriteError(Exception e)
        {
            var msg = $"Message: {e.Message}" + Environment.NewLine
             + $"Source: {e.Source}" + Environment.NewLine
                + $"StackTrace: {e.StackTrace}" + Environment.NewLine;
            if(e.InnerException != null)
            {
                msg += $"Inner Exception Message: {e.InnerException.Message}" + Environment.NewLine
                    + $"Inner Exception StackTrace: {e.InnerException.StackTrace}" + Environment.NewLine;
            }
            Write(EventTypes.Error, msg);
        }
        public static Task WriteErrorAsync(Exception e)
        {
            var msg = $"Message: {e.Message}\n"
                + $"Source: {e.Source}"
                + $"Stack Trace: {e.StackTrace}\n"
                + $"Inner Exception Message: {e.InnerException.Message}";
            return WriteAsync(EventTypes.Error, msg);
        }
        public static void Write(EventTypes eventType, string message)
        {
            var logPath = HostingEnvironment.MapPath(LogFilePath);
            var errorStr = Environment.NewLine + $"Event Type: { eventType.ToString()}" + Environment.NewLine
                + $"UTC DateTime: {DateTime.Now}" + Environment.NewLine
                + $"Event Info : " + Environment.NewLine + message + Environment.NewLine;
            using (var writer = File.AppendText(logPath))
            {
                writer.Write(errorStr);
            }
        }
        public static Task WriteAsync(EventTypes eventType, string message)
        {
            var logPath = HostingEnvironment.MapPath(LogFilePath);
            var errorStr = Environment.NewLine + $"Event Type: { eventType.ToString()}" + Environment.NewLine
                + $"UTC DateTime: {DateTime.Now}" + Environment.NewLine
                + $"Event Info : " + Environment.NewLine + message + Environment.NewLine;
            using (var writer = File.AppendText(logPath))
            {
                return writer.WriteAsync(errorStr);
            }
        }
    }
    public enum EventTypes
    {
        Info, Warning, Error
    }
}