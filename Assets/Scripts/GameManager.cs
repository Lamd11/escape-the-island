using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("State")]
    public int wood = 0;
    [Range(0, 100)]
    public int raftProgress = 0;
    public bool hasWon = false;

    [Header("UI")]
    public TextMeshProUGUI woodText;
    public TextMeshProUGUI raftText;
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI buildRequirementText;
    public TextMeshProUGUI winText;

    [Header("Player (assign in Inspector)")]
    public MonoBehaviour playerMovementScript;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    void Start()
    {
        if (winText != null) winText.gameObject.SetActive(false);
        UpdateUI();
    }

    public void AddWood(int amount)
    {
        if (hasWon) return;
        wood += Mathf.Max(0, amount);
        UpdateUI();
    }

    public bool TrySpendWood(int amount)
    {
        if (hasWon) return false;

        int cost = Mathf.Max(0, amount);
        if (wood < cost) return false;

        wood -= cost;
        UpdateUI();
        return true;
    }

    public void AddRaftProgress(int amount)
    {
        if (hasWon) return;

        raftProgress = Mathf.Clamp(raftProgress + amount, 0, 100);
        UpdateUI();

        if (raftProgress >= 100)
        {
            WinGame();
        }
    }

    public void SetFeedback(string message)
    {
        if (feedbackText == null)
        {
            TryAutoWireUI();
        }
        if (feedbackText == null) return;
        feedbackText.text = message;
    }

    public void SetBuildRequirement(string message)
    {
        if (buildRequirementText == null)
        {
            TryAutoWireUI();
        }
        if (buildRequirementText == null) return;
        buildRequirementText.text = message;
    }

    void UpdateUI()
    {
        TryAutoWireUI();
        if (woodText != null) woodText.text = "Wood: " + wood;
        if (raftText != null) raftText.text = "Raft: " + raftProgress + "%";
    }

    void TryAutoWireUI()
    {
        if (woodText == null)
        {
            GameObject go = GameObject.Find("WoodText");
            if (go != null) woodText = go.GetComponent<TextMeshProUGUI>();
        }
        if (raftText == null)
        {
            GameObject go = GameObject.Find("RaftText");
            if (go != null) raftText = go.GetComponent<TextMeshProUGUI>();
        }
        if (feedbackText == null)
        {
            GameObject go = GameObject.Find("FeedbackText");
            if (go != null) feedbackText = go.GetComponent<TextMeshProUGUI>();
        }
        if (buildRequirementText == null)
        {
            GameObject go = GameObject.Find("BuildRequirementText");
            if (go != null) buildRequirementText = go.GetComponent<TextMeshProUGUI>();
        }
        if (winText == null)
        {
            GameObject go = GameObject.Find("WinText");
            if (go != null) winText = go.GetComponent<TextMeshProUGUI>();
        }
    }

    void WinGame()
    {
        if (hasWon) return;
        hasWon = true;

        TryAutoWireUI();
        if (winText != null)
        {
            winText.text = "You Escaped!";
            winText.gameObject.SetActive(true);
        }

        SetFeedback("");
        SetBuildRequirement("");

        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = false;
        }
    }
}