using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereShape : BaseShape
{
    [SerializeField]
    private float Radius = .5f;
    public float radius
    {
        get { return Radius; }
        set {
            if (isStatic) return;
            if (Radius != value) {
                needsUpdate = true;
                Radius = value;
            }
        }
    }

    public override void DrawShapeGizmo(Color color, float expansion)
    {
        Gizmos.color = color;

        Bounds b = bounds;
        Gizmos.matrix = Matrix4x4.TRS(b.center, transform.rotation, b.size);

        Gizmos.DrawWireSphere(Vector3.zero, .5f + (expansion / ForcesStaticMembers.VectorHighest(b.size)));
    }

    public override Vector3 ClosestPointOnShape(Vector3 to)
    {
        Vector3 p;

        p = to - transform.position;
        p.Normalize();

        p *= radius * ForcesStaticMembers.VectorHighest(transform.localScale);
        p += transform.position;

        //Debug.DrawRay(p, normal, Color.red);

        return p;
    }

    public override Vector3 ClosestPointOnShape(Vector3 to, out Vector3 normal)
    {
        Vector3 p;

        p = to - transform.position;
        p.Normalize();

        //set normal for ref
        normal = p;

        p *= radius * ForcesStaticMembers.VectorHighest(transform.localScale);
        p += transform.position;

        //Debug.DrawRay(p, normal, Color.red);

        return p;
    }

    public override float SignedDistance(Vector3 to)
    {
        // TODO: add in center offset

        Vector3 local = to - transform.position;
        //Debug.Log(Vector3.Magnitude(local) - (radius * ForcesStaticMembers.VectorHighest(transform.lossyScale)));
        return Vector3.Magnitude(local) - (radius * ForcesStaticMembers.VectorHighest(transform.lossyScale));
    }


    protected override Bounds CalculateBounds()
    {
        return new Bounds
        (
            transform.rotation * transform.position,
            (radius * 2 * ForcesStaticMembers.VectorHighest(transform.lossyScale)) * Vector3.one
        );
    }

    public override Bounds CalculateExpandedBounds(float expansion)
    {
        return new Bounds(bounds.center, bounds.size + Vector3.one * expansion * 2);
    }

}
