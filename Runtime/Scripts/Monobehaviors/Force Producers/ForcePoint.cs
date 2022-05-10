using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForcePoint : ForceProducer
{
    [SerializeField]
    [Tooltip("The distance it takes for the force to fade")]
    protected float falloffRange = 10f;

    protected override void OnDrawGizmos()
    {
        if (preview)
        {
            switch (forceType)
            {
                case ForceType.Force:
                    Gizmos.color = new Color(1, 0, 0, 1);
                    break;
                case ForceType.Acceleration:
                    Gizmos.color = new Color(1, .5f, 0, 1);
                    break;
                case ForceType.Impulse:
                    Gizmos.color = new Color(1, 1, 0, 1);
                    break;
                case ForceType.VelocityChange:
                    Gizmos.color = new Color(.5f, 1, 0, 1);
                    break;
                case ForceType.Wind:
                    Gizmos.color = new Color(0, 1, 0, 1);
                    break;
                case ForceType.Gravity:
                    Gizmos.color = new Color(0, 1, .5f, 1);
                    break;
                case ForceType.Generic:
                    Gizmos.color = new Color(0, 1, 1, 1);
                    break;
            }
            Gizmos.color = (additive ? Gizmos.color : Gizmos.color * new Color(.75f, .75f, .75f, 1)) * (enableForce ? 1 : .25f);
            Gizmos.DrawWireSphere(transform.position, ForcesStaticMembers.VectorMax(transform.localScale));

            if (falloffRange > 0)
            {
                Gizmos.color = ForcesStaticMembers.MultiplyColors(Gizmos.color, new Color(1, 1, 1, .25f)); //makes falloff semi-transparent
                Gizmos.DrawWireSphere(transform.position, ForcesStaticMembers.VectorMax(transform.localScale) + falloffRange);
            }
        }
    }

    public override Vector3 ForceVector(Vector3 point, out float strength)
    {
        strength = 0;

        float radius = ForcesStaticMembers.VectorMax(transform.localScale);
        float falloffRadius = radius + falloffRange;
        float distance = Vector3.Distance(transform.position, point);
        Vector3 direction = (transform.position - point).normalized;
        //Debug.DrawRay(point, Vector3.up * 2, falloffBounds.Contains(transformPoint) ? (zoneBounds.Contains(transformPoint) ? Color.green : Color.yellow) : Color.red);
        if (distance < falloffRadius)
        {
            if (distance < radius)
            {
                strength = 1;
                return direction * forceStrength;
            }
            else
            {
                strength = 1 - ((distance - radius) / (falloffRadius - radius));
                return direction * forceStrength;
            }
        }
        else
        {
            return Vector3.zero;
        }
    }

    //returns the full gravity vector regardless of if the point is in the collider or not
    public override Vector3 ForceVector(Vector3 point)
    {
        return (transform.position - point).normalized * forceStrength;
    }

}
