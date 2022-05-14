using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshShapeKDTree))]
public class MeshShape : BaseShape
{
    public Mesh mesh;

    private MeshShapeKDTree mehsShapeKDTree;
    public override void DrawShapeGizmo(Color color, float expansion)
    {
        Gizmos.color = color;

        if(mesh != null)
        {
            if (expansion == 0)
            {
                Bounds b = bounds;
                Gizmos.matrix = Matrix4x4.TRS(b.center, transform.rotation, transform.lossyScale);

                Gizmos.DrawWireMesh(mesh);
            }
            else
            {
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
                Mesh rangeMesh = new Mesh();
                rangeMesh.vertices = mesh.vertices;
                rangeMesh.triangles = mesh.triangles;
                rangeMesh.normals = mesh.normals;
                Vector3[] vertices = rangeMesh.vertices;
                Vector3[] normals = rangeMesh.normals;
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] = ForcesStaticMembers.MultiplyVectors(vertices[i], transform.localScale);
                    vertices[i] += normals[i] * expansion;
                }
                rangeMesh.vertices = vertices;
                Gizmos.DrawWireMesh(rangeMesh);
            }
        }
    }

    private void Awake()
    {
        mehsShapeKDTree = GetComponent<MeshShapeKDTree>();
    }

    protected override Bounds CalculateBounds()
    {
        if(mesh != null)
        {
            //return ForcesStaticMembers.LocalToGlobalBounds(mesh.bounds, center, Vector3.one, transform);
            return ForcesStaticMembers.LocalToGlobalBounds(mesh.bounds, Vector3.zero, Vector3.one, transform);
        }
        else
        {
            return new Bounds();
        }
    }

    public override Bounds GetExpandedBounds(float expansion)
    {
        return new Bounds(bounds.center, bounds.size + Vector3.one * expansion * 2);
    }

    public override Vector3 ClosestPointOnShape(Vector3 to)
    {
        return mehsShapeKDTree.ClosestPointOnSurface(to);

    }

    public override Vector3 ClosestPointOnShape(Vector3 to, ref Vector3 normal)
    {
        return mehsShapeKDTree.ClosestPointOnSurface(to, ref normal);
    }

    public override float SignedDistance(Vector3 to)
    {
        return mehsShapeKDTree.SignedDistance(to);
    }
}
