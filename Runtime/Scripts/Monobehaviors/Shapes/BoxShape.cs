using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxShape : BaseShape
{
    public Vector3 size = Vector3.one;

    private readonly Bounds baseBounds = new Bounds(Vector3.zero, Vector3.one);

    // TODO: add support for center offset
    public override void DrawShapeGizmo(Color color, float expansion)
    {
        Gizmos.color = color;

        Bounds b = bounds;
        Gizmos.matrix = Matrix4x4.TRS(b.center, transform.rotation, transform.lossyScale);

        Gizmos.DrawWireCube(Vector3.zero, size + ForcesStaticMembers.DivideVectors((Vector3.one * expansion * 2), transform.lossyScale));
    }

    protected override Bounds CalculateBounds()
    {
        //return ForcesStaticMembers.LocalToGlobalBounds(baseBounds, center, size, transform);
        return ForcesStaticMembers.LocalToGlobalBounds(baseBounds, Vector3.zero, size, transform);
    }

    public override Bounds GetExpandedBounds(float expansion)
    {
        return new Bounds(bounds.center, bounds.size + Vector3.one * expansion * 2);
    }

    public override Vector3 ClosestPointOnShape(Vector3 to)
    {
        // Firstly, transform the point into the space of the collider
        var local = transform.InverseTransformPoint(to);

        // Now, shift it to be in the center of the box
        //local -= center;

        //Pre multiply to save operations.
        var halfSize = size * 0.5f;

        // Clamp the points to the collider's extents
        var localNorm = new Vector3(
                Mathf.Clamp(local.x, -halfSize.x, halfSize.x),
                Mathf.Clamp(local.y, -halfSize.y, halfSize.y),
                Mathf.Clamp(local.z, -halfSize.z, halfSize.z)
            );

        //Calculate distances from each edge
        var dx = Mathf.Min(Mathf.Abs(halfSize.x - localNorm.x), Mathf.Abs(-halfSize.x - localNorm.x));
        var dy = Mathf.Min(Mathf.Abs(halfSize.y - localNorm.y), Mathf.Abs(-halfSize.y - localNorm.y));
        var dz = Mathf.Min(Mathf.Abs(halfSize.z - localNorm.z), Mathf.Abs(-halfSize.z - localNorm.z));

        // Select a face to project on
        if (dx < dy && dx < dz)
        {
            localNorm.x = Mathf.Sign(localNorm.x) * halfSize.x;
        }
        else if (dy < dx && dy < dz)
        {
            localNorm.y = Mathf.Sign(localNorm.y) * halfSize.y;
        }
        else if (dz < dx && dz < dy)
        {
            localNorm.z = Mathf.Sign(localNorm.z) * halfSize.z;
        }

        //Debug.DrawRay(ct.TransformPoint(localNorm), normal, Color.red);

        // Return resulting point
        return transform.TransformPoint(localNorm);
    }

    public override Vector3 ClosestPointOnShape(Vector3 to, ref Vector3 normal)
    {
        // Firstly, transform the point into the space of the collider
        var local = transform.InverseTransformPoint(to);

        // Now, shift it to be in the center of the box
        //local -= center;

        //Pre multiply to save operations.
        var halfSize = size * 0.5f;

        // Clamp the points to the collider's extents
        var localNorm = new Vector3(
                Mathf.Clamp(local.x, -halfSize.x, halfSize.x),
                Mathf.Clamp(local.y, -halfSize.y, halfSize.y),
                Mathf.Clamp(local.z, -halfSize.z, halfSize.z)
            );

        //Calculate distances from each edge
        var dx = Mathf.Min(Mathf.Abs(halfSize.x - localNorm.x), Mathf.Abs(-halfSize.x - localNorm.x));
        var dy = Mathf.Min(Mathf.Abs(halfSize.y - localNorm.y), Mathf.Abs(-halfSize.y - localNorm.y));
        var dz = Mathf.Min(Mathf.Abs(halfSize.z - localNorm.z), Mathf.Abs(-halfSize.z - localNorm.z));

        // Select a face to project on
        if (dx < dy && dx < dz)
        {
            localNorm.x = Mathf.Sign(localNorm.x) * halfSize.x;
        }
        else if (dy < dx && dy < dz)
        {
            localNorm.y = Mathf.Sign(localNorm.y) * halfSize.y;
        }
        else if (dz < dx && dz < dy)
        {
            localNorm.z = Mathf.Sign(localNorm.z) * halfSize.z;
        }

        //set normal for ref
        //this could prob use some work
        normal = localNorm;
        if ((Mathf.Abs(normal.x) > Mathf.Abs(normal.y)) &&
            (Mathf.Abs(normal.x) > Mathf.Abs(normal.z)))
        {
            normal = new Vector3(normal.x, 0, 0);
        }
        else if ((Mathf.Abs(normal.y) > Mathf.Abs(normal.x)) &&
            (Mathf.Abs(normal.y) > Mathf.Abs(normal.z)))
        {
            normal = new Vector3(0, normal.y, 0);
        }
        else
        {
            normal = new Vector3(0, 0, normal.z);
        }
        normal = normal.normalized;
        normal = transform.TransformDirection(normal);

        // Now we undo our transformations
        //localNorm += center;

        //Debug.DrawRay(ct.TransformPoint(localNorm), normal, Color.red);

        // Return resulting point
        return transform.TransformPoint(localNorm);
    }

    public override float SignedDistance(Vector3 to)
    {
        // TODO: add in center offset
        Vector3 local = ForcesStaticMembers.MultiplyVectors(transform.InverseTransformPoint(to), transform.lossyScale);
        Vector3 offset = ForcesStaticMembers.AbsVector(local) - ForcesStaticMembers.MultiplyVectors(size / 2, transform.lossyScale);
        //Debug.Log(Vector3.Magnitude(ForcesStaticMembers.MaxVector(offset, 0)) + Mathf.Min(ForcesStaticMembers.VectorHighest(offset), 0));
        return Vector3.Magnitude(ForcesStaticMembers.MaxVector(offset, 0)) + Mathf.Min(ForcesStaticMembers.VectorHighest(offset), 0);
    }
}
