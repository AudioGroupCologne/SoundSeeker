using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        string sessionDir = Path.GetDirectoryName(SessionDataPath);
        if (!Directory.Exists(sessionDir))
        {
            Directory.CreateDirectory(sessionDir);
        }

        StringBuilder sb = new StringBuilder();
        sb.Append("[");
        sb.Append(JsonUtility.ToJson(sessionData));
        sb.Append(",\n[");
        for (int i = 0; i < PlayerDataEntries.Count; ++i)
        {
            sb.Append(JsonUtility.ToJson(PlayerDataEntries[i]));
            sb.Append(i == PlayerDataEntries.Count - 1 ? "\n]" : "\n,\n");
        }
        sb.Append("]");

        string tempPath = SessionDataPath + ".tmp";
        File.WriteAllText(tempPath, sb.ToString(), new UTF8Encoding(true));

        if (File.Exists(SessionDataPath))
            File.Replace(tempPath, SessionDataPath, null);
        else
            File.Move(tempPath, SessionDataPath);

        PathConfig.MirrorToBackup(SessionDataPath, PathConfig.Instance.dataRoot);

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