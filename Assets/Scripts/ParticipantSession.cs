// Carries the participant ID chosen in the Menu scene across the scene load
// into Setup or Training. 
public static class ParticipantSession
{
    public static short ParticipantId { get; set; }

    // Single source of truth for how many rounds one training session contains.
    public const int RoundsPerSession = 15;

    // Participants complete this many separate sessions (across separate visits).
    public const int SessionsRequired = 5;

    // Total rounds across a participant's entire involvement in the study.
    // This - not RoundsPerSession - is the ceiling that should ever lock a participant out.
    public const int TotalRoundsRequired = RoundsPerSession * SessionsRequired;
}