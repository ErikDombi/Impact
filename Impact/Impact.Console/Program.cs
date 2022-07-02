using Impact;
using Impact.Console;
using Impact.Logging;

ImpactServer server = new ServerBuilder()
    .WithPort(8080)
    .WithLogLevel(LogLevel.Trace)
    .BuildAndStart();

Console.ReadLine();