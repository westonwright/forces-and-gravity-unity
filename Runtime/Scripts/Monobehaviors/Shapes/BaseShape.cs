using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseShape : MonoBehaviour
{
    public Vector3 center;

    [HideInInspector]
    public Bounds bounds
    {
        get
        {
            return GetBounds();
        }
    }

    protected virtual void OnDrawGizmos()
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
    }

    protected virtual Bounds GetBounds()
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
}
