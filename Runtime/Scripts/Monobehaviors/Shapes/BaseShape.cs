using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Add OnValidate to control negative values
public abstract class BaseShape : MonoBehaviour
{
    [HideInInspector]
    public bool isStatic
    {
        get { return gameObject.isStatic; }
        set { gameObject.isStatic = value; }
    }
    // TODO: might bring back center offset in a later build
    //public Vector3 center;

    private bool NeedsUpdate = true;
    protected bool needsUpdate {
        get { return (NeedsUpdate || transform.hasChanged); }
        set {
            if (isStatic) {
                NeedsUpdate = false;
                return;
            }
            if (!value) transform.hasChanged = false;
            NeedsUpdate = value; 
        }
    }

    protected bool HasChanged = true;
    public bool hasChanged
    {
        get {
            if (needsUpdate) {
                Debug.Log("Bounds Changed");
                Bounds = CalculateBounds();
                needsUpdate = false;
                HasChanged = true;
            }
            return HasChanged;
        }
        set { HasChanged = value; }
    }

    protected Bounds Bounds;
    public Bounds bounds 
    { 
        get {
            if (needsUpdate) {
                Bounds = CalculateBounds();
                needsUpdate = false;
                hasChanged = true;
            }
            return Bounds;
        } 
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.matrix = Matrix4x4.identity;
        DrawShapeGizmo(ForcesStaticMembers.shapeColor, 0);
    }

    public virtual void DrawShapeGizmo(Color color, float expansion)
    {
    }

    protected virtual void OnDestroy()
    {
    }

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
    }
#endif

    protected virtual void Reset()
    {
        if (this.GetType() == typeof(BaseShape))
        {
            Debug.LogError("Do not use 'BaseShape'! Use one of its derived classes!", gameObject);
            Debug.LogError("Destorying 'BaseShape' on " + gameObject + "!", gameObject);
            DestroyImmediate(this);
        }
        else if(GetComponents<BaseShape>().Length > 1)
        {
            Debug.LogError("Don't use two shapes on the same object!", gameObject);
            Debug.LogError("Destorying Shape on " + gameObject + "!", gameObject);
            DestroyImmediate(this);
        }
    }

    public abstract Vector3 ClosestPointOnShape(Vector3 to);

    public abstract Vector3 ClosestPointOnShape(Vector3 to, out Vector3 normal);

    public abstract float SignedDistance(Vector3 to);

    protected abstract Bounds CalculateBounds();

    /// <summary>
    /// Returns the bounds of the shape if it were to be expanded outwards by some amount
    /// This essentially returns a bounds for falloff that a producer can use for culling
    /// </summary>
    /// <param name="expansion"></param>
    /// <returns></returns>
    public abstract Bounds CalculateExpandedBounds(float expansion);

}
