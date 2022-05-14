using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneShape : BaseShape
{
    public Vector2 size = Vector2.one;

    private readonly Bounds baseBounds = new Bounds(Vector3.zero, Vector3.one);

    public override void DrawShapeGizmo(Color color, float expansion)
    {
        Gizmos.color = color;
        Bounds b = bounds;
        Gizmos.matrix = Matrix4x4.TRS(b.center, transform.rotation, transform.lossyScale);

        Vector3 expansionOffset = ForcesStaticMembers.DivideVectors((Vector3.forward * expansion), transform.lossyScale);

        Vector3 bl = (Vector3.down + Vector3.left) * .5f * size;
        Vector3 br = (Vector3.down + Vector3.right) * .5f * size;
        Vector3 tl = (Vector3.up + Vector3.left) * .5f * size;
        Vector3 tr = (Vector3.up + Vector3.right) * .5f * size;

        Gizmos.DrawLine(bl + expansionOffset, br + expansionOffset);
        Gizmos.DrawLine(br + expansionOffset, tr + expansionOffset);
        Gizmos.DrawLine(tr + expansionOffset, tl + expansionOffset);
        Gizmos.DrawLine(tl + expansionOffset, bl + expansionOffset);
    }

    protected override Bounds CalculateBounds()
    {
        //return ForcesStaticMembers.LocalToGlobalBounds(baseBounds, center, size, transform);
        return ForcesStaticMembers.LocalToGlobalBounds(baseBounds, Vector3.zero, size, transform);
    }

    public override Bounds GetExpandedBounds(float expansion)
    {
        Bounds newBounds = bounds;
        newBounds.center = newBounds.center + (transform.rotation * (Vector3.forward * expansion));
        newBounds.SetMinMax(ForcesStaticMembers.MinOfVectors(bounds.min, newBounds.min), ForcesStaticMembers.MaxOfVectors(bounds.max, newBounds.max));
        return newBounds;
    }

    // TODO: Find better way to cache this
    // TODO: Find better behavior for falloff and range
    public override Vector3 ClosestPointOnShape(Vector3 to)
    {
        Vector3 local = transform.InverseTransformPoint(to);

        local = Vector3.Min(local, size / 2);
        local = Vector3.Max(local, -size / 2);
        local.z = 0;
        return transform.TransformPoint(local);
    }

    public override Vector3 ClosestPointOnShape(Vector3 to, ref Vector3 normal)
    {
        // TODO: make work with center offset
        Vector3 local = transform.InverseTransformPoint(to);

        // normal is 0 if off edge of plane
        if((local.x > size.x / 2) || (local.x < (-size.x / 2)) || (local.y > (size.y / 2)) || (local.y < (-size.x / 2)))
        {
            local = Vector3.Min(local, size / 2);
            local = Vector3.Max(local, -size / 2);
            local.z = 0;

            normal = Vector3.zero;
            return transform.TransformPoint(local);
        }

        Plane p = new Plane(transform.rotation * Vector3.forward, bounds.center);

        local = Vector3.Min(local, size / 2);
        local = Vector3.Max(local, -size / 2);
        local.z = 0;
        local.z = 0;
        Vector3 global = transform.TransformPoint(local);

        // normal is 0 if behind the plane
        if (p.GetSide(to))
        {
            normal = transform.rotation * Vector3.forward;
        }
        else
        {
            normal = Vector3.zero;
        }
        return global;
    }

    public override float SignedDistance(Vector3 to)
    {
        // TODO: make work with center offset
        Vector3 local = transform.InverseTransformPoint(to);

        // if off edge of plane, be infinitely far away
        if ((local.x > size.x / 2) || (local.x < (-size.x / 2)) || (local.y > (size.y / 2)) || (local.y < (-size.x / 2)))
        {
            local = Vector3.Min(local, size / 2);
            local = Vector3.Max(local, -size / 2);
            local.z = 0;

            return Mathf.Infinity;
        }

        Plane p = new Plane(transform.rotation * Vector3.forward, bounds.center);
        float distance = p.GetDistanceToPoint(to);
        return distance < 0 ? Mathf.Infinity : distance; // return infinity if behind the plane
    }
}
