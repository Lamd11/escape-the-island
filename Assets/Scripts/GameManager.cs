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
    public bool hasLost = false;

    [Header("Health")]
    public float maxHealth = 100f;
    public float health = 100f;

    [Header("UI")]
    public TextMeshProUGUI woodText;
    public TextMeshProUGUI raftText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI buildRequirementText;
    public TextMeshProUGUI winText;
    public TextMeshProUGUI deathText;

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
        health = Mathf.Clamp(health, 0f, maxHealth);
        if (winText != null) winText.gameObject.SetActive(false);
        if (deathText != null) deathText.gameObject.SetActive(false);
        UpdateUI();
    }

    public void AddWood(int amount)
    {
        if (hasWon || hasLost) return;
        wood += Mathf.Max(0, amount);
        UpdateUI();
    }

    public bool TrySpendWood(int amount)
    {
        if (hasWon || hasLost) return false;

        int cost = Mathf.Max(0, amount);
        if (wood < cost) return false;

        wood -= cost;
        UpdateUI();
        return true;
    }

    public void AddRaftProgress(int amount)
    {
        if (hasWon || hasLost) return;

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
        if (healthText != null) healthText.text = "HP: " + Mathf.CeilToInt(health);
    }

    public void TakeDamage(float amount)
    {
        if (hasWon || hasLost) return;

        health = Mathf.Clamp(health - Mathf.Max(0f, amount), 0f, maxHealth);
        UpdateUI();

        if (health <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        if (hasLost) return;
        hasLost = true;

        TryAutoWireUI();
        if (deathText != null)
        {
            deathText.text = "You Died";
            deathText.gameObject.SetActive(true);
        }

        SetFeedback("");
        SetBuildRequirement("");

        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = false;
        }
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
        if (deathText == null)
        {
            GameObject go = GameObject.Find("DeathText");
            if (go != null) deathText = go.GetComponent<TextMeshProUGUI>();
        }
        if (healthText == null)
        {
            GameObject go = GameObject.Find("HealthText");
            if (go != null) healthText = go.GetComponent<TextMeshProUGUI>();
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