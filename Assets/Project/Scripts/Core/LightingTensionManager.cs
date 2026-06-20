using UnityEngine;

[System.Serializable]
public struct LightingProfile
{
    [Tooltip("Color de la iluminación base de las sombras")]
    public Color ambientColor;
    [Tooltip("Color de la niebla que oculta el final de los pasillos")]
    public Color fogColor;
    [Tooltip("Espesor de la niebla (0.01 a 0.15)")]
    public float fogDensity;
}

public class LightingTensionManager : MonoBehaviour
{
    public static LightingTensionManager Instance { get; private set; }

    public enum TensionLevel { Illuminated, MediumDim, TotalDarkness }

    [Header("Perfiles de Tensión")]
    public LightingProfile illuminated;
    public LightingProfile mediumDim;
    public LightingProfile totalDarkness;

    [Header("Configuración")]
    [Tooltip("Velocidad de transición entre perfiles")]
    public float transitionSpeed = 1.5f;

    private LightingProfile currentProfile;
    private LightingProfile targetProfile;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;

        
        SetTensionState(TensionLevel.MediumDim);
        currentProfile = targetProfile;
        ApplyLightingImmediate();
    }

    private void Update()
    {
        currentProfile.ambientColor = Color.Lerp(currentProfile.ambientColor, targetProfile.ambientColor, Time.deltaTime * transitionSpeed);
        currentProfile.fogColor = Color.Lerp(currentProfile.fogColor, targetProfile.fogColor, Time.deltaTime * transitionSpeed);
        currentProfile.fogDensity = Mathf.Lerp(currentProfile.fogDensity, targetProfile.fogDensity, Time.deltaTime * transitionSpeed);

        ApplyLightingImmediate();
    }

    public void SetTensionState(TensionLevel level)
    {
        switch (level)
        {
            case TensionLevel.Illuminated: targetProfile = illuminated; break;
            case TensionLevel.MediumDim: targetProfile = mediumDim; break;
            case TensionLevel.TotalDarkness: targetProfile = totalDarkness; break;
        }
    }

    private void ApplyLightingImmediate()
    {
        RenderSettings.ambientLight = currentProfile.ambientColor;
        RenderSettings.fogColor = currentProfile.fogColor;
        RenderSettings.fogDensity = currentProfile.fogDensity;
    }
}