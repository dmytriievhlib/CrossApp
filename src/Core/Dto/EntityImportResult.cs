using System.Collections.Generic;

namespace Core.Dto;

public sealed record EntityImportResult<T>(
    IReadOnlyList<T> Items,
    IReadOnlyList<string> Errors);