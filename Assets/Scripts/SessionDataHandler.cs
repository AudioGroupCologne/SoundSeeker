using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor.Experimental.RestService;
using UnityEngine;

public sealed class SessionDataHandler : MonoBehaviour
{
    private static string sessionDataPath;
    public static string SessionDataPath { get => sessionDataPath; set => sessionDataPath = value; }
    public static List<PlayerData> PlayerDataEntries
    {
        get
        {
            if (playerDataEntries == null)
            {
                playerDataEntries = new List<PlayerData>();
            }
            return playerDataEntries;
        }
        set => playerDataEntries = value;
    }

    private static List<PlayerData> playerDataEntries;
    public static SessionData SessionData { get => sessionData; set => sessionData = value; }


    private static SessionData sessionData;

    public static void AddPlayerData(PlayerData dataPoint)
    {
        if (PlayerDataEntries == null)
        {
            PlayerDataEntries = new List<PlayerData>();
        }
        PlayerDataEntries.Add(dataPoint);
    }

    public static void WriteResultsToFile()
    {
        FileStream fileStream = null;
        if (!Directory.Exists(Application.persistentDataPath + "\\SessionData"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "\\SessionData");
        }
        if (!File.Exists(SessionDataPath))
        {
            fileStream = File.Create(SessionDataPath);

        }
        else
        {
            fileStream = new FileStream(SessionDataPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            fileStream.SetLength(0);
        }
        PlayerData[] rArray = new PlayerData[PlayerDataEntries.Count];
        int c = 0;
        foreach (PlayerData r in PlayerDataEntries)
        {
            rArray[c] = r;
            ++c;
        }

        StringBuilder sb = new StringBuilder();
        sb.Append("[");
        sb.Append(JsonUtility.ToJson(sessionData));
        sb.Append(",\n[");
        for (int i = 0; i < PlayerDataEntries.Count; ++i)
        {
            if (i == PlayerDataEntries.Count - 1)
            {
                sb.Append(JsonUtility.ToJson(rArray[i]));
                sb.Append("\n]");
                break;
            }
            sb.Append(JsonUtility.ToJson(rArray[i]));
            sb.Append("\n,\n");
        }
        sb.Append("]");

        Debug.Log(sb.ToString());
        byte[] info = new UTF8Encoding(true).GetBytes(sb.ToString());
        fileStream.Write(info, 0, info.Length);
        fileStream.Close();
        Debug.Log("Wrote session data to file");
    }


}

[Serializable]
public class SessionData
{
    [SerializeField]
    private string _timestampString;
    [SerializeField]
    private SerializableDateTime _timestamp;
    [SerializeField]
    private string _userName;
    [SerializeField]
    private int _attemptNo;
    [SerializeField]
    private float _targetX;
    [SerializeField]
    private float _targetY;

    public SessionData(SerializableDateTime timestamp, string userID, int attemptNo, float targetX, float targetY)
    {
        _timestamp = timestamp;
        _userName = userID;
        _attemptNo = attemptNo;
        TargetX = targetX;
        TargetY = targetY;
    }


    public SerializableDateTime Timestamp
    {
        get { return _timestamp; }
        set
        {
            _timestamp = value;
            _timestampString = _timestamp.DateTime.ToString();
        }
    }
    public string TimestampString { get => _timestampString; set => _timestampString = value; }

    public string UserID { get => _userName; set => _userName = value; }
    public int AttemptNo { get => _attemptNo; set => _attemptNo = value; }
    public float TargetX { get => _targetX; set => _targetX = value; }
    public float TargetY { get => _targetY; set => _targetY = value; }
}

[Serializable]
public class PlayerData
{
    [SerializeField]
    private long _sessionTimeStamp;
    [SerializeField]
    private float _playerPosX;
    [SerializeField]
    private float _playerPosY;
    [SerializeField]
    private float _playerTargetAngle;

    public PlayerData(long sessionTimeStamp, float playerPosX, float playerPosY, float playerTargetAngle)
    {
        _sessionTimeStamp = sessionTimeStamp;
        _playerPosX = playerPosX;
        _playerPosY = playerPosY;
        _playerTargetAngle = playerTargetAngle;
    }

    public long SessionTimeStamp { get => _sessionTimeStamp; set => _sessionTimeStamp = value; }
    public float PlayerPosX { get => _playerPosX; set => _playerPosX = value; }
    public float PlayerPosY { get => _playerPosY; set => _playerPosY = value; }
    public float PlayerTargetAngle { get => _playerTargetAngle; set => _playerTargetAngle = value; }
}