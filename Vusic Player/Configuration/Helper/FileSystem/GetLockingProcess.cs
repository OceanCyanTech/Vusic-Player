using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Vusic_Player.Configuration.Helper.FileSystem
{
    public class GetLockingProcess
    {
        [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
        private static extern int RmStartSession(out uint pSessionHandle, uint dwSessionFlags, string strSessionKey);

        [DllImport("rstrtmgr.dll")]
        private static extern int RmEndSession(uint pSessionHandle);

        [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
        private static extern int RmRegisterResources(uint dwSessionHandle, uint nFiles, string[] rgsFilenames,
                                                      uint nApplications, RM_UNIQUE_PROCESS[] rgApplications,
                                                      uint nServices, string[] rgsServiceNames);

        [DllImport("rstrtmgr.dll")]
        private static extern int RmGetList(uint dwSessionHandle, out uint pnProcInfoNeeded,
                                            ref uint pnProcInfo, [In, Out] RM_PROCESS_INFO[] rgAffectedApps,
                                            ref uint lpdwRebootReasons);

        // --- The Required Structs ---
        private const int RmRebootReasonNone = 0;

        [StructLayout(LayoutKind.Sequential)]
        public struct RM_UNIQUE_PROCESS
        {
            public int dwProcessId;
            public System.Runtime.InteropServices.ComTypes.FILETIME ProcessStartTime;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct RM_PROCESS_INFO
        {
            public RM_UNIQUE_PROCESS Process;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string strAppName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string strServiceShortName;
            public int ApplicationType;
            public uint AppStatus;
            public uint TSRessionId;
            [MarshalAs(UnmanagedType.Bool)]
            public bool bRestartable;
        }
        public static List<Process> GetLockingProcesses(string path)
        {
            uint handle;
            string key = Guid.NewGuid().ToString();
            List<Process> processes = new List<Process>();

            // 1. Start Session
            int res = RmStartSession(out handle, 0, key);
            if (res != 0) return processes;

            try
            {
                string[] resources = { path };
                // 2. Register File
                res = RmRegisterResources(handle, (uint)resources.Length, resources, 0, null!, 0, null!);
                if (res != 0) return processes;

                // 3. Get Process Info
                uint nProcInfoNeeded = 0;
                uint nProcInfo = 0;
                uint rebootReasons = 0;

                // First call to get the count of processes
                res = RmGetList(handle, out nProcInfoNeeded, ref nProcInfo, null!, ref rebootReasons);

                if (res == 234) // ERROR_MORE_DATA
                {
                    RM_PROCESS_INFO[] processInfo = new RM_PROCESS_INFO[nProcInfoNeeded];
                    nProcInfo = nProcInfoNeeded;

                    // Second call to get the actual data
                    res = RmGetList(handle, out nProcInfoNeeded, ref nProcInfo, processInfo, ref rebootReasons);

                    for (int i = 0; i < nProcInfo; i++)
                    {
                        try { processes.Add(Process.GetProcessById(processInfo[i].Process.dwProcessId)); }
                        catch { /* Process might have closed already */ }
                    }
                }
            }
            finally
            {
                RmEndSession(handle); // 4. Cleanup
            }

            return processes;
        }

    }

}
