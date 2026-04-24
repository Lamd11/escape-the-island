using UnityEngine;

public class CollectResource : MonoBehaviour
{
    public int woodValue = 1;
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
            GameManager.instance.SetFeedback($"Press {collectKey} to collect wood (+{woodValue})");
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

            GameManager.instance.AddWood(woodValue);
            GameManager.instance.SetFeedback($"Collected +{woodValue} wood");

            if (collectEffect != null)
            {
                ParticleSystem fx = Instantiate(collectEffect, transform.position, Quaternion.identity);
                fx.Play();
                Destroy(fx.gameObject, Mathf.Max(0.1f, fx.main.duration + fx.main.startLifetime.constantMax));
            }

            Destroy(gameObject);
        }
    }
}