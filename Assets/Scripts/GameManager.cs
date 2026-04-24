using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("State")]
    public int wood = 0;
    public int rope = 0;
    [Range(0, 100)]
    public int raftProgress = 0;
    public bool hasWon = false;
    public bool hasLost = false;

    [Header("Health")]
    public float maxHealth = 100f;
    public float health = 100f;

    [Header("Food")]
    [Min(0f)]
    public float maxFood = 100f;
    [Min(0f)]
    public float food = 100f;
    [Tooltip("Food lost per second.")]
    [Min(0f)]
    public float foodDrainPerSecond = 1f;
    [Tooltip("If food is 0, health lost per second.")]
    [Min(0f)]
    public float starvationDamagePerSecond = 2f;

    [Header("Healing")]
    public KeyCode healKey = KeyCode.F;
    [Min(0)]
    public int healFoodCost = 10;
    [Min(0f)]
    public float healAmount = 20f;

    [Header("UI")]
    public TextMeshProUGUI woodText;
    public TextMeshProUGUI ropeText;
    public TextMeshProUGUI foodText;
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
        food = Mathf.Clamp(food, 0f, maxFood);
        if (winText != null) winText.gameObject.SetActive(false);
        if (deathText != null) deathText.gameObject.SetActive(false);
        UpdateUI();
    }

    void Update()
    {
        if (hasWon || hasLost) return;

        // Hunger tick
        if (foodDrainPerSecond > 0f)
        {
            food = Mathf.Clamp(food - (foodDrainPerSecond * Time.deltaTime), 0f, maxFood);
        }

        // Starvation damage
        if (food <= 0f && starvationDamagePerSecond > 0f)
        {
            TakeDamage(starvationDamagePerSecond * Time.deltaTime);
        }

        // Healing
        if (Input.GetKeyDown(healKey))
        {
            TryHeal();
        }

        UpdateUI();
    }

    public void AddWood(int amount)
    {
        if (hasWon || hasLost) return;
        wood += Mathf.Max(0, amount);
        UpdateUI();
    }

    public void AddRope(int amount)
    {
        if (hasWon || hasLost) return;
        rope += Mathf.Max(0, amount);
        UpdateUI();
    }

    public void AddFood(float amount)
    {
        if (hasWon || hasLost) return;
        food = Mathf.Clamp(food + Mathf.Max(0f, amount), 0f, maxFood);
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

    public bool TrySpendRope(int amount)
    {
        if (hasWon || hasLost) return false;

        int cost = Mathf.Max(0, amount);
        if (rope < cost) return false;

        rope -= cost;
        UpdateUI();
        return true;
    }

    public bool TrySpendForRaft(int woodCost, int ropeCost)
    {
        if (hasWon || hasLost) return false;

        int w = Mathf.Max(0, woodCost);
        int r = Mathf.Max(0, ropeCost);
        if (wood < w || rope < r) return false;

        wood -= w;
        rope -= r;
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
        if (ropeText != null) ropeText.text = "Rope: " + rope;
        if (foodText != null) foodText.text = "Food: " + Mathf.FloorToInt(food);
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

    public void Heal(float amount)
    {
        if (hasWon || hasLost) return;
        health = Mathf.Clamp(health + Mathf.Max(0f, amount), 0f, maxHealth);
        UpdateUI();
    }

    public bool TryConsumeFood(int amount)
    {
        if (hasWon || hasLost) return false;
        int cost = Mathf.Max(0, amount);
        if (food < cost) return false;

        food = Mathf.Clamp(food - cost, 0f, maxFood);
        UpdateUI();
        return true;
    }

    void TryHeal()
    {
        if (health >= maxHealth)
        {
            SetFeedback("Health is already full");
            return;
        }

        if (!TryConsumeFood(healFoodCost))
        {
            SetFeedback($"Not enough food ({Mathf.FloorToInt(food)}/{healFoodCost})");
            return;
        }

        Heal(healAmount);
        SetFeedback($"+{Mathf.CeilToInt(healAmount)} HP (cost {healFoodCost} food)");
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
        if (ropeText == null)
        {
            GameObject go = GameObject.Find("RopeText");
            if (go != null) ropeText = go.GetComponent<TextMeshProUGUI>();
        }
        if (foodText == null)
        {
            GameObject go = GameObject.Find("FoodText");
            if (go != null) foodText = go.GetComponent<TextMeshProUGUI>();
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