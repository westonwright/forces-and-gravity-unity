using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//placed on the mesh providing gravity
//needs a MeshKDTree if Collider is a mesh Collider
[RequireComponent(typeof(Collider))]
public class ForceCollider : ForceProducer
{
    [SerializeField]
    [Tooltip("How far the full-strength force extends out from the collider")]
    //how far out this plante's gravity reaches (not including falloff range)
    private float forceRange = 5f;
    [SerializeField]
    [Tooltip("The distance it takes for the force to fade")]
    protected float falloffRange = 0f;

    private float activationRadius = 0;

    private Collider forceCollider;

    protected override void OnDrawGizmos()
    {
        if (preview)
        {
            Collider col = GetComponent<Collider>();
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
                case ForceType.Wind:
                    Gizmos.color = new Color(0, 1, 0, 1);
                    break;
                case ForceType.Gravity:
                    Gizmos.color = new Color(0, 1, .5f, 1);
                    break;
                case ForceType.Generic:
                    Gizmos.color = new Color(0, 1, 1, 1);
                    break;
            }
            Gizmos.color = (additive ? Gizmos.color : Gizmos.color * new Color(.75f, .75f, .75f, 1)) * (enableForce ? 1 : .25f);
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

            if (col is MeshCollider)
            {
                Mesh rangeMesh = new Mesh();
                rangeMesh.vertices = (col as MeshCollider).sharedMesh.vertices;
                rangeMesh.triangles = (col as MeshCollider).sharedMesh.triangles;
                rangeMesh.normals = (col as MeshCollider).sharedMesh.normals;
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
                    falloffMesh.vertices = (col as MeshCollider).sharedMesh.vertices;
                    falloffMesh.triangles = (col as MeshCollider).sharedMesh.triangles;
                    falloffMesh.normals = (col as MeshCollider).sharedMesh.normals;
                    Gizmos.color = ForcesStaticMembers.MultiplyColors(Gizmos.color, new Color(1, 1, 1, .25f)); //makes falloff semi-transparent
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        vertices[i] += normals[i] * falloffRange;
                    }
                    falloffMesh.vertices = vertices;
                    Gizmos.DrawWireMesh(falloffMesh);
                }

            }
            else if (col is SphereCollider)
            {
                Gizmos.DrawWireSphere(ForcesStaticMembers.MultiplyVectors(transform.localScale, (col as SphereCollider).center), (ForcesStaticMembers.VectorMax(transform.localScale) * (col as SphereCollider).radius) + forceRange);
                Gizmos.color = ForcesStaticMembers.MultiplyColors(Gizmos.color, new Color(1, 1, 1, .25f)); //makes falloff semi-transparent
                Gizmos.DrawWireSphere(ForcesStaticMembers.MultiplyVectors(transform.localScale, (col as SphereCollider).center), (ForcesStaticMembers.VectorMax(transform.localScale) * (col as SphereCollider).radius) + forceRange + falloffRange);
            }
            else if (col is BoxCollider)
            {
                Gizmos.DrawWireCube(ForcesStaticMembers.MultiplyVectors(transform.localScale, (col as BoxCollider).center), ForcesStaticMembers.AddToVector(ForcesStaticMembers.MultiplyVectors(transform.localScale, (col as BoxCollider).size), forceRange));
                Gizmos.color = ForcesStaticMembers.MultiplyColors(Gizmos.color, new Color(1, 1, 1, .25f)); //makes falloff semi-transparent
                Gizmos.DrawWireCube(ForcesStaticMembers.MultiplyVectors(transform.localScale, (col as BoxCollider).center), ForcesStaticMembers.AddToVector(ForcesStaticMembers.MultiplyVectors(transform.localScale, (col as BoxCollider).size), forceRange + falloffRange));
            }
            //figure this out later
            else if (col is CapsuleCollider)
            {
            }
            //figure this out later
            else if (col is TerrainCollider)
            {

            }
        }
    }

    protected override void Reset()
    {
        forceCollider = GetComponent<Collider>();
        if(forceCollider is MeshCollider)
        {
            if (gameObject.GetComponent<MeshColliderKDTree>() == null)
            {
                Debug.LogWarning("Force Collider using a Mesh Collider without a 'MeshColliderKDTree'!", gameObject);
                gameObject.AddComponent<MeshColliderKDTree>();
                Debug.LogWarning("Added a 'MeshColliderKDTree' to " + gameObject.name + ".", gameObject);
            }
        }
        else
        {
            if (gameObject.GetComponent<MeshColliderKDTree>() != null)
            {
                Debug.LogWarning("Force Collider \"" + gameObject.name + "\" does not need a 'MeshColliderKDTree' component because it is not using a Mesh Collider", gameObject);
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
        forceCollider = GetComponent<Collider>();

        if (forceCollider != null)
        {
            if (forceCollider is MeshCollider)
            {
                if (gameObject.GetComponent<MeshColliderKDTree>() == null)
                {
                    Debug.LogWarning("Force Collider \"" + gameObject.name + "\" does not have a 'MeshColliderKDTree' component for its Mesh! MeshColliderKDTree will be added at runtime!", gameObject);
                    gameObject.AddComponent<MeshColliderKDTree>();
                    Debug.LogWarning("MeshColliderKDTree added to " + gameObject.name + "!", gameObject);
                }
            }
            else
            {
                if (gameObject.GetComponent<MeshColliderKDTree>() != null)
                {
                    Debug.LogWarning("Force Collider \"" + gameObject.name + "\" does not need a 'MeshColliderKDTree' component because it is not using a Mesh Collider", gameObject);
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
        float distance = Vector3.Distance(point, forceCollider.bounds.center);
        if (distance <= activationRadius)
        {
            Vector3 surfacePoint = ColliderCalculations.ClosestPointOnCollider(forceCollider, point, ref normal);
            if (normal == Vector3.zero)
            {
                strength = 0;
                return Vector3.zero;
            }
            distance = Vector3.Distance(point, surfacePoint);

            if (distance > forceRange + falloffRange)
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
        ColliderCalculations.ClosestPointOnCollider(forceCollider, point, ref normal);
        return -normal * forceStrength;
    }

    //use these if position or scale is changing
    public void UpdateActivationRadius()
    {
        activationRadius = Vector3.Distance(forceCollider.bounds.max - forceCollider.bounds.center, Vector3.zero) + forceRange + falloffRange;
    }

}

