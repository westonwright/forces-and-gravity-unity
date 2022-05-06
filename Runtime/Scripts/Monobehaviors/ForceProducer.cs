using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for all force producers. This one just pulls in the object from anywhere
/// </summary>
public class ForceProducer : MonoBehaviour
{
    //if gizmos should be drawn
    [SerializeField]
    [Tooltip("If Gizmos should be drawn for this source")]
    protected bool preview = true;
    [Tooltip("If this force source should be enabled by default.")]
    public bool enableForce = true;
    [Tooltip("How you want the produced forces to apply to rigidbodies. Generic is a special types that allow you to have custom behaviors in your own scripts.")]
    public ForceType forceType = ForceType.Acceleration;
    [Tooltip("What layers this produce will effect")]
    public LayerMask layerMask = ~0;

    //how strong this source's gravity is
    [SerializeField]
    protected float forceStrength = 9.8f;

    // TODO: Add selector for type of falloff. eg. linear, inverse square, etc.)

    [Tooltip("If this source should be used over another. Higher is less important. Negative is less important than anything positive and are less important as they go down, 0 is least important of everything/means importance doesn't matter.")]
    public int importance = 1;
    [Tooltip("Additive means this effector will add its force to the active forces instead of overriding them. Each force mode adds/overrides seperately")]
    public bool additive = false;
    [Tooltip("If the Force Vector produced from this source should be inverted")]
    public bool invert = false;

    // broadcasts to
    protected ForceManagerSO forceManagerSO;

    protected virtual void OnDrawGizmos()
    {
        Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.identity, Vector3.one);

        switch (forceType)
        {
            case ForceType.Force:
                Gizmos.color = new Color(1, 0, 0, 1);
                break;
            case ForceType.Acceleration:
                Gizmos.color = new Color(1, .5f, 0, 1);
                break;
            case ForceType.Impulse:
                Gizmos.color = new Color(1, 1, 0, 1);
                break;
            case ForceType.VelocityChange:
                Gizmos.color = new Color(.5f, 1, 0, 1);
                break;
            case ForceType.Gravity:
                Gizmos.color = new Color(0, 1, 0, 1);
                break;
            case ForceType.Generic:
                Gizmos.color = new Color(0, 1, .5f, 1);
                break;
        }
        Gizmos.color = (additive ? Gizmos.color : Gizmos.color * new Color(.75f, .75f, .75f, 1)) * (enableForce ? 1 : .25f);

        Gizmos.DrawLine(Vector3.down, Vector3.up);
        Gizmos.DrawLine(Vector3.right, Vector3.left);
        Gizmos.DrawLine(Vector3.forward, Vector3.back);
    }

    protected virtual void OnEnable()
    {
        forceManagerSO = ForcesStaticMembers.forceManagerSO;
        if (forceManagerSO != null)
        {
            forceManagerSO.AddForceProducer(this);
        }
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
    }

    // these next two functions should be overridden by child classes
    //returns the gravity vector at this point regardless of range
    public virtual Vector3 ForceVector(Vector3 point)
    {
        return (transform.position - point).normalized * forceStrength;
    }

    //returns where in the falloff gradient the point is
    //0 means the point is outside the range
    //1 means teh point is completely covered   
    public virtual Vector3 ForceVector(Vector3 point, out float strength)
    {
        strength = 1;
        return (transform.position - point).normalized * forceStrength;
    }
}
