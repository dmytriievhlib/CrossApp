using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using Core;
using Core.Reports;
using Core.Services;
using Core.Storage;

Console.OutputEncoding = Encoding.UTF8;

var command = args.FirstOrDefault(
    a => !a.StartsWith("--", StringComparison.Ordinal))
    ?? "report-top";

using var cts = new CancellationTokenSource();

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
    Console.WriteLine("\nЗапит на скасування операції...");
};

var service = new CatalogService(StoreFactory.Create(args));
var reports = new ReportService();
var additionalReports = new AdditionalReportService();
var products = service.All().ToList();

try
{
    switch (command.ToLowerInvariant())
    {
        case "import":
            {
                int fileIndex = Array.FindIndex(
                    args,
                    a => string.Equals(
                        a,
                        "--file",
                        StringComparison.OrdinalIgnoreCase));

                if (fileIndex < 0 ||
                    fileIndex + 1 >= args.Length ||
                    args[fileIndex + 1].StartsWith(
                        "--",
                        StringComparison.Ordinal))
                {
                    Console.WriteLine(
                        "Вкажіть шлях до JSON-файлу: import --file шлях");
                    break;
                }

                string path = args[fileIndex + 1];

                int timeoutSeconds = 60;

                int timeoutIndex = Array.FindIndex(
                    args,
                    a => string.Equals(
                        a,
                        "--timeout-seconds",
                        StringComparison.OrdinalIgnoreCase));

                if (timeoutIndex >= 0)
                {
                    if (timeoutIndex + 1 >= args.Length ||
                        !int.TryParse(
                            args[timeoutIndex + 1],
                            out timeoutSeconds) ||
                        timeoutSeconds <= 0)
                    {
                        Console.WriteLine(
                            "Час очікування має бути додатним числом секунд.");
                        break;
                    }
                }

                var importer = new JsonCatalogImporter();

                var progress = new InlineProgress<int>(
                    percentage =>
                    {
                        Console.Write($"\rХід імпорту: {percentage,3}%");
                    });

                Console.WriteLine(
                    $"Імпорт даних із файлу: {path}");

                Console.WriteLine(
                    $"Таймаут: {timeoutSeconds} с");

                int added = await service.ImportAsync(
                    importer,
                    path,
                    cts.Token,
                    progress,
                    TimeSpan.FromSeconds(timeoutSeconds));

                Console.WriteLine();

                Console.WriteLine(
                    $"Імпорт завершено. Додано товарів: {added}");

                Console.WriteLine(
                    $"Усього товарів у каталозі: {service.All().Count}");

                break;
            }

        case "report-top":
            Console.WriteLine("=== ТОП-5 товарів за залишком ===");

            foreach (var item in reports.TopByQuantity(products, 5))
            {
                Console.WriteLine(
                    $"{item.Sku,-10} {item.Name,-32} {item.Quantity,6} {item.Unit}");
            }

            break;

        case "report-low":
            Console.WriteLine("=== Товари із залишком менше 10 ===");

            foreach (var item in reports.LowStock(products, 10))
            {
                Console.WriteLine(
                    $"{item.Sku,-10} {item.Name,-32} {item.Quantity,6} {item.Unit}");
            }

            break;

        case "report-groups":
            Console.WriteLine("=== Групування за одиницями вимірювання ===");

            foreach (var group in reports.ByUnit(products))
            {
                Console.WriteLine(
                    $"{group.Key,-8} товарів: {group.Count,3}; загальна кількість: {group.TotalQuantity}");
            }

            break;

        case "report-sum":
            {
                var totals = reports.Totals(products);

                Console.WriteLine("=== Загальні підсумки ===");
                Console.WriteLine($"Кількість товарів: {totals.Items}");
                Console.WriteLine($"Загальний залишок: {totals.Total}");
                Console.WriteLine($"Середній залишок: {totals.Average:F2}");
                Console.WriteLine($"Максимальний залишок: {totals.Max}");

                break;
            }

        case "report-deferred":
            {
                Console.WriteLine("=== Відкладене виконання LINQ ===");

                var source = service.All().ToList();
                var query = source.Where(p => p.Quantity < 10);

                Console.WriteLine(
                    $"Дефіцитних до змін: {query.Count()}");

                source.Add(Core.Domain.Product.Create(
                    "P-TMP",
                    "SKU-999",
                    "Тимчасовий товар",
                    "шт",
                    3));

                Console.WriteLine(
                    $"Дефіцитних після змін: {query.Count()}");

                var snapshot = query.ToList();
                source.Clear();

                Console.WriteLine(
                    $"У знімку лишилось: {snapshot.Count}");

                break;
            }

        case "report-top-units":
            Console.WriteLine(
                "=== ТОП-3 одиниці за сумарним залишком ===");

            foreach (var group in reports.TopUnits(products, 3))
            {
                Console.WriteLine(
                    $"{group.Key,-8} товарів: {group.Count,3}; загальна кількість: {group.TotalQuantity}");
            }

            break;

        case "report-groups-query":
            Console.WriteLine(
                "=== Групування: синтаксис запитів LINQ ===");

            foreach (var group in reports.ByUnitQuerySyntax(products))
            {
                Console.WriteLine(
                    $"{group.Key,-8} товарів: {group.Count,3}; загальна кількість: {group.TotalQuantity}");
            }

            break;

        case "report-join":
            {
                Console.WriteLine(
                    "=== Товари та їхні переміщення (GroupJoin) ===");

                var movements = new[]
                {
                new StockMovement("SKU-001", "Надходження", 20),
                new StockMovement("SKU-001", "Видача", -5),
                new StockMovement("SKU-003", "Видача", -100),
                new StockMovement("SKU-019", "Надходження", 2)
            };

                foreach (var item in additionalReports.GroupJoinMovements(
                    products,
                    movements))
                {
                    Console.WriteLine(
                        $"{item.Sku,-10} {item.Name,-30} " +
                        $"залишок: {item.CurrentQuantity,6}; " +
                        $"переміщень: {item.MovementCount,2}; " +
                        $"зміна: {item.NetMovement:+0;-0;0}");
                }

                Console.WriteLine(
                    "Примітка: переміщення в цьому звіті є демонстраційними.");

                break;
            }

        case "report-benchmark":
            {
                Console.WriteLine(
                    "=== Stopwatch: 100 000 товарів ===");

                Console.WriteLine(
                    "Середній час за 5 вимірюваннями:");

                foreach (var result in ReportBenchmark.Run())
                {
                    Console.WriteLine(
                        $"{result.Variant}: " +
                        $"{result.AverageMilliseconds:F3} мс; " +
                        $"контрольна сума: {result.Checksum}");
                }

                break;
            }

        default:
            Console.WriteLine("Невідома команда. Доступні команди:");
            Console.WriteLine("  import --file шлях-до-файлу.json");
            Console.WriteLine("  import --file шлях --timeout-seconds 30");
            Console.WriteLine("  report-top");
            Console.WriteLine("  report-low");
            Console.WriteLine("  report-groups");
            Console.WriteLine("  report-sum");
            Console.WriteLine("  report-deferred");
            Console.WriteLine("  report-top-units");
            Console.WriteLine("  report-groups-query");
            Console.WriteLine("  report-join");
            Console.WriteLine("  report-benchmark");

            break;
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("Операцію скасовано.");
}
catch (TimeoutException ex)
{
    Console.WriteLine($"Таймаут імпорту: {ex.Message}");
}
catch (DomainException ex)
{
    Console.WriteLine($"Дані відхилено: {ex.Message}");
}
catch (JsonException ex)
{
    Console.WriteLine($"Помилка формату JSON: {ex.Message}");
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"Файл не знайдено: {ex.FileName}");
}
catch (UnauthorizedAccessException ex)
{
    Console.WriteLine($"Немає доступу до файлу: {ex.Message}");
}
catch (IOException ex)
{
    Console.WriteLine($"Помилка введення-виведення: {ex.Message}");
}

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