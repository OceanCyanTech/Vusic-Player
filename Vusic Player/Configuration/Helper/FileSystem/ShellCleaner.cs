using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Vusic_Player.Configuration.Helper.FileSystem
{
    public static class ShellDestinationsManager
    {
        // Native Windows CLSID and IID for ApplicationDestinations
        private static readonly Guid CLSID_ApplicationDestinations = new("86c419d0-49ba-410e-a6a8-9782bd5264b9");
        private static readonly Guid IID_IApplicationDestinations = new("12337d35-9448-48dd-9770-7fba79870798");

        [ComImport]
        [Guid("12337d35-9448-48dd-9770-7fba79870798")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IApplicationDestinations
        {
            // Associates destination list operations with an explicit AppUserModelID
            [PreserveSig]
            int SetAppID([MarshalAs(UnmanagedType.LPWStr)] string pszAppID);

            // Removes a single IShellItem or IShellLink
            [PreserveSig]
            int RemoveDestination([MarshalAs(UnmanagedType.IUnknown)] object punk);

            // Clears the entire list of recent/frequent items for this app
            [PreserveSig]
            int RemoveAllDestinations();
        }

        /// <summary>
        /// Clears all items registered to this application's recent destinations.
        /// </summary>
        /// <param name="appUserModelId">
        /// Optional explicit AUMID. Pass null or string.Empty if running unpackaged 
        /// or if the process already defines its AUMID.
        /// </param>
        public static bool ClearRecentDestinations(string? appUserModelId = null)
        {
            Type? comType = Type.GetTypeFromCLSID(CLSID_ApplicationDestinations);
            if (comType == null)
            {
                Debug.WriteLine("[ShellDestinations] Failed to get COM type.");
                return false;
            }

            object? instance = Activator.CreateInstance(comType);
            if (instance is not IApplicationDestinations destinations)
            {
                Debug.WriteLine("[ShellDestinations] Failed to cast to IApplicationDestinations.");
                return false;
            }

            try
            {
                // If an explicit AUMID is supplied, target that specific list
                if (!string.IsNullOrWhiteSpace(appUserModelId))
                {
                    int hrSet = destinations.SetAppID(appUserModelId);
                    if (hrSet != 0)
                    {
                        Debug.WriteLine($"[ShellDestinations] SetAppID returned HRESULT: 0x{hrSet:X8}");
                    }
                }

                int hrRemove = destinations.RemoveAllDestinations();
                if (hrRemove == 0) // S_OK
                {
                    Debug.WriteLine("[ShellDestinations] Successfully removed all recent destinations.");
                    return true;
                }

                Debug.WriteLine($"[ShellDestinations] RemoveAllDestinations returned HRESULT: 0x{hrRemove:X8}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ShellDestinations] Error executing COM call: {ex.Message}");
                return false;
            }
            finally
            {
                Marshal.ReleaseComObject(destinations);
            }
        }
    }
}
