using UnityEngine;
using UnityEngine.Rendering;
using TMPro;

public class DayNightManager : MonoBehaviour
{
    public static DayNightManager instance;

    [Header("Cycle (asymmetric)")]
    [Tooltip("How long daytime lasts before switching to night.")]
    public float dayDurationSeconds = 7f;
    [Tooltip("How long night lasts before switching to day.")]
    public float nightDurationSeconds = 6f;
    [Tooltip("If true, the game starts in night. Otherwise starts in day.")]
    public bool startAtNight = false;

    public bool isNight = false;
    float phaseTimer = 0f;

    [Header("UI")]
    public TextMeshProUGUI dayNightText;

    [Header("Lighting (optional — auto-finds Directional Light if empty)")]
    public Light sunLight;
    [Header("Day look")]
    public Color daySunColor = new Color(1f, 0.96f, 0.88f);
    public float daySunIntensity = 1.1f;
    public Vector3 daySunEuler = new Vector3(55f, -35f, 0f);
    public Color dayAmbient = new Color(0.48f, 0.56f, 0.65f);
    [Header("Night look (readable — not pitch black)")]
    [Tooltip("Cool moon tint; still bright enough that lit surfaces read clearly.")]
    public Color nightSunColor = new Color(0.55f, 0.62f, 0.85f);
    [Tooltip("Keep moon high enough (X ~45–60) so ground/trees still catch light; low X = black silhouettes.")]
    public float nightSunIntensity = 0.48f;
    public Vector3 nightSunEuler = new Vector3(52f, 95f, 0f);
    [Tooltip("Flat ambient fill so shadows are blue-grey, not pure black.")]
    public Color nightAmbient = new Color(0.28f, 0.32f, 0.45f);

    void Awake()
    {
        instance = this;
        isNight = startAtNight;
        phaseTimer = 0f;
        TryFindSunLight();
    }

    void Start()
    {
        ApplyLightingForPhase(isNight);
        UpdateDayNightLabel();
    }

    void Update()
    {
        float phaseLen = isNight ? nightDurationSeconds : dayDurationSeconds;
        if (phaseLen <= 0f) return;

        phaseTimer += Time.deltaTime;
        if (phaseTimer >= phaseLen)
        {
            isNight = !isNight;
            phaseTimer = 0f;
            ApplyLightingForPhase(isNight);
            UpdateDayNightLabel();
        }
    }

    void TryFindSunLight()
    {
        if (sunLight != null) return;

        Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (Light l in lights)
        {
            if (l.type == LightType.Directional)
            {
                sunLight = l;
                break;
            }
        }
    }

    void ApplyLightingForPhase(bool night)
    {
        TryFindSunLight();

        if (sunLight != null)
        {
            sunLight.color = night ? nightSunColor : daySunColor;
            sunLight.intensity = night ? nightSunIntensity : daySunIntensity;
            sunLight.transform.rotation = Quaternion.Euler(night ? nightSunEuler : daySunEuler);
        }

        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = night ? nightAmbient : dayAmbient;
    }

    void UpdateDayNightLabel()
    {
        if (dayNightText == null) return;
        dayNightText.text = isNight ? "Night" : "Day";
    }
}
