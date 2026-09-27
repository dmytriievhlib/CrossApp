using Core.Domain;
using System;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("=== Сценарій 1: успіх ===");

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

Console.WriteLine("=== Перевірка ToDto / FromDto ===");

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

Console.WriteLine("=== Сценарій 2: порушення інваріантів ===");
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

Console.WriteLine();
Console.WriteLine($"Стан після всіх відмов: {product}");


static void TryDo(string title, Action action)
{
    try
    {
        action();

        Console.WriteLine(
            $" {title}: виняток НЕ спрацював — інваріант відсутній!");
    }
    catch (Exception ex)
    {
        Console.WriteLine(
            $" {title}: {ex.GetType().Name} — {ex.Message}");
    }
}