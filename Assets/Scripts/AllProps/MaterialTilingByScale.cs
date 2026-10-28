using System;
using Unity.Mathematics;using UnityEngine;

public class MaterialTilingByScale : MonoBehaviour
{
    [SerializeField] Vector2 factors;
    [SerializeField] bool ignoreX;
    [SerializeField] bool ignoreY;
    [Header("Affections X")]
    [SerializeField] bool useYAsX;
    [SerializeField] bool useZAsX;
    [Header("Affections Y")]
    [SerializeField] bool useXAsY;
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

        if (useZAsX)
            texScale.x = transform.lossyScale.z * factors.x;        
        if (useYAsX)
            texScale.x = transform.lossyScale.y * factors.x;
        
        if (useZAsY)
            texScale.y = transform.lossyScale.z * factors.y;        
        if (useXAsY)
            texScale.y = transform.lossyScale.x * factors.y;        
        
        GetComponent<MeshRenderer>().material.mainTextureScale = texScale;
    }
}
