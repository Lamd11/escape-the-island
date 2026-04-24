using UnityEngine;

public class CollectResource : MonoBehaviour
{
    public int woodValue = 1;
    public KeyCode collectKey = KeyCode.E;
    public string playerTag = "Player";
    bool playerInRange = false;
    Collider playerInRangeCollider;
    public ParticleSystem collectEffect;
    public string playerCollectAnimatorTrigger = "Chop";
    Animator cachedPlayerAnimator;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (GameManager.instance == null) return;

        playerInRange = true;
        playerInRangeCollider = other;
        if (cachedPlayerAnimator == null)
        {
            cachedPlayerAnimator = other.GetComponentInChildren<Animator>();
        }
        GameManager.instance.SetFeedback($"Press {collectKey} to collect wood (+{woodValue})");
    }

    void Update()
    {
        if (GameManager.instance == null) return;
        if (GameManager.instance.hasWon) return;

        if (!playerInRange) return;

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

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (GameManager.instance == null) return;

        if (playerInRangeCollider == other)
        {
            playerInRange = false;
            playerInRangeCollider = null;
        }
        GameManager.instance.SetFeedback("");
    }
}