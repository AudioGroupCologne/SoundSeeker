using System.IO;

// Read-only helper for the Menu scene: never writes anything, only inspects
// existing configuration files to support the auto-increment ID and the
// "X rounds remaining" confirmation dialog.
public static class ParticipantLookup
{
    private static string ConfigDir => Path.Combine(PathConfig.Instance.dataRoot, "Configuration");

    public static bool ConfigExists(short participantId)
    {
        return File.Exists(ConfigPathFor(participantId));
    }

    private static string ConfigPathFor(short participantId)
    {
        return Path.Combine(ConfigDir, participantId + "_configuration.json");
    }

    // Highest existing participant ID + 1, or 1 if none exist yet.
    public static short GetNextParticipantId()
    {
        if (!Directory.Exists(ConfigDir))
        {
            return 1;
        }

        short maxId = 0;
        foreach (string file in Directory.GetFiles(ConfigDir, "*_configuration.json"))
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            int sepIndex = fileName.IndexOf("_configuration");
            if (sepIndex > 0 && short.TryParse(fileName.Substring(0, sepIndex), out short id))
            {
                if (id > maxId)
                {
                    maxId = id;
                }
            }
        }
        return (short)(maxId + 1);
    }

    // Returns rounds remaining for this participant (0..TotalRoundsRequired),
    // or -1 if no configuration exists for this ID at all.
    public static int GetRemainingRounds(short participantId)
    {
        if (!ConfigExists(participantId))
        {
            return -1;
        }

        SettingsHandler.ConfigurationPath = ConfigPathFor(participantId);
        SettingsHandler.LoadSettingsFromFile();
        int completed = SettingsHandler.PlayerSettings.CompletedRounds;
        return System.Math.Max(0, ParticipantSession.TotalRoundsRequired - completed);
    }
}