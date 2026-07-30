using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    [DllImport("psapi.dll")]
    static extern int EmptyWorkingSet(IntPtr hwProc);

    [DllImport("kernel32.dll")]
    static extern bool SetProcessWorkingSetSize(IntPtr hProcess, IntPtr dwMinimumWorkingSetSize, IntPtr dwMaximumWorkingSetSize);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool GetPhysicallyInstalledSystemMemory(out ulong totalMemoryInKilobytes);

    private static bool isLooping = false;
    private static CancellationTokenSource cts;
    private static int loopIntervalMs = 60000;
    private static string profileName = "Standard Balance";

    static void Main(string[] args)
    {
        Console.Title = "AO";
        DetectHardwareAndConfigure();
        
        while (true)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("==================================================");
            Console.WriteLine("                  AOChill v2.0                    ");
            Console.WriteLine("==================================================");
            Console.ResetColor();
            
            DisplayHardwareDiagnostics();
            
            Console.WriteLine("\n 1. Run Core Memory Optimization Now");
            Console.WriteLine($" 2. Toggle Background Engine (Current: {(isLooping ? "ACTIVE" : "IDLE")})");
            Console.WriteLine(" 3. Exit");
            Console.Write("\nSelect an option: ");
            
            string choice = Console.ReadLine();
            
            if (choice == "1")
            {
                RunOptimization();
                Console.WriteLine("\nOptimization deployed. Press any key to return to menu...");
                Console.ReadKey();
            }
            else if (choice == "2")
            {
                ToggleLoop();
            }
            else if (choice == "3")
            {
                if (isLooping) StopLoop();
                break;
            }
        }
    }

    static void DetectHardwareAndConfigure()
    {
        try
        {
            if (GetPhysicallyInstalledSystemMemory(out ulong totalKb))
            {
                double totalGb = totalKb / 1024.0 / 1024.0;
                if (totalGb <= 4.5)
                {
                    loopIntervalMs = 30000;
                    profileName = "Aggressive Extreme (Low RAM Guard)";
                }
                else if (totalGb <= 8.5)
                {
                    loopIntervalMs = 45000;
                    profileName = "Moderate Response";
                }
                else
                {
                    loopIntervalMs = 90000;
                    profileName = "Lightweight Passive Maintenance";
                }
            }
        }
        catch
        {
            loopIntervalMs = 60000;
            profileName = "Fallback Safe Default";
        }
    }

    static void DisplayHardwareDiagnostics()
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        if (GetPhysicallyInstalledSystemMemory(out ulong totalKb))
        {
            double totalGb = Math.Round(totalKb / 1024.0 / 1024.0, 2);
            double estimatedVram = Math.Round(totalGb * 0.5, 2);
            Console.WriteLine($"Detected Hardware Capacity: {totalGb} GB Physical System RAM");
            Console.WriteLine($"Dynamic Allocation Target : Up to {estimatedVram} GB Unified Video Cache Buffer");
        }
        Console.WriteLine($"Active Optimization Profile: {profileName} (Interval: {loopIntervalMs / 1000}s)");
        Console.ResetColor();
        Console.WriteLine("==================================================");
    }

    static void RunOptimization()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[~] Executing native garbage eviction loops...");
        Console.ResetColor();

        Process[] processes = Process.GetProcesses();
        int optimizedCount = 0;

        foreach (Process proc in processes)
        {
            try
            {
                if (proc.Id == 0 || proc.ProcessName == "System" || proc.ProcessName == "Idle" || 
                    proc.ProcessName == "csrss" || proc.ProcessName == "explorer" || 
                    proc.ProcessName.Contains("AO")) 
                    continue;

                SetProcessWorkingSetSize(proc.Handle, (IntPtr)(-1), (IntPtr)(-1));
                EmptyWorkingSet(proc.Handle);
                optimizedCount++;
            }
            catch
            {
            }
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[_] Reset physical constraints for {optimizedCount} background architectures.");
        Console.ResetColor();
    }

    static void ToggleLoop()
    {
        if (isLooping)
        {
            StopLoop();
            Console.WriteLine("\nBackground processing suspended.");
        }
        else
        {
            isLooping = true;
            cts = new CancellationTokenSource();
            Task.Run(() => BackgroundLoopWorker(cts.Token));
            Console.WriteLine("\nBackground execution threaded successfully.");
        }
        Thread.Sleep(1500);
    }

    static void StopLoop()
    {
        isLooping = false;
        cts?.Cancel();
        cts?.Dispose();
    }

    static async Task BackgroundLoopWorker(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            RunOptimization();
            try
            {
                await Task.Delay(loopIntervalMs, token);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }
}
