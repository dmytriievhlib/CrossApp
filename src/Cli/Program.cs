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

var service = new CatalogService(
    StoreFactory.Create(args));

var reports = new ReportService();

var products = service.All().ToList();

switch (command.ToLowerInvariant())
{
    case "report-top":
        {
            Console.WriteLine("=== ТОП-5 товарів за залишком ===");

            foreach (var item in reports.TopByQuantity(products, 5))
            {
                Console.WriteLine(
                    $"{item.Sku,-10} {item.Name,-32} {item.Quantity,6} {item.Unit}");
            }

            break;
        }

    case "report-low":
        {
            Console.WriteLine("=== Товари із залишком менше 10 ===");

            foreach (var item in reports.LowStock(products, 10))
            {
                Console.WriteLine(
                    $"{item.Sku,-10} {item.Name,-32} {item.Quantity,6} {item.Unit}");
            }

            break;
        }

    case "report-groups":
        {
            Console.WriteLine("=== Групування за одиницями вимірювання ===");

            foreach (var group in reports.ByUnit(products))
            {
                Console.WriteLine(
                    $"{group.Key,-8} товарів: {group.Count,3}; загальна кількість: {group.TotalQuantity}");
            }

            break;
        }

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

            source.Add(
                Core.Domain.Product.Create(
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

    default:
        Console.WriteLine("Невідома команда.");
        Console.WriteLine("Доступні команди:");
        Console.WriteLine("  report-top");
        Console.WriteLine("  report-low");
        Console.WriteLine("  report-groups");
        Console.WriteLine("  report-sum");
        Console.WriteLine("  report-deferred");
        break;
}