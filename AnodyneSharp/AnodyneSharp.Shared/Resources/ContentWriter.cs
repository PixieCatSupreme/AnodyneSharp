using AnodyneSharp.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AnodyneSharp.Resources
{
    public class ContentWriter : IDisposable
    {
        public string FilePath { get; protected set; }

        private StreamWriter _writer;
        private Stream _stream;
        private int _lineNumber;


        public ContentWriter(string filePath)
        {
            _lineNumber = 0;
            FilePath = filePath;
            SetStreamreader(filePath);
        }

        public virtual void Dispose()
        {
            _writer.Dispose();
            _stream.Dispose();
        }

        protected void SetStreamreader(string path)
        {
            _stream = new FileStream(path, FileMode.Create);

            if (_stream != null)
            {
                _writer = new StreamWriter(_stream);
            }
            else
            {
                DebugLogger.AddError($"Unable to read content file: {FilePath}. File does not exist!");
            }
        }

        protected void ThrowFileWarning(string message)
        {
            DebugLogger.AddWarning(FormatFileError(message), false);
        }

        protected void ThrowFileError(string message)
        {
            DebugLogger.AddError(FormatFileError(message), false);
        }
        protected void WriteLine()
        {
            _lineNumber++;
            _writer.WriteLine();
        }

        protected void WriteLine(string line)
        {
            _lineNumber++;
            _writer.WriteLine(line);
        }

        protected void WriteLine(char line)
        {
            _lineNumber++;
            _writer.WriteLine(line);
        }

        protected void WriteLine(int value)
        {
            _lineNumber++;
            _writer.WriteLine(value.ToString());
        }

        protected void Write(string line)
        {
            _writer.Write(line);
        }

        private string FormatFileError(string message)
        {
            return $"{message} : {FilePath} {_lineNumber}";
        }
    }
}
