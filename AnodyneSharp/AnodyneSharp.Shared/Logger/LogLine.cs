namespace AnodyneSharp.Logging
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Critical
    }
    public struct LogLine
    {
        public LogLevel LogLevel { get; private set; }
        public string Message { get; internal set; }

        public LogLine(LogLevel logLevel, string message)
        {
            LogLevel = logLevel;
            Message = message;
        }
    }
}
