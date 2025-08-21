using System;
using System.IO;
using System.Threading;
using System.Security.Cryptography;

// ===== Helper functions =====
public class SynchronizerHelpMethods
{
    public static void SyncFolders(string source, string replica, string logFile)
    {
        // === DELETE EXTRA FILES IN REPLICA ===
        foreach (string file in Directory.GetFiles(replica, "*", SearchOption.AllDirectories))
        {
            string relativeFile = Path.GetRelativePath(replica, file);
            string sourceFile = Path.Combine(source, relativeFile);

            if (!File.Exists(sourceFile))
            {
                File.Delete(file);
                if (!IsIgnoredFile(file))
                    Log($"[DELETED FILE] {relativeFile}", logFile);
            }
        }

        // === DELETE EXTRA FOLDERS IN REPLICA ===
        var allDirs = Directory.GetDirectories(replica, "*", SearchOption.AllDirectories);
        Array.Reverse(allDirs);
        foreach (string dirPath in allDirs)
        {
            string relativeDir = Path.GetRelativePath(replica, dirPath);
            string sourceDir = Path.Combine(source, relativeDir);

            if (!Directory.Exists(sourceDir))
            {
                Directory.Delete(dirPath, true);
                if (!IsIgnoredFile(dirPath))
                    Log($"[DELETED FOLDER] {relativeDir}", logFile);
            }
        }

        // === CREATE / UPDATE FOLDERS ===
        foreach (string dirPath in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
        {
            string relativeDir = Path.GetRelativePath(source, dirPath);
            string destDir = Path.Combine(replica, relativeDir);

            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
                if (!IsIgnoredFile(destDir))
                    Log($"[CREATED FOLDER] {relativeDir}", logFile);
            }
        }

        // === CREATE / UPDATE FILES ===
        foreach (string file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
        {
            if (IsIgnoredFile(file))
                continue; // Ignora arquivos temporários

            string relativeFile = Path.GetRelativePath(source, file);
            string destFile = Path.Combine(replica, relativeFile);

            Directory.CreateDirectory(Path.GetDirectoryName(destFile)!);

            if (!File.Exists(destFile))
            {
                File.Copy(file, destFile);
                Log($"[CREATED FILE] {relativeFile}", logFile);
            }
            else if (FilesAreDifferent(file, destFile))
            {
                File.Copy(file, destFile, true);
                Log($"[UPDATED FILE] {relativeFile}", logFile);
            }
        }
    }

    // Compare files by content (MD5)
    private static bool FilesAreDifferent(string file1, string file2)
    {
        if (!File.Exists(file2)) return true;

        const int maxRetries = 5;
        const int delayMs = 500;

        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                using var md5 = MD5.Create();
                using var stream1 = File.OpenRead(file1);
                using var stream2 = File.OpenRead(file2);

                var hash1 = md5.ComputeHash(stream1);
                var hash2 = md5.ComputeHash(stream2);

                for (int i = 0; i < hash1.Length; i++)
                {
                    if (hash1[i] != hash2[i])
                        return true;
                }
                return false;
            }
            catch (IOException)
            {
                Thread.Sleep(delayMs);
            }
        }

        return true;
    }

    // Logging function
    private static void Log(string message, string logFile)
    {
        string logDir = Path.GetDirectoryName(logFile)!;
        if (!Directory.Exists(logDir))
            Directory.CreateDirectory(logDir);

        string line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
        Console.WriteLine(line);
        File.AppendAllText(logFile, line + Environment.NewLine);
    }

    // Ignore temporary files (~$ ou .tmp)
    private static bool IsIgnoredFile(string file)
    {
        string fileName = Path.GetFileName(file);
        return fileName.StartsWith("~$") || fileName.EndsWith(".tmp");
    }
}
