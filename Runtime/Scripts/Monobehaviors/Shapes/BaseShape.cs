using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Add OnValidate to control negative values
public class BaseShape : MonoBehaviour
{
    // TODO: might bring back center offset in a later build
    //public Vector3 center;

    [HideInInspector]
    public bool boundsChanged = true;

    [HideInInspector]
    public Bounds bounds
    {
        get
        {
            return CalculateBounds();
        }
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.matrix = Matrix4x4.identity;

        //Gizmos.DrawWireCube(bounds.center, bounds.size);

        DrawShapeGizmo(ForcesStaticMembers.shapeColor, 0);
    }

    public virtual void DrawShapeGizmo(Color color, float expansion)
    {
    }

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

    protected virtual Bounds CalculateBounds()
    {
        return new Bounds();
    }
    
    /// <summary>
    /// Returns the bounds of the shape if it were to be expanded outwards by some amount
    /// This essentially returns a bounds for falloff that a producer can use for culling
    /// </summary>
    /// <param name="expansion"></param>
    /// <returns></returns>
    public virtual Bounds GetExpandedBounds(float expansion)
    {
        return new Bounds();
    }

    public virtual Vector3 ClosestPointOnShape(Vector3 to)
    {
        return Vector3.zero;
    }

    public virtual Vector3 ClosestPointOnShape(Vector3 to, ref Vector3 normal)
    {
        normal = Vector3.zero;
        return Vector3.zero;
    }

    public virtual float SignedDistance(Vector3 to)
    {
        return 0;
    }
}
