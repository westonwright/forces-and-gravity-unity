using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereShape : BaseShape
{
    public float radius = .5f;

    protected override void OnDrawGizmos()
    {
        Gizmos.color = ForcesStaticMembers.shapeColor;

        Bounds b = bounds;
        Gizmos.matrix = Matrix4x4.TRS(b.center, transform.rotation, b.size);

        Gizmos.DrawWireSphere(Vector3.zero, radius);
    }

    protected override Bounds GetBounds()
    {
        return new Bounds
        (
            (transform.rotation * ForcesStaticMembers.MultiplyVectors(center, transform.lossyScale)) + transform.position,
            (radius * 2 * ForcesStaticMembers.VectorMax(transform.lossyScale)) * Vector3.one
        );
    }

    public override Vector3 ClosestPointOnShape(Vector3 to)
    {
        Vector3 p;

        p = to - transform.position;
        p.Normalize();

        p *= radius * ForcesStaticMembers.VectorMax(transform.localScale);
        p += transform.position;

        //Debug.DrawRay(p, normal, Color.red);

        return p;
    }

    public override Vector3 ClosestPointOnShape(Vector3 to, ref Vector3 normal)
    {
        Vector3 p;

        p = to - transform.position;
        p.Normalize();

        //set normal for ref
        normal = p;

        p *= radius * ForcesStaticMembers.VectorMax(transform.localScale);
        p += transform.position;

        //Debug.DrawRay(p, normal, Color.red);

        return p;
    }
}
