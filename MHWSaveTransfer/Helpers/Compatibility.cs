using Microsoft.Win32;
using System;
using System.Windows;

namespace MHWSaveTransfer.Helpers
{
    // Taken from https://stackoverflow.com/a/46062518/2125072
    static class Compatibility
    {
        public enum DotNetRelease
        {
            // Remember, the order matters
            NotFound,
            NET45,
            NET451,
            NET452,
            NET46,
            NET461,
            NET462,
            NET47,
            NET471,
            NET472
        }

        public static bool IsCompatible(DotNetRelease req)
        {
            DotNetRelease r = GetRelease();
            if (r < req)
            {
                MessageBox.Show($"This this application requires {req.ToString()} or greater.");
                return false;
            }
            return true;
        }

        public static DotNetRelease GetRelease(int release = default(int))
        {
            int r = release != default(int) ? release : GetVersion();
            if (r >= 461808) return DotNetRelease.NET472;
            if (r >= 461308) return DotNetRelease.NET471;
            if (r >= 460798) return DotNetRelease.NET47;
            if (r >= 394802) return DotNetRelease.NET462;
            if (r >= 394254) return DotNetRelease.NET461;
            if (r >= 393295) return DotNetRelease.NET46;
            if (r >= 379893) return DotNetRelease.NET452;
            if (r >= 378675) return DotNetRelease.NET451;
            if (r >= 378389) return DotNetRelease.NET45;
            return DotNetRelease.NotFound;
        }

        public static int GetVersion()
        {
            int release = 0;
            using (RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full\\"))
            {
                release = Convert.ToInt32(key.GetValue("Release"));
            }
            return release;
        }
    }
}
