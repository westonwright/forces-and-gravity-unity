using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshShape : BaseShape
{
    [SerializeField]
    private Mesh Mesh;
    public Mesh mesh
    {
        get { return Mesh; }
        set {
            if (isStatic) return;
            if (Mesh != value) {
                needsUpdate = true;
                Mesh = value;
            }
        }
    }
    private MeshKDTree meshKDTree;

    public static MeshShape AddMeshShapeComponent(GameObject gameObject, Mesh mesh)
    {
        MeshShape meshShape = gameObject.AddComponent<MeshShape>();
        meshShape.Mesh = mesh;
        meshShape.InitKDTree();
        return meshShape;
    }

    protected override void Reset()
    {
        base.Reset();
    }

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

    private void InitKDTree()
    {
        meshKDTree = new MeshKDTree(mesh, transform);
    }

    private void Awake()
    {
        if (mesh != null)
        {
            InitKDTree();
        }
    }

    public override Vector3 ClosestPointOnShape(Vector3 to)
    {
        return meshKDTree.ClosestPointOnSurface(to);

    }

    public override Vector3 ClosestPointOnShape(Vector3 to, out Vector3 normal)
    {
        return meshKDTree.ClosestPointOnSurface(to, out normal);
    }

    public override float SignedDistance(Vector3 to)
    {
        return meshKDTree.SignedDistance(to);
    }

    protected override Bounds CalculateBounds()
    {
        if (mesh != null)
        {
            //return ForcesStaticMembers.LocalToGlobalBounds(mesh.bounds, center, Vector3.one, transform);
            return ForcesStaticMembers.LocalToGlobalBounds(mesh.bounds, Vector3.zero, Vector3.one, transform);
        }
        else
        {
            return new Bounds();
        }
    }

    public override Bounds CalculateExpandedBounds(float expansion)
    {
        return new Bounds(bounds.center, bounds.size + Vector3.one * expansion * 2);
    }

}
