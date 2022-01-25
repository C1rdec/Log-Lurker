```cs
using LogLurker;

var lurker = new LogFileLurker("[FilePath]");
var t = lurker.Lurk();

lurker.NewLine += (sender, newLine) => Console.WriteLine(newLine);
Console.WriteLine("Listening...");
await t;
```
