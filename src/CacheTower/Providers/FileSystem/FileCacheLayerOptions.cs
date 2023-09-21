using System;

namespace CacheTower.Providers.FileSystem;

/// <summary>
/// Options for controlling a <see cref="FileCacheLayer"/>.
/// </summary>
/// <param name="DirectoryPath">The directory to store the cache in.</param>
/// <param name="Serializer">The serializer to use for the data.</param>
public record struct FileCacheLayerOptions(
	string DirectoryPath,
	ICacheSerializer Serializer
)
{
	/// <summary>
	/// The default manifest save interval of 30 seconds.
	/// </summary>
	public static readonly TimeSpan DefaultManifestSaveInterval = TimeSpan.FromSeconds(30);

	/// <summary>
	/// The time interval controlling how often the cache manifest is saved to disk.
	/// </summary>
	public TimeSpan ManifestSaveInterval { get; init; } = DefaultManifestSaveInterval;

	/// <summary>
	/// Options for controlling a <see cref="FileCacheLayer"/>.
	/// </summary>
	/// <param name="directoryPath">The directory to store the cache in.</param>
	/// <param name="serializer">The serializer to use for the data.</param>
	/// <param name="manifestSaveInterval">The time interval controlling how often the cache manifest is saved to disk.</param>
	public FileCacheLayerOptions(
		string directoryPath,
		ICacheSerializer serializer,
		TimeSpan manifestSaveInterval
	) : this(directoryPath, serializer)
	{
		ManifestSaveInterval = manifestSaveInterval;
	}
}
