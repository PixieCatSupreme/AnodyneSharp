using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using AnodyneSharp.Registry;

namespace AnodyneSharp.Logging
{
    public static class DebugLogger
    {
        private static readonly string LogPath = $"{GameConstants.SavePath}game.log";

        private static Queue<LogLine> DebugLog;
        private static readonly int MaxLogs;

        static DebugLogger()
        {
            DebugLog = new Queue<LogLine>(2);
            MaxLogs = 10;

            if (File.Exists(LogPath))
            {
                File.Delete(LogPath);
            }
        }

        public static void AddLog(LogLine logLine, bool showStack)
        {
            AddLine(logLine, showStack != true ? logLine.LogLevel <= LogLevel.Error : true);
        }

        [Conditional("DEBUG")]
        public static void AddDebug(string message, bool showStack = false)
        {
            AddLine(new LogLine(LogLevel.Debug, message), showStack);
        }

        public static void AddInfo(string message, bool showStack = false)
        {
            AddLine(new LogLine(LogLevel.Info, message), showStack);
        }

        public static void AddWarning(string message, bool showStack = false)
        {
            AddLine(new LogLine(LogLevel.Warning, message), showStack);
        }

        public static void AddError(string message, bool showStack = true)
        {
            AddLine(new LogLine(LogLevel.Error, message), showStack);
        }

        public static void AddCritical(string message, bool showStack = true)
        {
            AddLine(new LogLine(LogLevel.Critical, message), showStack);
        }

        public static void AddException(Exception exception)
        {
            string message = exception.ToString();

            if (exception.InnerException != null)
            {
                message += exception.InnerException.ToString();
            }

            AddLine(new LogLine(LogLevel.Error, message), false);
        }

        public static LogLine Read()
        {
            return DebugLog.Last();
        }
        public static LogLine Read(LogLevel logLevel)
        {
            return DebugLog.Where(l => l.LogLevel == logLevel).LastOrDefault();
        }

        public static LogLine Read(int index)
        {
            return DebugLog.ElementAtOrDefault(index);
        }

        public static LogLine Read(LogLevel logLevel, int index)
        {
            return DebugLog.Where(l => l.LogLevel == logLevel).ElementAtOrDefault(index);
        }

        private static void AddLine(LogLine logLine, bool showStack)
        {
            if (DebugLog.Count == MaxLogs)
            {
                DebugLog.Dequeue();
            }

            DebugLog.Enqueue(logLine);

#if DEBUG
            Debug.WriteLine(logLine.Message);
#endif

            var assembly = Assembly.GetCallingAssembly();

            using (StreamWriter writer = new StreamWriter(new FileStream(LogPath, FileMode.Append)))
            {
                if (showStack)
                {
                    AddStackFrame(ref logLine);
                }

                FormatLine(ref logLine);
                writer.WriteLine(logLine.Message);
                writer.Close();
            }
        }

        private static void AddStackFrame(ref LogLine logLine)
        {
            StackFrame stackTrace = new StackTrace().GetFrame(3);
            MethodBase method = stackTrace.GetMethod();

            StringBuilder parameters = new StringBuilder();
            ParameterInfo[] parameterInfo = method.GetParameters();

            for (int i = 0; i < parameterInfo.Length;)
            {
                ParameterInfo info = parameterInfo[i];

                parameters.Append(info.ParameterType.Name);
                parameters.Append(' ');
                parameters.Append(info.Name);

                i++;
                if (i < parameterInfo.Length)
                {
                    parameters.Append(", ");
                }
            }

            logLine.Message += $"\t{method.Name}({parameters})";
        }

        private static void FormatLine(ref LogLine logLine)
        {
            logLine.Message = $"{DateTime.Now.ToString("hh:mm:ss.ff")} | {Enum.GetName(logLine.LogLevel.GetType(), logLine.LogLevel)} | {logLine.Message}";
        }
    }
}
