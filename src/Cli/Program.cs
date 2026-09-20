using Core.Dto;
using Core.Import;

Console.OutputEncoding = System.Text.Encoding.UTF8;

string path = args.Length > 0
    ? args[0]
    : Path.Combine("data", "sample.csv");

if (!File.Exists(path))
{
    Console.WriteLine(
        $"Файл не знайдено: {Path.GetFullPath(path)}");

    return 1;
}

string extension = Path.GetExtension(path).ToLowerInvariant();

switch (extension)
{
    case ".csv" when Path.GetFileName(path)
        .Equals("mixed.csv", StringComparison.OrdinalIgnoreCase):

        RunMixedCsvImport(path);
        break;

    case ".csv":

        RunCsvImport(path);
        break;

    case ".json":

        RunJsonImport(path);
        break;

    default:

        Console.WriteLine(
            $"Непідтримуване розширення файлу: {extension}");

        Console.WriteLine(
            "Підтримуються: .csv та .json");

        return 1;
}

return 0;


static void RunCsvImport(string path)
{
    try
    {
        var result = ProductCsvImporter.Load(path);

        Console.WriteLine("CrossApp – імпорт товарів CSV");
        Console.WriteLine(new string('-', 70));

        PrintProducts(result.Items);

        PrintStatistics(
            total: result.Items.Count + result.Errors.Count,
            accepted: result.Items.Count,
            skipped: result.Errors.Count);

        PrintErrors(result.Errors);
    }
    catch (Exception ex)
    {
        Console.WriteLine(
            $"Помилка читання CSV: {ex.Message}");
    }
}


static void RunJsonImport(string path)
{
    var result = ProductJsonImporter.Load(path);

    Console.WriteLine("CrossApp – імпорт товарів JSON");
    Console.WriteLine(new string('-', 70));

    PrintProducts(result.Items);

    PrintStatistics(
        total: result.Items.Count + result.Errors.Count,
        accepted: result.Items.Count,
        skipped: result.Errors.Count);

    PrintErrors(result.Errors);
}


static void RunMixedCsvImport(string path)
{
    var result = MixedCatalogImporter.Load(path);

    Console.WriteLine("CrossApp – імпорт різнорідних даних");
    Console.WriteLine(new string('-', 70));

    foreach (object item in result.Items)
    {
        switch (item)
        {
            case ProductDto product:
                Console.WriteLine(
                    $" ТОВАР  {product.Id,-6} " +
                    $"{product.Sku,-10} " +
                    $"{product.Name,-30} " +
                    $"{product.Quantity,5} " +
                    $"{product.Unit}");
                break;

            case WarehouseDto warehouse:
                Console.WriteLine(
                    $" СКЛАД  {warehouse.Id,-6} " +
                    $"{warehouse.Name,-20} " +
                    $"{warehouse.Address,-25} " +
                    $"місткість: {warehouse.Capacity}");
                break;
        }
    }

    PrintStatistics(
        total: result.Items.Count + result.Errors.Count,
        accepted: result.Items.Count,
        skipped: result.Errors.Count);

    PrintErrors(result.Errors);
}


static void PrintProducts(
    IReadOnlyList<ProductDto> products)
{
    Console.WriteLine(
        $"Завантажено записів: {products.Count}");

    Console.WriteLine();

    foreach (ProductDto product in products.Take(5))
    {
        Console.WriteLine(
            $" {product.Id,-6} " +
            $"{product.Sku,-10} " +
            $"{product.Name,-30} " +
            $"{product.Quantity,5} " +
            $"{product.Unit}");
    }
}


static void PrintErrors(
    IReadOnlyList<string> errors)
{
    if (errors.Count == 0)
        return;

    Console.WriteLine();

    Console.WriteLine(
        $"Пропущено рядків: {errors.Count}");

    foreach (string error in errors)
    {
        Console.WriteLine($" ! {error}");
    }
}


static void PrintStatistics(
    int total,
    int accepted,
    int skipped)
{
    double errorPercent =
        total == 0
            ? 0
            : skipped * 100.0 / total;

    Console.WriteLine();

    Console.WriteLine(
        $"Статистика: усього {total} | " +
        $"прийнято {accepted} | " +
        $"пропущено {skipped} | " +
        $"помилок {errorPercent:F1}%");
}