using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// update to allow for other types of collider?
//change from requiring box collider
public class ForceZone : ForceProducer
{
    [SerializeField]
    [Tooltip("The direction of force from this zone. Will be normalized")]
    // TODO: find a better system for setting gravity direction from editor
    private Vector3 forceDirection = Vector3.down;

    [SerializeField]
    [Tooltip("The distance it takes for the force to fade")]
    protected float falloffRange = 0f;

    private Bounds bounds;

    private BaseShape baseShape;

    protected override void OnDrawGizmos()
    {
        if (preview && enabled && baseShape != null)
        {
            Color c = forceType.previewColor;
            c = (additive ? c : c * ForcesStaticMembers.lightGray) * (enableForce ? 1 : .25f);
            baseShape.DrawShapeGizmo(c, 0);

            if (falloffRange > 0)
            {
                c = ForcesStaticMembers.MultiplyColors(c, ForcesStaticMembers.semiTransparent); //makes falloff semi-transparent
                baseShape.DrawShapeGizmo(c, falloffRange);
            }

            DrawArrow(Color.white, transform.position, Quaternion.FromToRotation(Vector3.up, forceDirection.normalized), Vector3.one, Mathf.Max((Mathf.Log(forceStrength) + 1), .5f));
        }
    }
    protected override void Reset()
    {
        base.Reset();

        forceDirection = Vector3.down;
        falloffRange = 0f;

        Cleanup();
    }

    protected override void Cleanup()
    {
        baseShape = GetComponent<BaseShape>();
        if (baseShape != null)
        {
            if (baseShape is MeshShape)
            {
                if (gameObject.GetComponent<MeshShapeKDTree>() == null)
                {
                    Debug.LogWarning("Force Zone using a Mesh Shape without a 'MeshShapeKDTree'!", gameObject);
                    gameObject.AddComponent<MeshShapeKDTree>();
                    Debug.LogWarning("Added a 'MeshShapeKDTree' to " + gameObject.name + ".", gameObject);
                }
            }
            else
            {
                if (gameObject.GetComponent<MeshShapeKDTree>() != null)
                {
                    Debug.LogWarning("Force Zone \"" + gameObject.name + "\" does not need a 'MeshShapeKDTree' component because it is not using a Mesh Shape", gameObject);
                }
            }
        }
        else
        {
            Debug.LogError("Force Zone requires an attached 'Shape' to work! eg. 'BoxShape', 'SphereShape'", gameObject);
            Debug.LogError("Removed Force Zone from " + gameObject.name + "!", gameObject);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += () =>
            {
                DestroyImmediate(this);
            };
#else
            DestroyImmediate(this);
#endif
        }
    }


#if UNITY_EDITOR
    protected override void OnValidate()
    {
        Cleanup();
    }
#endif

    private void Awake()
    {
        baseShape = GetComponent<BaseShape>();

        if (baseShape != null)
        {
            if (baseShape is MeshShape)
            {
                if (gameObject.GetComponent<MeshShapeKDTree>() == null)
                {
                    Debug.LogWarning("Force Surface \"" + gameObject.name + "\" does not have a 'MeshShapeKDTree' component for its Mesh! MeshShapeKDTree will be added at runtime!", gameObject);
                    gameObject.AddComponent<MeshColliderKDTree>();
                    Debug.LogWarning("'MeshShapeKDTree' added to " + gameObject.name + "!", gameObject);
                }
            }
            else
            {
                if (gameObject.GetComponent<MeshShapeKDTree>() != null)
                {
                    Debug.LogWarning("Force Surface \"" + gameObject.name + "\" does not need a 'MeshShapeKDTree' component because it is not using a Mesh Shape", gameObject);
                }
            }
        }
        else
        {
            Debug.LogWarning("Force Surface requires an attached 'Shape' to work! eg. 'BoxShape', 'SphereShape'", gameObject);
            DestroyImmediate(this);
            Debug.LogWarning("Removed Force Surface from " + gameObject.name + "!", gameObject);
        }
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

    public void SetForceDirection(Vector3 direction)
    {
        forceDirection = direction;
    }


    public void SetFalloffRange(float range)
    {
        falloffRange = range;
        needsUpdate = true;
        UpdateProducer();
    }

    // use these if position or scale is changing
    // or if falloff/force range changed
    public override void UpdateProducer()
    {
        if (transform.hasChanged || needsUpdate)
        {
            bounds = baseShape.GetExpandedBounds(falloffRange);
            transform.hasChanged = false;
            needsUpdate = false;
        }
    }
}
