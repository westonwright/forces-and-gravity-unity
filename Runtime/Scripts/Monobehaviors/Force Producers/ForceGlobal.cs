using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// update to allow for other types of collider?
public class ForceGlobal : ForceProducer
{
#if UNITY_EDITOR
    [UnityEditor.MenuItem(MENU_NAME + "Global", false, 0)]
    static void InstantiateForceGlobal()
    {
        GameObject go = Instantiate(new ForceGlobal()).gameObject;
        go.name = "Force Global";
    }
#endif

    // TODO: find a better system for setting gravity direction from editor
    [SerializeField]
    private Vector3 ForceDirection = Vector3.down;
    public Vector3 forceDirection
    {
        get { return ForceDirection; }
        set { if (!isStatic) { ForceDirection = value; } }
    }

    private void OnDrawGizmos()
    {
        if (!preview) return;
        // draw planes if falloff isn't 0
        Gizmos.color = (additive ? forceType.previewColor : forceType.previewColor * ForcesStaticMembers.lightGray) * (enableForce ? 1 : .25f);

        DrawGizmoArrow(forceType.previewColor, transform.position, Quaternion.FromToRotation(Vector3.up, forceDirection.normalized), Vector3.one, Mathf.Max((Mathf.Log(forceStrength) + 1), .5f));

        if (falloffRange != 0)
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.FromToRotation(transform.forward, forceDirection), Vector3.one);

            Vector3 bl = (Vector3.down + Vector3.left) * .5f;
            Vector3 br = (Vector3.down + Vector3.right) * .5f;
            Vector3 tl = (Vector3.up + Vector3.left) * .5f;
            Vector3 tr = (Vector3.up + Vector3.right) * .5f;

            Gizmos.DrawLine(bl, br);
            Gizmos.DrawLine(br, tr);
            Gizmos.DrawLine(tr, tl);
            Gizmos.DrawLine(tl, bl);

            Vector3 expansionOffset = forceDirection.normalized * falloffRange;
            Gizmos.color = Gizmos.color * ForcesStaticMembers.semiTransparent;

            Gizmos.DrawLine(bl + expansionOffset, br + expansionOffset);
            Gizmos.DrawLine(br + expansionOffset, tr + expansionOffset);
            Gizmos.DrawLine(tr + expansionOffset, tl + expansionOffset);
            Gizmos.DrawLine(tl + expansionOffset, bl + expansionOffset);
        }

    }

    protected override void Reset()
    {
        base.Reset();

        ForceDirection = Vector3.down;
        Importance = 0;
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

    public override void TryUpdateProducer()
    {
    }

    public override void UpdateProducer()
    {
    }
}
