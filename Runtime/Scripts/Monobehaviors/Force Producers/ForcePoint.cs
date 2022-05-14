using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForcePoint : ForceProducer
{
    [SerializeField]
    [Tooltip("The distance it takes for the force to fade")]
    protected float falloffRange = 10f;

    private float radius;
    protected override void OnDrawGizmos()
    {
        if (preview)
        {
            Gizmos.color = forceType.previewColor;
            Gizmos.color = (additive ? Gizmos.color : Gizmos.color * ForcesStaticMembers.lightGray) * (enableForce ? 1 : .25f);
            Gizmos.DrawWireSphere(transform.position, ForcesStaticMembers.VectorHighest(transform.localScale));

            if (falloffRange > 0)
            {
                Gizmos.color = ForcesStaticMembers.MultiplyColors(Gizmos.color, ForcesStaticMembers.semiTransparent); //makes falloff semi-transparent
                Gizmos.DrawWireSphere(transform.position, ForcesStaticMembers.VectorHighest(transform.localScale) + falloffRange);
            }
        }
    }

    protected override void Reset()
    {
        base.Reset();
        falloffRange = 10f;
    }

    // returns the force vector with strength baked in
    public override Vector3 ForceVector(Vector3 point)
    {
        float distance = Vector3.Distance(transform.position, point);
        if (distance < (radius + falloffRange))
        {
            Vector3 direction = (point - transform.position).normalized;
            if (distance < radius)
            {
                return direction * forceStrength;
            }
            else
            {
                float strength = 1 - ((distance - radius) / (falloffRange));
                return (direction * forceStrength) * strength;
            }
        }
        else
        {
            return Vector3.zero;
        }
    }

    public override Vector3 ForceVector(Vector3 point, out float strength)
    {
        strength = 0;

        float distance = Vector3.Distance(transform.position, point);
        if (distance < (radius + falloffRange))
        {
            Vector3 direction = (point - transform.position).normalized;
            if (distance < radius)
            {
                strength = 1;
                return direction * forceStrength;
            }
            else
            {
                strength = 1 - ((distance - radius) / (falloffRange));
                return direction * forceStrength;
            }
        }
        else
        {
            return Vector3.zero;
        }
    }

    public override bool PointInRange(Vector3 point)
    {
        float distance = Vector3.Distance(transform.position, point);
        if (distance <= (radius + falloffRange))
        {
            return true;
        }
        return false;
    }

    public void SetFalloffRange(float range)
    {
        falloffRange = range;
    }

    // use these if position or scale is changing
    // or if falloff/force range changed
    public override void UpdateProducer()
    {
        if (transform.hasChanged || needsUpdate)
        {
            radius = ForcesStaticMembers.VectorHighest(transform.localScale);
            transform.hasChanged = false;
            needsUpdate = false;
        }
    }
}
