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
    public override void DrawShapeGizmo(Color color, float expansion)
    {
        // TODO: add in center offset
        Gizmos.color = color;

        Bounds b = bounds;
        Gizmos.matrix = Matrix4x4.TRS(b.center, transform.rotation, Vector3.one);

        Vector3 sphereOffset = Vector3.zero;
        float maxRad = 0;
        Vector3 capRadius = Vector3.zero;
        Vector3 lineUp = Vector3.zero;
        Vector3 lineRight = Vector3.zero;
        switch (_direction)
        {
            case Direction.X:
                maxRad = Mathf.Max(Mathf.Abs(transform.lossyScale.y), Mathf.Abs(transform.lossyScale.z));
                sphereOffset = Vector3.right * ((height / 2)) * transform.lossyScale.x;
                capRadius = new Vector3(radius * maxRad, 0, 0) ;
                lineUp = new Vector3(0, radius * maxRad + expansion, 0);
                lineRight = new Vector3(0, 0, radius * maxRad + expansion);
                break;
            case Direction.Y:
                maxRad = Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.z));
                sphereOffset = Vector3.up * ((height / 2)) * transform.lossyScale.y;
                capRadius = new Vector3(0, radius * maxRad, 0);
                lineUp = new Vector3(radius * maxRad + expansion, 0, 0);
                lineRight = new Vector3(0, 0, radius * maxRad + expansion);
                break;
            case Direction.Z:
                maxRad = Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y));
                sphereOffset = Vector3.forward * ((height / 2)) * transform.lossyScale.z;
                capRadius = new Vector3(0, 0, radius * maxRad);
                lineUp = new Vector3(radius * maxRad + expansion, 0, 0);
                lineRight = new Vector3(0, radius * maxRad + expansion, 0);
                break;
        }
        Vector3 upperSphere = Vector3.zero + ForcesStaticMembers.MaxVector((sphereOffset - capRadius), 0);
        Vector3 lowerSphere = Vector3.zero - ForcesStaticMembers.MaxVector((sphereOffset - capRadius), 0);

        Gizmos.DrawWireSphere(upperSphere, radius * maxRad + expansion);
        Gizmos.DrawWireSphere(lowerSphere, radius * maxRad + expansion);

        Gizmos.DrawLine(upperSphere + lineUp, lowerSphere + lineUp);
        Gizmos.DrawLine(upperSphere - lineUp, lowerSphere - lineUp);
        Gizmos.DrawLine(upperSphere + lineRight, lowerSphere + lineRight);
        Gizmos.DrawLine(upperSphere - lineRight, lowerSphere - lineRight);
    }

    // TODO: Make better for garbage collection
    // TODO: Find min size when scale is too large to prevent inverse growing
    protected override Bounds CalculateBounds()
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
            //(transform.rotation * (ForcesStaticMembers.MultiplyVectors(center + boundsOffset, transform.lossyScale) - capRadius)) + transform.position,
            (transform.rotation * (ForcesStaticMembers.MultiplyVectors(boundsOffset, transform.lossyScale) - capRadius)) + transform.position,
            (radius * 2 * maxRad) * Vector3.one
        );
        
        Bounds bottomBounds = new Bounds
        (
            //(transform.rotation * (ForcesStaticMembers.MultiplyVectors(center - boundsOffset, transform.lossyScale) + capRadius)) + transform.position,
            (transform.rotation * (ForcesStaticMembers.MultiplyVectors(boundsOffset, transform.lossyScale) + capRadius)) + transform.position,
            (radius * 2 * maxRad) * Vector3.one
        );

        Bounds totalBounds = new Bounds();
        //totalBounds.center = (transform.rotation * ForcesStaticMembers.MultiplyVectors(center, transform.lossyScale)) + transform.position;
        totalBounds.center =transform.position;
        totalBounds.max = Vector3.Max(topBounds.center + topBounds.extents, bottomBounds.center + bottomBounds.extents);
        totalBounds.min = Vector3.Min(topBounds.center - topBounds.extents, bottomBounds.center - bottomBounds.extents);

        return totalBounds;
    }

    public override Bounds GetExpandedBounds(float expansion)
    {
        return new Bounds(bounds.center, bounds.size + Vector3.one * expansion * 2);
    }


    public override Vector3 ClosestPointOnShape(Vector3 to)
    {
        Vector3 local = to - transform.position;
        local = Quaternion.Inverse(transform.rotation) * local;

        float lineLength;
        float localY;
        Vector3 dir;
        float maxRadScale;
        Vector3 scaleVector;
        switch (direction)
        {
            case 0:
                dir = Vector3.right;

                maxRadScale = Mathf.Max(transform.lossyScale.y, transform.lossyScale.z);
                scaleVector = new Vector3(transform.lossyScale.x, maxRadScale, maxRadScale);
                local = ForcesStaticMembers.DivideVectors(local, scaleVector);

                lineLength = Mathf.Max((height * transform.lossyScale.x) - (radius * maxRadScale) * 2, 0);
                lineLength /= transform.lossyScale.x;
                localY = local.x;
                break;
            case 1:
                dir = Vector3.up;

                maxRadScale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
                scaleVector = new Vector3(maxRadScale, transform.lossyScale.y, maxRadScale);
                local = ForcesStaticMembers.DivideVectors(local, scaleVector);

                lineLength = Mathf.Max((height * transform.lossyScale.y) - (radius * maxRadScale) * 2, 0);
                lineLength /= transform.lossyScale.y;
                localY = local.y;
                break;
            case 2:
                dir = Vector3.forward;

                maxRadScale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);
                scaleVector = new Vector3(maxRadScale, maxRadScale, transform.lossyScale.z);
                local = ForcesStaticMembers.DivideVectors(local, scaleVector);

                lineLength = Mathf.Max((height * transform.lossyScale.z) - (radius * maxRadScale) * 2, 0);
                lineLength /= transform.lossyScale.z;
                localY = local.z;
                break;
            default:
                dir = Vector3.up;

                maxRadScale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
                scaleVector = new Vector3(maxRadScale, transform.lossyScale.y, maxRadScale);
                local = ForcesStaticMembers.DivideVectors(local, scaleVector);

                lineLength = Mathf.Max((height * transform.lossyScale.y) - (radius * maxRadScale) * 2, 0);
                lineLength /= transform.lossyScale.y;
                localY = local.y;
                break;
        }
        //Vector3 upperSphere = dir * lineLength * 0.5f + center; // The position of the radius of the upper sphere in local coordinates
        //Vector3 lowerSphere = -dir * lineLength * 0.5f + center; // The position of the radius of the lower sphere in local coordinates
        Vector3 upperSphere = dir * lineLength * 0.5f; // The position of the radius of the upper sphere in local coordinates
        Vector3 lowerSphere = -dir * lineLength * 0.5f; // The position of the radius of the lower sphere in local coordinates

        Vector3 p = Vector3.zero; // Contact point

        if (localY < lineLength * 0.5f && localY > -lineLength * 0.5f) // Controller is contacting with cylinder, not spheres
        {
            //Debug.Log("Center");
            //p = dir * localY + center;
            p = dir * localY;
        }
        else if (localY > lineLength * 0.5f) // Controller is contacting with the upper sphere
        {
            //Debug.Log("Upper");
            p = upperSphere;
        }
        else if (localY < -lineLength * 0.5f)// Controller is contacting with lower sphere
        {
            //Debug.Log("Lower");
            p = lowerSphere;
        }

        p = ForcesStaticMembers.MultiplyVectors(p, scaleVector);
        p = transform.rotation * p;
        p += transform.position;

        //set normal for gravity
        Vector3 normal = (to - p).normalized;

        p = p + (normal * (radius * maxRadScale));

        //Debug.DrawRay(p, normal);

        return p;
    }

    public override Vector3 ClosestPointOnShape(Vector3 to, ref Vector3 normal)
    {
        Vector3 local = to - transform.position;
        local = Quaternion.Inverse(transform.rotation) * local;

        float lineLength;
        float localY;
        Vector3 dir;
        float maxRadScale;
        Vector3 scaleVector;
        switch (direction)
        {
            case 0:
                dir = Vector3.right;

                maxRadScale = Mathf.Max(transform.lossyScale.y, transform.lossyScale.z);
                scaleVector = new Vector3(transform.lossyScale.x, maxRadScale, maxRadScale);
                local = ForcesStaticMembers.DivideVectors(local, scaleVector);

                lineLength = Mathf.Max((height * transform.lossyScale.x) - (radius * maxRadScale) * 2, 0);
                lineLength /= transform.lossyScale.x;
                localY = local.x;
                break;
            case 1:
                dir = Vector3.up;

                maxRadScale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
                scaleVector = new Vector3(maxRadScale, transform.lossyScale.y, maxRadScale);
                local = ForcesStaticMembers.DivideVectors(local, scaleVector);

                lineLength = Mathf.Max((height * transform.lossyScale.y) - (radius * maxRadScale) * 2, 0);
                lineLength /= transform.lossyScale.y;
                localY = local.y;
                break;
            case 2:
                dir = Vector3.forward;

                maxRadScale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);
                scaleVector = new Vector3(maxRadScale, maxRadScale, transform.lossyScale.z);
                local = ForcesStaticMembers.DivideVectors(local, scaleVector);

                lineLength = Mathf.Max((height * transform.lossyScale.z) - (radius * maxRadScale) * 2, 0);
                lineLength /= transform.lossyScale.z;
                localY = local.z;
                break;
            default:
                dir = Vector3.up;

                maxRadScale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
                scaleVector = new Vector3(maxRadScale, transform.lossyScale.y, maxRadScale);
                local = ForcesStaticMembers.DivideVectors(local, scaleVector);

                lineLength = Mathf.Max((height * transform.lossyScale.y) - (radius * maxRadScale) * 2, 0);
                lineLength /= transform.lossyScale.y;
                localY = local.y;
                break;
        }
        //Vector3 upperSphere = dir * lineLength * 0.5f + center; // The position of the radius of the upper sphere in local coordinates
        //Vector3 lowerSphere = -dir * lineLength * 0.5f + center; // The position of the radius of the lower sphere in local coordinates
        Vector3 upperSphere = dir * lineLength * 0.5f; // The position of the radius of the upper sphere in local coordinates
        Vector3 lowerSphere = -dir * lineLength * 0.5f; // The position of the radius of the lower sphere in local coordinates
        
        Vector3 p = Vector3.zero; // Contact point

        if (localY < lineLength * 0.5f && localY > -lineLength * 0.5f) // Controller is contacting with cylinder, not spheres
        {
            //Debug.Log("Center");
            //p = dir * localY + center;
            p = dir * localY;
        }
        else if (localY > lineLength * 0.5f) // Controller is contacting with the upper sphere
        {
            //Debug.Log("Upper");
            p = upperSphere;
        }
        else if (localY < -lineLength * 0.5f)// Controller is contacting with lower sphere
        {
            //Debug.Log("Lower");
            p = lowerSphere;
        }

        p = ForcesStaticMembers.MultiplyVectors(p, scaleVector);
        p = transform.rotation * p;
        p += transform.position;

        //set normal for gravity
        normal = (to - p).normalized;
        
        p = p + (normal * (radius * maxRadScale));

        //Debug.DrawRay(p, normal);

        return p;
    }

    public override float SignedDistance(Vector3 to)
    {
        float lineLength;
        Vector3 dir;
        float maxRadScale;
        Vector3 scaleVector;
        switch (direction)
        {
            case 0:
                dir = Vector3.right;

                maxRadScale = Mathf.Max(transform.lossyScale.y, transform.lossyScale.z);
                scaleVector = new Vector3(transform.lossyScale.x, maxRadScale, maxRadScale);

                lineLength = Mathf.Max((height * transform.lossyScale.x) - (radius * maxRadScale) * 2, 0);
                lineLength /= transform.lossyScale.x;
                break;
            case 1:
                dir = Vector3.up;

                maxRadScale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
                scaleVector = new Vector3(maxRadScale, transform.lossyScale.y, maxRadScale);

                lineLength = Mathf.Max((height * transform.lossyScale.y) - (radius * maxRadScale) * 2, 0);
                lineLength /= transform.lossyScale.y;
                break;
            case 2:
                dir = Vector3.forward;

                maxRadScale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);
                scaleVector = new Vector3(maxRadScale, maxRadScale, transform.lossyScale.z);

                lineLength = Mathf.Max((height * transform.lossyScale.z) - (radius * maxRadScale) * 2, 0);
                lineLength /= transform.lossyScale.z;
                break;
            default:
                dir = Vector3.up;

                maxRadScale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
                scaleVector = new Vector3(maxRadScale, transform.lossyScale.y, maxRadScale);

                lineLength = Mathf.Max((height * transform.lossyScale.y) - (radius * maxRadScale) * 2, 0);
                lineLength /= transform.lossyScale.y;
                break;
        }

        //Vector3 upperSphere = dir * lineLength * 0.5f + center; // The position of the radius of the upper sphere in local coordinates
        Vector3 upperSphere = dir * lineLength * 0.5f; // The position of the radius of the upper sphere in local coordinates
        upperSphere = ForcesStaticMembers.MultiplyVectors(upperSphere, scaleVector);
        upperSphere = transform.rotation * upperSphere;
        upperSphere += transform.position;

        //Vector3 lowerSphere = -dir * lineLength * 0.5f + center; // The position of the radius of the lower sphere in local coordinates
        Vector3 lowerSphere = -dir * lineLength * 0.5f; // The position of the radius of the lower sphere in local coordinates
        lowerSphere = ForcesStaticMembers.MultiplyVectors(lowerSphere, scaleVector);
        lowerSphere = transform.rotation * lowerSphere;
        lowerSphere += transform.position;

        Vector3 upperOffset = to - upperSphere;
        Vector3 lowerOffset = lowerSphere - upperSphere;

        float h = Mathf.Clamp(Vector3.Dot(upperOffset, lowerOffset) / Vector3.Dot(lowerOffset, lowerOffset), 0, 1);

        Debug.Log(Vector3.Magnitude(upperOffset - lowerOffset * h) - (radius * maxRadScale));
        return Vector3.Magnitude(upperOffset - lowerOffset * h) - (radius * maxRadScale);
    }

}
