using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//placed on the mesh providing gravity
//needs a MeshKDTree if Collider is a mesh Collider
public class ForceShape : ForceProducer
{
    [SerializeField]
    [Tooltip("How far the full-strength force extends out from the collider")]
    //how far out this plante's gravity reaches (not including falloff range)
    private float forceRange = 5f;
    [SerializeField]
    [Tooltip("The distance it takes for the force to fade")]
    protected float falloffRange = 0f;

    private float activationRadius = 0;

    private BaseShape baseShape;

    protected override void OnDrawGizmos()
    {
        if (preview)
        {
            BaseShape bs = GetComponent<BaseShape>();
            //These show the activation radius and the bounding box
            //Gizmos.DrawWireSphere(col.bounds.center, Vector3.Distance(col.bounds.max - col.bounds.center, Vector3.zero) + gravityRange);
            //Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);

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
            Gizmos.color = (additive ? Gizmos.color : Gizmos.color * ForcesStaticMembers.lightGray) * (enableForce ? 1 : .25f);
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

            if (bs is MeshShape)
            {
                Mesh rangeMesh = new Mesh();
                rangeMesh.vertices = (bs as MeshShape).mesh.vertices;
                rangeMesh.triangles = (bs as MeshShape).mesh.triangles;
                rangeMesh.normals = (bs as MeshShape).mesh.normals;
                Vector3[] vertices = rangeMesh.vertices;
                Vector3[] normals = rangeMesh.normals;
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] = ForcesStaticMembers.MultiplyVectors(vertices[i], transform.localScale);
                    vertices[i] += normals[i] * forceRange;
                }
                rangeMesh.vertices = vertices;
                Gizmos.DrawWireMesh(rangeMesh);

                if (falloffRange > 0)
                {
                    Mesh falloffMesh = new Mesh();
                    falloffMesh.vertices = (bs as MeshShape).mesh.vertices;
                    falloffMesh.triangles = (bs as MeshShape).mesh.triangles;
                    falloffMesh.normals = (bs as MeshShape).mesh.normals;
                    Gizmos.color = ForcesStaticMembers.MultiplyColors(Gizmos.color, ForcesStaticMembers.semiTransparent); //makes falloff semi-transparent
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        vertices[i] += normals[i] * falloffRange;
                    }
                    falloffMesh.vertices = vertices;
                    Gizmos.DrawWireMesh(falloffMesh);
                }

            }
            else if (bs is SphereShape)
            {
                Gizmos.DrawWireSphere(ForcesStaticMembers.MultiplyVectors(transform.localScale, (bs as SphereShape).center), (ForcesStaticMembers.VectorMax(transform.localScale) * (bs as SphereShape).radius) + forceRange);
                Gizmos.color = ForcesStaticMembers.MultiplyColors(Gizmos.color, ForcesStaticMembers.semiTransparent); //makes falloff semi-transparent
                Gizmos.DrawWireSphere(ForcesStaticMembers.MultiplyVectors(transform.localScale, (bs as SphereShape).center), (ForcesStaticMembers.VectorMax(transform.localScale) * (bs as SphereShape).radius) + forceRange + falloffRange);
            }
            else if (bs is BoxShape)
            {
                Gizmos.DrawWireCube(ForcesStaticMembers.MultiplyVectors(transform.localScale, (bs as BoxShape).center), ForcesStaticMembers.AddToVector(ForcesStaticMembers.MultiplyVectors(transform.localScale, (bs as BoxShape).size), forceRange));
                Gizmos.color = ForcesStaticMembers.MultiplyColors(Gizmos.color, ForcesStaticMembers.semiTransparent); //makes falloff semi-transparent
                Gizmos.DrawWireCube(ForcesStaticMembers.MultiplyVectors(transform.localScale, (bs as BoxShape).center), ForcesStaticMembers.AddToVector(ForcesStaticMembers.MultiplyVectors(transform.localScale, (bs as BoxShape).size), forceRange + falloffRange));
            }
            //figure this out later
            else if (bs is CapsuleShape)
            {
            }
            //figure this out later
            else if (bs is PlaneShape)
            {
                Vector3 bl = (Vector3.down + Vector3.left) * .5f * ForcesStaticMembers.MultiplyVectors((bs as PlaneShape).size, (Vector2)bs.transform.lossyScale);
                Vector3 br = (Vector3.down + Vector3.right) * .5f * ForcesStaticMembers.MultiplyVectors((bs as PlaneShape).size, (Vector2)bs.transform.lossyScale);
                Vector3 tl = (Vector3.up + Vector3.left) * .5f * ForcesStaticMembers.MultiplyVectors((bs as PlaneShape).size, (Vector2)bs.transform.lossyScale);
                Vector3 tr = (Vector3.up + Vector3.right) * .5f * ForcesStaticMembers.MultiplyVectors((bs as PlaneShape).size, (Vector2)bs.transform.lossyScale);

                Gizmos.DrawLine(bl + Vector3.forward * forceRange, br + Vector3.forward * forceRange);
                Gizmos.DrawLine(br + Vector3.forward * forceRange, tr + Vector3.forward * forceRange);
                Gizmos.DrawLine(tr + Vector3.forward * forceRange, tl + Vector3.forward * forceRange);
                Gizmos.DrawLine(tl + Vector3.forward * forceRange, bl + Vector3.forward * forceRange);

                Gizmos.color = ForcesStaticMembers.MultiplyColors(Gizmos.color, ForcesStaticMembers.semiTransparent); //makes falloff semi-transparent

                Gizmos.DrawLine(bl + Vector3.forward * forceRange + Vector3.forward * falloffRange, br + Vector3.forward * forceRange + Vector3.forward * falloffRange);
                Gizmos.DrawLine(br + Vector3.forward * forceRange + Vector3.forward * falloffRange, tr + Vector3.forward * forceRange + Vector3.forward * falloffRange);
                Gizmos.DrawLine(tr + Vector3.forward * forceRange + Vector3.forward * falloffRange, tl + Vector3.forward * forceRange + Vector3.forward * falloffRange);
                Gizmos.DrawLine(tl + Vector3.forward * forceRange + Vector3.forward * falloffRange, bl + Vector3.forward * forceRange + Vector3.forward * falloffRange);
            }
        }
    }

    protected override void Reset()
    {
        baseShape = GetComponent<BaseShape>();
        if(baseShape is MeshShape)
        {
            if (gameObject.GetComponent<MeshShapeKDTree>() == null)
            {
                Debug.LogWarning("Force Shape using a Mesh Shape without a 'MeshShapeKDTree'!", gameObject);
                gameObject.AddComponent<MeshShapeKDTree>();
                Debug.LogWarning("Added a 'MeshShapeKDTree' to " + gameObject.name + ".", gameObject);
            }
        }
        else
        {
            if (gameObject.GetComponent<MeshShapeKDTree>() != null)
            {
                Debug.LogWarning("Force Shape \"" + gameObject.name + "\" does not need a 'MeshShapeKDTree' component because it is not using a Mesh Shape", gameObject);
            }
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        Reset();
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
                    Debug.LogWarning("Force Shape \"" + gameObject.name + "\" does not have a 'MeshShapeKDTree' component for its Mesh! MeshShapeKDTree will be added at runtime!", gameObject);
                    gameObject.AddComponent<MeshColliderKDTree>();
                    Debug.LogWarning("'MeshShapeKDTree' added to " + gameObject.name + "!", gameObject);
                }
            }
            else
            {
                if (gameObject.GetComponent<MeshShapeKDTree>() != null)
                {
                    Debug.LogWarning("Force Shape \"" + gameObject.name + "\" does not need a 'MeshShapeKDTree' component because it is not using a Mesh Shape", gameObject);
                }
            }
        }
        UpdateActivationRadius();
    }

    public override Vector3 ForceVector(Vector3 point, out float strength)
    {
        strength = 0;
        Vector3 normal = Vector3.zero;
        //don't calculate gravity vector if strength is 0

        //see if point is in activation radius before getting strength from distance to surface
        float distance = Vector3.Distance(point, baseShape.bounds.center);
        if (distance <= activationRadius)
        {
            Vector3 surfacePoint = baseShape.ClosestPointOnShape(point, ref normal);
            if (normal == Vector3.zero)
            {
                strength = 0;
                return Vector3.zero;
            }
            distance = Vector3.Distance(point, surfacePoint);

            if(distance > forceRange + falloffRange)
            {
                return Vector3.zero;
            }
            else if(distance > forceRange)
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
        else
        {
            return Vector3.zero;
        }
    }

    public override Vector3 ForceVector(Vector3 point)
    {
        Vector3 normal = Vector3.zero;
        baseShape.ClosestPointOnShape(point, ref normal);
        return -normal * forceStrength;
    }

    //use these if position or scale is changing
    public void UpdateActivationRadius()
    {
        activationRadius = Vector3.Distance(baseShape.bounds.max - baseShape.bounds.center, Vector3.zero) + forceRange + falloffRange;
    }

}

