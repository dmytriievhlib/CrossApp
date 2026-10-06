using Core.Abstractions;
using System.IO;
using System.Linq;
using System;

namespace Core.Storage;

public static class StoreFactory
{
    public static ICatalogStore Create(string[] args)
    {
        bool useFile = args.Contains(
            "--file",
            StringComparer.OrdinalIgnoreCase);

        if (useFile)
        {
            string dataPath = Path.Combine(
                AppContext.BaseDirectory,
                "data",
                "catalog.json");

            return new FileCatalogStore(dataPath);
        }

        return new InMemoryCatalogStore(
            Core.SampleData.Products());
    }
}