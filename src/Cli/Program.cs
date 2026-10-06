using System.Collections.Generic;
using System;
using System.Text;
using Core.Abstractions;
using Core.Services;
using Core.Storage;

Console.OutputEncoding = Encoding.UTF8;

// Composition Root:
// конкретне сховище створюється через фабрику.
ICatalogStore store = StoreFactory.Create(args);

// Декоратор кешування.
// CatalogService працює тільки з ICatalogStore.
ICatalogStore cachedStore =
    new CachingCatalogStore(store);

var service = new CatalogService(cachedStore);

Console.WriteLine("=== CrossApp — лабораторна робота №5 ===");
Console.WriteLine($"Основне сховище: {store.GetType().Name}");
Console.WriteLine($"Декоратор: {cachedStore.GetType().Name}");

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

// 3. Зміна кількості — надходження
Console.WriteLine("Надходження товару:");

service.Receive(
    addedProduct.Id,
    5);

Console.WriteLine(
    $"  Після надходження: {service.Find(addedProduct.Id)}");

Console.WriteLine();

// 4. Зміна кількості — видача
Console.WriteLine("Видача товару:");

service.Issue(
    addedProduct.Id,
    3);

Console.WriteLine(
    $"  Після видачі: {service.Find(addedProduct.Id)}");

Console.WriteLine();

// 5. Пошук за ID
Console.WriteLine("Пошук за ID:");

var found = service.Find(addedProduct.Id);

Console.WriteLine(
    found is null
        ? "  Товар не знайдено."
        : $"  Знайдено: {found}");

Console.WriteLine();

// 5.1. Пошук за умовою Func<Product, bool>
Console.WriteLine("Пошук товарів за умовою:");

var searchResults = service.Search(
    product => product.Quantity >= 100);

foreach (var product in searchResults)
{
    Console.WriteLine($"  {product}");
}

Console.WriteLine(
    $"  Знайдено товарів: {searchResults.Count}");

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

// 7. Перевірка видалення
Console.WriteLine("Видалення тестового товару:");

bool removed = service.Remove(addedProduct.Id);

Console.WriteLine(
    removed
        ? "  Тестовий товар успішно видалено."
        : "  Товар не знайдено.");

Console.WriteLine();

// 8. Фінальний список
Console.WriteLine("Фінальний список товарів:");

IReadOnlyList<Core.Domain.Product> finalProducts =
    service.All();

if (finalProducts.Count == 0)
{
    Console.WriteLine("  Список порожній.");
}
else
{
    foreach (var product in finalProducts)
    {
        Console.WriteLine($"  {product}");
    }
}

Console.WriteLine();
Console.WriteLine("=== Завершено ===");