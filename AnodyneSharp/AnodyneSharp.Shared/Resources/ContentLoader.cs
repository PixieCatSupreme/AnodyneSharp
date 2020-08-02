using AnodyneSharp.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AnodyneSharp.Resources
{
    public class ContentLoader : IDisposable
    {
        public string FilePath { get; protected set; }

        protected string[] CurrentLine { get; private set; }
        protected int LineNumber { get; private set; }

        protected bool EndOfStream
        {
            get
            {
                return _reader.EndOfStream;
            }
        }

        private StreamReader _reader;
        private Stream _stream;

        internal protected ContentLoader(string filePath)
        {
            LineNumber = 0;
            FilePath = filePath;
            SetStreamreader(filePath);
        }

        public virtual void Dispose()
        {
            _reader.Dispose();
            _stream.Dispose();
        }

        protected void SetStreamreader(string path)
        {
            _stream = new FileStream(path, FileMode.OpenOrCreate);

            if (_stream != null)
            {
                _reader = new StreamReader(_stream);
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

        protected string[] SplitNextLine()
        {
            if (!_reader.EndOfStream)
            {
                do
                {
                    string line = _reader.ReadLine().Trim();
                    LineNumber++;

                    if (!line.StartsWith("//") && !string.IsNullOrWhiteSpace(line))
                    {
                        CurrentLine = line.Split('\t');
                        return CurrentLine;
                    }
                }
                while (!_reader.EndOfStream);
                ThrowFileWarning($"Reached end of file!");
            }
            else
            {
                ThrowFileError($"Trying to read past end of file!");
            }

            return null;
        }

        private string FormatFileError(string message)
        {
            return $"{message} : {FilePath} {LineNumber}";
        }

    }
}
