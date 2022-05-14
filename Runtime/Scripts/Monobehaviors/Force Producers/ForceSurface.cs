using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//placed on the mesh providing gravity
// TODO: prevent multiple surfaces (or shapes) from being applied to the same object
public class ForceSurface : ForceProducer
{
    [SerializeField]
    [Tooltip("How far the full-strength force extends out from the collider")]
    //how far out this plante's gravity reaches (not including falloff range)
    private float forceRange = 5f;
    [SerializeField]
    [Tooltip("The distance it takes for the force to fade")]
    protected float falloffRange = 0f;

    private Bounds bounds;

    private BaseShape baseShape;

    protected override void OnDrawGizmos()
    {
        if (preview && enabled && baseShape != null)
        {
            Gizmos.matrix = Matrix4x4.identity;
            //Bounds b = baseShape.GetExpandedBounds(forceRange + falloffRange);
            //Gizmos.DrawWireCube(b.center, b.size);
            Color c = forceType.previewColor;
            c = (additive ? c : c * ForcesStaticMembers.lightGray) * (enableForce ? 1 : .25f);

            if(baseShape != null)
            {
                baseShape.DrawShapeGizmo(c, forceRange);
                if (falloffRange > 0)
                {
                    c = ForcesStaticMembers.MultiplyColors(c, ForcesStaticMembers.semiTransparent); //makes falloff semi-transparent

                    baseShape.DrawShapeGizmo(c, falloffRange + forceRange);
                }
            }
        }
    }

    protected override void Reset()
    {
        base.Reset();

        forceRange = 5f;
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
                    Debug.LogWarning("Force Surface using a Mesh Shape without a 'MeshShapeKDTree'!", gameObject);
                    gameObject.AddComponent<MeshShapeKDTree>();
                    Debug.LogWarning("Added a 'MeshShapeKDTree' to " + gameObject.name + ".", gameObject);
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
            Debug.LogError("Force Surface requires an attached 'Shape' to work! eg. 'BoxShape', 'SphereShape'", gameObject);
            Debug.LogError("Removed Force Surface from " + gameObject.name + "!", gameObject);
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
            Debug.LogWarning("Removed Force Surface from " + gameObject.name + "!", gameObject);
            DestroyImmediate(this);
        }
    }

    public override Vector3 ForceVector(Vector3 point)
    {
        Vector3 normal = Vector3.zero;
        Vector3 surfacePoint = baseShape.ClosestPointOnShape(point, ref normal);
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
        Vector3 surfacePoint = baseShape.ClosestPointOnShape(point, ref normal);
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

    public void SetFalloffRange(float range)
    {
        falloffRange = range;
        needsUpdate = true;
        UpdateProducer();
    }

    public void SetForceRange(float range)
    {
        forceRange = range;
        needsUpdate = true;
        UpdateProducer();
    }

    // use these if position or scale is changing
    // or if falloff/force range changed
    public override void UpdateProducer()
    {
        if (transform.hasChanged || needsUpdate)
        {
            bounds = baseShape.GetExpandedBounds(forceRange + falloffRange);
            transform.hasChanged = false;
            needsUpdate = false;
        }
    }
}

