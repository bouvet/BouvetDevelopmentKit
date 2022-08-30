using System;
using UnityEngine;

namespace Bouvet.DevelopmentKit.Internal.Utils
{
    /// <summary>
    /// Logger that globally allows logging of errors and information.
    /// </summary>
    public static class BdkLogger
    {
        public static bool LogToConsole = true;
        private static LogSeverity logSeverity = LogSeverity.Verbose;
        public static event Action<string, LogSeverity> LogMessageEvent;
        public static event Action<string, Exception> LogExceptionEvent;

        public static void SetLogSeverity(LogSeverity severity)
        {
            logSeverity = severity;
        }

        public static LogSeverity GetLogSeverity()
        {
            return logSeverity;
        }

        public static bool WillLog(LogSeverity severity)
        {
            return logSeverity <= severity;
        }

        public static void LogException(string message, Exception exception)
        {
            LogExceptionEvent?.Invoke(message, exception);
            if (!LogToConsole)
            {
                return;
            }

#if !UNITY_EDITOR
            Debug.Log($"{message}\n{exception.Message}\n{exception.StackTrace}");
#else
            Debug.LogError(ColorCodeLog($"{message}\n{exception.Message}\n{exception.StackTrace}", LogSeverity.Error, false));
#endif
        }

        public static void Log(string message)
        {
            Log(message, logSeverity);
        }

        public static void Log(string message, LogSeverity severity)
        {
            if (severity < logSeverity)
            {
                return;
            }

            LogMessageEvent?.Invoke(message, severity);
            if (!LogToConsole)
            {
                return;
            }

#if !UNITY_EDITOR
            Debug.Log(message);
#else
            string colorMessage = ColorCodeLog(message, severity, false);

            if (severity == LogSeverity.Warn)
            {
                Debug.LogWarning(colorMessage);
            }
            else if (severity < LogSeverity.Error || severity == LogSeverity.Custom)
            {
                Debug.Log(colorMessage);
            }
            else
            {
                Debug.LogError(colorMessage);
            }
#endif
        }

        public static string ColorCodeLog(string message, LogSeverity severity, bool textMeshPro)
        {
            switch (severity)
            {
                case LogSeverity.Verbose:
                    return $"<color=white>{message}</color>";
                case LogSeverity.Info:
                    return $"<{(textMeshPro ? "#00FFF1" : "color=cyan")}>{message}</color>";
                case LogSeverity.Warn:
                    return $"<color=yellow>{message}</color>";
                case LogSeverity.Error:
                case LogSeverity.Critical:
                    return $"<color=red>{message}</color>";
                case LogSeverity.Custom:
                    return $"<{(textMeshPro ? "#00FF00" : "color=lime")}>{message}</color>";
                default:
                    return message;
            }
        }

        private static string GetTabulator(LogSeverity severity)
        {
            switch (severity)
            {
                case LogSeverity.Info:
#if !UNITY_EDITOR
                    return $"{severity}\t\t\t";
#else
                    return $"{severity}\t\t";
#endif
                case LogSeverity.Error:
                case LogSeverity.Verbose:
                case LogSeverity.Warn:
                case LogSeverity.Custom:
#if !UNITY_EDITOR
                    return $"{severity}\t\t";
#else
                    return $"{severity}\t";
#endif
                case LogSeverity.Critical:
                    return $"{severity}\t";
                default:
                    return "";
            }
        }
    }

    public enum LogSeverity
    {
        Verbose = 0,
        Info = 1,
        Warn = 2,
        Error = 3,
        Critical = 4,
        Custom = 5,
        None = 6
    }
}