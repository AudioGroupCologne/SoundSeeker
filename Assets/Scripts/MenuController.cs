using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [SerializeField] private TMP_InputField participantIdInput;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private GameObject confirmButton;
    [SerializeField] private GameObject cancelButton;
    [SerializeField] private GameObject newParticipantButton;
    [SerializeField] private GameObject startTrainingButton;

    private enum PendingAction { None, CreateNew, StartExisting }
    private PendingAction pendingAction = PendingAction.None;
    private short pendingId;

    void Start()
    {
        ShowConfirm(false);
    }

    // Wire to the "New Participant" button's OnClick()
    public void OnNewParticipantClicked()
    {
        pendingId = ParticipantLookup.GetNextParticipantId();
        pendingAction = PendingAction.CreateNew;
        messageText.text = "Create new participant with ID " + pendingId + "?";
        ShowConfirm(true);
    }

    // Wire to the "Start Training" button's OnClick() (reads participantIdInput)
    public void OnExistingParticipantClicked()
    {
        if (!short.TryParse(participantIdInput.text, out short id))
        {
            messageText.text = "Please enter a valid participant ID.";
            ShowConfirm(false);
            return;
        }

        int remaining = ParticipantLookup.GetRemainingRounds(id);

        if (remaining == -1)
        {
            messageText.text = "No configuration found for participant " + id + ".";
            ShowConfirm(false);
            return;
        }

        if (remaining == 0)
        {
            messageText.text = "Training already completed for participant " + id + ".";
            ShowConfirm(false);
            return;
        }

        pendingId = id;
        pendingAction = PendingAction.StartExisting;

        int completed = ParticipantSession.TotalRoundsRequired - remaining;
        int sessionNumber = completed / ParticipantSession.RoundsPerSession + 1;
        int completedInSession = completed % ParticipantSession.RoundsPerSession;
        int roundsRemainingInSession = completedInSession == 0
            ? ParticipantSession.RoundsPerSession
            : ParticipantSession.RoundsPerSession - completedInSession;

        messageText.text = completedInSession == 0
            ? "Run training for participant " + id + " - starting session " + sessionNumber + " of " + ParticipantSession.SessionsRequired + " (" + roundsRemainingInSession + " rounds)."
            : "Run training for participant " + id + " - continuing with " + roundsRemainingInSession + " rounds remaining in session " + sessionNumber + " of " + ParticipantSession.SessionsRequired + ".";
        ShowConfirm(true);
    }

    // Wire to the confirm button's OnClick()
    public void OnConfirmClicked()
    {
        ParticipantSession.ParticipantId = pendingId;
        if (pendingAction == PendingAction.CreateNew)
        {
            SceneManager.LoadScene("LevelSetup");
        }
        else if (pendingAction == PendingAction.StartExisting)
        {
            SceneManager.LoadScene("Training");
        }
    }

    // Wire to the cancel button's OnClick()
    public void OnCancelClicked()
    {
        pendingAction = PendingAction.None;
        messageText.text = "";
        ShowConfirm(false);
    }

    private void ShowConfirm(bool show)
    {
        confirmButton.SetActive(show);
        cancelButton.SetActive(show);
        newParticipantButton.SetActive(!show);
        startTrainingButton.SetActive(!show);
    }
}