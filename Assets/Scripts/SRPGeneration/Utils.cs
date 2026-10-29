
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;
using Object = System.Object;
using Random = UnityEngine.Random;

public static class Utils {

    /// <summary>
    /// Destroy a UnityEngine.Object depends on Application.isPlaying
    /// Do Destroy or DestroyImmediate
    /// </summary>
    public static void Destroy(UnityEngine.Object obj)
    {
        if(Application.isPlaying)
            UnityEngine.Object.Destroy(obj);
        else
            UnityEngine.Object.DestroyImmediate(obj);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public static DateTime GetElapsedTime(DateTime start, DateTime end)
    {
        return new DateTime((end.Ticks - start.Ticks));
    }
    
    /// <summary>
    /// Shuffles the element order of the specified list.
    /// </summary>
    public static void Shuffle<T>(IList<T> ts) {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i) {
            var r = Random.Range(i, count);
            (ts[i], ts[r]) = (ts[r], ts[i]);
        }
    }
    
    /// <summary>
    /// Destroy (Immediate) all GameObjects in the list
    /// </summary>
    static public void DestroyObjectsInList(ref List<GameObject> list)
    {
        for (int i = list.Count - 1; i >= 0; i--)
            Utils.Destroy(list[i]);
        list.Clear();
    }

    /// <summary>
    /// Destroy (Immediate) all Component in the list
    /// </summary>
    static public void DestroyObjectsInList<T>(ref List<T> list) where T : Component
    {
        for (int i = list.Count - 1; i >= 0; i--)
            Utils.Destroy(list[i]);
        list.Clear();
    } 
    
    /// <summary>
    /// Do a Bounds.Intersects with GameObject scales
    /// </summary>
    static public bool CollidingWith(GameObject go1, GameObject go2)
    {
        Bounds b1 = new Bounds(go1.transform.position, go1.transform.localScale);
        Bounds b2 = new Bounds(go2.transform.position, go2.transform.localScale);
        
        return CollidingWith(b1, b2);
    }    
    /// <summary>
    /// Do a Bounds.Intersects with Collider.bounds.size
    /// </summary>
    static public bool CollidingWith(Collider c1, Collider c2)
    {
        Bounds b1 = new Bounds(c1.transform.position,  c1.bounds.size);
        Bounds b2 = c2.bounds;
        
        return CollidingWith(b1, b2);
    }    
    /// <summary>
    /// Do a Bounds.Intersects
    /// </summary>
    static public bool CollidingWith(Bounds b1, Bounds b2)
    {
        return b1.Intersects(b2);
    }
    
    /// <summary>
    /// Get edges of a rect
    /// </summary>
    public static Vector3[] GetRectCorners(Vector3 center, Vector3 right, Vector3 up, float width, float height)
    {
        Vector3 hx = right * (width * 0.5f);
        Vector3 hy = up * (height * 0.5f);
        return new[]
        {
            center - hx - hy, // bot left
            center + hx - hy, // bot right
            center + hx + hy, // top right
            center - hx + hy, // top left
        };
    }

    /// <summary>
    /// Add 4 (or 8 if doubleSided) vertices (quad) to vertices list with associate triangles
    /// World points are converted to "parent" local space
    /// </summary>
    public static void AddQuad(List<Vector3> verts, List<int> tris, List<Vector2> uvs, Transform parent,
        Vector3 a, Vector3 b, Vector3 c, Vector3 d, bool doubleSided)
    {
        int start = verts.Count;
        verts.Add(parent.InverseTransformPoint(a));
        verts.Add(parent.InverseTransformPoint(b));
        verts.Add(parent.InverseTransformPoint(c));
        verts.Add(parent.InverseTransformPoint(d));
        
        // UV : bottom-left, bottom-right, top-right, top-left
        uvs.Add(new Vector2(0f, 0f));
        uvs.Add(new Vector2(1f, 0f));
        uvs.Add(new Vector2(1f, 1f));
        uvs.Add(new Vector2(0f, 1f));

        // Front face
        tris.AddRange(new[] { start, start + 1, start + 2, start, start + 2, start + 3 });

        // Need double face, otherwise transparent from one side
        if (!doubleSided) return;

        // Back face : duplicate vertex to keep working normals
        int back = verts.Count;
        verts.Add(verts[start]);
        verts.Add(verts[start + 1]);
        verts.Add(verts[start + 2]);
        verts.Add(verts[start + 3]);
        
        // same as front
        uvs.Add(new Vector2(0f, 0f));
        uvs.Add(new Vector2(1f, 0f));
        uvs.Add(new Vector2(1f, 1f));
        uvs.Add(new Vector2(0f, 1f));
        
        tris.AddRange(new[]
        {
            back, back + 2, back + 1, 
            back, back + 3, back + 2
        });
    }

    /// <summary>
    /// Get Real Collider size (do not depend on rotation --> better than bounds.size AABB)
    /// </summary>
    public static Vector3 GetWorldSize(Collider col)
    {
        // "is" : accepts inheritance and null
        if (col is BoxCollider box)
        {
            Vector3 s = Vector3.Scale(box.size, box.transform.lossyScale);
            return new Vector3(Mathf.Abs(s.x), Mathf.Abs(s.y), Mathf.Abs(s.z));
        }

        return col.bounds.size;
    }
    
    public static async Task<bool> WaitOneAsyncFrame(CancellationTokenSource cs, Action task = null)
    {
        return await WaitOneAsyncFrame(cs.Token, task);
    }
    /// <summary>
    /// Wait for one async frame
    /// </summary>
    /// <returns>return false if cancel with token, otherwise true</returns>
    public static async Task<bool> WaitOneAsyncFrame(CancellationToken token = default, Action task = null)
    {
        await Awaitable.NextFrameAsync(token);
        if (token.IsCancellationRequested)
            return false;
        
        task?.Invoke();
        return true;
    }
}

public class RayData
{
    public Vector3 origin;
    public Vector3 direction;
}