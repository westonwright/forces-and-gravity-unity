using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForcePoint : ForceProducer
{
#if UNITY_EDITOR
    [UnityEditor.MenuItem(MENU_NAME + "Point", false, 0)]
    static void InstantiateForcePoint()
    {
        GameObject go = new GameObject("Force Point");
        go.AddComponent<ForcePoint>();
    }
#endif

    private float Radius;
    public float radius
    {
        get { return Radius; }
    }

    private void OnDrawGizmos()
    {
        if (!preview) return;

        Gizmos.color = forceType.previewColor;
        Gizmos.color = (additive ? Gizmos.color : Gizmos.color * ForcesStaticMembers.lightGray) * (enableForce ? 1 : .25f);
        Gizmos.DrawWireSphere(transform.position, ForcesStaticMembers.VectorHighest(transform.localScale));

        if (falloffRange > 0)
        {
            Gizmos.color = Gizmos.color * ForcesStaticMembers.semiTransparent; //makes falloff semi-transparent
            Gizmos.DrawWireSphere(transform.position, ForcesStaticMembers.VectorHighest(transform.localScale) + falloffRange);
        }
    }

    protected override void Reset()
    {
        base.Reset();

        FalloffRange = 10f;
    }

    // returns the force vector with strength baked in
    public override Vector3 ForceVector(Vector3 point)
    {
        float distance = Vector3.Distance(transform.position, point);
        if (distance < (radius + falloffRange))
        {
            Vector3 direction = (transform.position - point).normalized;
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
            Vector3 direction = (transform.position - point).normalized;
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

    // use these if position or scale is changing
    // or if falloff/force range changed
    public override void TryUpdateProducer()
    {
        if (transform.hasChanged || needsUpdate)
        {
            UpdateProducer();
            transform.hasChanged = false;
            needsUpdate = false;
        }
    }

    public override void UpdateProducer()
    {
        Radius = ForcesStaticMembers.VectorHighest(transform.localScale);
    }
}
