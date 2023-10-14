using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class Utility
{
    static readonly string path = Application.persistentDataPath + "/settingsT.cfg";
    [Serializable]
    public struct Settings
    {
        public float CheckRadius, TargetSize, BeamDistance, LockDistance;
    }

    static Settings settings;

    public static void InitSettings(Settings initSettings = new Settings())
    {
        settings = initSettings;
        LoadSettings();
        SaveSettings();
    }

    public static void SaveSettings()
    {
        File.WriteAllText(path, JsonUtility.ToJson(settings,true));
    }
    public static void LoadSettings() { 
        if (!File.Exists(path))
            return;
        settings = JsonUtility.FromJson<Settings>(File.ReadAllText(path));

        Debug.Log(settings.LockDistance);
    }
}
