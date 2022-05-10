using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshShape : BaseShape
{
    public Mesh mesh;

    protected override void OnDrawGizmos()
    {

        Gizmos.color = ForcesStaticMembers.shapeColor;

        Bounds b = bounds;
        Gizmos.matrix = Matrix4x4.TRS(b.center, transform.rotation, transform.lossyScale);

        Gizmos.DrawWireMesh(mesh);
    }

    protected override Bounds GetBounds()
    {
        return ForcesStaticMembers.LocalToGlobalBounds(mesh.bounds, center, Vector3.one, transform);
    }

    public override Vector3 ClosestPointOnShape(Vector3 to)
    {
        MeshShapeKDTree mehsShapeKDTree = GetComponent<MeshShapeKDTree>();

        //runs if the MeshKDTree exists, meaning the KDTree has baked data
        if (mehsShapeKDTree != null)
        {
            return mehsShapeKDTree.ClosestPointOnSurface(to);
        }
        //runs if the MeshKDTree does not exist and must calculate data each frame
        else
        {
            Debug.LogError("Mesh Shape " + gameObject.name + " does not have a mehsShapeKDTree! Closest point can't be calculated!", gameObject);
            return Vector3.zero;
        }
    }

    public override Vector3 ClosestPointOnShape(Vector3 to, ref Vector3 normal)
    {
        MeshShapeKDTree mehsShapeKDTree = GetComponent<MeshShapeKDTree>();

        //runs if the MeshKDTree exists, meaning the KDTree has baked data
        if (mehsShapeKDTree != null)
        {
            return mehsShapeKDTree.ClosestPointOnSurface(to, ref normal);
        }
        //runs if the MeshKDTree does not exist and must calculate data each frame
        else
        {
            Debug.LogError("Mesh Shape " + gameObject.name + " does not have a mehsShapeKDTree! Closest point can't be calculated!", gameObject);
            return Vector3.zero;
        }
    }
}
