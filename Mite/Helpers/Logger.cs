using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Mite.Helpers
{
    public static class Logger
    {
        private const string SourceName = "Mite";
        private const string LogName = "Application";
        private const int EventId = 1309;

        public static void WriteError(Exception exception)
        {
            var log = new EventLog();
            var message = $"Event Message: {exception.Message}\nEvent time: {DateTime.Now}\nEventTimeUtc: {DateTime.UtcNow}\nStackTrace:\n";
            message += exception.StackTrace;
            EventLog.WriteEntry(SourceName, message, EventLogEntryType.Warning, EventId);
        }
        public static void WriteError(string message)
        {
            EventLog.WriteEntry(SourceName, message, EventLogEntryType.Warning, EventId);
        }
        /// <summary>
        /// Пишет в лог несколько сообщений
        /// </summary>
        /// <param name="source">Источник или место ошибки</param>
        /// <param name="messages"></param>
        public static void WriteErrors(string source, IEnumerable<string> messages)
        {
            var errors = new StringBuilder($"Ошибка в {source}\n");
            foreach(var message in messages)
            {
                errors.Append(message).Append("\n");
            }
            EventLog.WriteEntry(SourceName, errors.ToString());
        }
        public static void WriteInfo(string message)
        {
            EventLog.WriteEntry(SourceName, message, EventLogEntryType.Information, EventId);
        }
    }
}