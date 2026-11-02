using Core.Abstractions;
using System;
using System.IO;
using System.Linq;

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
            string currentDirectory = Directory.GetCurrentDirectory();

            string dataPath = Path.Combine(
                currentDirectory,
                "data",
                "catalog.json");

            return new FileCatalogStore(dataPath);
        }

        return new InMemoryCatalogStore(
            Core.SampleData.Products());
    }
}