using UnityEngine;
using UnityEngine.Serialization;

public class CollectResource : MonoBehaviour
{
    public enum ResourceType { Wood, Food, Rope }

    public ResourceType resourceType = ResourceType.Wood;

    [FormerlySerializedAs("woodValue")]
    public int resourceValue = 1;
    [Tooltip("For Food pickups: HP restored. If < 0, uses resourceValue (same as food gained).")]
    public float foodHealOverride = -1f;
    public KeyCode collectKey = KeyCode.E;
    public string playerTag = "Player";
    [Tooltip("How close the player must be to collect (arm's reach).")]
    public float interactionDistance = 2.25f;
    Transform playerTransform;
    bool promptVisible = false;
    public ParticleSystem collectEffect;
    public string playerCollectAnimatorTrigger = "Chop";
    Animator cachedPlayerAnimator;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            playerTransform = player.transform;
            cachedPlayerAnimator = player.GetComponentInChildren<Animator>();
        }
    }

    void Update()
    {
        if (GameManager.instance == null) return;
        if (GameManager.instance.hasWon) return;

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag(playerTag);
            if (player != null)
            {
                playerTransform = player.transform;
                cachedPlayerAnimator = player.GetComponentInChildren<Animator>();
            }
            else
            {
                return;
            }
        }

        float distance = Vector3.Distance(playerTransform.position, transform.position);
        bool inReach = distance <= interactionDistance;

        if (inReach && !promptVisible)
        {
            promptVisible = true;
            GameManager.instance.SetFeedback($"Press {collectKey} to collect {resourceType.ToString().ToLower()} (+{resourceValue})");
        }
        else if (!inReach && promptVisible)
        {
            promptVisible = false;
            GameManager.instance.SetFeedback("");
        }

        if (!inReach) return;

        if (Input.GetKeyDown(collectKey))
        {
            if (cachedPlayerAnimator != null && !string.IsNullOrWhiteSpace(playerCollectAnimatorTrigger))
            {
                cachedPlayerAnimator.SetTrigger(playerCollectAnimatorTrigger);
            }

            Collect();

            if (collectEffect != null)
            {
                ParticleSystem fx = Instantiate(collectEffect, transform.position, Quaternion.identity);
                fx.Play();
                Destroy(fx.gameObject, Mathf.Max(0.1f, fx.main.duration + fx.main.startLifetime.constantMax));
            }

            Destroy(gameObject);
        }
    }

    void Collect()
    {
        if (GameManager.instance == null) return;

        int v = Mathf.Max(0, resourceValue);
        switch (resourceType)
        {
            case ResourceType.Wood:
                GameManager.instance.AddWood(v);
                GameManager.instance.SetFeedback($"Collected +{v} wood");
                break;
            case ResourceType.Rope:
                GameManager.instance.AddRope(v);
                GameManager.instance.SetFeedback($"Collected +{v} rope");
                break;
            case ResourceType.Food:
            {
                float hp = foodHealOverride < 0f ? v : foodHealOverride;
                GameManager.instance.ApplyFoodPickup(v, hp);
                GameManager.instance.SetFeedback($"+{v} food, +{Mathf.RoundToInt(hp)} HP");
                break;
            }
        }
    }
}