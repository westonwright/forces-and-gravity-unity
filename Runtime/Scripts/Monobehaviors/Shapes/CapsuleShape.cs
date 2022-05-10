using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapsuleShape : BaseShape
{
   private enum Direction
    {
        X,
        Y,
        Z
    }

    public float radius = .5f;
    public float height = 1;
    [SerializeField]
    private Direction _direction;

    public int direction
    {
        get
        {
            return (int)_direction;
        }
        set
        {
            switch (value)
            {
                case 0:
                    _direction = Direction.X;
                    break;
                case 1:
                    _direction = Direction.Y;
                    break;
                case 2:
                    _direction = Direction.Z;
                    break;
                default:
                    _direction = Direction.X;
                    break;
            }
        }
    }


    // TODO: Come up with better visualization for this
    protected override void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(bounds.center, bounds.size);
        Gizmos.color = ForcesStaticMembers.shapeColor;

        Bounds b = bounds;
        Gizmos.matrix = Matrix4x4.TRS(b.center, transform.rotation, Vector3.one);

        Vector3 sphereOffset = Vector3.zero;
        float maxRad = 0;
        Vector3 capRadius = Vector3.zero;
        switch (_direction)
        {
            case Direction.X:
                maxRad = Mathf.Max(Mathf.Abs(transform.lossyScale.y), Mathf.Abs(transform.lossyScale.z));
                sphereOffset = Vector3.right * ((height / 2)) * transform.lossyScale.x;
                capRadius = new Vector3(radius * maxRad, 0, 0);
                break;
            case Direction.Y:
                maxRad = Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.z));
                sphereOffset = Vector3.up * ((height / 2)) * transform.lossyScale.y;
                capRadius = new Vector3(0, radius * maxRad, 0);
                break;
            case Direction.Z:
                maxRad = Mathf.Max(Mathf.Abs(transform.lossyScale.y), Mathf.Abs(transform.lossyScale.y));
                sphereOffset = Vector3.forward * ((height / 2)) * transform.lossyScale.z;
                capRadius = new Vector3(0, 0, radius * maxRad);
                break;
        }

        Gizmos.DrawWireSphere(Vector3.zero + (sphereOffset - capRadius), radius * maxRad);
        Gizmos.DrawWireSphere(Vector3.zero - (sphereOffset - capRadius), radius * maxRad);
    }

    // TODO: Make better for garbage collection
    // TODO: Find min size when scale is too large to prevent inverse growing
    protected override Bounds GetBounds()
    {
        Vector3 boundsOffset = Vector3.zero;
        float maxRad = 0;
        Vector3 capRadius = Vector3.zero;
        switch (_direction)
        {
            case Direction.X:
                maxRad = Mathf.Max(Mathf.Abs(transform.lossyScale.y), Mathf.Abs(transform.lossyScale.z));
                boundsOffset = Vector3.right * ((height / 2));
                capRadius = new Vector3(radius * maxRad, 0, 0);
                break;
            case Direction.Y:
                maxRad = Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.z));
                boundsOffset = Vector3.up * ((height / 2));
                capRadius = new Vector3(0, radius * maxRad, 0);
                break;
            case Direction.Z:
                maxRad = Mathf.Max(Mathf.Abs(transform.lossyScale.y), Mathf.Abs(transform.lossyScale.y));
                boundsOffset = Vector3.forward * ((height / 2));
                capRadius = new Vector3( 0, 0, radius * maxRad);
                break;
        }

        Bounds topBounds = new Bounds
        (
            (transform.rotation * (ForcesStaticMembers.MultiplyVectors(center + boundsOffset, transform.lossyScale) - capRadius)) + transform.position,
            (radius * 2 * maxRad) * Vector3.one
        );
        
        Bounds bottomBounds = new Bounds
        (
            (transform.rotation * (ForcesStaticMembers.MultiplyVectors(center - boundsOffset, transform.lossyScale) + capRadius)) + transform.position,
            (radius * 2 * maxRad) * Vector3.one
        );

        Bounds totalBounds = new Bounds();
        totalBounds.center = (transform.rotation * ForcesStaticMembers.MultiplyVectors(center, transform.lossyScale)) + transform.position;
        totalBounds.max = Vector3.Max(topBounds.center + topBounds.extents, bottomBounds.center + bottomBounds.extents);
        totalBounds.min = Vector3.Min(topBounds.center - topBounds.extents, bottomBounds.center - bottomBounds.extents);

        return totalBounds;
    }

    public override Vector3 ClosestPointOnShape(Vector3 to)
    {
        float lineLength = height - radius * 2; // The length of the line connecting the center of both sphere
        Vector3 dir;
        switch (direction)
        {
            case 0:
                dir = Vector3.right;
                break;
            case 1:
                dir = Vector3.up;
                break;
            case 2:
                dir = Vector3.forward;
                break;
            default:
                dir = Vector3.up;
                break;
        }

        Vector3 upperSphere = dir * lineLength * 0.5f + center; // The position of the radius of the upper sphere in local coordinates
        Vector3 lowerSphere = -dir * lineLength * 0.5f + center; // The position of the radius of the lower sphere in local coordinates

        Vector3 local = transform.InverseTransformPoint(to); // The position of the controller in local coordinates

        Vector3 p = Vector3.zero; // Contact point
        Vector3 pt = Vector3.zero; // The point we need to use to get a direction vector with the controller to calculate contact point

        if (local.y < lineLength * 0.5f && local.y > -lineLength * 0.5f) // Controller is contacting with cylinder, not spheres
        {
            pt = dir * local.y + center;
        }
        else if (local.y > lineLength * 0.5f) // Controller is contacting with the upper sphere
        {
            pt = upperSphere;
        }
        else if (local.y < -lineLength * 0.5f)// Controller is contacting with lower sphere
        {

            pt = lowerSphere;
        }

        //Calculate contact point in local coordinates and return it in world coordinates
        p = local - pt;
        p.Normalize();
        p = p * radius + pt;

        //Debug.DrawRay(ct.TransformPoint(p), normal, Color.red);

        return transform.TransformPoint(p);
    }

    public override Vector3 ClosestPointOnShape(Vector3 to, ref Vector3 normal)
    {
        float lineLength = height - radius * 2; // The length of the line connecting the center of both sphere
        Vector3 dir;
        switch (direction)
        {
            case 0:
                dir = Vector3.right;
                break;
            case 1:
                dir = Vector3.up;
                break;
            case 2:
                dir = Vector3.forward;
                break;
            default:
                dir = Vector3.up;
                break;
        }

        Vector3 upperSphere = dir * lineLength * 0.5f + center; // The position of the radius of the upper sphere in local coordinates
        Vector3 lowerSphere = -dir * lineLength * 0.5f + center; // The position of the radius of the lower sphere in local coordinates

        Vector3 local = transform.InverseTransformPoint(to); // The position of the controller in local coordinates

        Vector3 p = Vector3.zero; // Contact point
        Vector3 pt = Vector3.zero; // The point we need to use to get a direction vector with the controller to calculate contact point

        if (local.y < lineLength * 0.5f && local.y > -lineLength * 0.5f) // Controller is contacting with cylinder, not spheres
        {
            pt = dir * local.y + center;
            normal = dir * local.y;
        }
        else if (local.y > lineLength * 0.5f) // Controller is contacting with the upper sphere
        {
            pt = upperSphere;
            normal = dir * lineLength * 0.5f;
        }
        else if (local.y < -lineLength * 0.5f)// Controller is contacting with lower sphere
        {

            pt = lowerSphere;
            normal = -dir * lineLength * 0.5f;
        }

        //set normal for gravity
        normal = transform.TransformDirection((local - normal).normalized);

        //Calculate contact point in local coordinates and return it in world coordinates
        p = local - pt;
        p.Normalize();
        p = p * radius + pt;

        //Debug.DrawRay(ct.TransformPoint(p), normal, Color.red);

        return transform.TransformPoint(p);
    }
}
