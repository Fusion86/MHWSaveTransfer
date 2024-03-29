﻿using Gameloop.Vdf;
using MHWSaveTransfer.Models;
using Microsoft.Win32;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace MHWSaveTransfer.Helpers
{
    public static class SteamUtility
    {
        private static readonly Lazy<string?> steamRoot = new Lazy<string?>(GetSteamRoot);
        public static string? SteamRoot => steamRoot.Value;

        public const string MONSTER_HUNTER_WORLD_APPID = "582010";
        private static readonly ILogger CtxLog = Log.ForContext(typeof(SteamUtility));

        private static string? GetSteamRoot()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return null;

            string reg = Environment.Is64BitOperatingSystem ? @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Valve\Steam" : @"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam";
            var val = Registry.GetValue(reg, "InstallPath", null);

            if (val != null)
                return (string)val;
            else
                return null;
        }

        public static string? GetMhwSaveDir()
        {
            if (SteamRoot == null)
                return null;

            string userDataRoot = Path.Combine(SteamRoot, "userdata");

            // Each steam account has its own userdata folder, and there could be multiple steam accounts on this computer
            foreach (var userDataPath in Directory.GetDirectories(userDataRoot))
            {
                if (Directory.GetDirectories(userDataPath).FirstOrDefault(x => x.Contains(MONSTER_HUNTER_WORLD_APPID)) != null)
                    return Path.Combine(userDataPath, MONSTER_HUNTER_WORLD_APPID, "remote");
            }

            return null;
        }

        public static string? GetMhwSaveDir(SteamAccount user)
        {
            if (SteamRoot == null)
                return null;

            string userDataPath = Path.Combine(SteamRoot, "userdata", user.SteamId3.ToString());

            if (Directory.GetDirectories(userDataPath).FirstOrDefault(x => x.Contains(MONSTER_HUNTER_WORLD_APPID)) != null)
                return Path.Combine(userDataPath, MONSTER_HUNTER_WORLD_APPID, "remote");

            return null;
        }

        public static List<SteamAccount> GetSteamUsersWithMhw()
        {
            List<SteamAccount> steamUsers = new List<SteamAccount>();

            if (SteamRoot == null)
                return steamUsers; // Empty list

            string configPath = Path.Combine(SteamRoot, "config", "loginusers.vdf");

            try
            {
                // Yikes, dynamic data warning. We JavaScript now.
                // This throws an error when you enable "catch all errors" in the debugger, but this does actually work.
                string configText = File.ReadAllText(configPath);
                var vdf = VdfConvert.Deserialize(configText);
                foreach (dynamic user in vdf.Value)
                {
                    SteamAccount account = new SteamAccount
                    {
                        SteamId64 = long.Parse(user.Key),
                        AccountName = user.Value.AccountName.Value,
                        PersonaName = user.Value.PersonaName.Value,
                        //RememberPassword = user.Value.RememberPassword.Value == "1",
                        //MostRecent = user.Value.mostrecent.Value == "1",
                        //Timestamp = long.Parse(user.Value.Timestamp.Value)
                    };

                    // Check if user has MHW saves
                    if (GetMhwSaveDir(account) != null)
                        steamUsers.Add(account);
                }
            }
            catch (Exception ex)
            {
                CtxLog.Error(ex, ex.Message);
            }

            return steamUsers;
        }
    }
}
