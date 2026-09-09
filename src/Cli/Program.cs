using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Encodings.Web;

Console.OutputEncoding = System.Text.Encoding.UTF8;

bool jsonMode = args.Contains("--json");

var systemInfo = new
{
    Application = "CrossApp – практикум з крос-платформного програмування",
    Student = "Дмитрієв Гліб Олександрович",
    Group = "ФЕІ-36",
    OSDescription = RuntimeInformation.OSDescription,
    EnvironmentOSVersion = Environment.OSVersion.ToString(),
    ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
    DotNetVersion = Environment.Version.ToString(),
    Runtime = RuntimeInformation.FrameworkDescription,
    ApplicationDirectory = AppContext.BaseDirectory,
    CurrentDirectory = Environment.CurrentDirectory,
    Domain = "Склад",
    Entities = new[]
    {
        "Product",
        "StockBatch",
        "Warehouse",
        "Movement"
    }
};

if (jsonMode)
{
    var options = new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    Console.WriteLine(JsonSerializer.Serialize(systemInfo, options));
    }
else
{
    Console.WriteLine("CrossApp – практикум з крос-платформного програмування");
    Console.WriteLine("Студент: Дмитрієв Гліб Олександрович, група ФЕІ-36");
    Console.WriteLine(new string('-', 60));

    Console.WriteLine($"ОС (OSDescription)      : {systemInfo.OSDescription}");
    Console.WriteLine($"ОС (Environment)        : {systemInfo.EnvironmentOSVersion}");
    Console.WriteLine($"Архітектура процесу     : {systemInfo.ProcessArchitecture}");
    Console.WriteLine($"Версія .NET (CLR)       : {systemInfo.DotNetVersion}");
    Console.WriteLine($"Runtime                 : {systemInfo.Runtime}");
    Console.WriteLine($"Каталог застосунку      : {systemInfo.ApplicationDirectory}");
    Console.WriteLine($"Поточний каталог        : {systemInfo.CurrentDirectory}");

    Console.WriteLine(new string('-', 60));
    Console.WriteLine($"Предметна область: {systemInfo.Domain}");
    Console.WriteLine("Сутності: Product, StockBatch, Warehouse, Movement");
    Console.WriteLine("Призначення: облік залишків товарів по партіях.");
}