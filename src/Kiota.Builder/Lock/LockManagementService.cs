using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Kiota.Builder.Extensions;
using Kiota.Builder.Filesystem;

namespace Kiota.Builder.Lock;

/// <summary>
/// A service that manages the lock file for a Kiota project implemented using the file system.
/// </summary>
public class LockManagementService : ILockManagementService
{
    internal const string LockFileName = "kiota-lock.json";
    /// <inheritdoc/>
    public IEnumerable<string> GetDirectoriesContainingLockFile(string searchDirectory, IFilesystem filesystem)
    {
        ArgumentException.ThrowIfNullOrEmpty(searchDirectory);
        ArgumentNullException.ThrowIfNull(filesystem);
        return filesystem.EnumerateFiles(searchDirectory, recursive: true)
            .Where(x => x.EndsWith(LockFileName, StringComparison.OrdinalIgnoreCase))
            .Select(Path.GetDirectoryName)
            .Where(x => !string.IsNullOrEmpty(x))
            .OfType<string>();
    }

    /// <inheritdoc/>
    public Task<KiotaLock?> GetLockFromDirectoryAsync(string directoryPath, IFilesystem filesystem, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(directoryPath);
        ArgumentNullException.ThrowIfNull(filesystem);
        return GetLockFromDirectoryInternalAsync(directoryPath, filesystem, cancellationToken);
    }
    private static async Task<KiotaLock?> GetLockFromDirectoryInternalAsync(string directoryPath, IFilesystem filesystem, CancellationToken cancellationToken)
    {
        var lockFilePath = Path.Combine(directoryPath, LockFileName);
        if (filesystem.FileExists(lockFilePath))
        {
#pragma warning disable CA2007
            await using var fileStream = filesystem.OpenRead(lockFilePath);
#pragma warning restore CA2007
            var result = await GetLockFromStreamInternalAsync(fileStream, cancellationToken).ConfigureAwait(false);
            if (result is not null && IsDescriptionLocal(result.DescriptionLocation) && !Path.IsPathRooted(result.DescriptionLocation))
            {
                result.DescriptionLocation = Path.GetFullPath(Path.Combine(directoryPath, result.DescriptionLocation));
            }
            return result;
        }
        return null;
    }
    /// <inheritdoc/>
    public Task<KiotaLock?> GetLockFromStreamAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        return GetLockFromStreamInternalAsync(stream, cancellationToken);
    }
    private static async Task<KiotaLock?> GetLockFromStreamInternalAsync(Stream stream, CancellationToken cancellationToken)
    {
        return await JsonSerializer.DeserializeAsync(stream, context.KiotaLock, cancellationToken).ConfigureAwait(false);
    }
    /// <inheritdoc/>
    public Task WriteLockFileAsync(string directoryPath, IFilesystem filesystem, KiotaLock lockInfo, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(directoryPath);
        ArgumentNullException.ThrowIfNull(filesystem);
        ArgumentNullException.ThrowIfNull(lockInfo);
        return WriteLockFileInternalAsync(directoryPath, filesystem, lockInfo, cancellationToken);
    }
    private static readonly JsonSerializerOptions options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };
    private static readonly KiotaLockGenerationContext context = new(options);
    private static async Task WriteLockFileInternalAsync(string directoryPath, IFilesystem filesystem, KiotaLock lockInfo, CancellationToken cancellationToken)
    {
        var lockFilePath = Path.Combine(directoryPath, LockFileName);
#pragma warning disable CA2007
        await using var fileStream = filesystem.OpenWrite(lockFilePath);
#pragma warning restore CA2007
        lockInfo.DescriptionLocation = GetRelativeDescriptionPath(lockInfo.DescriptionLocation, lockFilePath);
        await JsonSerializer.SerializeAsync(fileStream, lockInfo, context.KiotaLock, cancellationToken).ConfigureAwait(false);
    }

    private static bool IsDescriptionLocal(string descriptionPath) => !descriptionPath.StartsWith("http", StringComparison.OrdinalIgnoreCase);

    private static string GetRelativeDescriptionPath(string descriptionPath, string lockFilePath)
    {
        if (IsDescriptionLocal(descriptionPath) &&
            Path.GetDirectoryName(lockFilePath) is string lockFileDirectoryPath)
            return Path.GetRelativePath(lockFileDirectoryPath, descriptionPath).NormalizePathSeparators();
        return descriptionPath;
    }

    /// <inheritdoc/>
    public Task BackupLockFileAsync(string directoryPath, IFilesystem filesystem, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(directoryPath);
        ArgumentNullException.ThrowIfNull(filesystem);
        return BackupLockFileInternalAsync(directoryPath, filesystem);
    }

    private static Task BackupLockFileInternalAsync(string directoryPath, IFilesystem filesystem)
    {
        var lockFilePath = Path.Combine(directoryPath, LockFileName);
        if (filesystem.FileExists(lockFilePath))
        {
            var backupFilePath = GetBackupFilePath(directoryPath, filesystem);
            var targetDirectory = Path.GetDirectoryName(backupFilePath);
            if (string.IsNullOrEmpty(targetDirectory)) return Task.CompletedTask;
            if (!filesystem.DirectoryExists(targetDirectory))
                filesystem.CreateDirectory(targetDirectory);
            filesystem.CopyFile(lockFilePath, backupFilePath, true);
        }
        return Task.CompletedTask;
    }
    /// <inheritdoc/>
    public Task RestoreLockFileAsync(string directoryPath, IFilesystem filesystem, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(directoryPath);
        ArgumentNullException.ThrowIfNull(filesystem);
        return RestoreLockFileInternalAsync(directoryPath, filesystem);
    }
    private static Task RestoreLockFileInternalAsync(string directoryPath, IFilesystem filesystem)
    {
        var lockFilePath = Path.Combine(directoryPath, LockFileName);
        var targetDirectory = Path.GetDirectoryName(lockFilePath);
        if (string.IsNullOrEmpty(targetDirectory)) return Task.CompletedTask;
        if (!filesystem.DirectoryExists(targetDirectory))
            filesystem.CreateDirectory(targetDirectory);
        var backupFilePath = GetBackupFilePath(directoryPath, filesystem);
        if (filesystem.FileExists(backupFilePath))
        {
            filesystem.CopyFile(backupFilePath, lockFilePath, true);
        }
        return Task.CompletedTask;
    }

    private static readonly ThreadLocal<HashAlgorithm> HashAlgorithm = new(SHA256.Create);
    private static string GetBackupFilePath(string outputPath, IFilesystem filesystem)
    {
        ArgumentNullException.ThrowIfNull(filesystem);
        var hashedPath = Convert.ToHexString((HashAlgorithm.Value ?? throw new InvalidOperationException("unable to get hash algorithm")).ComputeHash(Encoding.UTF8.GetBytes(outputPath))).Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase);
        return Path.Combine(filesystem.GetTempPath(), Constants.TempDirectoryName, "backup", hashedPath, LockFileName);
    }

    public void DeleteLockFile(string directoryPath, IFilesystem filesystem)
    {
        ArgumentException.ThrowIfNullOrEmpty(directoryPath);
        ArgumentNullException.ThrowIfNull(filesystem);
        var lockFilePath = Path.Combine(directoryPath, LockFileName);
        if (filesystem.FileExists(lockFilePath))
            filesystem.DeleteFile(lockFilePath);
    }
}
