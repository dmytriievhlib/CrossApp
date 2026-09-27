using Core.Domain;
using Core.Dto;
using Core.Import;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("=== ЛАБОРАТОРНА РОБОТА №4 ===");
Console.WriteLine();

Console.WriteLine("=== Основна частина: Product ===");

Product product = Product.Create(
    "P-001",
    "sku-001",
    "Цемент М400 25кг",
    "шт",
    100);

Console.WriteLine(product);

product.RegisterArrival(50);
product.Issue(30);

Console.WriteLine(product);

Console.WriteLine();

Console.WriteLine("=== ToDto / FromDto ===");

var dto = product.ToDto();

Console.WriteLine(
    $"DTO: {dto.Id} | {dto.Sku} | {dto.Name} | " +
    $"{dto.Quantity} {dto.Unit}");

Product restoredProduct = Product.FromDto(dto);

Console.WriteLine(
    $"Відновлена сутність: {restoredProduct}");

Console.WriteLine(
    $"Результат: {product.Quantity == restoredProduct.Quantity}");

Console.WriteLine();

Console.WriteLine("=== Порушення інваріантів Product ===");

TryDo(
    "видача більша за залишок",
    () => product.Issue(1000));

TryDo(
    "порожній SKU",
    () => Product.Create(
        "P-002",
        " ",
        "Пісок",
        "т",
        10));

TryDo(
    "від'ємний залишок",
    () => Product.Create(
        "P-003",
        "SKU-003",
        "Цегла",
        "шт",
        -5));

Console.WriteLine(
    $"Стан після відмов: {product}");

Console.WriteLine();

Console.WriteLine("=== Додаткове завдання 1: ImportResult → Entity ===");

var importResult = new ImportResult<ProductDto>(
    new List<ProductDto>
    {
        new(
            "P-101",
            "SKU-101",
            "Товар коректний",
            "шт",
            10),

        new(
            "P-102",
            "SKU-102",
            "Ще один товар",
            "шт",
            25)
    },
    new List<string>
    {
        "рядок 15: кількість не є числом"
    });

var entityResult =
    ProductEntityImporter.FromImportResult(importResult);

Console.WriteLine(
    $"Створено сутностей: {entityResult.Items.Count}");

Console.WriteLine(
    $"Помилок: {entityResult.Errors.Count}");

foreach (string error in entityResult.Errors)
{
    Console.WriteLine($" - {error}");
}

Console.WriteLine();

Console.WriteLine("=== Додаткове завдання 2: правило між двома сутностями ===");

var reader = Reader.Create(
    "R-001",
    "Іван Петренко");

var libraryService = new LibraryService();

var books = Enumerable.Range(1, 6)
    .Select(i =>
        BookCopy.Create(
            $"B-{i:000}",
            $"Книга №{i}"))
    .ToList();

for (int i = 0; i < 5; i++)
{
    libraryService.IssueBook(
        reader,
        books[i]);
}

Console.WriteLine(reader);

TryDo(
    "шоста видача при 5 відкритих видачах",
    () => libraryService.IssueBook(reader, books[5]));

Console.WriteLine(reader);

Console.WriteLine();

Console.WriteLine("=== Додаткове завдання 3: OrderStatus ===");

var order = Order.Create("ORD-001");

Console.WriteLine(order);

order.ChangeStatus(OrderStatus.Confirmed);

Console.WriteLine(order);

TryDo(
    "перехід Confirmed → Cancelled",
    () => order.ChangeStatus(OrderStatus.Cancelled));

Console.WriteLine(order);

Console.WriteLine();

Console.WriteLine("=== Завершено ===");

static void TryDo(
    string title,
    Action action)
{
    try
    {
        action();

        Console.WriteLine(
            $" {title}: виконано успішно");
    }
    catch (Exception ex)
    {
        Console.WriteLine(
            $" {title}: {ex.GetType().Name} — {ex.Message}");
    }
}