```
using LogLurker;

var lurker = new LogFileLurker("FileName");
lurker.Lurk();

lurker.NewLine += (sender, newLine) => Console.WriteLine(newLine);
```
