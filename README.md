# AOChill v2.0
An advanced real-time RAM and shared VRAM optimization engine built in C# for low-end PCs and laptops.

⚠️ **!!! TESTING PHASE !!!**

*This program has not been tested yet use for your own risk*
*Requirements: This optimization tool requires the .NET 6 Desktop Runtime. If you use Bloxstrap, you already have this installed! Otherwise, download it at dotnet.microsoft.com.*

---

## 🛠️ How to Compile AOChill from Source

If you want to build the executable (`.exe`) yourself, follow these steps:

1. Download and install the **.NET 6 SDK** on your computer.
2. Download both **`AOChill.cs`** and **`AOChill.csproj`** from this repository and put them into the same folder.
3. Open **Command Prompt (cmd)** or **Terminal** inside that folder.
4. Run this exact compilation command:
   ```cmd
   dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true
   ```
5. Navigate to the compiled output folder:
   `bin/Release/net6.0/win-x64/publish/`
6. Take the **`AOChill.exe`** file and move it to your desktop!

---

## 🚀 How to Run & Use AOChill

1. Right-click **`AOChill.exe`** and select **"Run as administrator"**. *(Required so the tool can safely reach into heavy background processes to clear their RAM).*
2. Look at the dashboard. The script will automatically read your hardware limits and choose the best optimization engine speed for your machine.
3. Type **`2`** and press **Enter** to turn on the Background Loop.
4. Keep the console window open (minimized) and launch your game (like Bloxstrap/Roblox) or office programs.
5. When you are finished, open the console window, type **`3`**, and press **Enter** to safely exit.
