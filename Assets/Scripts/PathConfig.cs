using System;
using System.IO;
using UnityEngine;


[Serializable]
public class PathConfig
{
    public string dataRoot = "";
    public string backupRoot = "";

    private static PathConfig instance;
    public static PathConfig Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Load();
            }
            return instance;
        }
    }

    private static string ConfigFilePath =>
        Path.Combine(Application.dataPath, "..", "soundseeker_config.json");

    private static string DefaultDataRoot =>
        Path.Combine(Application.dataPath, "..", "Data");

    private static string DefaultBackupRoot =>
        Path.Combine(Application.dataPath, "..", "Backup");

    private static PathConfig Load()
    {
        try
        {
            if (File.Exists(ConfigFilePath))
            {
                PathConfig cfg = JsonUtility.FromJson<PathConfig>(File.ReadAllText(ConfigFilePath));
                if (cfg == null)
                {
                    Debug.LogWarning("soundseeker_config.json could not be parsed, using defaults.");
                    return MakeDefaultAndWriteTemplate();
                }
                if (string.IsNullOrEmpty(cfg.dataRoot))
                {
                    cfg.dataRoot = DefaultDataRoot;
                }
                return cfg;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to read soundseeker_config.json, falling back to defaults: " + e.Message);
        }

        return MakeDefaultAndWriteTemplate();
    }

    private static PathConfig MakeDefaultAndWriteTemplate()
    {
        PathConfig fallback = new PathConfig
        {
            dataRoot = DefaultDataRoot,
            backupRoot = ""
        };
        TryWriteTemplate(fallback);
        return fallback;
    }

    private static void TryWriteTemplate(PathConfig cfg)
    {
        try
        {
            if (!File.Exists(ConfigFilePath))
            {
                File.WriteAllText(ConfigFilePath, JsonUtility.ToJson(cfg, true));
                Debug.Log("Wrote default soundseeker_config.json to " + ConfigFilePath);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Could not write default soundseeker_config.json template: " + e.Message);
        }
    }


    public static void MirrorToBackup(string primaryPath, string dataRoot)
    {
        string backupRoot = Instance.backupRoot;
        if (string.IsNullOrEmpty(backupRoot)) return;

        try
        {
            string relative = Path.GetRelativePath(dataRoot, primaryPath);
            string backupPath = Path.Combine(backupRoot, relative);
            string backupDir = Path.GetDirectoryName(backupPath);
            if (!Directory.Exists(backupDir))
            {
                Directory.CreateDirectory(backupDir);
            }
            File.Copy(primaryPath, backupPath, overwrite: true);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Backup copy failed (primary data is unaffected): " + e.Message);
        }
    }
}