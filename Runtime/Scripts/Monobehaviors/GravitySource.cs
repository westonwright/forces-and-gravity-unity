using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class GravitySource : MonoBehaviour
{
    //if gizmos should be drawn
    [SerializeField]
    [Tooltip("If Gizmos should be drawn for this source")]
    protected bool preview = true;
    [SerializeField]
    [Tooltip("If this gravity source should be enabled by default.")]
    protected bool enableGravity = true;

    //how strong this source's gravity is
    [SerializeField]
    protected float gravityStrength = 9.8f;
    [SerializeField]
    protected float falloffRange = 0f;
    // TODO: Add selector for type of falloff. eg. linear, inverse square, etc.)

    [Tooltip("If this source should be used over another. Higher is less important. Set to 0 if importance doesn't matter")]
    public int importance = 0;
    [Tooltip("Additive means this effector will add its force to the active gravity field instead of overriding it")]
    public bool additive = false;
    [Tooltip("If the Gravity Vector produced from this source should be inverted")]
    public bool invert = false;
    //[SerializeField]
    // used if you dont want the gravity manager to manually detect this gravity source. Should usually be true
    //protected bool addSelfToGravityManager = true;

    protected Collider gravityCollider;
    //protected GravityManager gravityManager;
    // broadcasts to
    private GravitySourceEventsChannelSO gravitySourceEventsChannel;

    private void Awake()
    {
        Initialize();
        if (Application.isPlaying)
        {
            gravitySourceEventsChannel = CustomGravityHelperFunctions._gravitySourceEventsChannel;
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        if (Application.isPlaying)
        {
            if ((gravitySourceEventsChannel != null) && enableGravity)
            {
                gravitySourceEventsChannel.RaiseAddEvent(this);
            }
        }
    }

    //use to remove from gravity manager
    private void OnDisable()
    {
        if (Application.isPlaying)
        {
            if ((gravitySourceEventsChannel != null) && enableGravity)
            {
                gravitySourceEventsChannel.RaiseRemoveEvent(this);
            }
        }

    }

    public virtual void Initialize()
    {
        //prevents this from being added to objects
        this.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector;
        Debug.LogError("Use \"GravityZone\", \"GravitySurface\", or \"GravityPoint\" instead of \"GravitySource\"!", gameObject);
        Debug.LogWarning("Destroyed \"GravitySource\" Component", gameObject);
        DestroyImmediate(this);
    }

    public void EnableGravitySource(bool on)
    {
        enableGravity = on;
        if(gravitySourceEventsChannel != null)
        {
            if (enableGravity)
            {
                gravitySourceEventsChannel.RaiseAddEvent(this);
            }
            else
            {
                gravitySourceEventsChannel.RaiseRemoveEvent(this);
            }
        }
    }

    // these next two functions should be overridden by child classes

    //returns the gravity vector at this point regardless of range
    public virtual Vector3 GravityVector(Vector3 point)
    {
        return Vector3.zero;
    }

    //returns where in the falloff gradient the point is
    //0 means the point is outside the range
    //1 means teh point is completely covered   
    public virtual Vector3 GravityVector(Vector3 point, out float strength)
    {
        //don't calculate gravity vector if strength is 0
        strength = 0;
        return Vector3.zero;
    }
}
