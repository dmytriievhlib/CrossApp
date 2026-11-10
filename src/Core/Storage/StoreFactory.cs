
using Core.Abstractions;
using Core.Configuration;
using System;
using System.IO;
using System.Linq;

namespace Core.Storage;

public static class StoreFactory
{
    // Старий варіант для сумісності.
    public static ICatalogStore Create(string[] args)
    {
        var options = new AppOptions();

        return Create(
            args,
            options,
            Directory.GetCurrentDirectory());
    }

    // Новий варіант із використанням конфігурації.
    public static ICatalogStore Create(
        string[] args,
        AppOptions options,
        string baseDirectory)
    {
        bool useFile = args.Contains(
            "--file",
            StringComparer.OrdinalIgnoreCase);

        if (useFile)
        {
            string dataPath =
                options.Storage.ResolveFullPath(baseDirectory);

            return new FileCatalogStore(dataPath);
        }

        return new InMemoryCatalogStore(
            Core.SampleData.Products());
    }
}
