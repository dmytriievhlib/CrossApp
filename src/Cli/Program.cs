using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Text;
using Core;
using Core.Abstractions;
using Core.Services;
using Core.Storage;

Console.OutputEncoding = Encoding.UTF8;

// Composition Root:
// саме тут обираємо конкретну реалізацію сховища.
bool useFile = args.Contains(
    "--file",
    StringComparer.OrdinalIgnoreCase);

string dataPath = Path.Combine(
    AppContext.BaseDirectory,
    "data",
    "catalog.json");

ICatalogStore store = useFile
    ? new FileCatalogStore(dataPath)
    : new InMemoryCatalogStore(SampleData.Products());

var service = new CatalogService(store);

Console.WriteLine("=== CrossApp — лабораторна робота №5 ===");
Console.WriteLine($"Сховище: {store.GetType().Name}");

if (useFile)
{
    Console.WriteLine($"Файл: {dataPath}");
}

Console.WriteLine();

// 1. Показ початкового списку
Console.WriteLine("Початковий список товарів:");

IReadOnlyList<Core.Domain.Product> initialProducts =
    service.All();

if (initialProducts.Count == 0)
{
    Console.WriteLine("  Список порожній.");
}
else
{
    foreach (var product in initialProducts)
    {
        Console.WriteLine($"  {product}");
    }
}

Console.WriteLine();

// 2. Додавання нового товару
Console.WriteLine("Додавання нового товару:");

var addedProduct = service.Add(
    "SKU-NEW",
    "Тестовий товар",
    "шт",
    10);

Console.WriteLine($"  Додано: {addedProduct}");

Console.WriteLine();

// 3. Зміна кількості
Console.WriteLine("Надходження товару:");

service.Receive(
    addedProduct.Id,
    5);

Console.WriteLine(
    $"  Після надходження: {service.Find(addedProduct.Id)}");

Console.WriteLine();

// 4. Видача товару
Console.WriteLine("Видача товару:");

service.Issue(
    addedProduct.Id,
    3);

Console.WriteLine(
    $"  Після видачі: {service.Find(addedProduct.Id)}");

Console.WriteLine();

// 5. Пошук
Console.WriteLine("Пошук за ID:");

var found = service.Find(addedProduct.Id);

Console.WriteLine(
    found is null
        ? "  Товар не знайдено."
        : $"  Знайдено: {found}");

Console.WriteLine();

// 6. Перевірка помилки
Console.WriteLine("Перевірка помилки:");

try
{
    service.Receive("P-999", 10);
}
catch (Exception ex)
{
    Console.WriteLine($"  Помилка: {ex.Message}");
}

Console.WriteLine();

// 7. Фінальний список
Console.WriteLine("Фінальний список товарів:");

foreach (var product in service.All())
{
    Console.WriteLine($"  {product}");
}

Console.WriteLine();
Console.WriteLine("=== Завершено ===");