using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// update to allow for other types of collider?
//change from requiring box collider
[RequireComponent(typeof(BaseShape))]
public class ForceZone : ForceProducer
{
#if UNITY_EDITOR
    protected const string SUB_MENU = "Force Zone/";
    [UnityEditor.MenuItem(MENU_NAME + SUB_MENU + "Box", false, 0)]
    static void InstantiateCubeZone()
    {
        GameObject go = new GameObject("Force Box Zone");
        go.AddComponent<BoxShape>();
        go.AddComponent<ForceZone>();
    }
    [UnityEditor.MenuItem(MENU_NAME + SUB_MENU + "Sphere", false, 1)]
    static void InstantiateSphereZone()
    {
        GameObject go = new GameObject("Force Sphere Zone");
        go.AddComponent<SphereShape>();
        go.AddComponent<ForceZone>();
    }
    [UnityEditor.MenuItem(MENU_NAME + SUB_MENU + "Capsule", false, 2)]
    static void InstantiateCapsuleZone()
    {
        GameObject go = new GameObject("Force Capsule Zone");
        go.AddComponent<CapsuleShape>();
        go.AddComponent<ForceZone>();
    }
    [UnityEditor.MenuItem(MENU_NAME + SUB_MENU + "Plane", false, 3)]
    static void InstantiatePlaneZone()
    {
        GameObject go = new GameObject("Force Plane Zone");
        go.AddComponent<PlaneShape>();
        go.AddComponent<ForceZone>();
    }
    [UnityEditor.MenuItem(MENU_NAME + SUB_MENU + "Mesh", false, 4)]
    static void InstantiateMeshZone()
    {
        GameObject go = new GameObject("Force Mesh Zone");
        Mesh mesh = Resources.GetBuiltinResource<Mesh>("Cylinder.fbx");
        MeshShape.AddMeshShapeComponent(go, mesh);
        go.AddComponent<ForceZone>();
    }
#endif

    [SerializeField]
    [Tooltip("The direction of force from this zone. Will be normalized")]
    // TODO: find a better system for setting gravity direction from editor
    private Vector3 ForceDirection = Vector3.down;
    public Vector3 forceDirection
    {
        get { return ForceDirection; }
        set { if (!isStatic) { ForceDirection = value; } }
    }

    private Bounds Bounds;
    public Bounds bounds { get { return Bounds; } }


    private BaseShape BaseShape;
    public BaseShape baseShape { get { return BaseShape; } }

    private void OnDrawGizmos()
    {
        if (!preview) return;
        BaseShape baseShape = GetComponent<BaseShape>();

        Color c = forceType.previewColor;
        c = (additive ? c : c * ForcesStaticMembers.lightGray) * (enableForce ? 1 : .25f);
        baseShape.DrawShapeGizmo(c, 0);

        if (falloffRange > 0)
        {
            c = c * ForcesStaticMembers.semiTransparent; //makes falloff semi-transparent
            baseShape.DrawShapeGizmo(c, falloffRange);
        }

        DrawGizmoArrow(Color.white, transform.position, Quaternion.FromToRotation(Vector3.up, forceDirection.normalized), Vector3.one, Mathf.Max((Mathf.Log(forceStrength) + 1), .5f));
    }
    protected override void Reset()
    {
        base.Reset();

        ForceDirection = Vector3.down;
    }

    private void Awake()
    {
        BaseShape = GetComponent<BaseShape>();
    }


    // change to just not provide strength. eg. strength is baked in to the force returned
    //returns the full gravity vector regardless of if the point is in the collider or not
    public override Vector3 ForceVector(Vector3 point)
    {
        float dist = baseShape.SignedDistance(point);
        if (dist <= 0)
        {
            return forceDirection.normalized * forceStrength;
        }
        else
        {
            if (dist < falloffRange)
            {
                float strength = 1 - (dist / falloffRange);
                return forceDirection.normalized * forceStrength * strength;
            }
            else
            {
                return Vector3.zero;
            }
        }
    }

    public override Vector3 ForceVector(Vector3 point, out float strength)
    {
        strength = 0;

        float dist = baseShape.SignedDistance(point);
        if (dist <= 0)
        {
            strength = 1;
            return forceDirection.normalized * forceStrength;
        }
        else
        {
            if(dist < falloffRange)
            {
                strength = 1 - (dist / falloffRange);
                return forceDirection.normalized * forceStrength;
            }
            else
            {
                strength = 0;
                return Vector3.zero;
            }
        }
    }

    public override bool PointInRange(Vector3 point)
    {
        if (bounds.Contains(point))
        {
            return true;
        }
        return false;
    }

    // use these if position or scale is changing
    // or if falloff/force range changed
    public override void TryUpdateProducer()
    {
        if (baseShape.hasChanged || needsUpdate)
        {
            UpdateProducer();
            baseShape.hasChanged = false;
            needsUpdate = false;
        }
    }

    public override void UpdateProducer()
    {
        Bounds = baseShape.CalculateExpandedBounds(falloffRange);
    }
}
