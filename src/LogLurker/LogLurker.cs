using System.Text;
using Timer = System.Timers.Timer;

namespace LogLurker
{
    public class LogLurker : IDisposable
    {
        #region Fields

        private static readonly int DefaultInterval = 300;
        private string _lastLine;
        private CancellationTokenSource _tokenSource;
        private int _interval;

        #endregion

        #region Constructors

        public LogLurker(string filePath)
            : this(filePath, DefaultInterval)
        {
        }

        public LogLurker(string filePath, int interval)
        {
            FilePath = filePath;
            _interval = interval;
            _tokenSource = new CancellationTokenSource();
        }

        #endregion

        #region Properties

        public string FilePath { get; init; }

        #endregion

        #region Events

        public event EventHandler<string> NewLine;

        #endregion

        #region Methods

        public async Task Lurk()
        {
            _tokenSource = new CancellationTokenSource();
            _lastLine = GetLastLine();

            var token = _tokenSource.Token;
            while (true)
            {
                IEnumerable<string> newLines;
                do
                {
                    await Task.Delay(_interval);
                    newLines = GetNewLines();
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                }
                while (newLines.Count() == 0);

                _lastLine = newLines.First();
                foreach (var line in newLines.Reverse())
                {
                    NewLine?.Invoke(NewLine, line);
                }
            }
        }

        public void Stop()
        {
            _tokenSource.Cancel();
            _tokenSource.Dispose();
            _tokenSource = null;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_tokenSource != null)
                {
                    _tokenSource.Cancel();
                    _tokenSource.Dispose();
                }
            }
        }

        private IEnumerable<string> GetNewLines()
        {
            using var stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            if (stream.Length == 0)
            {
                return Enumerable.Empty<string>();
            }

            stream.Position = stream.Length - 1;

            // Last Line
            var currentNewLine = PreviousLine(stream);
            var newLines = new List<string>();

            while (_lastLine != currentNewLine)
            {
                newLines.Add(currentNewLine);
                currentNewLine = PreviousLine(stream);
            }

            newLines.Reverse();
            return newLines;
        }

        private string GetLastLine()
        {
            using (var stream = new FileStream(this.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (stream.Length == 0)
                {
                    return string.Empty;
                }

                stream.Position = stream.Length - 1;

                return PreviousLine(stream);
            }
        }

        private static string PreviousLine(Stream stream)
        {
            var lineLength = 0;
            while (stream.Position > 0)
            {
                stream.Position--;
                var byteFromFile = stream.ReadByte();

                if (byteFromFile < 0)
                {
                    throw new IOException("Error reading from file");
                }
                else if (byteFromFile == '\n')
                {
                    break;
                }

                lineLength++;
                stream.Position--;
            }

            var oldPosition = stream.Position;
            var bytes = new BinaryReader(stream).ReadBytes(lineLength - 1);

            stream.Position = oldPosition - 1;
            return Encoding.UTF8.GetString(bytes).Replace(Environment.NewLine, string.Empty);
        }

        #endregion
    }
}