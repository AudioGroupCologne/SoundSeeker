using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [SerializeField] private TMP_InputField participantIdInput;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private GameObject confirmButton;
    [SerializeField] private GameObject cancelButton;

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
        messageText.text = remaining == ParticipantSession.RequiredRoundsPerSession
            ? "Run training for participant " + id + " - starting a fresh session (" + remaining + " rounds)."
            : "Run training for participant " + id + " - continuing with " + remaining + " rounds remaining.";
        ShowConfirm(true);
    }

    // Wire to the confirm button's OnClick()
    public void OnConfirmClicked()
    {
        ParticipantSession.ParticipantId = pendingId;
        if (pendingAction == PendingAction.CreateNew)
        {
            SceneManager.LoadScene("Setup");
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
    }
}