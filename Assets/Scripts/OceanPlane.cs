using UnityEngine;

/// <summary>
/// Spawns a large plane for water. Add to an empty GameObject (e.g. at y = 0), tune size/color in Inspector.
/// Collider is removed by default so it does not block the player.
/// </summary>
public class OceanPlane : MonoBehaviour
{
    public bool createOnAwake = true;
    [Min(1f)]
    public float width = 220f;
    [Min(1f)]
    public float depth = 220f;
    public Color waterColor = new Color(0.12f, 0.38f, 0.48f, 1f);
    [Range(0f, 1f)]
    public float smoothness = 0.55f;
    [Tooltip("If false, removes MeshCollider so the ocean is visual-only.")]
    public bool keepCollider = false;

    void Awake()
    {
        if (createOnAwake)
            BuildOcean();
    }

    [ContextMenu("Build Ocean")]
    public void BuildOcean()
    {
        Transform existing = transform.Find("Ocean");
        if (existing != null)
        {
            if (Application.isPlaying)
                Destroy(existing.gameObject);
            else
                DestroyImmediate(existing.gameObject);
        }

        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.name = "Ocean";
        plane.transform.SetParent(transform, false);
        plane.transform.localPosition = Vector3.zero;
        plane.transform.localRotation = Quaternion.identity;
        plane.transform.localScale = new Vector3(width / 10f, 1f, depth / 10f);

        var mr = plane.GetComponent<MeshRenderer>();
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("HDRP/Lit");
        if (shader == null)
            shader = Shader.Find("Standard");
        if (shader == null)
            shader = Shader.Find("Unlit/Color");

        Material mat = new Material(shader);
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", waterColor);
        else if (mat.HasProperty("_Color"))
            mat.SetColor("_Color", waterColor);
        if (mat.HasProperty("_Smoothness"))
            mat.SetFloat("_Smoothness", smoothness);
        mr.sharedMaterial = mat;

        if (!keepCollider)
        {
            Collider c = plane.GetComponent<Collider>();
            if (c != null)
            {
                if (Application.isPlaying)
                    Destroy(c);
                else
                    DestroyImmediate(c);
            }
        }
    }
}
