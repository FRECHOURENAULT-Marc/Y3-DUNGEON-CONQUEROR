using UnityEngine;

public class MaterialTilingByScale : MonoBehaviour
{
    [SerializeField] Vector2 factors;
    [SerializeField] bool ignoreX;
    [SerializeField] bool ignoreY;
    [SerializeField] bool useZAsY;

    void Reset()
    {
        factors = Vector3.one;
        
        ignoreX = false;
        ignoreY = false;
        useZAsY = false;
    }
    
    void Start()
    {
        Vector2 texScale = new Vector2(
            ignoreX ? 1f : transform.lossyScale.x * factors.x, 
            ignoreY ? 1f : transform.lossyScale.y * factors.y
        );

        if (useZAsY)
            texScale.y = transform.lossyScale.z * factors.y;
        
        GetComponent<MeshRenderer>().material.mainTextureScale = texScale;
    }
}
