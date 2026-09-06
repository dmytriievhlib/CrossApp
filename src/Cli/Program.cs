using System.Runtime.InteropServices;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("CrossApp – практикум з крос-платформного програмування");
Console.WriteLine("Студент: Дмитрієв Гліб Олександрович, група ФЕІ-36");
Console.WriteLine(new string('-', 60));

Console.WriteLine($"ОС (OSDescription)      : {RuntimeInformation.OSDescription}");
Console.WriteLine($"ОС (Environment)        : {Environment.OSVersion}");
Console.WriteLine($"Архітектура процесу     : {RuntimeInformation.ProcessArchitecture}");
Console.WriteLine($"Версія .NET (CLR)       : {Environment.Version}");
Console.WriteLine($"Runtime                 : {RuntimeInformation.FrameworkDescription}");
Console.WriteLine($"Каталог застосунку      : {AppContext.BaseDirectory}");
Console.WriteLine($"Поточний каталог        : {Environment.CurrentDirectory}");

Console.WriteLine(new string('-', 60));
Console.WriteLine("Предметна область: Склад");
Console.WriteLine("Сутності: Product, StockBatch, Warehouse, Movement");
Console.WriteLine("Призначення: облік залишків товарів по партіях.");