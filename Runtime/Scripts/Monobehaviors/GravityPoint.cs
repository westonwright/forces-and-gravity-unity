using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityPoint : GravitySource
{
    private void OnDrawGizmos()
    {
        if (preview)
        {
            Gizmos.color = (additive ? Color.yellow : Color.red) * (enableGravity ? 1 : .25f);
            Gizmos.DrawWireSphere(transform.position, CustomGravityHelperFunctions.VectorMax(transform.localScale));

            if (falloffRange > 0)
            {
                Gizmos.color = CustomGravityHelperFunctions.MultiplyColors(Gizmos.color, new Color(1, 1, 1, .25f)); //makes falloff semi-transparent
                Gizmos.DrawWireSphere(transform.position, CustomGravityHelperFunctions.VectorMax(transform.localScale) + falloffRange);
            }
        }
    }

    public override void Initialize()
    {
    }

    public override Vector3 GravityVector(Vector3 point, out float strength)
    {
        strength = 0;

        float radius = CustomGravityHelperFunctions.VectorMax(transform.localScale);
        float falloffRadius = radius + falloffRange;
        float distance = Vector3.Distance(transform.position, point);
        Vector3 direction = (transform.position - point).normalized;
        //Debug.DrawRay(point, Vector3.up * 2, falloffBounds.Contains(transformPoint) ? (zoneBounds.Contains(transformPoint) ? Color.green : Color.yellow) : Color.red);
        if (distance < falloffRadius)
        {
            if (distance < radius)
            {
                strength = 1;
                return direction * gravityStrength;
            }
            else
            {
                strength = 1 - ((distance - radius) / (falloffRadius - radius));
                return direction * gravityStrength;
            }
        }
        else
        {
            return Vector3.zero;
        }
    }

    //returns the full gravity vector regardless of if the point is in the collider or not
    public override Vector3 GravityVector(Vector3 point)
    {
        return (transform.position - point).normalized * gravityStrength;
    }

}
