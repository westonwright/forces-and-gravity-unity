using UnityEngine;
using System.Collections.Generic;
// with code from HiddenMonk: https://forum.unity.com/threads/get-the-collision-points-in-physics-overlapsphere.395176/#post-2581349

//place on mesh used to get gravity vector
[RequireComponent(typeof(MeshCollider))]
public class MeshKDTree : MonoBehaviour
{
    int[] tris;
    Vector3[] verts;
    Vector3[] norms;
    KDTree kd;
    VertTriList vt;

    void Awake()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        Mesh mesh = mf.mesh;
        //MeshCollider mc = GetComponent<MeshCollider>();
        //Mesh mesh = mc.sharedMesh;
        vt = new VertTriList(mesh);
        verts = mesh.vertices;
        norms = mesh.normals;
        tris = mesh.triangles;
        kd = KDTree.MakeFromPoints(verts);
    }

    public Vector3 ClosestPointOnSurface(Vector3 position, ref Vector3 normal)
    {
        position = transform.InverseTransformPoint(position);
        return transform.TransformPoint(NearestPointOnMesh(position, verts, kd, tris, norms, vt, ref normal));
    }

    List<int> nearests = new List<int>();
    Vector3 NearestPointOnMesh(Vector3 pt, Vector3[] verts, KDTree vertProx, int[] tris, Vector3[] norms, VertTriList vt, ref Vector3 normal)
    {
        //First, find the nearest vertex (the nearest point must be on one of the triangles
        //that uses this vertex if the mesh is convex).
        //Since there can be multiple vertices on a single spot, we need to find the correct vert and triangle.
        vertProx.FindNearestEpsilon(pt, nearests);

        Vector3 nearestPt = Vector3.zero;
        float nearestSqDist = 100000000f;
        Vector3 possNearestPt;

        Vector3 A = Vector3.zero;
        Vector3 B = Vector3.zero;
        Vector3 C = Vector3.zero;
        int T = 0;
        for (int i = 0; i < nearests.Count; i++)
        {
            //Get the list of triangles in which the nearest vert "participates".
            int[] nearTris = vt[nearests[i]];

            for (int j = 0; j < nearTris.Length; j++)
            {
                int triOff = nearTris[j] * 3;
                Vector3 a = verts[tris[triOff]];
                Vector3 b = verts[tris[triOff + 1]];
                Vector3 c = verts[tris[triOff + 2]];

                CustomGravityHelperFunctions.ClosestPointOnTriangleToPoint(ref pt, ref a, ref b, ref c, out possNearestPt);
                float possNearestSqDist = (pt - possNearestPt).sqrMagnitude;

                if (possNearestSqDist < nearestSqDist)
                {
                    A = a;
                    B = b;
                    C = c;
                    T = triOff;

                    nearestPt = possNearestPt;
                    nearestSqDist = possNearestSqDist;
                }
            }
        }
        //set normal vector for gravity
        normal = CustomGravityHelperFunctions.SmoothedNormalVector(nearestPt, A, B, C, norms[tris[T]], norms[tris[T + 1]], norms[tris[T + 2]], transform);

        return nearestPt;
    }
}