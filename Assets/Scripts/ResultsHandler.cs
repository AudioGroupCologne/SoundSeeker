using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.IO.Pipes;

public sealed class ResultsHandler
{
    private static string resultPath;
    public static string ResultPath { get => resultPath; set => resultPath = value; }
    public static List<Result> SessionResults
    {
        get
        {
            if (sessionResults == null)
            {
                sessionResults = new List<Result>();
            }
            return sessionResults;
        }
        set => sessionResults = value;
    }

    private static List<Result> sessionResults;

    public static void AddResult(Result result)
    {
        if (SessionResults == null)
        {
            SessionResults = new List<Result>();
        }
        SessionResults.Add(result);
    }

    public static void WriteResultsToFile()
    {
        FileStream fileStream = null;
        if (!Directory.Exists(Application.persistentDataPath + "\\Results"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "\\Results");
        }
        if (!File.Exists(ResultPath))
        {
            fileStream = File.Create(ResultPath);

        }
        else
        {
            fileStream = new FileStream(ResultPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            fileStream.SetLength(0);
        }
        Result[] rArray = new Result[SessionResults.Count];
        int c = 0;
        foreach (Result r in SessionResults)
        {
            rArray[c] = r;
            ++c;
        }

        StringBuilder sb = new StringBuilder();
        sb.Append("[");
        for (int i = 0; i < SessionResults.Count; ++i)
        {
            if (i == SessionResults.Count - 1)
            {
                sb.Append(JsonUtility.ToJson(rArray[i]));
                sb.Append("\n]");
                break;
            }
            sb.Append(JsonUtility.ToJson(rArray[i]));
            sb.Append("\n,\n");
        }
        Debug.Log(sb.ToString());
        byte[] info = new UTF8Encoding(true).GetBytes(sb.ToString());
        fileStream.Write(info, 0, info.Length);
        fileStream.Close();
        Debug.Log("Wrote session results to file");
    }

    public static List<Result> LoadResultFile()
    {
        string jsonRaw = File.ReadAllText(ResultPath);
        Result[] results = GetJsonArray<Result>(jsonRaw);
        SessionResults = new List<Result>();
        foreach (Result r in results)
        {
            SessionResults.Add(r);
        }
        Debug.Log("Loaded " + SessionResults.Count + " results from previous sessions.");
        return SessionResults;
    }

    public static T[] GetJsonArray<T>(string json)
    {
        string newJson = "{ \"array\": " + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.array;
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] array;
    }
}

[Serializable]
public class Result
{
    [SerializeField]
    private string timestampString;
    private SerializableDateTime timestamp;
    [SerializeField]
    private int attemptNumber;
    [SerializeField]
    private float snrAtTarget;
    [SerializeField]
    private float snrAtMaxDistance;
    [SerializeField]
    private float completionTime;
    [SerializeField]
    private float distanceDifference;
    [SerializeField]
    private Vector2 assumedTargetPosition;
    [SerializeField]
    private Vector2 actualtargetPosition;
    [SerializeField]
    private Vector2 playerStartPosition;

    public Result(SerializableDateTime timestamp, int attemptNumber, float snrAtTarget, float snrAtMaxDistance, float completionTime, float distanceDifference, Vector2 assumedTargetPosition, Vector2 actualtargetPosition, Vector2 playerStartPosition)
    {
        this.Timestamp = timestamp;
        this.AttemptNumber = attemptNumber;
        this.SnrAtTarget = snrAtTarget;
        this.SnrAtMaxDistance = snrAtMaxDistance;
        this.CompletionTime = completionTime;
        this.DistanceDifference = distanceDifference;
        this.AssumedTargetPosition = assumedTargetPosition;
        this.ActualtargetPosition = actualtargetPosition;
        this.PlayerStartPosition = playerStartPosition;
    }

    public SerializableDateTime Timestamp
    {
        get { return timestamp; }
        set
        {
            timestamp = value;
            timestampString = timestamp.DateTime.ToString();
        }
    }

    public int AttemptNumber { get => attemptNumber; set => attemptNumber = value; }
    public float SnrAtTarget { get => snrAtTarget; set => snrAtTarget = value; }
    public float CompletionTime { get => completionTime; set => completionTime = value; }
    public float DistanceDifference { get => distanceDifference; set => distanceDifference = value; }
    public Vector2 AssumedTargetPosition { get => assumedTargetPosition; set => assumedTargetPosition = value; }
    public Vector2 ActualtargetPosition { get => actualtargetPosition; set => actualtargetPosition = value; }
    public Vector2 PlayerStartPosition { get => playerStartPosition; set => playerStartPosition = value; }
    public string TimestampString { get => timestampString; set => timestampString = value; }
    public float SnrAtMaxDistance { get => snrAtMaxDistance; set => snrAtMaxDistance = value; }
}


[System.Serializable]
public class SerializableDateTime : IComparable<SerializableDateTime>
{
    [SerializeField]
    private long m_ticks;

    private bool initialized;
    private DateTime m_dateTime;
    public DateTime DateTime
    {
        get
        {
            if (!initialized)
            {
                m_dateTime = new DateTime(m_ticks);
                initialized = true;
            }

            return m_dateTime;
        }
    }

    public SerializableDateTime(DateTime dateTime)
    {
        m_ticks = dateTime.Ticks;
        m_dateTime = dateTime;
        initialized = true;
    }

    public int CompareTo(SerializableDateTime other)
    {
        if (other == null)
        {
            return 1;
        }
        return m_ticks.CompareTo(other.m_ticks);
    }
}
