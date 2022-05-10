using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// update to allow for other types of collider?
[RequireComponent(typeof(BoxShape))]
//change from requiring box collider
public class ForceZone : ForceProducer
{
    [SerializeField]
    [Tooltip("The direction of force from this zone. Will be normalized")]
    // TODO: find a better system for setting gravity direction from editor
    private Vector3 forceDirection = Vector3.down;

    [SerializeField]
    [Tooltip("The distance it takes for the force to fade")]
    protected float falloffRange = 0f;

    private Bounds zoneBounds;
    private Bounds falloffBounds;

    private BoxShape zoneCube;
    protected override void OnDrawGizmos()
    {
        if (preview)
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            BoxShape cube = GetComponent<BoxShape>();
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
                case ForceType.Gravity:
                    Gizmos.color = new Color(0, 1, 0, 1);
                    break;
                case ForceType.Generic:
                    Gizmos.color = new Color(0, 1, .5f, 1);
                    break;
            }
            Gizmos.color = (additive ? Gizmos.color : Gizmos.color * ForcesStaticMembers.lightGray) * (enableForce ? 1 : .25f);
            Gizmos.DrawWireCube(ForcesStaticMembers.MultiplyVectors(transform.localScale, cube.center), ForcesStaticMembers.MultiplyVectors(transform.localScale, cube.size));

            if (falloffRange > 0)
            {
            Gizmos.color = ForcesStaticMembers.MultiplyColors(Gizmos.color, new Color(1, 1, 1, .25f)); //makes falloff semi-transparent
            Gizmos.DrawWireCube(ForcesStaticMembers.MultiplyVectors(transform.localScale, cube.center), ForcesStaticMembers.AddToVector(ForcesStaticMembers.MultiplyVectors(transform.localScale, cube.size), falloffRange * 2));
            }
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

    private void Awake()
    {
        zoneCube = GetComponent<BoxShape>();

        UpdateBounds();
    }

    public override Vector3 ForceVector(Vector3 point, out float strength)
    {
        strength = 0;

        Vector3 transformPoint = ForcesStaticMembers.MultiplyVectors(transform.localScale, transform.InverseTransformPoint(point));
        //Debug.DrawRay(point, Vector3.up * 2, falloffBounds.Contains(transformPoint) ? (zoneBounds.Contains(transformPoint) ? Color.green : Color.yellow) : Color.red);
        if (falloffBounds.Contains(transformPoint))
        {
            if (zoneBounds.Contains(transformPoint))
            {
                strength = 1;
                return forceDirection.normalized * forceStrength;
            }
            else
            {
                //I think I need to use clamp here because there might be times where the distance
                //is greater than the falloff range particularly at the corners of the box
                strength = 1 - Mathf.Clamp(Vector3.Distance(zoneBounds.ClosestPoint(transformPoint), transformPoint) / falloffRange, 0, 1);
                return forceDirection.normalized * forceStrength;
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
        return forceDirection.normalized * forceStrength;
    }

    public void UpdateBounds()
    {
        zoneBounds = new Bounds(ForcesStaticMembers.MultiplyVectors(transform.localScale, zoneCube.center), ForcesStaticMembers.MultiplyVectors(transform.localScale, zoneCube.size));
        falloffBounds = new Bounds(zoneBounds.center, ForcesStaticMembers.AddToVector(zoneBounds.size, falloffRange * 2));
    }

    public void UpdateForceDirection(Vector3 direction)
    {
        forceDirection = direction;
    }
}
