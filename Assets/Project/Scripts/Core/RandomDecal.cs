using UnityEngine;

public class RandomDecal : MonoBehaviour
{
    [Header("Variaciones de Decals")]
    [Tooltip("Arrastra aquí los GameObjects hijos que tienen los componentes Decal/Projector")]
    [SerializeField] private GameObject[] decalVariations;

    private void Start()
    {
        if (decalVariations == null || decalVariations.Length == 0) return;

        
        foreach (GameObject decal in decalVariations)
        {
            if (decal != null) decal.SetActive(false);
        }

        
        int randomIndex = Random.Range(0, decalVariations.Length);
        if (decalVariations[randomIndex] != null)
        {
            decalVariations[randomIndex].SetActive(true);
        }

        
        float randomYRotation = Random.Range(0f, 360f);
        transform.Rotate(0f, randomYRotation, 0f, Space.Self);
    }
}
