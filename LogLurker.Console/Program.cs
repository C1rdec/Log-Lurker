using LogLurker;

var lurker = new LogLurker.LogLurker(@"C:\Program Files (x86)\Grinding Gear Games\Path of Exile\logs\Client.txt");
var t = lurker.Lurk();

lurker.NewLine += (sender, newLine) => Console.WriteLine(newLine);
Console.WriteLine("Listening...");
await t;