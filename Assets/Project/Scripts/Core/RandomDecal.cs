using UnityEngine;

public class RandomDecal : MonoBehaviour
{
    [Header("Variaciones de Decals")]
    [Tooltip("Arrastra aquí los GameObjects hijos que tienen los componentes Decal/Projector")]
    [SerializeField] private GameObject[] decalVariations;

    private void Start()
    {
        if (decalVariations == null || decalVariations.Length == 0) return;

        // 1. Apagamos todas las variaciones por seguridad
        foreach (GameObject decal in decalVariations)
        {
            if (decal != null) decal.SetActive(false);
        }

        // 2. Elegimos una al azar y la encendemos
        int randomIndex = Random.Range(0, decalVariations.Length);
        if (decalVariations[randomIndex] != null)
        {
            decalVariations[randomIndex].SetActive(true);
        }

        // 3. Rotación orgánica: Giramos el contenedor sobre su propio eje vertical (Y local)
        // para que los papeles parezcan esparcidos de forma caótica en el suelo.
        float randomYRotation = Random.Range(0f, 360f);
        transform.Rotate(0f, randomYRotation, 0f, Space.Self);
    }
}
