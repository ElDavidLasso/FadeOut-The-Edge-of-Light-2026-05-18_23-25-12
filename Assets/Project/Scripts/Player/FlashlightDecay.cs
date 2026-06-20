using UnityEngine;

[RequireComponent(typeof(Light))]
public class FlashlightDecay : MonoBehaviour
{
    [Header("Ajustes de Batería (Decaimiento Exponencial)")]
    [SerializeField] private float maxIntensity = 10f;
    [SerializeField] private float decayRate = 0.02f;
    [SerializeField] private bool isFlashlightOn = true;

    [Header("Efecto de Pánico (Parpadeo)")]
    [SerializeField] private float flickerThreshold = 2.5f;

    private Light flashlight;
    private float timeActive = 0f;

    public bool IsFlashlightOn => isFlashlightOn && flashlight != null && flashlight.enabled;
    public float CurrentRange => flashlight != null ? flashlight.range : 0f;
    public float SpotlightAngle => flashlight != null ? flashlight.spotAngle : 0f;

    public bool IsLightEffective => IsFlashlightOn && flashlight.intensity >= flickerThreshold;

    
    public float BatteryLevel
    {
        get
        {
            float theoreticalIntensity = maxIntensity * Mathf.Exp(-decayRate * timeActive);
            return Mathf.Clamp((theoreticalIntensity / maxIntensity) * 100f, 0f, 100f);
        }
    }

    private void Awake()
    {
        flashlight = GetComponent<Light>();
        if (flashlight.type != LightType.Spot)
        {
            Debug.LogWarning("La linterna debería ser un SpotLight.");
        }
        flashlight.intensity = maxIntensity;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateFlashlightIcon(isFlashlightOn);
        }
    }

    private void Update()
    {
        ToggleFlashlight();

        if (isFlashlightOn)
        {
            timeActive += Time.deltaTime;
            CalculateMathematicalFalloff();
        }
        if (UIManager.Instance != null) UIManager.Instance.UpdateBatteryUI(BatteryLevel);
    }

    private void ToggleFlashlight()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            isFlashlightOn = !isFlashlightOn;
            flashlight.enabled = isFlashlightOn;


            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateFlashlightIcon(isFlashlightOn);
            }
        }
    }

    private void CalculateMathematicalFalloff()
    {
        float currentIntensity = maxIntensity * Mathf.Exp(-decayRate * timeActive);

        if (currentIntensity < flickerThreshold)
        {
            float noise = Mathf.PerlinNoise(Time.time * 15f, 0f);

            if (noise < 0.2f)
                currentIntensity = 0f;
            else
                currentIntensity = Mathf.Lerp(0.1f, currentIntensity, noise);
        }

        if (currentIntensity < 0.05f && timeActive > 10f)
            currentIntensity = 0f;

        flashlight.intensity = currentIntensity;
    }
}
