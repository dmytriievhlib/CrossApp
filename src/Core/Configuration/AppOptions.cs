using System;
using System.IO;

namespace Core.Configuration;

public sealed class AppOptions
{
	public StorageOptions Storage { get; init; } = new();

	public string EnvironmentName { get; init; } = "Development";
}

public sealed class StorageOptions
{
	public string DataPath { get; init; } = "data";

	public string FileName { get; init; } = "catalog.json";

	public string Format { get; init; } = "json";

	public string ResolveFullPath(string baseDirectory)
	{
		string root = Path.IsPathRooted(DataPath)
			? DataPath
			: Path.Combine(baseDirectory, DataPath);

		Directory.CreateDirectory(root);

		return Path.GetFullPath(Path.Combine(root, FileName));
	}

	public string ResolveExportPath(string baseDirectory)
	{
		string root = Path.IsPathRooted(DataPath)
			? DataPath
			: Path.Combine(baseDirectory, DataPath);

		Directory.CreateDirectory(root);

		string extension = Format.Equals(
			"csv",
			StringComparison.OrdinalIgnoreCase)
				? ".csv"
				: ".json";

		return Path.GetFullPath(
			Path.Combine(root, $"catalog-export{extension}"));
	}
}