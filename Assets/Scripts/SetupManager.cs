using API_3DTI;
using OpenCover.Framework.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;


public class SetupManager : MonoBehaviour
{
    public short participant_id;
    public string participant_name;
    public float initialDistractorDb;
    public AudioMixer mixer;
    public AudioSource target;
    public AudioSource distractor;

    private float min_vol_dB = -70;
    private float max_vol_dB = 2;
    private float currentTargetDb;
    private float dbStep = 2;


    // Start is called before the first frame update
    void Start()
    {
        currentTargetDb = initialDistractorDb;
        SetChannelLevel(1, currentTargetDb);
        SetChannelLevel(2, initialDistractorDb);


        SettingsHandler.ConfiguarationPath = Application.persistentDataPath + "\\Configuration\\" + participant_id + "_configuration.json";
        PlayerSettings settings = new PlayerSettings(participant_id, participant_name, 0, 0, 0, initialDistractorDb);
        if (UnityEngine.Windows.File.Exists(SettingsHandler.ConfiguarationPath))
        {
            Debug.LogError("Config file already exists. Make sure that participant ID has not been used yet.");
            throw new System.Exception("Config file already exists. Make sure that participant ID has not been used yet.");
        }
        SettingsHandler.PlayerSettings = settings;

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("1"))
        {
            if((currentTargetDb + dbStep) < initialDistractorDb)
            {
                SetChannelLevel(1, currentTargetDb + dbStep);
                currentTargetDb += dbStep;
                Debug.Log("Target DB increased by " + dbStep + "dB. SNR at " + (currentTargetDb - initialDistractorDb) + "dB");
            } else
            {
                SetChannelLevel(1, initialDistractorDb);
                currentTargetDb  = initialDistractorDb;
                Debug.Log("Target maxed out. SNR at " + (currentTargetDb - initialDistractorDb) + "dB");

            }
        }
        else if (Input.GetKeyDown("2"))
        {
            SetChannelLevel(1, currentTargetDb - dbStep);
            currentTargetDb -= dbStep;
            Debug.Log("Target DB lowered by " + dbStep + "dB. SNR at " + (currentTargetDb - initialDistractorDb) + "dB");

        }
        
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Testing concluded. Final SNR limit: " + (currentTargetDb - initialDistractorDb) + "dB\nInitial distractor level: " + initialDistractorDb + "dB");
            target.Stop();
            distractor.Stop();
            float newTargetDB = currentTargetDb - initialDistractorDb + 6;
            SettingsHandler.PlayerSettings.CloseSNR = newTargetDB;

            if ((newTargetDB + 20) > initialDistractorDb)
            {
                SettingsHandler.PlayerSettings.FarSNR = initialDistractorDb;
            }
            else
            {
                SettingsHandler.PlayerSettings.FarSNR = currentTargetDb + 20;
            }
            SettingsHandler.WriteConfigToFile();

        }

    }

    public void SetChannelLevel(short channel, float level_db)
    {
        if (channel < 0 || channel > 2)
        {
            Debug.LogWarning("Illegal channel number. Must be 0, 1 or 2");
            return;
        }

        string channelName = "";
        switch (channel)
        {
            case 0:
                channelName = "MasterVol";
                break;
            case 1:
                channelName = "TargetVol";
                break;
            case 2:
                channelName = "DistractorVol";
                break;
        }
        //Debug.Log("Set " + channelName + " to: " + level_db + " dB");
        SetLevel(channelName, level_db);
    }

    private void SetLevel(string channel, float level_db)
    {
        // apply limits to volume
        if (level_db < min_vol_dB)
        {
            level_db = min_vol_dB;
            Debug.LogWarning("Min Volume is reached: " + level_db);
        }
        else if (level_db > max_vol_dB)
        {
            level_db = max_vol_dB;
            Debug.LogWarning("Max Volume is reached: " + level_db);
        }

        // write updated volume level to 'AudioMixer'
        mixer.SetFloat(channel, level_db);
    }


}
