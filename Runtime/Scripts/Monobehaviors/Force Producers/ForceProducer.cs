using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for all force producers. This one is not to be used
/// </summary>
public class ForceProducer : MonoBehaviour
{
    [Tooltip("If this producer should update its transform when needed")]
    public bool isStatic = false;
    //if gizmos should be drawn
    [SerializeField]
    [Tooltip("If Gizmos should be drawn for this source")]
    protected bool preview = true;
    [Tooltip("If this force source should be enabled by default.")]
    public bool enableForce = true;
    [SerializeField]
    [Tooltip("The Force Type scriptable object that will define how this prodecer behaves")]
    protected ForceTypeSO ForceType;
    public ForceTypeSO forceType { get { return ForceType; } }

    //[Tooltip("How you want the produced forces to apply to rigidbodies. Generic is a special types that allow you to have custom behaviors in your own scripts.")]
    //public ForceType forceType = ForceType.Acceleration;
    [Tooltip("What layers this produce will effect")]
    public LayerMask layerMask = ~0;

    //how strong this source's gravity is
    [SerializeField]
    protected float forceStrength = 10f;

    // TODO: Add selector for type of falloff. eg. linear, inverse square, etc.)

    [Tooltip("If this source should be used over another. Higher is less important. Negative is less important than anything positive and are less important as they go down, 0 is least important of everything/means importance doesn't matter.")]
    public int importance = 1;
    [Tooltip("Additive means this effector will add its force to the active forces instead of overriding them. Each force mode adds/overrides seperately")]
    public bool additive = false;
    [Tooltip("If the Force Vector produced from this source should be inverted")]
    public bool invert = false;

    // broadcasts to
    protected ForceManagerSO forceManagerSO;

    // used to determin if bounds/other aspects should be recalculated
    protected bool needsUpdate = true;

    protected virtual void OnDrawGizmos()
    {
    }

    protected virtual void DrawArrow(Color color, Vector3 pos, Quaternion rot, Vector3 scale, float magnitude)
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
            preview = true;
            enableForce = true;
            layerMask = ~0;

            forceStrength = 10f;
            importance = 1;
            additive = false;
            invert = false;
        }
    }

    protected virtual void Cleanup()
    {

    }

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {

    }
#endif

    protected virtual void OnEnable()
    {
        forceManagerSO = ForcesStaticMembers.forceManagerSO;
        if (forceManagerSO != null)
        {
            forceManagerSO.AddForceProducer(this);
        }
        needsUpdate = true;
        UpdateProducer();
    }

    protected virtual void OnDisable()
    {
        if (forceManagerSO != null)
        {
            forceManagerSO.RemoveForceProducer(this);
        }
    }

    public virtual void EnableForce(bool enabled)
    {
        enableForce = enabled;
        if (enableForce)
        {
            UpdateProducer();
        }
    }

    // these next two functions should be overridden by child classes
    //returns the gravity vector at this point regardless of range
    public virtual Vector3 ForceVector(Vector3 point)
    {
        return Vector3.zero;
    }

    //returns where in the falloff gradient the point is
    //0 means the point is outside the range
    //1 means teh point is completely covered   
    public virtual Vector3 ForceVector(Vector3 point, out float strength)
    {
        strength = 1;
        return Vector3.zero;
    }

    public virtual bool PointInRange(Vector3 point)
    {
        return false;
    }

    public virtual void UpdateProducer()
    {
        // update bounds or anything else thats needed here
        // be sure to check if things need to actually be updated to avoid unecessary calculations
    }
}
