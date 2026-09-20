using Core.Dto;
using Core.Import;
using System.IO;
using System;

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

ImportResult<ProductDto> result;

try
{
    result = ProductCsvImporter.Load(path);
}
catch (Exception ex)
{
    Console.WriteLine(
        $"Помилка читання файлу: {ex.Message}");

    return 1;
}

Console.WriteLine("CrossApp – імпорт товарів");
Console.WriteLine(new string('-', 70));

Console.WriteLine(
    $"Завантажено записів: {result.Items.Count}");

Console.WriteLine();

foreach (ProductDto product in result.Items.Take(5))
{
    Console.WriteLine(
        $" {product.Id,-6} " +
        $"{product.Sku,-10} " +
        $"{product.Name,-30} " +
        $"{product.Quantity,5} " +
        $"{product.Unit}");
}

if (result.Errors.Count > 0)
{
    Console.WriteLine();
    Console.WriteLine(
        $"Пропущено рядків: {result.Errors.Count}");

    foreach (string error in result.Errors)
    {
        Console.WriteLine($" ! {error}");
    }
}

return 0;