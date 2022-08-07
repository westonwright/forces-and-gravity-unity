using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for all force producers. This one is not to be used
/// </summary>
public abstract class ForceProducer : MonoBehaviour
{
    protected const string MENU_NAME = "GameObject/Force Producer/";

    public bool isStatic {
        get { return gameObject.isStatic; }
        set { gameObject.isStatic = value; }
    }

    //if gizmos should be drawn
    [SerializeField]
    [Tooltip("If Gizmos should be drawn for this source")]
    private bool Preview = true;
    protected bool preview
    {
        get { return Preview && enabled; }
    }

    [Tooltip("If this force source should be enabled by default.")]
    protected bool EnableForce = true;
    public bool enableForce { 
        get { return EnableForce; } 
        set {
            if (value && !EnableForce && !isStatic) { UpdateProducer(); }
            EnableForce = value;
        } 
    }
    [SerializeField]
    [Tooltip("The Force Type scriptable object that will define how this prodecer behaves")]
    protected ForceTypeSO ForceType;
    public ForceTypeSO forceType { get { return ForceType; } }

    //how strong this source's gravity is
    [SerializeField]
    protected float ForceStrength = 10f;
    public float forceStrength { 
        get { return ForceStrength; }
        set { if (!isStatic) { ForceStrength = value; } } 
    }
    [SerializeField]
    [Tooltip("The distance it takes for the force to fade to zero")]
    protected float FalloffRange = 0f;
    public float falloffRange
    {
        get { return FalloffRange; }
        set {
            if (isStatic) return;
            if(FalloffRange != value)
            {
                needsUpdate = true;
                FalloffRange = value;
            }
        }
    }
    // TODO: Add selector for type of falloff. eg. linear, inverse square, etc.)

    [Tooltip("What layers this produce will effect")]
    protected LayerMask LayerMask = ~0;
    public LayerMask layerMask { 
        get { return LayerMask; }
        set { if (!isStatic) { LayerMask = value; } }
    }
    [Tooltip("If this source should be used over another. Higher is less important. Negative is less important than anything positive and are less important as they go down, 0 is least important of everything/means importance doesn't matter.")]
    protected int Importance = 1;
    public int importance {
        get { return Importance; }
        set { if (!isStatic) { Importance = value; } }
    }
    [Tooltip("Additive means this effector will add its force to the active forces instead of overriding them. Each force mode adds/overrides seperately")]
    protected bool Additive = false;
    public bool additive
    {
        get { return Additive; }
        set { if (!isStatic) { Additive = value; } }
    }
    [Tooltip("If the Force Vector produced from this source should be inverted")]
    protected bool Invert = false;
    public bool invert
    {
        get { return Invert; }
        set { if (!isStatic) { Invert = value; } }
    }

    // broadcasts to
    protected ForceManagerSO forceManagerSO;

    // used to determin if bounds/other aspects should be recalculated
    protected bool needsUpdate = false;

    protected void DrawGizmoArrow(Color color, Vector3 pos, Quaternion rot, Vector3 scale, float magnitude)
    {
        Gizmos.color = color;
        float pointSize = .2f * magnitude;

        Gizmos.matrix = Matrix4x4.TRS(pos, rot, scale);
        Vector3 arrowBottom = Vector3.zero;
        Vector3 arrowTop = arrowBottom + Vector3.up * magnitude;
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

    protected virtual void Reset()
    {
        if(this.GetType() == typeof(ForceProducer))
        {
            Debug.LogError("Do not use 'ForceProducer'! Use one of its derived classes!", gameObject);
            Debug.LogError("Destorying 'ForceProducer' on " + gameObject + "!", gameObject);
            DestroyImmediate(this);
        }
        else
        {
#if UNITY_EDITOR
            ForceType = ForcesStaticMembers.defaultForceTypeSO;
#endif
            //preview = true;
            EnableForce = true;
            LayerMask = ~0;

            ForceStrength = 10f;
            FalloffRange = 0;
            Importance = 1;
            Additive = false;
            Invert = false;
        }
    }

    protected virtual void OnEnable()
    {
        forceManagerSO = ForcesStaticMembers.forceManagerSO;
        if (forceManagerSO != null)
        {
            forceManagerSO.AddForceProducer(this);
        }
        UpdateProducer();
    }
    protected virtual void OnDisable()
    {
        if (forceManagerSO != null)
        {
            forceManagerSO.RemoveForceProducer(this);
        }
    }

    // these next two functions should be overridden by child classes
    //returns the gravity vector at this point regardless of range
    public abstract Vector3 ForceVector(Vector3 point);

    //returns where in the falloff gradient the point is
    //0 means the point is outside the range
    //1 means teh point is completely covered   
    public abstract Vector3 ForceVector(Vector3 point, out float strength);

    public abstract bool PointInRange(Vector3 point);

    // update bounds or anything else thats needed here
    // only runs if an update needs to occur
    public abstract void TryUpdateProducer();

    // same as try update producer but will update no matter what
    public abstract void UpdateProducer();
}
