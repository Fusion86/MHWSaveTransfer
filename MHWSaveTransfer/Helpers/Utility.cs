using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;

namespace MHWSaveTransfer.Helpers
{
    // Taken from https://github.com/Fusion86/MHWAppearanceEditor/blob/master/MHWAppearanceEditor/Helpers/Utility.cs
    static class Utility
    {
        public const string MONSTER_HUNTER_WORLD_APPID = "582010";

        public static string GetSteamRoot()
        {
            string reg = Environment.Is64BitOperatingSystem ? @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Valve\Steam" : @"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam";
            object val = Registry.GetValue(reg, "InstallPath", null);

            if (val != null)
                return (string)val;
            else
                return null;
        }

        /// <summary>
        /// Returns path to savedata dir for first first steam user who played the game (and thus has save data)
        /// </summary>
        /// <returns></returns>
        public static string GetMhwSaveDir()
        {
            string steamRoot = GetSteamRoot();

            if (steamRoot == null)
                return null;

            string userDataRoot = Path.Combine(steamRoot, "userdata");

            // Not sure why this needs to be inside a try-catch, but it should solve some crashes for some users.
            try
            {
                // Each steam account has its own userdata folder, and there could be multiple steam accounts on this computer
                foreach (var userDataPath in Directory.GetDirectories(userDataRoot))
                {
                    if (Directory.GetDirectories(userDataPath).FirstOrDefault(x => x.Contains(MONSTER_HUNTER_WORLD_APPID)) != null)
                        return Path.Combine(userDataPath, MONSTER_HUNTER_WORLD_APPID, "remote");
                }
            }
            catch { }

            return null;
        }
    }
}
