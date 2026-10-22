
using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

public static class Utils {
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
    public static void AddQuad(List<Vector3> verts, List<int> tris, Transform parent,
        Vector3 a, Vector3 b, Vector3 c, Vector3 d, bool doubleSided)
    {
        int start = verts.Count;
        verts.Add(parent.InverseTransformPoint(a));
        verts.Add(parent.InverseTransformPoint(b));
        verts.Add(parent.InverseTransformPoint(c));
        verts.Add(parent.InverseTransformPoint(d));

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
        tris.AddRange(new[] { back, back + 2, back + 1, back, back + 3, back + 2 });
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
}

public class RayData
{
    public Vector3 origin;
    public Vector3 direction;
}