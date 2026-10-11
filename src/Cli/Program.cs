
using System.IO;
using System;
using System.Text;
using Core.Abstractions;
using Core.Domain;
using Core.Events;
using Core.Notifications;
using Core.Services;
using Core.Storage;
using Core.Import;
Console.OutputEncoding = Encoding.UTF8;

// 1. Ручне впровадження залежностей (DI).
ICatalogStore store = StoreFactory.Create(args);

var logPath = Path.Combine(
    AppContext.BaseDirectory,
    "logs",
    "app.log");

INotificationSink[] sinks =
[
    new ConsoleSink(),
    new FileSink(logPath)
];

var cachedStore = new CachingCatalogStore(store);
var service = new CatalogService(cachedStore, sinks);

// 2. Підписники події Changed.
int changes = 0;

void OnCatalogChanged(
    object? sender,
    CatalogChangedEventArgs e)
{
    Console.WriteLine(
        $"Подія: {e.Kind}, товар {e.Id}, залишок {e.QuantityAfter}");
}

service.Changed += OnCatalogChanged;
service.Changed += (_, _) => changes++;

Console.WriteLine("=== CrossApp — лабораторна робота №6 ===");
Console.WriteLine($"Сховище: {store.GetType().Name}");
Console.WriteLine($"Журнал: {logPath}");
Console.WriteLine();

// 3. Додавання товару.
Console.WriteLine("1. Додавання товару:");

var created = service.Add(
    "SKU-101",
    "Кабель UTP cat6",
    "м",
    50);

Console.WriteLine();

// 4. Надходження товару.
Console.WriteLine("2. Надходження товару:");

service.Receive(created.Id, 25);

Console.WriteLine($"Подій отримано: {changes}");
Console.WriteLine();

// 5. Пошук через Func і лямбду.
Console.WriteLine("3. Пошук товарів із залишком менше 10:");

var lowStock = service.FindAll(
    product => product.Quantity < 10);

if (lowStock.Count == 0)
{
    Console.WriteLine("Товарів із таким залишком немає.");
}
else
{
    foreach (Product product in lowStock)
        Console.WriteLine(product);
}

Console.WriteLine();

// 6. Перевірка помилки.
Console.WriteLine("4. Перевірка помилки:");

try
{
    service.Receive("P-999", 10);
}
catch (Exception ex)
{
    Console.WriteLine($"Помилка: {ex.Message}");
}

Console.WriteLine($"Подій отримано: {changes}");
Console.WriteLine();

// 7. Відписка від події.
Console.WriteLine("5. Відписуємося від події:");

service.Changed -= OnCatalogChanged;

service.Receive(created.Id, 5);

Console.WriteLine(
    "Обробник OnCatalogChanged більше не отримує події.");

Console.WriteLine($"Подій отримано: {changes}");
Console.WriteLine();

// 8. Видача та видалення.
Console.WriteLine("6. Видача товару:");

service.Issue(created.Id, 10);

Console.WriteLine();
Console.WriteLine("7. Видалення товару:");

service.Remove(created.Id);

Console.WriteLine();
Console.WriteLine("8. Імпорт CSV:");

var csvPath = Path.Combine(
    AppContext.BaseDirectory,
    "data",
    "sample.csv");

if (!File.Exists(csvPath))
{
    Console.WriteLine($"CSV-файл не знайдено: {csvPath}");
}
else
{
    var result = ProductCsvImporter.Load(
        csvPath,
        message => Console.WriteLine($"Пропущено: {message}"));

    Console.WriteLine(
        $"Імпортовано товарів: {result.Items.Count}");

    Console.WriteLine(
        $"Помилок: {result.Errors.Count}");
}

Console.WriteLine();
Console.WriteLine("=== Завершено ===");
