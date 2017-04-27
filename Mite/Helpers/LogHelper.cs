using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace Mite.Helpers
{
    public static class LogHelper
    {
        public static void WriteException(Exception exception)
        {
            EventLog.WriteEntry("Mite", exception.StackTrace);
        }
    }
}