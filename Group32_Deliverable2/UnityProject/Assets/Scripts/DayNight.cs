using UnityEngine;

public class DayNight : MonoBehaviour
{
    public GameObject enemy;
    float timer = 0f;
    public float cycleTime = 10f;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer > cycleTime)
        {
            enemy.SetActive(!enemy.activeSelf);
            timer = 0f;
        }
    }
}