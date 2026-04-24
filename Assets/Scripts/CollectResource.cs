using UnityEngine;

public class CollectResource : MonoBehaviour
{
    public int woodValue = 1;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.instance.wood += 1;
            Debug.Log("Hit tree");
            Destroy(gameObject);
        }
    }
}