```Powershell
Install-Package LogLurker
```

<br/>

```cs
using LogLurker;

var file = new LogFileLurker("[FilePath]");
var t = file.Lurk();

file.NewLine += (sender, newLine) => Console.WriteLine(newLine);
Console.WriteLine("Listening...");
await t;
```
