using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Kiota.Builder.Filesystem;

namespace Kiota.Builder.Lock;
/// <summary>
/// A service that manages the lock file for a Kiota project.
/// </summary>
public interface ILockManagementService
{
    /// <summary>
    /// Gets the lock file for a Kiota project by crawling the directory tree.
    /// </summary>
    /// <param name="searchDirectory">The root directory to crawl</param>
    /// <param name="filesystem">The filesystem to use to read the lock file.</param>
    IEnumerable<string> GetDirectoriesContainingLockFile(string searchDirectory, IFilesystem filesystem);
    /// <summary>
    /// Gets the lock file for a Kiota project by reading it from the target directory.
    /// </summary>
    /// <param name="directoryPath">The target directory to read the lock file from.</param>
    /// <param name="filesystem">The filesystem to use to read the lock file.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<KiotaLock?> GetLockFromDirectoryAsync(string directoryPath, IFilesystem filesystem, CancellationToken cancellationToken = default);
    /// <summary>
    /// Gets the lock file for a Kiota project by reading it from a stream.
    /// </summary>
    /// <param name="stream">The stream to read the lock file from.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<KiotaLock?> GetLockFromStreamAsync(Stream stream, CancellationToken cancellationToken = default);
    /// <summary>
    /// Writes the lock file for a Kiota project to the target directory.
    /// </summary>
    /// <param name="directoryPath">The target directory to write the lock file to.</param>
    /// <param name="filesystem">The filesystem to use to write the lock file.</param>
    /// <param name="lockInfo">The lock information to write.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task WriteLockFileAsync(string directoryPath, IFilesystem filesystem , KiotaLock lockInfo, CancellationToken cancellationToken = default);
    /// <summary>
    /// Backs up the lock file for a Kiota project to the target directory.
    /// </summary>
    /// <param name="directoryPath">The target directory to write the lock file from.</param>
    /// <param name="filesystem">The filesystem to use to write the lock file.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task BackupLockFileAsync(string directoryPath, IFilesystem filesystem, CancellationToken cancellationToken = default);
    /// <summary>
    /// Restores the lock file for a Kiota project to the target directory.
    /// </summary>
    /// <param name="directoryPath">The target directory to write the lock file to.</param>
    /// <param name="filesystem">The filesystem to use to write the lock file.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task RestoreLockFileAsync(string directoryPath, IFilesystem filesystem, CancellationToken cancellationToken = default);
}
