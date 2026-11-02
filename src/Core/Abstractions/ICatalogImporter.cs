using Core.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Core.Abstractions;

public interface ICatalogImporter
{
    IAsyncEnumerable<Product> StreamAsync(
        string path,
        IProgress<int>? progress = null,
        CancellationToken ct = default);

    Task<IReadOnlyList<Product>> LoadAsync(
        string path,
        CancellationToken ct = default);
}