// Carries the participant ID chosen in the Menu scene across the scene load
// into Setup or Training. 
public static class ParticipantSession
{
    public static short ParticipantId { get; set; }

    // Single source of truth for how many rounds a full session contains.
    // GameController and the Menu both read this instead of each keeping their own number.
    public const int RequiredRoundsPerSession = 15;
}