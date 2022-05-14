using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

public static class ForcesStaticMembers
{
    /*
    private static string forceManagerSOName = "ForceManagerSO.asset";
    private static string defaultForceTypeSOName = "ForceSO.asset";
    private static string packageName = "com.weston-wright.forces-and-gravity";
    private static string scriptableObjectsPath = "/Runtime/Scriptable Objects/";
    private static string managersSOPath = "Managers/";
    private static string forceTypeSOPath = "Force Types/";
    */
    public static ForceManagerSO forceManagerSO;
    public static ForceTypeSO defaultForceTypeSO;

    public static Color shapeColor;
    public static Color semiTransparent;
    public static Color lightGray;

    static ForcesStaticMembers()
    {
        forceManagerSO = Resources.Load<ForceManagerSO>("ForceManagerSO");
        defaultForceTypeSO = Resources.Load<ForceTypeSO>("ForceSO");
        /*
        string dataPath = GetDataPath();
        forceManagerSO = (ForceManagerSO)AssetDatabase.LoadAssetAtPath(dataPath + packageName + scriptableObjectsPath + managersSOPath + forceManagerSOName, typeof(ForceManagerSO));
        if (forceManagerSO == null)
        {
            Debug.LogError("Missing Scriptable Object " + forceManagerSOName + "! Package may be corrupted !");
        }

        defaultForceTypeSO = (ForceTypeSO)AssetDatabase.LoadAssetAtPath(dataPath + packageName + scriptableObjectsPath + forceTypeSOPath + defaultForceTypeSOName, typeof(ForceTypeSO));
        if (defaultForceTypeSO == null)
        {
            Debug.LogError("Missing Scriptable Object " + defaultForceTypeSOName + "! Package may be corrupted !");
        }
        */
        shapeColor = new Color(.5f, .5f, 1f);
        semiTransparent = new Color(1, 1, 1, .5f);
        lightGray = new Color(.75f, .75f, .75f, 1);
    }

    /*
    private static string GetDataPath()
    {
        // detect if in packages or assets folder
        string dataPath = "Packages/";
#if UNITY_EDITOR
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(assembly);
        if (packageInfo != null)
        {
            dataPath = "Packages/";
            //Debug.Log("In package " + packageInfo.name);
        }
        else
        {
            dataPath = "Assets/";
            //Debug.Log("Not in package");
        }
#endif  
        return dataPath;
    }
    */
    public static Vector3 SmoothedNormalVector(Vector3 nearestPt, Vector3 A, Vector3 B, Vector3 C, Vector3 N0, Vector3 N1, Vector3 N2, Transform transform)
    {
        Vector3 bary = ForcesStaticMembers.Barycentric(nearestPt, A, B, C);
        Vector3 normal = (bary.x * N0 + bary.y * N1 + bary.z * N2).normalized;
        return transform.TransformDirection(normal);
    }

    // with refrence from Roystan Ross: https://roystanross.wordpress.com/category/unity-character-controller-series/
    public static void ClosestPointOnTriangleToPoint(ref Vector3 point, ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3, out Vector3 result)
    {
        //Source: Real-Time Collision Detection by Christer Ericson
        //Reference: Page 136

        //Check if P in vertex region outside A
        Vector3 ab = vertex2 - vertex1;
        Vector3 ac = vertex3 - vertex1;
        Vector3 ap = point - vertex1;

        float d1 = Vector3.Dot(ab, ap);
        float d2 = Vector3.Dot(ac, ap);
        if (d1 <= 0.0f && d2 <= 0.0f)
        {
            result = vertex1; //Barycentric coordinates (1,0,0)
            return;
        }

        //Check if P in vertex region outside B
        Vector3 bp = point - vertex2;
        float d3 = Vector3.Dot(ab, bp);
        float d4 = Vector3.Dot(ac, bp);
        if (d3 >= 0.0f && d4 <= d3)
        {
            result = vertex2; // barycentric coordinates (0,1,0)
            return;
        }

        //Check if P in edge region of AB, if so return projection of P onto AB
        float vc = d1 * d4 - d3 * d2;
        if (vc <= 0.0f && d1 >= 0.0f && d3 <= 0.0f)
        {
            float v = d1 / (d1 - d3);
            result = vertex1 + v * ab; //Barycentric coordinates (1-v,v,0)
            return;
        }

        //Check if P in vertex region outside C
        Vector3 cp = point - vertex3;
        float d5 = Vector3.Dot(ab, cp);
        float d6 = Vector3.Dot(ac, cp);
        if (d6 >= 0.0f && d5 <= d6)
        {
            result = vertex3; //Barycentric coordinates (0,0,1)
            return;
        }

        //Check if P in edge region of AC, if so return projection of P onto AC
        float vb = d5 * d2 - d1 * d6;
        if (vb <= 0.0f && d2 >= 0.0f && d6 <= 0.0f)
        {
            float w = d2 / (d2 - d6);
            result = vertex1 + w * ac; //Barycentric coordinates (1-w,0,w)
            return;
        }

        //Check if P in edge region of BC, if so return projection of P onto BC
        float va = d3 * d6 - d5 * d4;
        if (va <= 0.0f && (d4 - d3) >= 0.0f && (d5 - d6) >= 0.0f)
        {
            float w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
            result = vertex2 + w * (vertex3 - vertex2); //Barycentric coordinates (0,1-w,w)
            return;
        }

        //P inside face region. Compute Q through its barycentric coordinates (u,v,w)
        float denom = 1.0f / (va + vb + vc);
        float v2 = vb * denom;
        float w2 = vc * denom;
        result = vertex1 + ab * v2 + ac * w2; //= u*vertex1 + v*vertex2 + w*vertex3, u = va * denom = 1.0f - v - w
    }

    // Compute barycentric coordinates (u, v, w) for
    // point p with respect to triangle (a, b, c)
    public static Vector3 Barycentric(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
    {
        float u = 0;
        float v = 0;
        float w = 0;
        Vector3 v0 = b - a, v1 = c - a, v2 = p - a;
        float d00 = Vector3.Dot(v0, v0);
        float d01 = Vector3.Dot(v0, v1);
        float d11 = Vector3.Dot(v1, v1);
        float d20 = Vector3.Dot(v2, v0);
        float d21 = Vector3.Dot(v2, v1);
        float denom = d00 * d11 - d01 * d01;
        v = (d11 * d20 - d01 * d21) / denom;
        w = (d00 * d21 - d01 * d20) / denom;
        u = 1.0f - v - w;
        return new Vector3(u, v, w);
    }

    public static Bounds LocalToGlobalBounds(Bounds localBounds, Vector3 center, Vector3 size, Transform transform)
    {
        Bounds globalBounds = new Bounds();

        globalBounds.center = MultiplyVectors(localBounds.center, transform.lossyScale) + transform.position + (transform.rotation * MultiplyVectors(center, transform.lossyScale));
        globalBounds.size = MultiplyVectors(MultiplyVectors(localBounds.size, size), transform.lossyScale);

        Vector3 extents = globalBounds.extents;

        Vector3[] points = new Vector3[8];
        points[0] = center + (transform.rotation * new Vector3(extents.x, extents.y, extents.z));
        points[1] = center + (transform.rotation * new Vector3(extents.x, extents.y, -extents.z));
        points[2] = center + (transform.rotation * new Vector3(extents.x, -extents.y, extents.z));
        points[3] = center + (transform.rotation * new Vector3(extents.x, -extents.y, -extents.z));
        points[4] = center + (transform.rotation * new Vector3(-extents.x, extents.y, extents.z));
        points[5] = center + (transform.rotation * new Vector3(-extents.x, extents.y, -extents.z));
        points[6] = center + (transform.rotation * new Vector3(-extents.x, -extents.y, extents.z));
        points[7] = center + (transform.rotation * new Vector3(-extents.x, -extents.y, -extents.z));

        Vector3 maxVector = Vector3.negativeInfinity;

        Vector3 minVector = Vector3.positiveInfinity;

        foreach (Vector3 point in points)
        {
            if (point.x > maxVector.x)
            {
                maxVector.x = point.x;
            }
            if (point.y > maxVector.y)
            {
                maxVector.y = point.y;
            }
            if (point.z > maxVector.z)
            {
                maxVector.z = point.z;
            }
            if (point.x < minVector.x)
            {
                minVector.x = point.x;
            }
            if (point.y < minVector.y)
            {
                minVector.y = point.y;
            }
            if (point.z < minVector.z)
            {
                minVector.z = point.z;
            }
        }
        globalBounds.size = maxVector - minVector;

        return globalBounds;
    }

    public static void VectorCompositeStraight(float alphaA, float alphaB, Vector3 vectorA, Vector3 vectorB, out float alpha, out Vector3 color)
    {
        alpha = alphaA + (alphaB * (1 - alphaA));
        color = ((vectorA * alphaA) + ((vectorB * alphaB) * (1 - alphaA))) / alpha;
    }

    public static Color MultiplyColors(Color c1, Color c2)
    {
        return new Color(c1.r * c2.r, c1.g * c2.g, c1.b * c2.b, c1.a * c2.a);
    }
    
    public static Vector3 MultiplyVectors(Vector3 v1, Vector3 v2)
    {
        return new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
    }

    public static Vector2 MultiplyVectors(Vector2 v1, Vector2 v2)
    {
        return new Vector2(v1.x * v2.x, v1.y * v2.y);
    }
    
    public static Vector3 DivideVectors(Vector3 v1, Vector3 v2)
    {
        return new Vector3(v1.x / v2.x, v1.y / v2.y, v1.z / v2.z);
    }

    public static Vector2 DivideVectors(Vector2 v1, Vector2 v2)
    {
        return new Vector2(v1.x / v2.x, v1.y / v2.y);
    }

    public static Vector3 AddToVector(Vector3 v, float f)
    {
        return new Vector3(v.x + f, v.y + f, v.z + f);
    }

    public static Vector2 AddToVector(Vector2 v, float f)
    {
        return new Vector2(v.x + f, v.y + f);
    }

    public static Vector3 AbsVector(Vector3 v)
    {
        return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
    }
    
    public static Vector2 AbsVector(Vector2 v)
    {
        return new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y));
    }
    public static Vector3 MaxOfVectors(Vector3 v1, Vector3 v2)
    {
        return new Vector3(Mathf.Max(v1.x, v2.x), Mathf.Max(v1.y, v2.y), Mathf.Max(v1.z, v2.z));
    }

    public static Vector2 MaxOfVectors(Vector2 v1, Vector2 v2)
    {
        return new Vector2(Mathf.Max(v1.x, v2.x), Mathf.Max(v1.y, v2.y));
    }

    public static Vector3 MinOfVectors(Vector3 v1, Vector3 v2)
    {
        return new Vector3(Mathf.Min(v1.x, v2.x), Mathf.Min(v1.y, v2.y), Mathf.Min(v1.z, v2.z));
    }

    public static Vector2 MinOfVectors(Vector2 v1, Vector2 v2)
    {
        return new Vector2(Mathf.Min(v1.x, v2.x), Mathf.Min(v1.y, v2.y));
    }

    public static Vector3 MaxVector(Vector3 v, float f)
    {
        return new Vector3(Mathf.Max(v.x, f), Mathf.Max(v.y, f), Mathf.Max(v.z, f));
    }
    
    public static Vector2 MaxVector(Vector2 v, float f)
    {
        return new Vector2(Mathf.Max(v.x, f), Mathf.Max(v.y, f));
    }
    
    public static Vector3 MinVector(Vector3 v, float f)
    {
        return new Vector3(Mathf.Min(v.x, f), Mathf.Min(v.y, f), Mathf.Min(v.z, f));
    }
    
    public static Vector2 MinVector(Vector2 v, float f)
    {
        return new Vector2(Mathf.Min(v.x, f), Mathf.Min(v.y, f));
    }

    public static float VectorHighest(Vector3 v)
    {
        return Mathf.Max(Mathf.Max(v.x, v.y), v.z);
    }

    public static float VectorHighest(Vector2 v)
    {
        return Mathf.Max(v.x, v.y);
    }
    
    public static float VectorLowest(Vector3 v)
    {
        return Mathf.Min(Mathf.Min(v.x, v.y), v.z);
    }

    public static float VectorLowest(Vector2 v)
    {
        return Mathf.Min(v.x, v.y);
    }


}
