using System;
using System.Linq;
using Core;
using Core.Reports;
using Core.Services;
using Core.Storage;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var command = args.FirstOrDefault(
    a => !a.StartsWith("--", StringComparison.Ordinal))
    ?? "report-top";

var service = new CatalogService(StoreFactory.Create(args));
var reports = new ReportService();
var additionalReports = new AdditionalReportService();
var products = service.All().ToList();

switch (command.ToLowerInvariant())
{
    case "report-top":
        Console.WriteLine("=== ТОП-5 товарів за залишком ===");
        foreach (var item in reports.TopByQuantity(products, 5))
            Console.WriteLine(
                $"{item.Sku,-10} {item.Name,-32} {item.Quantity,6} {item.Unit}");
        break;

    case "report-low":
        Console.WriteLine("=== Товари із залишком менше 10 ===");
        foreach (var item in reports.LowStock(products, 10))
            Console.WriteLine(
                $"{item.Sku,-10} {item.Name,-32} {item.Quantity,6} {item.Unit}");
        break;

    case "report-groups":
        Console.WriteLine("=== Групування за одиницями вимірювання ===");
        foreach (var group in reports.ByUnit(products))
            Console.WriteLine(
                $"{group.Key,-8} товарів: {group.Count,3}; загальна кількість: {group.TotalQuantity}");
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

            Console.WriteLine($"Дефіцитних до змін: {query.Count()}");

            source.Add(Core.Domain.Product.Create(
                "P-TMP", "SKU-999", "Тимчасовий товар", "шт", 3));

            Console.WriteLine($"Дефіцитних після змін: {query.Count()}");

            var snapshot = query.ToList();
            source.Clear();
            Console.WriteLine($"У знімку лишилось: {snapshot.Count}");
            break;
        }

    case "report-top-units":
        Console.WriteLine("=== ТОП-3 одиниці за сумарним залишком ===");
        foreach (var group in reports.TopUnits(products, 3))
            Console.WriteLine(
                $"{group.Key,-8} товарів: {group.Count,3}; загальна кількість: {group.TotalQuantity}");
        break;

    case "report-groups-query":
        Console.WriteLine("=== Групування: синтаксис запитів LINQ ===");
        foreach (var group in reports.ByUnitQuerySyntax(products))
            Console.WriteLine(
                $"{group.Key,-8} товарів: {group.Count,3}; загальна кількість: {group.TotalQuantity}");
        break;

    case "report-join":
        {
            Console.WriteLine("=== Товари та їхні переміщення (GroupJoin) ===");

            var movements = new[]
            {
            new StockMovement("SKU-001", "Надходження", 20),
            new StockMovement("SKU-001", "Видача", -5),
            new StockMovement("SKU-003", "Видача", -100),
            new StockMovement("SKU-019", "Надходження", 2)
        };

            foreach (var item in additionalReports.GroupJoinMovements(
                         products, movements))
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
            Console.WriteLine("=== Stopwatch: 100 000 товарів ===");
            Console.WriteLine("Середній час за 5 вимірюваннями:");

            foreach (var result in ReportBenchmark.Run())
                Console.WriteLine(
                    $"{result.Variant}: " +
                    $"{result.AverageMilliseconds:F3} мс; " +
                    $"контрольна сума: {result.Checksum}");

            break;
        }

    default:
        Console.WriteLine("Невідома команда. Доступні команди:");
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
