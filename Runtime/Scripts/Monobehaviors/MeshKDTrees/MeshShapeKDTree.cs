using UnityEngine;
using System.Collections.Generic;
// with code from HiddenMonk: https://forum.unity.com/threads/get-the-collision-points-in-physics-overlapsphere.395176/#post-2581349

// place on mesh used to get gravity vector
// TODO: change so that you can delete component on objects
[RequireComponent(typeof(MeshShape))]
public class MeshShapeKDTree : MeshKDTree
{
    MeshShape ms;
    protected override void Awake()
    {
        ms = GetComponent<MeshShape>();
        if (ms != null)
        {
            mesh = ms.mesh;
            InitializeKDTree();
        }
    }

    protected override void Reset()
    {
        ms = GetComponent<MeshShape>();
        if(ms != null)
        {
            mesh = ms.mesh;
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        ms = GetComponent<MeshShape>();

        if (ms != null)
        {
            if (mesh != ms.mesh)
            {
                Debug.LogWarning("Don't manually change mesh on 'MeshShapeKDTree'!", gameObject);
                mesh = ms.mesh;
            }
        }
    }
#endif

    public override void SetMesh(Mesh m)
    {
        ms.mesh = m;
        mesh = ms.mesh;
        InitializeKDTree();
    }

    public override void UpdateMesh()
    {
        mesh = ms.mesh;
        InitializeKDTree();
    }

}