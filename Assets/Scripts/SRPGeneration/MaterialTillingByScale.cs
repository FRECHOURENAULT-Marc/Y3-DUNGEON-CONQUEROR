using UnityEngine;

public class MaterialTillingByScale : MonoBehaviour
{
    [SerializeField] Vector2 factors;
    [SerializeField] bool ignoreX;
    [SerializeField] bool ignoreY;

    void Reset()
    {
        factors = Vector3.one;
        
        ignoreX = false;
        ignoreY = false;
    }
    
    void Start()
    {
        Vector2 texScale = new Vector2(
            ignoreX ? 1f : transform.lossyScale.x * factors.x, 
            ignoreY ? 1f : transform.lossyScale.y * factors.y
        );
        
        GetComponent<MeshRenderer>().material.mainTextureScale = texScale;
    }
}
