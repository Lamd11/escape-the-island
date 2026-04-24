using UnityEngine;
using TMPro;

public class DayNightManager : MonoBehaviour
{
    public static DayNightManager instance;

    public bool isNight = false;
    float timer = 0f;

    public TextMeshProUGUI dayNightText;

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= 5f)
        {
            isNight = !isNight;
            timer = 0f;

            Debug.Log("Night: " + isNight);

            if (dayNightText != null)
            {
                dayNightText.text = isNight ? "Night" : "Day";
            }
        }
    }
}