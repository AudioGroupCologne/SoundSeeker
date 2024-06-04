using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEditor.XR.LegacyInputHelpers;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit;
using static UnityEditor.PlayerSettings;

public class GameController : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] GameObject gamePlane;
    [SerializeField] GameObject singleTargetGameObject;
    [SerializeField] GameObject distractorGameObject;
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] GameObject uiObject;
    [SerializeField] GameObject distractorAudioClips;
    private DistanceCalculator distanceCalculator;
    private GameGrid grid;
    public GameGrid Grid
    {
        get { return grid; }
        set { grid = value; }
    }
    public float SNRAttarget, SNRAtMaxDistance, initialDistractorDb;

    private int gridSizeFactor;
    private StimulusController StimulusController;


    //UI Elements
    private TextMeshProUGUI resultText;
    private TextMeshProUGUI infoText;
    private GameObject startButton;

    private GameObject mainCamera;
    private bool sessionRunning;
    private bool confirmTargetFound;
    private Vector2 ppcCurrent; //Player - Polygon centroid current angle
    private Vector2 ppcStart; //Player - Polygon centroid starting angle
    private float ppcAngle; //Angle to detect a 360° rotation to see if the player has circled the polygon
    private bool fullCircle;
    private short clockwise; //1 if player is moving in clock wise motion, -1 if counter clock wise
    private LineRenderer lineRenderer;

    // For debug purposes: Track player position as well as closest point on the polygon to the player
    private Vector3 p = new Vector3(0, 0, 0);
    private Vector3 cp = new Vector3(1, 1, 1);
    private float repeatRate;
    IEnumerator rateLoop;
    IEnumerator constantLoop;
    public short participant_id;
    public int numberOfRounds;
    public int numberOfConsideredTrials; //Number or previous results that are considered take the avg. distance of to decide whether SNR should be lowered or increased
    public float movementSpeed;
    private int attemptNumber;
    private Vector2 playerStartPosition;
    private SingleTarget singleTarget;

    private List<Result> resultsFromFile;
    private List<PlayerData> playerData;

    public bool debugMode = false;
    private bool pathLongEnough;


    // Start is called before the first frame update
    void Start()
    {
        gridSizeFactor = 4;
        ResultsHandler.ResultPath = Application.persistentDataPath + "\\Results\\" + participant_id + "_results.json";
        SettingsHandler.ConfiguarationPath = Application.persistentDataPath + "\\Configuration\\" + participant_id + "_configuration.json";
        LoadConfiguration();
        LoadPreviousResults();
        SessionDataHandler.SessionDataPath = BuildSessionDataPath();

        InitSessionData(SettingsHandler.PlayerSettings.UserID, attemptNumber);
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        //UI init
        resultText = GameObject.FindGameObjectWithTag("ResultText").GetComponent<TextMeshProUGUI>();
        infoText = GameObject.FindGameObjectWithTag("InfoText").GetComponent<TextMeshProUGUI>();
        startButton = GameObject.FindGameObjectWithTag("StartButton");
        AdjustToPlayerPosition(0, 10.0f, 2.0f);


        //Player and target init.
        Vector2 playerPos, targetPos;
        RandomizePositions(out playerPos, out targetPos);
        singleTarget = new SingleTarget(targetPos);
        singleTargetGameObject.transform.position = new Vector3(targetPos.x, mainCamera.transform.position.y, targetPos.y);


        float sideLengthMeter = gamePlane.transform.localScale.x; //Length of the square game plane in meters
        int gridSize = (int)(gridSizeFactor * sideLengthMeter); //Size of GameGrid Array
        //Create internal position grid and place the player as well as the target; mainly necessary for polygon game variant
        Grid = new GameGrid(gridSize, gridSize, sideLengthMeter / gridSize, new Vector3(gridSize / 2, 0, gridSize / 2), GetVector2Position(player.transform.position), singleTarget);

        if (debugMode)
        {
            if (Grid.Target.Type == 1)
            {
                singleTargetGameObject.GetComponent<MeshRenderer>().enabled = true;
            }
        }
        else
        {
            singleTargetGameObject.GetComponent<MeshRenderer>().enabled = false;
        }

        List<AudioClip> distClips = distractorAudioClips.GetComponent<DistractorAudioFiles>().distractorClipList;
        StimulusController = new StimulusController(player, Grid, sideLengthMeter, StimulusController.Mode.SingleTarget, new DistanceCalculator(), singleTargetGameObject.GetComponent<AudioSource>(), distractorGameObject.GetComponent<AudioSource>(), audioMixer, distClips);

        //Read SNR values from config (not from editor) 
        StimulusController.Close_SNR = SettingsHandler.PlayerSettings.CloseSNR;
        StimulusController.Far_SNR = SettingsHandler.PlayerSettings.FarSNR;
        StimulusController.InitialDistractorDb = SettingsHandler.PlayerSettings.InitialDistractorDb;

        /**
        //Debug: Read SNR values from editor rather than config file
        StimulusController.Close_SNR = SNRAttarget;
        StimulusController.Far_SNR = SNRAtMaxDistance;
        StimulusController.InitialDistractorDb = initialDistractorDb;
        **/
        distanceCalculator = new DistanceCalculator();

    }

    private string BuildSessionDataPath()
    {
        return Application.persistentDataPath + "\\SessionData\\" + SettingsHandler.PlayerSettings.UserID + "\\" + attemptNumber + ".json";
    }

    private void LoadConfiguration()
    {
        if (!File.Exists(SettingsHandler.ConfiguarationPath))
        {
            PlayerSettings settings = new PlayerSettings(100, "test", 0, SNRAttarget, SNRAtMaxDistance, initialDistractorDb);
            SettingsHandler.PlayerSettings = settings;
            SettingsHandler.WriteConfigToFile();
            Debug.Log("Player settings file does not exist. Creating new settings file.");
        }
        else
        {
            SettingsHandler.LoadSettingsFromFile();
            //Overwrite values from editor with saved settings from file.
            attemptNumber = SettingsHandler.PlayerSettings.CompletedRounds;
            SNRAttarget = SettingsHandler.PlayerSettings.CloseSNR;
            SNRAtMaxDistance = SettingsHandler.PlayerSettings.FarSNR;
        }
    }

    private void LoadPreviousResults()
    {
        if (!Directory.Exists(Application.persistentDataPath + "\\Results"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "\\Results");
        }
        if (File.Exists(ResultsHandler.ResultPath))
        {
            resultsFromFile = ResultsHandler.LoadResultFile();
            attemptNumber = resultsFromFile[resultsFromFile.Count - 1].AttemptNumber + 1;
            Debug.Log("Previous results loaded from file");
        }
        else
        {
            attemptNumber = 1;
            resultsFromFile = ResultsHandler.SessionResults;
            Debug.Log("Result file does not exist. Creating initial file after training");
        }

    }

    private void InitSessionData(int participant_id, int attemptNo)
    {
        if (!Directory.Exists(Application.persistentDataPath + "\\SessionData\\" + participant_id + "\\"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "\\SessionData\\" + participant_id + "\\");

        }
        playerData = new List<PlayerData>();
    }

    private float GetAverageDistanceDifference(int numOfConsideredResults)
    {
        if (numOfConsideredResults <= ResultsHandler.SessionResults.Count)
        {
            float avgDistancePrevTrials = 0;
            int index = attemptNumber - numOfConsideredResults;
            for (int i = 0; i < numOfConsideredResults; ++i)
            {
                avgDistancePrevTrials += resultsFromFile[index].DistanceDifference;
                ++index;
            }
            return avgDistancePrevTrials /= numOfConsideredResults;
        }
        else
        {
            Debug.LogError("Not enough previous results to calculate distance average over the last " + numOfConsideredResults + " results.");
            return float.MaxValue;
        }

    }

    private void RoundInit()
    {
        //Player and target init.
        Vector2 playerPos, targetPos;
        RandomizePositions(out playerPos, out targetPos);
        singleTargetGameObject.transform.position = new Vector3(targetPos.x, mainCamera.transform.position.y, targetPos.y);
        singleTarget.Position = targetPos;
        singleTargetGameObject.GetComponent<AudioSource>().Stop();
        distractorGameObject.GetComponent<AudioSource>().Stop();
        player.transform.position = new Vector3(playerPos.x, 0.5f, playerPos.y);
        playerStartPosition = new Vector3(playerPos.x, 0.5f, playerPos.y);
        AdjustToPlayerPosition(0, 10.0f, 2.0f);
        SessionDataHandler.SessionData = new SessionData(new SerializableDateTime(DateTime.Now), SettingsHandler.PlayerSettings.UserName, attemptNumber, singleTargetGameObject.transform.position.x, singleTargetGameObject.transform.position.z);
        SessionDataHandler.SessionDataPath = BuildSessionDataPath();
        playerData = new List<PlayerData>();


        PlayerMovement.onConfirmPosition += ConfirmTargetPosition;

    }


    //Executed when Start-Button is clicked
    public void StartHandler()
    {
        if (numberOfRounds > 0)
        {
            Debug.Log("Start");
            RoundInit();
            sessionRunning = true;
            confirmTargetFound = false;
            rateLoop = SessionLoop(0.3f);
            constantLoop = RunSession();
            uiObject.SetActive(false);
            singleTargetGameObject.GetComponent<MeshRenderer>().enabled = false;
            GameObject.FindGameObjectWithTag("RightHand").GetComponent<XRInteractorLineVisual>().enabled = false;


            StimulusController.StartPlaying();
            StartCoroutine(rateLoop);
            StartCoroutine(constantLoop);
        }
        else
        {
            Debug.Log("No rounds remaining.");
        }



    }

    public void OnDisable()
    {
        PlayerMovement.onConfirmPosition -= ConfirmTargetPosition;
    }

    //Actions that are continuously executed
    public IEnumerator RunSession()
    {
        float startTime = Time.time;
        while (sessionRunning)
        {
            if (Grid.Target.Type == 0)
            {
                UpdateAngle();

                if (fullCircle && pathLongEnough)
                {
                    float totalDistance = CalculateTotalDistanceScore();
                    Debug.Log("Total: " + totalDistance);
                    sessionRunning = false;
                    StimulusController.StopPlaying();
                    PlayerMovement.onConfirmPosition -= ConfirmTargetPosition;

                    StopCoroutine(rateLoop);
                    StopCoroutine(constantLoop);
                }
            }
            else if (Grid.Target.Type == 1)
            {
                yield return new WaitForSeconds(0f);
                if (confirmTargetFound)
                {
                    sessionRunning = false;
                    StimulusController.StopPlaying();
                    StopCoroutine(rateLoop);
                    StopCoroutine(constantLoop);
                    PlayerMovement.onConfirmPosition -= ConfirmTargetPosition;
                    numberOfRounds--;
                    singleTargetGameObject.GetComponent<MeshRenderer>().enabled = true;
                    GameObject.FindGameObjectWithTag("RightHand").GetComponent<XRInteractorLineVisual>().enabled = true;

                    AdjustToPlayerPosition(0, 10.0f, 2.0f);
                    AdjustToPlayerPosition(0, 10.0f, 2.0f);
                    uiObject.SetActive(true);
                    float distance = (float)Math.Round(Vector2.Distance(singleTarget.Position, GetVector2Position(player.transform.position)), 2);
                    AdjustToPlayerPosition(0, 10.0f, 2.0f);
                    float t = (float)Math.Round(Time.time - startTime, 2);
                    resultText.text = "Round over. Time needed: " + t.ToString("n2") + " seconds<br>Distance from target: " + distance.ToString("n2") + "m";
                    LogResults(t, distance);

                    //SessionDataHandler.PlayerDataEntries = playerData;
                    //SessionDataHandler.WriteResultsToFile();


                    GameObject.Find("MaterialController").GetComponent<MaterialContainer>().SetTargetColor(distance);

                    Debug.Log("Stopped. Remaining rounds: " + numberOfRounds);
                    Debug.Log("Distance difference: " + distance);


                    // Possibly ugly work around to adjust SNR every numberOfConsideredTrials rounds.
                    if (attemptNumber % numberOfConsideredTrials == 0 && attemptNumber != 0)
                    {
                        float avgDist = GetAverageDistanceDifference(numberOfConsideredTrials);
                        if (avgDist == float.MaxValue)
                        {
                            Debug.Log("Not enough trials to calc avg. over the last " + numberOfConsideredTrials + " trials. Please check settings.");

                        }
                        //Player was closer than 1m to the target on avg. SNR is decreased by 2db
                        else if (avgDist < 1)
                        {
                            
                            StimulusController.Close_SNR -= 2;
                            SettingsHandler.PlayerSettings.CloseSNR = StimulusController.Close_SNR;

                            //Keep 20db "distance" between closeSNR and farSNR
                            if (Math.Abs(StimulusController.Close_SNR - StimulusController.Far_SNR) >= 20)
                            {
                                StimulusController.Far_SNR -= 2;
                                SettingsHandler.PlayerSettings.FarSNR = StimulusController.Far_SNR;
                            }
                            Debug.Log("Average distance over the last " + numberOfConsideredTrials + ": " + avgDist + ".\n Decreasing SNR by 2db");

                        }
                        //Player was between 1 and 3m to the target on avg. SNR is increased by 2db unless it would be louder than the babble (0dB SNR)
                        else if (avgDist > 1 && avgDist < 3)
                        {
                            

                            if (!(StimulusController.Close_SNR + 2 > initialDistractorDb))
                            {
                                StimulusController.Close_SNR += 2;
                                SettingsHandler.PlayerSettings.CloseSNR = StimulusController.Close_SNR;
                                Debug.Log("Average distance over the last " + numberOfConsideredTrials + ": " + avgDist + ".\n Increasing SNR by 2db");
                            }
                            else
                            {
                                StimulusController.Close_SNR = 0;
                                SettingsHandler.PlayerSettings.CloseSNR = StimulusController.Close_SNR;
                            }
                            if (!(StimulusController.Far_SNR + 2 > initialDistractorDb))
                            {
                                StimulusController.Far_SNR += 2;
                                SettingsHandler.PlayerSettings.FarSNR = StimulusController.Far_SNR;
                            }
                            else
                            {
                                StimulusController.Far_SNR = 0;
                                SettingsHandler.PlayerSettings.FarSNR = StimulusController.Far_SNR;
                            }
                        }
                        //Player was more than 3m away on avg. SNR is increased by 4db
                        else
                        {

                            if (!(StimulusController.Close_SNR + 4 > initialDistractorDb))
                            {
                                StimulusController.Close_SNR += 4;
                                SettingsHandler.PlayerSettings.CloseSNR = StimulusController.Close_SNR;
                                Debug.Log("Average distance over the last " + numberOfConsideredTrials + ": " + avgDist + ".\n Increasing SNR by 4db");
                            }
                            else
                            {
                                StimulusController.Close_SNR = 0;
                                SettingsHandler.PlayerSettings.CloseSNR = StimulusController.Close_SNR;
                            }
                            if (!(StimulusController.Far_SNR + 4 > initialDistractorDb))
                            {
                                StimulusController.Far_SNR += 4;
                                SettingsHandler.PlayerSettings.FarSNR = StimulusController.Far_SNR;
                            }
                            else
                            {
                                StimulusController.Far_SNR = 0;
                                SettingsHandler.PlayerSettings.FarSNR = StimulusController.Far_SNR;
                            }
                        }
                    }
                    SessionDataHandler.PlayerDataEntries = playerData;
                    SessionDataHandler.WriteResultsToFile();
                    attemptNumber++;
                    //SessionDataHandler.SessionDataPath = SessionDataHandler.SessionDataPath = Application.persistentDataPath + "\\Configuration\\" + SettingsHandler.PlayerSettings.UserName + "_" + attemptNumber + ".json";

                    if (numberOfRounds == 0)
                    {
                        Debug.Log("Training is concluded");
                        startButton.SetActive(false);
                        GameObject.FindGameObjectWithTag("RightHand").GetComponent<XRInteractorLineVisual>().enabled = true;
                        singleTargetGameObject.GetComponent<MeshRenderer>().enabled = true;
                        resultText.text = "Time needed: " + t.ToString("n2") + "seconds<br>Distance from target: " + distance.ToString("n2") + "m";
                        infoText.text = "Training concluded.";
                        ResultsHandler.WriteResultsToFile();
                        SettingsHandler.WriteConfigToFile();
                        SessionDataHandler.WriteResultsToFile();
                    }
                }

            }
            if (debugMode)
            {
                if (Grid.Target.Type == 0)
                {
                    DrawPolygon();
                    Debug.DrawLine(p, cp, Color.red); //Shortest line between player and polygon
                    DrawPlayerPath();
                }

            }

        }

    }

    //Have all actions that need to be executed at a certain rate here
    public IEnumerator SessionLoop(float repeatRate)
    {
        DateTime now = DateTime.Now;


        //uiObject.SetActive(true); //set active for debugging
        while (true)
        {
            yield return new WaitForSeconds(repeatRate);
            StimulusController.SendStimulusData();
            float distance = distanceCalculator.MinDistanceToTarget(Grid.Target, GetVector2Position(player.transform.position));
            Grid.SetGridValue(GetVector2Position(player.transform.position), distance);
            //Debug distance calc
            //AdjustToPlayerPosition(0, 10.0f, 2.0f);
            //resultText.text = "distance:  " + distance;
            GameObject.FindGameObjectWithTag("RightHand").GetComponent<XRInteractorLineVisual>().enabled = false;
            Grid.UpdateDistanceTravelled(GetVector2Position(player.transform.position));

            //Calculate angle deviation from player look direction to the target position
            Vector2 vecToTarget = new Vector2(singleTargetGameObject.transform.position.x, singleTargetGameObject.transform.position.z) - (new Vector2(mainCamera.transform.position.x, mainCamera.transform.position.z));
            float goalAngleDeviation = -Vector2.SignedAngle(new Vector2(mainCamera.transform.forward.x, mainCamera.transform.forward.z), vecToTarget);

            if (goalAngleDeviation < 0)
            {
                goalAngleDeviation += 360f;
            }
            now = DateTime.Now;
            playerData.Add(new PlayerData(now.ToFileTime(), player.transform.position.x, player.transform.position.z, goalAngleDeviation));

            //AdjustToPlayerPosition(0, 10.0f, 2.0f);
            //resultText.text = "angle: " + goalAngleDeviation;

            //Debug.Log(distanceCalculator.MinDistanceToTarget(singleTarget, GetVector2Position(player.transform.position)));
            //Check if player has drawn circle and the total travelled path length is greater than sum of polygon sides
            //pathLongEnough = Grid.GetDistanceTravelled() > (Grid.Target.GetSideLength() + ppcStart.magnitude) ? true : false; //Does not work  with new target class structure
            if (Grid.Target.Type == 0)
            {
                pathLongEnough = Grid.GetDistanceTravelled() > (ppcStart.magnitude) ? true : false;
            }
            else
            {
                //confirmTargetFound = (Vector3.Distance(singleTargetGameObject.transform.position, player.transform.position) < 1f) ? true : false;
            }
        }

    }

    private void LogResults(float time, float distance)
    {
        Debug.Log("Time needed: " + time);

        Result result = new Result
            (new SerializableDateTime(DateTime.Now),
            attemptNumber,
            StimulusController.Close_SNR,
            StimulusController.Far_SNR,
            time,
            Vector2.Distance(GetVector2Position(player.transform.position), Grid.Target.Position),
            GetVector2Position(player.transform.position),
            Grid.Target.Position,
            playerStartPosition);

        ResultsHandler.AddResult(result);

        SettingsHandler.PlayerSettings.CloseSNR = StimulusController.Close_SNR;
        SettingsHandler.PlayerSettings.FarSNR = StimulusController.Far_SNR;

    }




    //Randomize starting positions for player and target
    private void RandomizePositions(out Vector2 playerPos, out Vector2 targetPos)
    {
        int gridSize = ((int)gamePlane.transform.localScale.x / 2);
        System.Random random = new System.Random();
        int tx = random.Next(-gridSize + 2, gridSize - 2);
        int ty = random.Next(-gridSize + 2, gridSize - 2);

        bool validTargetPos = false;
        int radius = 15;
        double theta = random.NextDouble() * 2 * Math.PI;
        double px = tx + radius * Math.Cos(theta);
        double py = ty + radius * Math.Sin(theta);

        while (!validTargetPos)
        {
            theta = random.NextDouble() * 2 * Math.PI;
            px = tx + radius * Math.Cos(theta);
            py = ty + radius * Math.Sin(theta);

            //Check if player starting position is still on the gird
            if (px > (-gridSize + 2) && px < (gridSize - 2))
            {
                if (py > (-gridSize + 2) && py < (gridSize - 2))
                    validTargetPos = true;
            }

        }
        playerPos.x = (int)px; playerPos.y = (int)py;
        targetPos.x = tx; targetPos.y = ty;

    }


    // Updates player angle compared starting position to monitor if a 360° rotation has been performed
    private void UpdateAngle()
    {
        ppcCurrent = Grid.Target.Position - GetVector2Position(player.transform.position);
        float angleCurrent = Vector2.SignedAngle(ppcCurrent, ppcStart);
        if (angleCurrent < 0)
        {
            angleCurrent += 360; //Translate negative angles to 180-360°
        }

        //Set inital rotation 
        if (clockwise == 0 && (angleCurrent > 30 && angleCurrent < 180))
        {
            clockwise = 1;
            Debug.Log("CW");
        }
        else if (clockwise == 0 && (angleCurrent < 330 && angleCurrent > 180))
        {
            clockwise = -1;
            ppcAngle = 331;
            Debug.Log("CCW");

        }

        //Update angle if player has progressed in circle
        if (clockwise == 1 && angleCurrent > ppcAngle)
        {
            ppcAngle = angleCurrent;
            if (ppcAngle > 355) fullCircle = true;
        }

        if (clockwise == -1 && angleCurrent < ppcAngle)
        {
            ppcAngle = angleCurrent;
            if (ppcAngle < 5) fullCircle = true;
        }
    }

    //Draw polygon for debug purposes;
    private void DrawPolygon()
    {
        lineRenderer.startWidth = 1f;
        lineRenderer.endWidth = 1f;
        lineRenderer.useWorldSpace = true;

        List<Vector3> vector3s = new List<Vector3>();

        foreach (var vec in Grid.Target.Vertices)
        {
            vector3s.Add(new Vector3(vec.x, 0.5f, vec.y));
        }
        Vector3 v1 = new Vector3(Grid.Target.Vertices[0].x, 0.5f, Grid.Target.Vertices[0].y);
        vector3s.Add(v1);
        lineRenderer.positionCount = vector3s.Count;
        lineRenderer.SetPositions(vector3s.ToArray());

    }

    void OnGUI()
    {
        //GUI.Label(new Rect(0, 0, 100, 100), "Dist:" + CalculateTotalDistanceScore().ToString());
    }


    //TODO: Currently only works in 2D version
    private void DrawPlayerPath()
    {
        for (int i = 0; i < grid.GridArray.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GridArray.GetLength(1); j++)
            {
                if (grid.GridArray[i, j] != 0)
                {
                    Debug.DrawLine(grid.GridToWorldPosition(i, j), grid.GridToWorldPosition(i, j) + new Vector3(0.1f, 0, 0.1f), Color.blue);
                }
            }
        }
    }

    public void RecDistData(Vector2 p, Vector2 cp)
    {
        this.p = new Vector3(p.x, 0, p.y);
        this.cp = new Vector3(cp.x, 0, cp.y);
    }

    //Helper method to convert Vector3 (x,y,z) position to Vector2 coordinate (x, z)
    private Vector2 GetVector2Position(Vector3 pos)
    {
        return new Vector2(pos.x, pos.z);
    }

    private float CalculateTotalDistanceScore()
    {
        float totalDistanceScore = 0;
        for (int i = 0; i < grid.GridArray.GetLength(0) - 1; i++)
        {
            for (int j = 0; j < grid.GridArray.GetLength(1) - 1; j++)
            {
                totalDistanceScore += grid.GridArray[i, j];
            }
        }
        return totalDistanceScore;
    }

    //Adjust the UI to be in front of the player when spawned
    private void AdjustToPlayerPosition(float angle, float distance, float height = 0)
    {
        Vector3 tmp = Quaternion.AngleAxis(angle, Vector3.up) * (mainCamera.transform.forward * distance);

        //make sure UI doesn't spawn in the ground when player is looking down when confirming
        tmp.y += height;
        if(tmp.y < 3)
        {
            tmp.y = 3;
        }
        uiObject.transform.position = player.transform.position + tmp;
        uiObject.transform.rotation = Quaternion.LookRotation(mainCamera.transform.forward);

    }

    private void ConfirmTargetPosition(bool val)
    {
        confirmTargetFound = val;
        Debug.Log("Target position confirmed.");
    }
}
