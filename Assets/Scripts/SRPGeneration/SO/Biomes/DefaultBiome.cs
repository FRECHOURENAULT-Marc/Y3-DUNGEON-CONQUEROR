using UnityEngine;

public class DefaultBiome : MonoBehaviour
{
    [SerializeField] GameObject centerCubePrefab;
    
    void Start()
    {
        Instantiate(centerCubePrefab, transform);
    }
}
