using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//placed on the mesh providing gravity
// TODO: prevent multiple surfaces (or shapes) from being applied to the same object
[RequireComponent(typeof(BaseShape))]
public class ForceSurface : ForceProducer
{
#if UNITY_EDITOR
    protected const string SUB_MENU = "Surface/";
    [UnityEditor.MenuItem(MENU_NAME + SUB_MENU + "Box", false, 0)]
    static void InstantiateCubeSurface()
    {
        GameObject go = new GameObject("Force Box Surface");
        go.AddComponent<BoxShape>();
        go.AddComponent<ForceSurface>();
    }
    [UnityEditor.MenuItem(MENU_NAME + SUB_MENU + "Sphere", false, 1)]
    static void InstantiateSphereSurface()
    {
        GameObject go = new GameObject("Force Sphere Surface");
        go.AddComponent<SphereShape>();
        go.AddComponent<ForceSurface>();
    }
    [UnityEditor.MenuItem(MENU_NAME + SUB_MENU + "Capsule", false, 2)]
    static void InstantiateCapsuleSurface()
    {
        GameObject go = new GameObject("Force Capsule Surface");
        go.AddComponent<CapsuleShape>();
        go.AddComponent<ForceSurface>();
    }
    [UnityEditor.MenuItem(MENU_NAME + SUB_MENU + "Plane", false, 3)]
    static void InstantiatePlaneSurface()
    {
        GameObject go = new GameObject("Force Plane Surface");
        go.AddComponent<PlaneShape>();
        go.AddComponent<ForceSurface>();
    }
    [UnityEditor.MenuItem(MENU_NAME + SUB_MENU + "Mesh", false, 4)]
    static void InstantiateMeshSurface()
    {
        GameObject go = new GameObject("Force Mesh Surface");
        Mesh mesh = Resources.GetBuiltinResource<Mesh>("Cylinder.fbx");
        MeshShape.AddMeshShapeComponent(go, mesh);
        go.AddComponent<ForceSurface>();

    }
#endif

    [SerializeField]
    [Tooltip("How far the full-strength force extends out from the collider")]
    //how far out this plante's gravity reaches (not including falloff range)
    private float ForceRange = 5f;
    public float forceRange
    {
        get { return ForceRange; }
        set {
            if (isStatic) return;
            if(ForceRange != value)
            {
                needsUpdate = true;
                ForceRange = value;
            }
        }
    }

    private Bounds Bounds;
    public Bounds bounds { get { return Bounds; } }

    private BaseShape BaseShape;
    public BaseShape baseShape { get { return BaseShape; } }

    private MeshKDTree meshKDTree;

    private void OnDrawGizmos()
    {
        if (!preview) return;
        BaseShape baseShape = GetComponent<BaseShape>();

        Gizmos.matrix = Matrix4x4.identity;

        Color c = forceType.previewColor;
        c = (additive ? c : c * ForcesStaticMembers.lightGray) * (enableForce ? 1 : .25f);

        if(baseShape != null)
        {
            baseShape.DrawShapeGizmo(c, forceRange);
            if (falloffRange > 0)
            {
                c = c * ForcesStaticMembers.semiTransparent; //makes falloff semi-transparent

                baseShape.DrawShapeGizmo(c, falloffRange + forceRange);
            }
        }
    }

    protected override void Reset()
    {
        base.Reset();

        ForceRange = 5f;
    }

    private void Awake()
    {
        BaseShape = GetComponent<BaseShape>();
    }

    public override Vector3 ForceVector(Vector3 point)
    {
        Vector3 normal = Vector3.zero;
        Vector3 surfacePoint = baseShape.ClosestPointOnShape(point, out normal);
        if (normal == Vector3.zero)
        {
            return Vector3.zero;
        }

        float distance = Vector3.Distance(point, surfacePoint);
        if (distance > forceRange + falloffRange)
        {
            return Vector3.zero;
        }
        else if (distance > forceRange)
        {
            float strength = 1 - ((distance - forceRange) / falloffRange);
            return -normal * forceStrength * strength;
        }
        else
        {
            return -normal * forceStrength;
        }
    }

    public override Vector3 ForceVector(Vector3 point, out float strength)
    {
        strength = 0;

        Vector3 normal = Vector3.zero;
        Vector3 surfacePoint = baseShape.ClosestPointOnShape(point, out normal);
        if (normal == Vector3.zero)
        {
            strength = 0;
            return Vector3.zero;
        }

        float distance = Vector3.Distance(point, surfacePoint);
        if (distance > forceRange + falloffRange)
        {
            return Vector3.zero;
        }
        else if (distance > forceRange)
        {
            strength = 1 - ((distance - forceRange) / falloffRange);
            return -normal * forceStrength;
        }
        else
        {
            strength = 1;
            return -normal * forceStrength;
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
        // don't check if transform has changed becaus Shape already does that
        if (baseShape.hasChanged || needsUpdate)
        {
            UpdateProducer();
            baseShape.hasChanged = false;
            needsUpdate = false;
        }
    }

    public override void UpdateProducer()
    {
        Bounds = baseShape.CalculateExpandedBounds(forceRange + falloffRange);
    }
}

