using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// update to allow for other types of collider?
public class ForceGlobal : ForceProducer
{
    [SerializeField]
    // TODO: find a better system for setting gravity direction from editor
    private Vector3 forceDirection = Vector3.down;

    protected override void OnDrawGizmos()
    {
        if (preview)
        {
            Gizmos.color = Color.white;

            float pointSize = .2f;
            Quaternion rot = Quaternion.FromToRotation(Vector3.up, forceDirection.normalized);
            Gizmos.matrix = Matrix4x4.TRS(transform.position, rot, Vector3.one);
            Vector3 arrowBottom = Vector3.zero;
            Vector3 arrowTop = arrowBottom + Vector3.up;
            Vector3 leftPoint = arrowTop + (Vector3.left * pointSize) + (Vector3.down * pointSize);
            Vector3 rightPoint = arrowTop + (Vector3.right * pointSize) + (Vector3.down * pointSize);
            Vector3 frontPoint = arrowTop + (Vector3.forward * pointSize) + (Vector3.down * pointSize);
            Vector3 backPoint = arrowTop + (Vector3.back * pointSize) + (Vector3.down * pointSize);
            Gizmos.DrawLine(arrowBottom, arrowTop);
            Gizmos.DrawLine(arrowTop, leftPoint);
            Gizmos.DrawLine(arrowTop, rightPoint);
            Gizmos.DrawLine(arrowTop, frontPoint);
            Gizmos.DrawLine(arrowTop, backPoint);
            Gizmos.DrawLine(frontPoint, leftPoint);
            Gizmos.DrawLine(leftPoint, backPoint);
            Gizmos.DrawLine(backPoint, rightPoint);
            Gizmos.DrawLine(rightPoint, frontPoint);
        }
    }
   
    public override Vector3 ForceVector(Vector3 point, out float strength)
    {
        strength = 1;

        return forceDirection.normalized * forceStrength;
    }

    //returns the full gravity vector regardless of if the point is in the collider or not
    public override Vector3 ForceVector(Vector3 point)
    {
        return forceDirection.normalized * forceStrength;
    }

    public void UpdateForceDirection(Vector3 direction)
    {
        forceDirection = direction;
    }
}
