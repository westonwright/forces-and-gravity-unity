using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//placed on the mesh providing gravity
//needs a MeshKDTree if Collider is a mesh Collider
[RequireComponent(typeof(Collider))]
public class GravitySurface : GravitySource
{
    [SerializeField]
    //how far out this plante's gravity reaches (not including falloff range)
    private float gravityRange = 5f;

    private float activationRadius;
    private void OnDrawGizmos()
    {
        if (preview)
        {
            Collider col = GetComponent<Collider>();
            //These show the activation radius and the bounding box
            //Gizmos.DrawWireSphere(col.bounds.center, Vector3.Distance(col.bounds.max - col.bounds.center, Vector3.zero) + gravityRange);
            //Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);

            Gizmos.color = (additive ? Color.yellow : Color.red) * (enableGravity ? 1 : .25f);
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
                    vertices[i] = CustomGravityHelperFunctions.MultiplyVectors(vertices[i], transform.localScale);
                    vertices[i] += normals[i] * gravityRange;
                }
                rangeMesh.vertices = vertices;
                Gizmos.DrawWireMesh(rangeMesh);

                if (falloffRange > 0)
                {
                    Mesh falloffMesh = new Mesh();
                    falloffMesh.vertices = (col as MeshCollider).sharedMesh.vertices;
                    falloffMesh.triangles = (col as MeshCollider).sharedMesh.triangles;
                    falloffMesh.normals = (col as MeshCollider).sharedMesh.normals;
                    Gizmos.color = CustomGravityHelperFunctions.MultiplyColors(Gizmos.color, new Color(1, 1, 1, .25f)); //makes falloff semi-transparent
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
                Gizmos.DrawWireSphere(CustomGravityHelperFunctions.MultiplyVectors(transform.localScale, (col as SphereCollider).center), (CustomGravityHelperFunctions.VectorMax(transform.localScale) * (col as SphereCollider).radius) + gravityRange);
                Gizmos.color = CustomGravityHelperFunctions.MultiplyColors(Gizmos.color, new Color(1, 1, 1, .25f)); //makes falloff semi-transparent
                Gizmos.DrawWireSphere(CustomGravityHelperFunctions.MultiplyVectors(transform.localScale, (col as SphereCollider).center), (CustomGravityHelperFunctions.VectorMax(transform.localScale) * (col as SphereCollider).radius) + gravityRange + falloffRange);
            }
            else if (col is BoxCollider)
            {
                Gizmos.DrawWireCube(CustomGravityHelperFunctions.MultiplyVectors(transform.localScale, (col as BoxCollider).center), CustomGravityHelperFunctions.AddToVector(CustomGravityHelperFunctions.MultiplyVectors(transform.localScale, (col as BoxCollider).size), gravityRange));
                Gizmos.color = CustomGravityHelperFunctions.MultiplyColors(Gizmos.color, new Color(1, 1, 1, .25f)); //makes falloff semi-transparent
                Gizmos.DrawWireCube(CustomGravityHelperFunctions.MultiplyVectors(transform.localScale, (col as BoxCollider).center), CustomGravityHelperFunctions.AddToVector(CustomGravityHelperFunctions.MultiplyVectors(transform.localScale, (col as BoxCollider).size), gravityRange + falloffRange));
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

    public override void Initialize()
    {
        //be sure to only run when playing
        if (Application.isPlaying)
        {
            gravityCollider = GetComponent<Collider>();

            if (gravityCollider != null)
            {
                if (gravityCollider is MeshCollider)
                {
                    if (gameObject.GetComponent<MeshKDTree>() == null)
                    {
                        Debug.LogWarning("Gravity Surface \"" + gameObject.name + "\" does not have a MeshKDTree component for its Mesh! Additional calculations will happen every frame!", gameObject);
                    }
                }
                else
                {
                    if (gameObject.GetComponent<MeshKDTree>() != null)
                    {
                        Debug.LogWarning("Gravity Surface \"" + gameObject.name + "\" does not need a MeshKDTree component because it is not using a Mesh Collider", gameObject);
                    }
                }
            }

            UpdateActivationRadius();
        }
    }

    public override Vector3 GravityVector(Vector3 point, out float strength)
    {
        strength = 0;
        Vector3 normal = Vector3.zero;
        //don't calculate gravity vector if strength is 0

        //see if point is in activation radius before getting strength from distance to surface
        float distance = Vector3.Distance(point, gravityCollider.bounds.center);
        if (distance <= activationRadius)
        {
            Vector3 surfacePoint = ColliderCalculations.ClosestPointOnSurface(gravityCollider, point, 0, ref normal);
            distance = Vector3.Distance(point, surfacePoint);
            if(distance > gravityRange + falloffRange)
            {
                return Vector3.zero;
            }
            else if(distance > gravityRange)
            {
                strength = 1 - ((distance - gravityRange) / falloffRange);
                return -normal * gravityStrength;
            }
            else
            {
                strength = 1;
                return -normal * gravityStrength;
            }
        }
        else
        {
            return Vector3.zero;
        }
    }

    public override Vector3 GravityVector(Vector3 point)
    {
        Vector3 normal = Vector3.zero;
        ColliderCalculations.ClosestPointOnSurface(gravityCollider, point, 0, ref normal);
        return -normal * gravityStrength;
    }

    //use these if position or scale is changing
    public void UpdateActivationRadius()
    {
        activationRadius = Vector3.Distance(gravityCollider.bounds.max - gravityCollider.bounds.center, Vector3.zero) + gravityRange + falloffRange;
    }

}

