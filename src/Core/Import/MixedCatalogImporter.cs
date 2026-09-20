using Core.Dto;
using System.Collections.Generic;
using System;

namespace Core.Import;

public static class MixedCatalogImporter
{
    private const char Separator = ';';

    public static ImportResult<object> Load(string path)
    {
        var items = new List<object>();
        var errors = new List<string>();

        string[] lines = File.ReadAllLines(
            path,
            System.Text.Encoding.UTF8);

        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line) ||
                line.StartsWith('#'))
            {
                continue;
            }

            switch (ParseLine(line))
            {
                case ProductDto product:
                    items.Add(product);
                    break;

                case WarehouseDto warehouse:
                    items.Add(warehouse);
                    break;

                case string error:
                    errors.Add($"рядок {number}: {error}");
                    break;
            }
        }

        return new ImportResult<object>(items, errors);
    }

    private static object ParseLine(string line)
    {
        string[] parts = line.Split(
            Separator,
            StringSplitOptions.TrimEntries);

        return parts switch
        {
        ["P", var id, var sku, var name, var unit, var qty]
            when int.TryParse(qty, out int quantity)
                 && quantity >= 0
                 && !string.IsNullOrWhiteSpace(id)
                 && !string.IsNullOrWhiteSpace(sku)
                 && !string.IsNullOrWhiteSpace(name)
                 && !string.IsNullOrWhiteSpace(unit)
            =>
            new ProductDto(
                id,
                sku,
                name,
                unit,
                quantity),

                ["P", _, _, _, _, var invalidQty]
                        =>
                        $"товар: кількість '{invalidQty}' не є невід'ємним числом",

                        ["P", ..]
                    =>
                    "товар: очікується 6 колонок у форматі P;id;sku;name;unit;quantity",

                    ["W", var id, var name, var address, var capacity]
                    when int.TryParse(capacity, out int warehouseCapacity)
                         && warehouseCapacity >= 0
                         && !string.IsNullOrWhiteSpace(id)
                         && !string.IsNullOrWhiteSpace(name)
                         && !string.IsNullOrWhiteSpace(address)
                    =>
                    new WarehouseDto(
                        id,
                        name,
                        address,
                        warehouseCapacity),

                        ["W", _, _, _, var invalidCapacity]
                        =>
                        $"склад: місткість '{invalidCapacity}' не є невід'ємним числом",

                        ["W", ..]
                    =>
                    "склад: очікується 5 колонок у форматі W;id;name;address;capacity",

                    [var type, ..]
                    =>
                    $"невідомий тип запису '{type}'",

            _
                =>
                "порожній або некоректний запис"
        };
    }
}