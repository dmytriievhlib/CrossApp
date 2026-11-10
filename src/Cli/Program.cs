
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using Core;
using Core.Abstractions;
using Core.Configuration;
using Core.Reports;
using Core.Services;
using Core.Storage;
using Microsoft.Extensions.Configuration;

// --------------------------------------------------
// 1. Початкові налаштування
// --------------------------------------------------

Console.OutputEncoding = Encoding.UTF8;

string baseDirectory = AppContext.BaseDirectory;

string environmentName =
    Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
    ?? "Development";

// --------------------------------------------------
// 2. Підготовка аргументів конфігурації
// --------------------------------------------------

// Перетворюємо --data-path=... на повний ключ
// конфігурації Storage:DataPath.

string[] configurationArgs = args
    .Where(a => a.Contains('='))
    .Select(a =>
    {
        if (a.StartsWith(
            "--data-path=",
            StringComparison.OrdinalIgnoreCase))
        {
            return "--Storage:DataPath=" +
                   a["--data-path=".Length..];
        }

        return a;
    })
    .ToArray();

// --------------------------------------------------
// 3. Побудова конфігурації
// --------------------------------------------------

var configurationBuilder = new ConfigurationBuilder()
    .SetBasePath(baseDirectory)
    .AddJsonFile(
        "appsettings.json",
        optional: false,
        reloadOnChange: false)
    .AddJsonFile(
        $"appsettings.{environmentName}.json",
        optional: true,
        reloadOnChange: false)
    .AddUserSecrets(
        Assembly.GetExecutingAssembly(),
        optional: true)
    .AddEnvironmentVariables(prefix: "CROSSAPP_");

// Підтримка змінної CROSSAPP_DATA_PATH.
// Вона має нижчий пріоритет, ніж аргументи командного рядка.

string? legacyDataPath =
    Environment.GetEnvironmentVariable("CROSSAPP_DATA_PATH");

if (legacyDataPath is not null)
{
    configurationBuilder.AddInMemoryCollection(
        new Dictionary<string, string?>
        {
            ["Storage:DataPath"] = legacyDataPath
        });
}

// Аргументи командного рядка мають найвищий пріоритет.

var config = configurationBuilder
    .AddCommandLine(configurationArgs)
    .Build();

AppOptions options =
    config.Get<AppOptions>() ?? new AppOptions();

// --------------------------------------------------
// 4. Валідація конфігурації
// --------------------------------------------------

if (string.IsNullOrWhiteSpace(options.Storage.DataPath))
{
    Console.WriteLine(
        "Помилка конфігурації: Storage:DataPath не може бути порожнім.");

    Environment.ExitCode = 2;
    return;
}

if (string.IsNullOrWhiteSpace(options.Storage.FileName) ||
    !options.Storage.FileName.EndsWith(
        ".json",
        StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine(
        "Помилка конфігурації: Storage:FileName повинен " +
        "містити назву файлу з розширенням .json.");

    Environment.ExitCode = 2;
    return;
}

if (string.IsNullOrWhiteSpace(options.Storage.Format) ||
    !(options.Storage.Format.Equals(
          "json",
          StringComparison.OrdinalIgnoreCase) ||
      options.Storage.Format.Equals(
          "csv",
          StringComparison.OrdinalIgnoreCase)))
{
    Console.WriteLine(
        "Помилка конфігурації: Storage:Format повинен бути json або csv.");

    Environment.ExitCode = 2;
    return;
}

// --------------------------------------------------
// 5. Відображення інформації про програму
// --------------------------------------------------

string? apiKey = config["Integration:ApiKey"];

Console.WriteLine("====================================");
Console.WriteLine(" CrossApp - каталог товарів");
Console.WriteLine("====================================");

Console.WriteLine($"Середовище: {options.EnvironmentName}");

Console.WriteLine(
    $"API-ключ: {(string.IsNullOrWhiteSpace(apiKey) ? "не задано" : "налаштовано")}");

// --------------------------------------------------
// 6. Створення сховища та сервісів
// --------------------------------------------------

var store = StoreFactory.Create(
    args,
    options,
    baseDirectory);

var service = new CatalogService(store);

var reportService = new ReportService();

// --------------------------------------------------
// 7. Визначення команди
// --------------------------------------------------

string command = args.FirstOrDefault(
    a => !a.StartsWith("--", StringComparison.Ordinal))
    ?? "report-top";

// --------------------------------------------------
// 8. Отримання значень аргументів
// --------------------------------------------------

string? GetArgumentValue(string name)
{
    for (int i = 0; i < args.Length - 1; i++)
    {
        if (string.Equals(
            args[i],
            name,
            StringComparison.OrdinalIgnoreCase))
        {
            return args[i + 1];
        }
    }

    return null;
}

// --------------------------------------------------
// 9. Скасування операції через Ctrl+C
// --------------------------------------------------

using var cts = new CancellationTokenSource();

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();

    Console.WriteLine(
        "\nЗапит на скасування операції...");
};

// --------------------------------------------------
// 10. Виконання команд
// --------------------------------------------------

try
{
    switch (command.ToLowerInvariant())
    {
        // ==========================================
        // ЕКСПОРТ КАТАЛОГУ
        // ==========================================

        case "export":
            {
                ICatalogExporter exporter =
                    CatalogExporterFactory.Create(
                        options.Storage.Format);

                string exportPath =
                    options.Storage.ResolveExportPath(baseDirectory);

                exporter.Export(
                    service.All(),
                    exportPath);

                Console.WriteLine();
                Console.WriteLine(
                    "Експорт каталогу успішно завершено.");

                Console.WriteLine(
                    $"Формат: {options.Storage.Format}");

                Console.WriteLine(
                    $"Файл: {exportPath}");

                break;
            }

        // ==========================================
        // ІМПОРТ JSON
        // ==========================================

        case "import":
            {
                string path =
                    GetArgumentValue("--input")
                    ?? "data/products.json";

                int timeoutSeconds = 60;

                string? timeoutArgument =
                    GetArgumentValue("--timeout-seconds");

                if (timeoutArgument is not null &&
                    !int.TryParse(
                        timeoutArgument,
                        out timeoutSeconds))
                {
                    Console.WriteLine(
                        "Некоректне значення таймауту.");

                    break;
                }

                if (timeoutSeconds <= 0)
                {
                    Console.WriteLine(
                        "Таймаут повинен бути більшим за нуль.");

                    break;
                }

                var importer = new JsonCatalogImporter();

                var progress = new InlineProgress<int>(
                    value =>
                    {
                        Console.Write(
                            $"\rПрогрес імпорту: {value}%   ");
                    });

                Console.WriteLine();
                Console.WriteLine($"Файл імпорту: {path}");
                Console.WriteLine($"Таймаут: {timeoutSeconds} с");

                int imported = await service.ImportAsync(
                    importer,
                    path,
                    cts.Token,
                    progress,
                    TimeSpan.FromSeconds(timeoutSeconds));

                Console.WriteLine();
                Console.WriteLine(
                    $"Додано записів: {imported}");

                Console.WriteLine(
                    $"Усього записів у каталозі: {service.All().Count}");

                break;
            }

        // ==========================================
        // ТОП ТОВАРІВ ЗА КІЛЬКІСТЮ
        // ==========================================

        case "report-top":
            {
                var result = reportService.TopByQuantity(
                    service.All(),
                    5);

                Console.WriteLine();
                Console.WriteLine("ТОП-5 ТОВАРІВ ЗА КІЛЬКІСТЮ");

                Console.WriteLine(
                    "{0,-15} {1,-25} {2,10} {3,-10}",
                    "SKU",
                    "Назва",
                    "Кількість",
                    "Одиниця");

                foreach (var item in result)
                {
                    Console.WriteLine(
                        "{0,-15} {1,-25} {2,10} {3,-10}",
                        item.Sku,
                        item.Name,
                        item.Quantity,
                        item.Unit);
                }

                break;
            }

        // ==========================================
        // ТОВАРИ З МАЛИМ ЗАЛИШКОМ
        // ==========================================

        case "report-low":
            {
                const int threshold = 10;

                var result = reportService.LowStock(
                    service.All(),
                    threshold);

                Console.WriteLine();
                Console.WriteLine(
                    $"ТОВАРИ З ЗАЛИШКОМ МЕНШЕ {threshold}");

                foreach (var item in result)
                {
                    Console.WriteLine(
                        $"{item.Sku,-15} {item.Name,-25} " +
                        $"{item.Quantity,8} {item.Unit}");
                }

                break;
            }

        // ==========================================
        // ГРУПУВАННЯ ЗА ОДИНИЦЕЮ ВИМІРЮВАННЯ
        // ==========================================

        case "report-groups":
            {
                var result = reportService.ByUnit(
                    service.All());

                Console.WriteLine();
                Console.WriteLine(
                    "ГРУПУВАННЯ ТОВАРІВ ЗА ОДИНИЦЕЮ");

                foreach (var group in result)
                {
                    Console.WriteLine(
                        $"{group.Key,-15} " +
                        $"Записів: {group.Count,5} " +
                        $"Загальна кількість: {group.TotalQuantity,8}");
                }

                break;
            }

        // ==========================================
        // ЗАГАЛЬНА КІЛЬКІСТЬ ТОВАРІВ
        // ==========================================

        case "report-sum":
            {
                var totals = reportService.Totals(
                    service.All());

                Console.WriteLine();
                Console.WriteLine("ЗАГАЛЬНА СТАТИСТИКА");

                Console.WriteLine(
                    $"Кількість записів: {totals.Items}");

                Console.WriteLine(
                    $"Загальна кількість товарів: {totals.Total}");

                Console.WriteLine(
                    $"Середня кількість: {totals.Average:F2}");

                Console.WriteLine(
                    $"Максимальна кількість: {totals.Max}");

                break;
            }

        // ==========================================
        // ВІДКЛАДЕНЕ ВИКОНАННЯ LINQ
        // ==========================================

        case "report-deferred":
            {
                Console.WriteLine();
                Console.WriteLine(
                    "ДЕМОНСТРАЦІЯ ВІДКЛАДЕНОГО ВИКОНАННЯ LINQ");

                var query = service.All()
                    .Where(p => p.Quantity > 0)
                    .OrderBy(p => p.Name);

                Console.WriteLine(
                    "LINQ-запит створено. Він ще не виконався.");

                Console.WriteLine();
                Console.WriteLine(
                    "Результат після початку перебирання:");

                foreach (var product in query)
                {
                    Console.WriteLine(
                        $"{product.Name,-25} " +
                        $"{product.Quantity,8} {product.Unit}");
                }

                break;
            }

        // ==========================================
        // ТОП ОДИНИЦЬ ВИМІРЮВАННЯ
        // ==========================================

        case "report-top-units":
            {
                var result = reportService.TopUnits(
                    service.All(),
                    5);

                Console.WriteLine();
                Console.WriteLine(
                    "ТОП-5 ОДИНИЦЬ ЗА ЗАГАЛЬНОЮ КІЛЬКІСТЮ");

                foreach (var group in result)
                {
                    Console.WriteLine(
                        $"{group.Key,-15} " +
                        $"Записів: {group.Count,5} " +
                        $"Кількість: {group.TotalQuantity,8}");
                }

                break;
            }

        // ==========================================
        // ГРУПУВАННЯ ЗА СИНТАКСИСОМ QUERY
        // ==========================================

        case "report-groups-query":
            {
                var result = reportService.ByUnitQuerySyntax(
                    service.All());

                Console.WriteLine();
                Console.WriteLine(
                    "ГРУПУВАННЯ ЗА ДОПОМОГОЮ QUERY SYNTAX");

                foreach (var group in result)
                {
                    Console.WriteLine(
                        $"{group.Key,-15} " +
                        $"Записів: {group.Count,5} " +
                        $"Кількість: {group.TotalQuantity,8}");
                }

                break;
            }

        // ==========================================
        // JOIN
        // ==========================================

        case "report-join":
            {
                var products = service.All();
                var groups = reportService.ByUnit(products);

                var query =
                    from product in products
                    join unitGroup in groups
                        on product.Unit equals unitGroup.Key
                    select new
                    {
                        product.Sku,
                        product.Name,
                        product.Unit,
                        product.Quantity,
                        UnitTotal = unitGroup.TotalQuantity,
                        UnitCount = unitGroup.Count
                    };

                Console.WriteLine();
                Console.WriteLine(
                    "ОБ'ЄДНАННЯ ДАНИХ ЗА ДОПОМОГОЮ JOIN");

                foreach (var item in query)
                {
                    Console.WriteLine(
                        $"{item.Sku} | {item.Name} | {item.Unit} | " +
                        $"Кількість: {item.Quantity} | " +
                        $"Усього в групі: {item.UnitTotal} | " +
                        $"Товарів у групі: {item.UnitCount}");
                }

                break;
            }

        // ==========================================
        // BENCHMARK
        // ==========================================

        case "report-benchmark":
            {
                var products = service.All();

                var stopwatch = Stopwatch.StartNew();

                var methodResult =
                    reportService.ByUnit(products);

                stopwatch.Stop();

                long methodTime = stopwatch.ElapsedTicks;

                stopwatch.Restart();

                var queryResult =
                    reportService.ByUnitQuerySyntax(products);

                stopwatch.Stop();

                long queryTime = stopwatch.ElapsedTicks;

                Console.WriteLine();
                Console.WriteLine(
                    "ПОРІВНЯННЯ LINQ METHOD SYNTAX І QUERY SYNTAX");

                Console.WriteLine(
                    $"Method syntax: {methodTime} ticks");

                Console.WriteLine(
                    $"Query syntax:  {queryTime} ticks");

                Console.WriteLine(
                    $"Кількість груп Method syntax: {methodResult.Count}");

                Console.WriteLine(
                    $"Кількість груп Query syntax: {queryResult.Count}");

                Console.WriteLine(
                    "Примітка: для малих наборів даних результати " +
                    "часу можуть суттєво коливатися.");

                break;
            }

        // ==========================================
        // НЕВІДОМА КОМАНДА
        // ==========================================

        default:
            {
                Console.WriteLine();
                Console.WriteLine(
                    $"Невідома команда: {command}");

                Console.WriteLine();
                Console.WriteLine("Доступні команди:");

                Console.WriteLine("  report-top");
                Console.WriteLine("  report-low");
                Console.WriteLine("  report-groups");
                Console.WriteLine("  report-sum");
                Console.WriteLine("  report-deferred");
                Console.WriteLine("  report-top-units");
                Console.WriteLine("  report-groups-query");
                Console.WriteLine("  report-join");
                Console.WriteLine("  report-benchmark");
                Console.WriteLine("  import");
                Console.WriteLine("  export");

                Console.WriteLine();
                Console.WriteLine("Параметри:");

                Console.WriteLine("  --file");
                Console.WriteLine("  --input <шлях>");
                Console.WriteLine("  --timeout-seconds <секунди>");
                Console.WriteLine("  --data-path=<шлях>");

                break;
            }
    }
}

// --------------------------------------------------
// 11. Обробка винятків
// --------------------------------------------------

catch (OperationCanceledException)
{
    Console.WriteLine();
    Console.WriteLine("Операцію скасовано.");
}

catch (TimeoutException ex)
{
    Console.WriteLine();
    Console.WriteLine(
        $"Перевищено час очікування: {ex.Message}");
}

catch (DomainException ex)
{
    Console.WriteLine();
    Console.WriteLine(
        $"Помилка предметної області: {ex.Message}");
}

catch (JsonException ex)
{
    Console.WriteLine();
    Console.WriteLine(
        $"Помилка JSON: {ex.Message}");
}

catch (FileNotFoundException ex)
{
    Console.WriteLine();
    Console.WriteLine(
        $"Файл не знайдено: {ex.Message}");
}

catch (UnauthorizedAccessException ex)
{
    Console.WriteLine();
    Console.WriteLine(
        $"Немає доступу до файлу: {ex.Message}");
}

catch (IOException ex)
{
    Console.WriteLine();
    Console.WriteLine(
        $"Помилка введення-виведення: {ex.Message}");
}

catch (ArgumentException ex)
{
    Console.WriteLine();
    Console.WriteLine(
        $"Помилка параметрів: {ex.Message}");
}

// --------------------------------------------------
// 12. Клас відображення прогресу
// --------------------------------------------------

public sealed class InlineProgress<T> : IProgress<T>
{
    private readonly Action<T> _handler;

    public InlineProgress(Action<T> handler)
    {
        _handler = handler;
    }

    public void Report(T value)
    {
        _handler(value);
    }
}
