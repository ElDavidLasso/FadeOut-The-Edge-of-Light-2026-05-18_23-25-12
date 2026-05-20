using UnityEngine;

[RequireComponent(typeof(Light))]
public class FlashlightDecay : MonoBehaviour
{
    [Header("Ajustes de Batería (Decaimiento Exponencial)")]
    [SerializeField] private float maxIntensity = 10f;
    [SerializeField] private float decayRate = 0.02f; // El valor Lambda
    [SerializeField] private bool isFlashlightOn = true;

    [Header("Efecto de Pánico (Parpadeo)")]
    [SerializeField] private float flickerThreshold = 2.5f; // Cuando la luz baja de esto, empieza a fallar

    private Light flashlight;
    private float timeActive = 0f;

    private void Awake()
    {
        flashlight = GetComponent<Light>();
        if (flashlight.type != LightType.Spot)
        {
            Debug.LogWarning("La linterna debería ser un SpotLight.");
        }
        flashlight.intensity = maxIntensity;
    }

    private void Update()
    {
        ToggleFlashlight();

        if (isFlashlightOn)
        {
            timeActive += Time.deltaTime;
            CalculateMathematicalFalloff();
        }
    }

    private void ToggleFlashlight()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            isFlashlightOn = !isFlashlightOn;
            flashlight.enabled = isFlashlightOn;
        }
    }

    private void CalculateMathematicalFalloff()
    {
        // I(t) = I_0 * e^(-k * t)
        float currentIntensity = maxIntensity * Mathf.Exp(-decayRate * timeActive);

        // Lógica de parpadeo de terror cuando la batería está crítica
        if (currentIntensity < flickerThreshold)
        {
            // Usamos Mathf.PerlinNoise para generar un parpadeo orgánico e impredecible, no un On/Off robótico
            float noise = Mathf.PerlinNoise(Time.time * 15f, 0f);

            // Si el ruido es muy bajo, simulamos un "apagón" momentáneo
            if (noise < 0.2f)
                currentIntensity = 0f;
            else
                currentIntensity = Mathf.Lerp(0.1f, currentIntensity, noise);
        }

        // Si la batería se agotó por completo
        if (currentIntensity < 0.05f && timeActive > 10f)
            currentIntensity = 0f;

        flashlight.intensity = currentIntensity;
    }
}
