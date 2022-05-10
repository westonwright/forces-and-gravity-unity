using UnityEngine;
using System.Collections.Generic;
// with code from HiddenMonk: https://forum.unity.com/threads/get-the-collision-points-in-physics-overlapsphere.395176/#post-2581349

//place on mesh used to get gravity vector
[RequireComponent(typeof(MeshCollider))]
public class MeshColliderKDTree : MeshKDTree
{
    MeshCollider mc;
    protected override void Awake()
    {
        mc = GetComponent<MeshCollider>();
        mesh = mc.sharedMesh;
        InitializeKDTree();
    }

    protected override void Reset()
    {
        mc = GetComponent<MeshCollider>();
        mesh = mc.sharedMesh;
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        mc = GetComponent<MeshCollider>();

        if (mesh != mc.sharedMesh)
        {
            Debug.LogWarning("Don't manually change mesh on 'MeshColliderKDTree'!", gameObject);
            mesh = mc.sharedMesh;
        }
    }
#endif

    public override void SetMesh(Mesh m)
    {
        mc.sharedMesh = m;
        mesh = mc.sharedMesh;
        InitializeKDTree();
    }

    public override void UpdateMesh()
    {
        mesh = mc.sharedMesh;
        InitializeKDTree();
    }

}