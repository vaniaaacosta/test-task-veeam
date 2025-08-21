using System;
using System.IO;
using System.Threading;
using System.Security.Cryptography;


Console.WriteLine("===== Folder Synchronizer =====");

// User inputs
Console.Write("Enter SOURCE folder path: ");
string source = Console.ReadLine() ?? "";

Console.Write("Enter REPLICA folder path: ");
string replica = Console.ReadLine() ?? "";

Console.Write("Enter synchronization interval (in seconds): ");
int interval = int.Parse(Console.ReadLine() ?? "10");

Console.Write("Enter LOG folder path: ");
string logFolder = Console.ReadLine() ?? "";
string logFile = Path.Combine(logFolder, "log.txt");

// Validate SOURCE folder
if (!Directory.Exists(source))
{
    Console.WriteLine("SOURCE folder does not exist. Program will exit.");
    return;
}

// Validate/create REPLICA
try
{
    Directory.CreateDirectory(replica);
}
catch (Exception ex)
{
    Console.WriteLine($"Error creating REPLICA folder: {ex.Message}");
    return;
}

// Validate/create LOG folder
try
{
    Directory.CreateDirectory(logFolder);
}
catch (Exception ex)
{
    Console.WriteLine($"Error creating LOG folder: {ex.Message}");
    return;
}

Console.WriteLine("\n=== Synchronization starting! ===\n");

// Infinite synchronization loop
while (true)
{
    //SynchronizerMethods.SyncFolders(source, replica, logFile);
    Thread.Sleep(interval * 1000);
}

