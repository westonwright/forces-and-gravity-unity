using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// update to allow for other types of collider?
[RequireComponent(typeof(BoxCollider))]
public class GravityZone : GravitySource
{
    [SerializeField]
    //find a better system for setting gravity direction from editor
    private Vector3 gravityDirection = Vector3.down;

    private Bounds zoneBounds;
    private Bounds falloffBounds;

    private void OnDrawGizmos()
    {
        if (preview)
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            BoxCollider col = GetComponent<BoxCollider>();
            Gizmos.color = (additive ? Color.cyan : Color.blue) * (enableGravity ? 1 : .25f);
            Gizmos.DrawWireCube(CustomGravityHelperFunctions.MultiplyVectors(transform.localScale, col.center), CustomGravityHelperFunctions.MultiplyVectors(transform.localScale, col.size));

            if (falloffRange > 0)
            {
                Gizmos.color = new Color(0, 0, 1, .25f);//semi-transparent blue
                Gizmos.DrawWireCube(CustomGravityHelperFunctions.MultiplyVectors(transform.localScale, col.center), CustomGravityHelperFunctions.AddToVector(CustomGravityHelperFunctions.MultiplyVectors(transform.localScale, col.size), falloffRange * 2));
            }

            Gizmos.color = Color.white;

            float pointSize = .2f;
            Quaternion rot = Quaternion.FromToRotation(Vector3.up, gravityDirection.normalized);
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
   

    public override void Initialize()
    {
        if (Application.isPlaying)
        {
            gravityCollider = GetComponent<BoxCollider>();
            gravityCollider.isTrigger = true;

            UpdateBounds();
        }
    }

    public override Vector3 GravityVector(Vector3 point, out float strength)
    {
        strength = 0;

        Vector3 transformPoint = CustomGravityHelperFunctions.MultiplyVectors(transform.localScale, transform.InverseTransformPoint(point));
        //Debug.DrawRay(point, Vector3.up * 2, falloffBounds.Contains(transformPoint) ? (zoneBounds.Contains(transformPoint) ? Color.green : Color.yellow) : Color.red);
        if (falloffBounds.Contains(transformPoint))
        {
            if (zoneBounds.Contains(transformPoint))
            {
                strength = 1;
                return gravityDirection.normalized * gravityStrength;
            }
            else
            {
                //I think I need to use clamp here because there might be times where the distance
                //is greater than the falloff range particularly at the corners of the box
                strength = 1 - Mathf.Clamp(Vector3.Distance(zoneBounds.ClosestPoint(transformPoint), transformPoint) / falloffRange, 0, 1);
                return gravityDirection.normalized * gravityStrength;
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
        return gravityDirection.normalized * gravityStrength;
    }

    public void UpdateBounds()
    {
        zoneBounds = new Bounds(CustomGravityHelperFunctions.MultiplyVectors(transform.localScale, (gravityCollider as BoxCollider).center), CustomGravityHelperFunctions.MultiplyVectors(transform.localScale, (gravityCollider as BoxCollider).size));
        falloffBounds = new Bounds(zoneBounds.center, CustomGravityHelperFunctions.AddToVector(zoneBounds.size, falloffRange * 2));
    }

    public void UpdateGravityDirection(Vector3 direction)
    {
        gravityDirection = direction;
    }
}
