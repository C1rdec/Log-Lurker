using System.Text;

namespace LogLurker
{
    public class LogLurker : IDisposable
    {
        #region Fields

        private static readonly int DefaultInterval = 800;
        private string _lastLine;
        private System.Timers.Timer _timer;

        #endregion

        #region Constructors

        public LogLurker(string filePath)
            : this(filePath, DefaultInterval)
        {
        }

        public LogLurker(string filePath, int interval)
        {
            FilePath = filePath;
            _timer = new System.Timers.Timer(interval);
            _timer.Elapsed += Timer_Elapsed;
        }

        #endregion

        #region Properties

        public string FilePath { get; init; }

        #endregion

        #region Events

        public event EventHandler<string> NewLine;

        #endregion

        #region Methods

        public void Lurk()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer.Stop();
                _timer.Dispose();
                _timer.Elapsed -= Timer_Elapsed;
            }
        }

        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            IEnumerable<string> newLines;
            do
            {
                newLines = GetNewLines();
            }
            while (newLines.Count() == 0);

            _lastLine = newLines.First();
            foreach (var line in newLines)
            {
                NewLine?.Invoke(this, line);
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

            return newLines.Reverse();
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