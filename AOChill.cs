using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    static bool isGamingModeActive = false;
    static CancellationTokenSource? cts;
    static int loopIntervalMs = 15000;

    static readonly string[] GameTargets = { 
        "robloxplayerbeta", 
        "bloxstrap", 
        "robloxplayerlauncher",
        "javaw",                   // Minecraft Java Edition runtime
        "minecraft.windows",       // Minecraft Bedrock Edition (UWP)
        "cs2", 
        "fortniteclient-win64-shipping", 
        "valorant-win64-shipping", 
        "gta5",
        "r5apex",                  // Apex Legends
        "cod",                     // Call of Duty (HQ / Warzone)
        "league of legends",       // League of Legends (In-game client)
        "overwatch"                // Overwatch 2
    };
    
    static readonly string[] BackgroundDrainers = { "chrome", "msedge", "epicgameslauncher" };

    static readonly Dictionary<int, ProcessPriorityClass> SavedPriorities = new();

    static void Main()
    {
        Console.Title = "AOChill Engine v3.1 - Intelligent Gaming Mode";

        if (!IsAdministrator())
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("CRITICAL: Please run AOChill as Administrator to alter process priorities.");
            Console.ResetColor();
            Console.ReadKey();
            return;
        }

        while (true)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("==================================================");
            Console.WriteLine("        AOChill v3.1 - Optimization               ");
            Console.WriteLine("==================================================");
            Console.ResetColor();

            Console.WriteLine($"Gaming Monitor Engine: {(isGamingModeActive ? "ACTIVE (Monitoring)" : "DISABLED")}");
            Console.WriteLine("\n1. Start Gaming Monitor Engine");
            Console.WriteLine("2. Stop Gaming Monitor Engine");
            Console.WriteLine("3. Exit");
            Console.Write("\nChoice: ");

            string? input = Console.ReadLine();

            if (input == "1" && !isGamingModeActive)
            {
                isGamingModeActive = true;
                cts = new CancellationTokenSource();
                Task.Run(() => GamingMonitorWorker(cts.Token));
                Console.WriteLine("Engine started successfully.");
                Thread.Sleep(1000);
            }
            else if (input == "2" && isGamingModeActive)
            {
                StopEngine();
                Console.WriteLine("Engine stopped. All priorities reverted.");
                Thread.Sleep(1000);
            }
            else if (input == "3")
            {
                StopEngine();
                break;
            }
        }
    }

    static bool IsAdministrator()
    {
        WindowsIdentity identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    static void StopEngine()
    {
        isGamingModeActive = false;
        cts?.Cancel();
        cts?.Dispose();
        RevertAllPriorities();
    }

    static int GetForegroundProcessId()
    {
        IntPtr hwnd = GetForegroundWindow();
        if (hwnd == IntPtr.Zero) return -1;
        GetWindowThreadProcessId(hwnd, out uint pid);
        return (int)pid;
    }

    static async Task GamingMonitorWorker(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            bool gameRunningAndFocused = false;
            int activePid = GetForegroundProcessId();

            if (activePid != -1)
            {
                try
                {
                    using (Process activeProc = Process.GetProcessById(activePid))
                    {
                        string name = activeProc.ProcessName.ToLower();
                        if (Array.Exists(GameTargets, x => name.Contains(x)))
                        {
                            gameRunningAndFocused = true;

                            if (!SavedPriorities.ContainsKey(activeProc.Id))
                            {
                                SavedPriorities[activeProc.Id] = activeProc.PriorityClass;
                            }

                            if (activeProc.PriorityClass != ProcessPriorityClass.AboveNormal)
                            {
                                activeProc.PriorityClass = ProcessPriorityClass.AboveNormal;
                                Console.WriteLine($"\n[BOOST] Focused game detected. Set {activeProc.ProcessName} to ABOVE NORMAL Priority.");
                            }
                        }
                    }
                }
                catch { }
            }

            Process[] allProcesses = Process.GetProcesses();
            foreach (Process proc in allProcesses)
            {
                try
                {
                    string name = proc.ProcessName.ToLower();
                    if (Array.Exists(BackgroundDrainers, x => name.Contains(x)))
                    {
                        if (gameRunningAndFocused)
                        {
                            if (!SavedPriorities.ContainsKey(proc.Id))
                            {
                                SavedPriorities[proc.Id] = proc.PriorityClass;
                            }

                            if (proc.PriorityClass != ProcessPriorityClass.BelowNormal)
                            {
                                proc.PriorityClass = ProcessPriorityClass.BelowNormal;
                            }
                        }
                        else
                        {
                            if (SavedPriorities.ContainsKey(proc.Id))
                            {
                                proc.PriorityClass = SavedPriorities[proc.Id];
                                SavedPriorities.Remove(proc.Id);
                            }
                            else if (proc.PriorityClass != ProcessPriorityClass.Normal)
                            {
                                proc.PriorityClass = ProcessPriorityClass.Normal;
                            }
                        }
                    }
                }
                catch { }
                finally
                {
                    proc.Dispose();
                }
            }

            try { await Task.Delay(loopIntervalMs, token); } catch { break; }
        }
    }

    static void RevertAllPriorities()
    {
        foreach (var item in SavedPriorities)
        {
            try
            {
                using (Process p = Process.GetProcessById(item.Key))
                {
                    p.PriorityClass = item.Value;
                }
            }
            catch { }
        }
        SavedPriorities.Clear();
    }
}
