using OscCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

public class StimulusController
{
    private GameObject player;
    private GameGrid grid;
    private DistanceCalculator distanceCalculator;
    private float distance;
    private float gamePlaneSize;
    private AudioSource targetSource;
    private AudioSource distractorSource;
    private List<AudioClip> distractorAudioClips;

    private AudioMixer mixer;

    private OscClient Client;


    private float maxDistance = 12;
    private float minDistance = 0;

    private float minFrequency = 250;
    private float maxFrequency = 8000;
    private float minAMRate = 2;
    private float maxAMRate = 32;
    private float minLevel = 0;
    private float maxLevel = 128;

    private float minParameter;
    private float maxParameter;

    private float min_vol_dB = -70;
    private float max_vol_dB = 2;
    private float close_SNR;
    private float far_SNR;
    private float initialDistractorDb;

    private String modePrefix;

    private Mode mode;

    public float Close_SNR
    {
        get => close_SNR;
        set
        {
            close_SNR = value;
            AdjustSnrBoundaries();
        }
    }
    public float Far_SNR
    {
        get => far_SNR;
        set
        {
            far_SNR = value;
            AdjustSnrBoundaries();
        }
    }

    public float InitialDistractorDb
    {
        get => initialDistractorDb;
        set
        {
            initialDistractorDb = value;
            SetChannelLevel(2, value);
        }
    }

    public StimulusController(GameObject player, GameGrid grid, float gamePlaneSize, Mode mode, DistanceCalculator distanceCalculator, AudioSource audioSource, AudioSource distractor, AudioMixer mixer, List<AudioClip> distClips)
    {
        this.player = player;
        this.grid = grid;
        this.gamePlaneSize = gamePlaneSize;
        this.mode = mode;
        this.distanceCalculator = distanceCalculator;
        this.targetSource = audioSource;
        this.distractorSource = distractor;
        this.mixer = mixer;
        this.distractorAudioClips = distClips;
        SetScale();
        SetModePrefixAndParam();
        SetChannelLevel(2, -5);
        Client = new OscClient("127.0.0.1", 7400);

    }



    //Adjust boundaries by the size of the game plane to accurately map stimulus values 
    private void SetScale()
    {
        minDistance = 0;
        maxDistance = (float)Math.Sqrt(2 * Math.Pow(gamePlaneSize, 2));
    }

    //Set the OSC string dependent on current modulation mode
    private void SetModePrefixAndParam()
    {
        switch (mode)
        {
            case Mode.Pitch:
                {
                    this.modePrefix = "/frequency/";
                    this.minParameter = minFrequency;
                    this.maxParameter = maxFrequency;
                    break;
                }
            case Mode.AMRate:
                {
                    this.modePrefix = "/modrate/";
                    this.minParameter = minAMRate;
                    this.maxParameter = maxAMRate;
                    break;
                }
            case Mode.Level:
                {
                    this.modePrefix = "/level/";
                    this.minParameter = minLevel;
                    this.maxParameter = maxLevel;
                    break;
                }
            case Mode.SingleTarget:
                {
                    AdjustSnrBoundaries();
                    break;
                }
            default: break;

        }
    }

    public void SendStimulusData()
    {
        distance = distanceCalculator.MinDistanceToTarget(grid.Target, new Vector2(player.transform.position.x, player.transform.position.z));
        float param = DistanceToParameterRange(distance);
        if (mode == Mode.SingleTarget)
        {
            SetChannelLevel(1, param);
        }
        float targetdb, distdb;
        mixer.GetFloat("TargetVol", out targetdb);
        mixer.GetFloat("DistractorVol", out distdb);
        //Debug.Log(GetSNR(targetdb, distdb));
        string paramStr = param.ToString(CultureInfo.InvariantCulture.NumberFormat);
        Client.Send(modePrefix + paramStr);
    }

    private void AdjustSnrBoundaries()
    {
        float distDb;
        mixer.GetFloat("DistractorVol", out distDb);

        this.minParameter = GetTargetLevelDb(close_SNR, distDb);
        this.maxParameter = GetTargetLevelDb(far_SNR, distDb);
        SetChannelLevel(1, DistanceToParameterRange(distanceCalculator.MinDistanceToTarget(grid.Target, new Vector2(player.transform.position.x, player.transform.position.z))));
    }


    //Map distance to stimulus parameter range (linear)
    private float DistanceToParameterRange(float distance)
    {
        //return ((distance - minDistance) * (maxParameter - minParameter)) / (maxDistance - minDistance);

        return minParameter + ((maxParameter - minParameter) / (maxDistance - minDistance)) * (distance - minDistance);
    }

    private float GetTargetLevelDb(float targetSNR, float distractorDb)
    {
        //return  distractorDb * (float) Math.Pow(Math.E, 1 / 10 * targetSNR * (Math.Log(2) + Math.Log(5)));
        return targetSNR + distractorDb;
    }

    private float GetSNR(float signalDb, float noiseDb)
    {
        //return 10 * (float) Math.Log10(signalDb / noiseDb);
        return signalDb - noiseDb;
    }






    public enum Mode
    {
        Pitch,
        Level,
        AMRate,
        SingleTarget
    }



    /**
     * Change level of audio channel by x dB
     * 0 = master
     * 1 = target 
     * 2 = distractor
     */
    public void ChangeChannelLevel(short channel, float deltaVolume_db)
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
        float volume;

        // get volume in dB from talker
        mixer.GetFloat(channelName, out volume);

        // increase/decrease by dVol
        volume += deltaVolume_db;

        Debug.Log("Set " + channelName + " to: " + volume + " dB" + " (change: " + deltaVolume_db + " dB)");
        SetLevel(channelName, volume);
    }

    /**
     * Set level of  audio channel to x dB
     * 0 = master
     * 1 = target 
     * 2 = distractor
     */
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


    /**
     * Write new level to AudioMixer using exposed parameters.
     * Enforces min/max limits
     */
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

    public void StartPlaying()
    {
        System.Random r = new System.Random();
        distractorSource.clip = distractorAudioClips[r.Next(distractorAudioClips.Count)];
        targetSource.Play();
        distractorSource.Play();
    }

    public void StopPlaying()
    {
        targetSource.Stop();
        distractorSource.Stop();
    }

}
