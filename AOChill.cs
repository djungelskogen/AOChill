using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    [DllImport("psapi.dll")]
    static extern int EmptyWorkingSet(IntPtr hwProc);

    [DllImport("kernel32.dll")]
    static extern bool SetProcessWorkingSetSize(
        IntPtr hProcess,
        IntPtr dwMinimumWorkingSetSize,
        IntPtr dwMaximumWorkingSetSize);

    [DllImport("kernel32.dll")]
    static extern bool GetPhysicallyInstalledSystemMemory(out ulong totalMemoryInKilobytes);


    static bool isLooping = false;
    static CancellationTokenSource? cts;

    static int loopIntervalMs = 120000;
    static string profileName = "Balanced";


    static readonly string[] SafeTargets =
    {
        "chrome",
        "msedge",
        "discord",
        "steam",
        "epicgameslauncher",
        "spotify",
        "bloxstrap",
        "robloxplayerbeta"
    };


    static void Main()
    {
        Console.Title = "AOChill v2.1";

        if (!IsAdministrator())
        {
            Console.WriteLine("Please run AOChill as Administrator.");
            Console.ReadKey();
            return;
        }


        DetectHardware();


        while (true)
        {
            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("===============================");
            Console.WriteLine("        AOChill v2.1");
            Console.WriteLine("===============================");
            Console.ResetColor();


            DisplayInfo();


            Console.WriteLine();
            Console.WriteLine("1. Run Safe Optimization");
            Console.WriteLine("2. Toggle Background Engine");
            Console.WriteLine("3. Exit");

            Console.Write("\nChoice: ");

            string? input = Console.ReadLine();


            if (input == "1")
            {
                RunOptimization();

                Console.WriteLine("\nDone. Press key...");
                Console.ReadKey();
            }


            else if (input == "2")
            {
                ToggleLoop();
                Thread.Sleep(1000);
            }


            else if (input == "3")
            {
                StopLoop();
                break;
            }
        }
    }



    static bool IsAdministrator()
    {
        WindowsIdentity identity = WindowsIdentity.GetCurrent();

        WindowsPrincipal principal =
            new WindowsPrincipal(identity);

        return principal.IsInRole(
            WindowsBuiltInRole.Administrator);
    }



    static void DetectHardware()
    {
        if (!GetPhysicallyInstalledSystemMemory(out ulong kb))
            return;


        double ram =
            kb / 1024.0 / 1024.0;


        if (ram <= 4)
        {
            loopIntervalMs = 90000;
            profileName = "Low RAM Mode";
        }

        else if (ram <= 8)
        {
            loopIntervalMs = 120000;
            profileName = "Balanced Mode";
        }

        else
        {
            loopIntervalMs = 180000;
            profileName = "Light Mode";
        }
    }




    static void DisplayInfo()
    {
        if(GetPhysicallyInstalledSystemMemory(out ulong kb))
        {
            double ram =
                Math.Round(kb / 1024.0 / 1024.0,2);

            Console.WriteLine($"RAM Detected: {ram} GB");
        }


        Console.WriteLine(
            $"Profile: {profileName}");

        Console.WriteLine(
            $"Loop Interval: {loopIntervalMs / 1000}s");
    }





    static void RunOptimization()
    {
        Console.WriteLine("\nScanning safe applications...");

        int count = 0;


        foreach(Process proc in Process.GetProcesses())
        {
            try
            {
                string name =
                    proc.ProcessName.ToLower();


                if(!Array.Exists(
                    SafeTargets,
                    x => name.Contains(x)))
                    continue;



                SetProcessWorkingSetSize(
                    proc.Handle,
                    (IntPtr)(-1),
                    (IntPtr)(-1));


                EmptyWorkingSet(proc.Handle);


                count++;

                Console.WriteLine(
                    $"Optimized: {proc.ProcessName}");
            }


            catch
            {

            }
        }


        GC.Collect();
        GC.WaitForPendingFinalizers();


        Console.WriteLine(
            $"Finished. {count} apps optimized.");
    }




    static void ToggleLoop()
    {
        if(isLooping)
        {
            StopLoop();
            Console.WriteLine(
                "Background engine stopped.");
        }

        else
        {
            isLooping = true;

            cts =
            new CancellationTokenSource();


            Task.Run(() =>
            BackgroundWorker(cts.Token));


            Console.WriteLine(
                "Background engine started.");
        }
    }



    static void StopLoop()
    {
        isLooping = false;

        cts?.Cancel();

        cts?.Dispose();
    }




    static async Task BackgroundWorker(
        CancellationToken token)
    {
        while(!token.IsCancellationRequested)
        {
            RunOptimization();


            try
            {
                await Task.Delay(
                    loopIntervalMs,
                    token);
            }

            catch
            {
                break;
            }
        }
    }
}
