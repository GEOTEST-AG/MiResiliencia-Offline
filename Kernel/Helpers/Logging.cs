using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ResTB_API.Helpers
{
    public class Logging
    {
        public static Logger getNLogger(string name)
        {
            var logger = NLog.LogManager.GetLogger(name);

            return logger;
        }

        public static void info(string message, string name = "")
        {
            getNLogger(name).Info(message);
        }

        public static void warn(string message, string name = "")
        {
            getNLogger(name).Warn(message);
        }

        public static void error(string message, string name = "")
        {
            getNLogger(name).Error(message);
        }
    }
}