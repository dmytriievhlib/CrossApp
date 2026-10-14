
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Core.Domain;

namespace Core.Reports;

public sealed record BenchmarkResult(
    string Variant,
    double AverageMilliseconds,
    long Checksum);

public static class ReportBenchmark
{
    public static IReadOnlyList<BenchmarkResult> Run(
        int recordCount = 100_000,
        int iterations = 5)
    {
        if (recordCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(recordCount));

        if (iterations <= 0)
            throw new ArgumentOutOfRangeException(nameof(iterations));

        var products = Enumerable.Range(1, recordCount)
            .Select(i => Product.Create(
                $"BENCH-{i:D6}",
                $"B{i:D6}",
                "Тестовий товар",
                "шт",
                i % 1000))
            .ToList();

        long WithoutIntermediateList() =>
            products
                .Where(p => p.Quantity >= 0)
                .Select(p => (long)p.Quantity)
                .Sum();

        long WithIntermediateList() =>
            products
                .Where(p => p.Quantity >= 0)
                .ToList()
                .Select(p => (long)p.Quantity)
                .Sum();

        // Прогрів перед вимірюваннями.
        _ = WithoutIntermediateList();
        _ = WithIntermediateList();

        var results = new List<BenchmarkResult>();

        Measure(
            "Без проміжного ToList",
            WithoutIntermediateList,
            iterations,
            results);

        Measure(
            "З проміжним ToList",
            WithIntermediateList,
            iterations,
            results);

        return results;
    }

    private static void Measure(
        string name,
        Func<long> operation,
        int iterations,
        List<BenchmarkResult> results)
    {
        long checksum = 0;
        var stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < iterations; i++)
            checksum += operation();

        stopwatch.Stop();

        results.Add(new BenchmarkResult(
            name,
            stopwatch.Elapsed.TotalMilliseconds / iterations,
            checksum));
    }
}
