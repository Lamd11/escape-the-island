using UnityEngine;

public class BuildRaft : MonoBehaviour
{
    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Pressed E near raft");

            if (GameManager.instance.wood >= 3)
            {
                GameManager.instance.wood -= 3;
                GameManager.instance.raftProgress += 10;

                Debug.Log("Raft Progress: " + GameManager.instance.raftProgress);
            }
            else
            {
                Debug.Log("Not enough wood");
            }
        }
    }
}