using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;


public sealed class SettingsHandler
{
    private static string configuarationPath;

    public static string ConfiguarationPath { get => configuarationPath; set => configuarationPath = value; }

    private static PlayerSettings playerSettings;

    public static PlayerSettings PlayerSettings { get => playerSettings; set => playerSettings = value; }


    public static void WriteConfigToFile()
    {
        FileStream fileStream = null;
        if(!Directory.Exists(Application.persistentDataPath + "\\Configuration"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "\\Configuration");
        }
        if(!File.Exists(ConfiguarationPath))
        {
            fileStream = File.Create(ConfiguarationPath);
        }
        else
        {
            fileStream = new FileStream(ConfiguarationPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            fileStream.SetLength(0);
        }
        string jsonString = JsonUtility.ToJson(playerSettings);
        byte[] info = new UTF8Encoding(true).GetBytes(jsonString);
        fileStream.Write(info, 0, info.Length);
        fileStream.Close();
        Debug.Log("Wrote player configuration to file");
    }

    public static void LoadSettingsFromFile()
    {
        string jsonRaw = File.ReadAllText(ConfiguarationPath);
        PlayerSettings = JsonUtility.FromJson<PlayerSettings>(jsonRaw);
        Debug.Log("Loaded player configuration from file. Current user: " + PlayerSettings.UserID + ", " + PlayerSettings.UserName);
    }

}

[Serializable]
public class PlayerSettings
{
    [SerializeField]
    private short _userID;
    [SerializeField]
    private string _userName;
    [SerializeField]
    private int _completedRounds;
    [SerializeField]
    private float _closeSNR;
    [SerializeField]
    private float _farSNR;
    [SerializeField]
    private float _initialDistractorDb;
    public PlayerSettings(short userID, string userName, int completedRounds, float closeSNR, float farSNR, float initialDistractorDb)
    {
        UserID = userID;
        UserName = userName;
        CompletedRounds = completedRounds;
        CloseSNR = closeSNR;
        FarSNR = farSNR;
        InitialDistractorDb = initialDistractorDb;

    }

    public short UserID { get => _userID; set => _userID = value; }
    public string UserName { get => _userName; set => _userName = value; }
    public int CompletedRounds { get => _completedRounds; set => _completedRounds = value; }
    public float CloseSNR { get => _closeSNR; set => _closeSNR = value; }
    public float FarSNR { get => _farSNR; set => _farSNR = value; }
    public float InitialDistractorDb { get => _initialDistractorDb; set => _initialDistractorDb = value; }
}
