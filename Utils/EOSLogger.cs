using BepInEx.Logging;
using System;
using System.Globalization;

namespace ExtraObjectiveSetup.Utils
{
    public static class EOSLogger
    {
        private static ManualLogSource logger = Logger.CreateLogSource("ExtraObjectiveSetup");

        private static string Timestamped(string str)
        {
            return "[" + DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture) + "] " + str;
        }

        public static void Log(string format, params object[] args)
        {
            Log(string.Format(format, args));
        }

        public static void Log(string str)
        {
            if (logger == null) return;

            logger.Log(LogLevel.Message, Timestamped(str));
        }

        public static void Warning(string format, params object[] args)
        {
            Warning(string.Format(format, args));
        }

        public static void Warning(string str)
        {
            if (logger == null) return;

            logger.Log(LogLevel.Warning, Timestamped(str));
        }

        public static void Error(string format, params object[] args)
        {
            Error(string.Format(format, args));
        }

        public static void Error(string str)
        {
            if (logger == null) return;

            logger.Log(LogLevel.Error, Timestamped(str));
        }

        public static void Debug(string format, params object[] args)
        {
            Debug(string.Format(format, args));
        }

        public static void Debug(string str)
        {
            if (logger == null) return;

            logger.Log(LogLevel.Debug, Timestamped(str));
        }
    }

}
