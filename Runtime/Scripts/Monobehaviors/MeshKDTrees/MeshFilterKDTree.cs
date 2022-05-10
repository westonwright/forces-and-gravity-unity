using UnityEngine;
using System.Collections.Generic;
// with code from HiddenMonk: https://forum.unity.com/threads/get-the-collision-points-in-physics-overlapsphere.395176/#post-2581349

//place on mesh used to get gravity vector
[RequireComponent(typeof(MeshFilter))]
public class MeshFilterKDTree : MeshKDTree
{
    MeshFilter mf;

    protected override void Awake()
    {
        mf = GetComponent<MeshFilter>();
        mesh = mf.mesh;
        InitializeKDTree();
    }
    protected override void Reset()
    {
        mf = GetComponent<MeshFilter>();
        mesh = mf.mesh;
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        mf = GetComponent<MeshFilter>();

        if (mesh != mf.mesh)
        {
            Debug.LogWarning("Don't manually change mesh on 'MeshFilterKDTree'!");
            mesh = mf.mesh;
        }
    }
#endif

    public override void SetMesh(Mesh m)
    {
        mf.mesh = m;
        mesh = mf.mesh;
        InitializeKDTree();
    }

    public override void UpdateMesh()
    {
        mesh = mf.mesh;
        InitializeKDTree();
    }
}