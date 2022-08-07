using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapsuleShape : BaseShape
{
   private enum DirectionEnum
    {
        X,
        Y,
        Z
    }

    [SerializeField]
    private float Radius = .5f;
    public float radius
    {
        get { return Radius; }
        set{
            if (isStatic) return;
            if (Radius != value){
                needsUpdate = true;
                Radius = value;
            }
        }
    }

    [SerializeField]
    private float Height = 1;
    public float height
    {
        get { return Height; }
        set {
            if (isStatic) return;
            if (Height != value) {
                needsUpdate = true;
                Height = value;
            }
        }
    }

    [SerializeField]
    private DirectionEnum Direction;

    public int direction
    {
        get
        {
            return (int)Direction;
        }
        set
        {
            if (isStatic) return;
            if ((int)Direction != value)
            {
                needsUpdate = true;

                switch (value)
                {
                    case 0:
                        Direction = DirectionEnum.X;
                        break;
                    case 1:
                        Direction = DirectionEnum.Y;
                        break;
                    case 2:
                        Direction = DirectionEnum.Z;
                        break;
                    default:
                        Direction = DirectionEnum.X;
                        break;
                }
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
        switch (Direction)
        {
            case DirectionEnum.X:
                maxRad = Mathf.Max(Mathf.Abs(transform.lossyScale.y), Mathf.Abs(transform.lossyScale.z));
                sphereOffset = Vector3.right * ((height / 2)) * transform.lossyScale.x;
                capRadius = new Vector3(radius * maxRad, 0, 0) ;
                lineUp = new Vector3(0, radius * maxRad + expansion, 0);
                lineRight = new Vector3(0, 0, radius * maxRad + expansion);
                break;
            case DirectionEnum.Y:
                maxRad = Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.z));
                sphereOffset = Vector3.up * ((height / 2)) * transform.lossyScale.y;
                capRadius = new Vector3(0, radius * maxRad, 0);
                lineUp = new Vector3(radius * maxRad + expansion, 0, 0);
                lineRight = new Vector3(0, 0, radius * maxRad + expansion);
                break;
            case DirectionEnum.Z:
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

    public override Vector3 ClosestPointOnShape(Vector3 to)
    {
        Vector3 local = to - transform.position;
        local = Quaternion.Inverse(transform.rotation) * local;

        float lineLength;
        float localY;
        Vector3 dir;
        float maxRadScale;
        Vector3 scaleVector;
        switch (Direction)
        {
            case DirectionEnum.X:
                dir = Vector3.right;

                maxRadScale = Mathf.Max(transform.lossyScale.y, transform.lossyScale.z);
                scaleVector = new Vector3(transform.lossyScale.x, maxRadScale, maxRadScale);
                local = ForcesStaticMembers.DivideVectors(local, scaleVector);

                lineLength = Mathf.Max((height * transform.lossyScale.x) - (radius * maxRadScale) * 2, 0);
                lineLength /= transform.lossyScale.x;
                localY = local.x;
                break;
            case DirectionEnum.Y:
                dir = Vector3.up;

                maxRadScale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
                scaleVector = new Vector3(maxRadScale, transform.lossyScale.y, maxRadScale);
                local = ForcesStaticMembers.DivideVectors(local, scaleVector);

                lineLength = Mathf.Max((height * transform.lossyScale.y) - (radius * maxRadScale) * 2, 0);
                lineLength /= transform.lossyScale.y;
                localY = local.y;
                break;
            case DirectionEnum.Z:
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

    public override Vector3 ClosestPointOnShape(Vector3 to, out Vector3 normal)
    {
        Vector3 local = to - transform.position;
        local = Quaternion.Inverse(transform.rotation) * local;

        float lineLength;
        float localY;
        Vector3 dir;
        float maxRadScale;
        Vector3 scaleVector;
        switch (Direction)
        {
            case DirectionEnum.X:
                dir = Vector3.right;

                maxRadScale = Mathf.Max(transform.lossyScale.y, transform.lossyScale.z);
                scaleVector = new Vector3(transform.lossyScale.x, maxRadScale, maxRadScale);
                local = ForcesStaticMembers.DivideVectors(local, scaleVector);

                lineLength = Mathf.Max((height * transform.lossyScale.x) - (radius * maxRadScale) * 2, 0);
                lineLength /= transform.lossyScale.x;
                localY = local.x;
                break;
            case DirectionEnum.Y:
                dir = Vector3.up;

                maxRadScale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
                scaleVector = new Vector3(maxRadScale, transform.lossyScale.y, maxRadScale);
                local = ForcesStaticMembers.DivideVectors(local, scaleVector);

                lineLength = Mathf.Max((height * transform.lossyScale.y) - (radius * maxRadScale) * 2, 0);
                lineLength /= transform.lossyScale.y;
                localY = local.y;
                break;
            case DirectionEnum.Z:
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
        switch (Direction)
        {
            case DirectionEnum.X:
                dir = Vector3.right;

                maxRadScale = Mathf.Max(transform.lossyScale.y, transform.lossyScale.z);
                scaleVector = new Vector3(transform.lossyScale.x, maxRadScale, maxRadScale);

                lineLength = Mathf.Max((height * transform.lossyScale.x) - (radius * maxRadScale) * 2, 0);
                lineLength /= transform.lossyScale.x;
                break;
            case DirectionEnum.Y:
                dir = Vector3.up;

                maxRadScale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
                scaleVector = new Vector3(maxRadScale, transform.lossyScale.y, maxRadScale);

                lineLength = Mathf.Max((height * transform.lossyScale.y) - (radius * maxRadScale) * 2, 0);
                lineLength /= transform.lossyScale.y;
                break;
            case DirectionEnum.Z:
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

    // TODO: Make better for garbage collection
    // TODO: Find min size when scale is too large to prevent inverse growing
    protected override Bounds CalculateBounds()
    {
        Vector3 boundsOffset = Vector3.zero;
        float maxRad = 0;
        Vector3 capRadius = Vector3.zero;
        switch (Direction)
        {
            case DirectionEnum.X:
                maxRad = Mathf.Max(Mathf.Abs(transform.lossyScale.y), Mathf.Abs(transform.lossyScale.z));
                boundsOffset = Vector3.right * ((height / 2));
                capRadius = new Vector3(radius * maxRad, 0, 0);
                break;
            case DirectionEnum.Y:
                maxRad = Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.z));
                boundsOffset = Vector3.up * ((height / 2));
                capRadius = new Vector3(0, radius * maxRad, 0);
                break;
            case DirectionEnum.Z:
                maxRad = Mathf.Max(Mathf.Abs(transform.lossyScale.y), Mathf.Abs(transform.lossyScale.y));
                boundsOffset = Vector3.forward * ((height / 2));
                capRadius = new Vector3(0, 0, radius * maxRad);
                break;
        }

        Bounds topBounds = new Bounds
        (
            //(transform.rotation * (ForcesStaticMembers.MultiplyVectors(center + boundsOffset, transform.lossyScale) - capRadius)) + transform.position,
            (transform.rotation * (ForcesStaticMembers.MultiplyVectors(Vector3.zero + boundsOffset, transform.lossyScale) - capRadius)) + transform.position,
            //(transform.rotation * (ForcesStaticMembers.MultiplyVectors(boundsOffset, transform.lossyScale) - capRadius)) + transform.position,
            (radius * 2 * maxRad) * Vector3.one
        );

        Bounds bottomBounds = new Bounds
        (
            //(transform.rotation * (ForcesStaticMembers.MultiplyVectors(center - boundsOffset, transform.lossyScale) + capRadius)) + transform.position,
            (transform.rotation * (ForcesStaticMembers.MultiplyVectors(Vector3.zero - boundsOffset, transform.lossyScale) + capRadius)) + transform.position,
            //(transform.rotation * (ForcesStaticMembers.MultiplyVectors(boundsOffset, transform.lossyScale) + capRadius)) + transform.position,
            (radius * 2 * maxRad) * Vector3.one
        );

        Bounds totalBounds = new Bounds();
        //totalBounds.center = (transform.rotation * ForcesStaticMembers.MultiplyVectors(center, transform.lossyScale)) + transform.position;
        totalBounds.center = (transform.rotation * ForcesStaticMembers.MultiplyVectors(Vector3.zero, transform.lossyScale)) + transform.position;
        //totalBounds.center =transform.position;
        totalBounds.max = Vector3.Max(topBounds.center + topBounds.extents, bottomBounds.center + bottomBounds.extents);
        totalBounds.min = Vector3.Min(topBounds.center - topBounds.extents, bottomBounds.center - bottomBounds.extents);

        return totalBounds;
    }

    public override Bounds CalculateExpandedBounds(float expansion)
    {
        return new Bounds(bounds.center, bounds.size + Vector3.one * expansion * 2);
    }
}
