using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// update to allow for other types of collider?
public class ForceGlobal : ForceProducer
{
    // TODO: find a better system for setting gravity direction from editor
    public Vector3 forceDirection = Vector3.down;

    protected override void OnDrawGizmos()
    {
        if (preview)
        {
            DrawArrow(forceType.previewColor, transform.position, Quaternion.FromToRotation(Vector3.up, forceDirection.normalized), Vector3.one, Mathf.Max((Mathf.Log(forceStrength) + 1), .5f));
        }
    }

    protected override void Reset()
    {
        base.Reset();
        forceDirection = Vector3.down;
        importance = 0;
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

    public override bool PointInRange(Vector3 point)
    {
        return true;
    }

    public void SetForceDirection(Vector3 direction)
    {
        forceDirection = direction;
    }

    public override void UpdateProducer()
    {
    }
}
