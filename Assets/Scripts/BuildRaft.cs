using UnityEngine;

public class BuildRaft : MonoBehaviour
{
    public int woodCost = 3;
    public int progressPerBuild = 10;
    [Tooltip("How close the player must be to build (arm's reach).")]
    public float interactionDistance = 2.25f;
    public ParticleSystem buildEffect;
    public string playerBuildAnimatorTrigger = "Build";
    Animator cachedPlayerAnimator;
    public string playerTag = "Player";
    Transform playerTransform;
    bool promptVisible = false;

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
            GameManager.instance.SetBuildRequirement($"Press E to build raft ({woodCost} wood)");
        }
        else if (!inReach && promptVisible)
        {
            promptVisible = false;
            GameManager.instance.SetBuildRequirement("");
            GameManager.instance.SetFeedback("");
        }

        if (!inReach) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (GameManager.instance.TrySpendWood(woodCost))
            {
                if (cachedPlayerAnimator != null && !string.IsNullOrWhiteSpace(playerBuildAnimatorTrigger))
                {
                    cachedPlayerAnimator.SetTrigger(playerBuildAnimatorTrigger);
                }

                GameManager.instance.AddRaftProgress(progressPerBuild);
                GameManager.instance.SetFeedback("Built raft!");

                if (buildEffect != null)
                {
                    ParticleSystem fx = Instantiate(buildEffect, transform.position, Quaternion.identity);
                    fx.Play();
                    Destroy(fx.gameObject, Mathf.Max(0.1f, fx.main.duration + fx.main.startLifetime.constantMax));
                }
            }
            else
            {
                GameManager.instance.SetFeedback($"Not enough wood ({GameManager.instance.wood}/{woodCost})");
            }
        }
    }
}