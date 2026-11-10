using Core.Domain;
using System.Collections.Generic;

namespace Core.Abstractions;

public interface ICatalogExporter
{
    void Export(IEnumerable<Product> products, string path);
}