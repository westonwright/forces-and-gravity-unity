using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//placed on the mesh providing gravity
//needs a MeshKDTree if Collider is a mesh Collider
[RequireComponent(typeof(Collider))]
public class ForceCollider : ForceProducer
{
#if UNITY_EDITOR
    protected const string SUB_MENU = "Collider/";
    [UnityEditor.MenuItem(MENU_NAME + SUB_MENU + "Cube", false, 0)]
    static void InstantiateForceCube()
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Force Cube";
        go.AddComponent<ForceCollider>();
    }
    [UnityEditor.MenuItem(MENU_NAME + SUB_MENU + "Sphere", false, 1)]
    static void InstantiateForceSphere()
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "Force Sphere";
        go.AddComponent<ForceCollider>();
    }

    [UnityEditor.MenuItem(MENU_NAME + SUB_MENU + "Capsule", false, 2)]
    static void InstantiateForceCapsule()
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = "Force Capsule";
        go.AddComponent<ForceCollider>();
    }
    [UnityEditor.MenuItem(MENU_NAME + SUB_MENU + "Mesh", false, 3)]
    static void InstantiateForceMesh()
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Plane);
        go.name = "Force mesh";
        go.AddComponent<ForceCollider>();
    }
#endif

    [SerializeField]
    [Tooltip("How far the full-strength force extends out from the collider")]
    //how far out this plante's gravity reaches (not including falloff range)
    private float ForceRange = 5f;
    public float forceRange {
        get { return ForceRange; }
        set {
            if (isStatic) return;
            if(ForceRange != value)
            {
                needsUpdate = true;
                ForceRange = value;
            }
        }
    }

    private Collider AttachedCollider;
    public Collider attachedCollider
    {
        get { return AttachedCollider; }
    }

    // TODO: change this to checking bounds instead of radius
    private float InfluenceRadius = 0;
    public float influenceRadius
    {
        get { return InfluenceRadius; }
    }

    private ColliderCalculator colliderCalculator = null;

    private void OnDrawGizmos()
    {
        if (!preview) return;
        Collider attachedCollider = GetComponent<Collider>();

        Gizmos.color = forceType.previewColor;

        Gizmos.color = (additive ? Gizmos.color : Gizmos.color * ForcesStaticMembers.lightGray) * (enableForce ? 1 : .25f);
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

        // TODO: figure out visualization for if mesh is convex
        if (attachedCollider is MeshCollider)
        {
            Mesh rangeMesh = new Mesh();
            rangeMesh.vertices = (attachedCollider as MeshCollider).sharedMesh.vertices;
            rangeMesh.triangles = (attachedCollider as MeshCollider).sharedMesh.triangles;
            rangeMesh.normals = (attachedCollider as MeshCollider).sharedMesh.normals;
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
                falloffMesh.vertices = (attachedCollider as MeshCollider).sharedMesh.vertices;
                falloffMesh.triangles = (attachedCollider as MeshCollider).sharedMesh.triangles;
                falloffMesh.normals = (attachedCollider as MeshCollider).sharedMesh.normals;
                Gizmos.color = Gizmos.color * ForcesStaticMembers.semiTransparent; //makes falloff semi-transparent
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] += normals[i] * falloffRange;
                }
                falloffMesh.vertices = vertices;
                Gizmos.DrawWireMesh(falloffMesh);
            }

        }
        else if (attachedCollider is SphereCollider)
        {
            Gizmos.DrawWireSphere(ForcesStaticMembers.MultiplyVectors(transform.localScale, (attachedCollider as SphereCollider).center), (ForcesStaticMembers.VectorHighest(transform.localScale) * (attachedCollider as SphereCollider).radius) + forceRange);
            Gizmos.color = Gizmos.color * ForcesStaticMembers.semiTransparent; //makes falloff semi-transparent
            Gizmos.DrawWireSphere(ForcesStaticMembers.MultiplyVectors(transform.localScale, (attachedCollider as SphereCollider).center), (ForcesStaticMembers.VectorHighest(transform.localScale) * (attachedCollider as SphereCollider).radius) + forceRange + falloffRange);
        }
        else if (attachedCollider is BoxCollider)
        {
            Gizmos.DrawWireCube(ForcesStaticMembers.MultiplyVectors(transform.localScale, (attachedCollider as BoxCollider).center), ForcesStaticMembers.AddToVector(ForcesStaticMembers.MultiplyVectors(transform.localScale, (attachedCollider as BoxCollider).size), forceRange));
            Gizmos.color = Gizmos.color * ForcesStaticMembers.semiTransparent; //makes falloff semi-transparent
            Gizmos.DrawWireCube(ForcesStaticMembers.MultiplyVectors(transform.localScale, (attachedCollider as BoxCollider).center), ForcesStaticMembers.AddToVector(ForcesStaticMembers.MultiplyVectors(transform.localScale, (attachedCollider as BoxCollider).size), forceRange + falloffRange));
        }
        // TODO: fill these in from other shape gizmos
        else if (attachedCollider is CapsuleCollider)
        {
        }
        // TODO: figure out visualization for this
        else if (attachedCollider is TerrainCollider)
        {

        }
    }

    protected override void Reset()
    {
        base.Reset();

        ForceRange = 5f;
    }

    private void Awake()
    {
        AttachedCollider = GetComponent<Collider>();
        colliderCalculator = ColliderCalculator.SelectColliderCalculator(AttachedCollider);
    }

    public override Vector3 ForceVector(Vector3 point)
    {
        //Vector3 surfacePoint = ColliderCalculations.ClosestPointOnCollider(attachedCollider, point, ref normal);
        Vector3 surfacePoint = colliderCalculator.ClosestPoint(point, out Vector3 normal);
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

        //Vector3 normal = Vector3.zero;
        //Vector3 surfacePoint = ColliderCalculations.ClosestPointOnCollider(attachedCollider, point, ref normal);
        Vector3 surfacePoint = colliderCalculator.ClosestPoint(point, out Vector3 normal);

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

    public override bool PointInRange(Vector3 point)
    {
        float distance = Vector3.Distance(point, attachedCollider.bounds.center);
        if (distance <= InfluenceRadius)
        {
            return true;
        }
        return false;
    }

    // use these if position or scale is changing
    // or if falloff/force range changed
    public override void TryUpdateProducer()
    {
        if (transform.hasChanged || needsUpdate)
        {
            UpdateProducer();
            transform.hasChanged = false;
            needsUpdate = false;
        }
    }

    public override void UpdateProducer()
    {
        InfluenceRadius = Vector3.Distance(attachedCollider.bounds.max - attachedCollider.bounds.center, Vector3.zero) + forceRange + falloffRange;
    }
}

// with refrence from Roystan Ross: https://roystanross.wordpress.com/category/unity-character-controller-series/
// and posts here: https://forum.unity.com/threads/get-the-collision-points-in-physics-overlapsphere.395176/#post-2581349
public class ColliderCalculator
{
    protected Collider collider;

    public static ColliderCalculator SelectColliderCalculator(Collider collider)
    {
        if (collider is TerrainCollider) return new TerrainColliderCalculator(collider);
        else if (collider is MeshCollider)
            if (!((MeshCollider)collider).convex) return new ConcaveColliderCalculator(collider);
        return new ColliderCalculator(collider);
    }
    public virtual Vector3 ClosestPoint(Vector3 to)
    {
        return collider.ClosestPoint(to);
    }
    public virtual Vector3 ClosestPoint(Vector3 to, out Vector3 normal)
    {
        Vector3 point = ClosestPoint(to);
        normal = (to - point).normalized;
        return point;
    }

    public ColliderCalculator(Collider collider)
    {
        this.collider = collider;
    }
}
public class TerrainColliderCalculator : ColliderCalculator
{
    public TerrainColliderCalculator(Collider collider) : base(collider)
    {
    }

    public override Vector3 ClosestPoint(Vector3 to)
    {
        // currently always set to zero
        float radius = 0;

        var terrainData = ((TerrainCollider)collider).terrainData;

        var local = ((TerrainCollider)collider).transform.InverseTransformPoint(to);

        // Calculate the size of each tile on the terrain horizontally and vertically
        float pixelSizeX = terrainData.size.x / (terrainData.heightmapResolution - 1);
        float pixelSizeZ = terrainData.size.z / (terrainData.heightmapResolution - 1);

        var percentZ = Mathf.Clamp01(local.z / terrainData.size.z);
        var percentX = Mathf.Clamp01(local.x / terrainData.size.x);

        float positionX = percentX * (terrainData.heightmapResolution - 1);
        float positionZ = percentZ * (terrainData.heightmapResolution - 1);

        // Calculate our position, in tiles, on the terrain
        int pixelX = Mathf.FloorToInt(positionX);
        int pixelZ = Mathf.FloorToInt(positionZ);

        // Calculate the distance from our point to the edge of the tile we are in
        float distanceX = (positionX - pixelX) * pixelSizeX;
        float distanceZ = (positionZ - pixelZ) * pixelSizeZ;

        // Find out how many tiles we are overlapping on the X plane
        float radiusExtentsLeftX = radius - distanceX;
        float radiusExtentsRightX = radius - (pixelSizeX - distanceX);

        int overlappedTilesXLeft = radiusExtentsLeftX > 0 ? Mathf.FloorToInt(radiusExtentsLeftX / pixelSizeX) + 1 : 0;
        int overlappedTilesXRight = radiusExtentsRightX > 0 ? Mathf.FloorToInt(radiusExtentsRightX / pixelSizeX) + 1 : 0;

        // Find out how many tiles we are overlapping on the Z plane
        float radiusExtentsLeftZ = radius - distanceZ;
        float radiusExtentsRightZ = radius - (pixelSizeZ - distanceZ);

        int overlappedTilesZLeft = radiusExtentsLeftZ > 0 ? Mathf.FloorToInt(radiusExtentsLeftZ / pixelSizeZ) + 1 : 0;
        int overlappedTilesZRight = radiusExtentsRightZ > 0 ? Mathf.FloorToInt(radiusExtentsRightZ / pixelSizeZ) + 1 : 0;

        // Retrieve the heights of the pixels we are testing against
        int startPositionX = pixelX - overlappedTilesXLeft;
        int startPositionZ = pixelZ - overlappedTilesZLeft;

        int numberOfXPixels = overlappedTilesXRight + overlappedTilesXLeft + 1;
        int numberOfZPixels = overlappedTilesZRight + overlappedTilesZLeft + 1;

        // Account for if we are off the terrain
        if (startPositionX < 0)
        {
            numberOfXPixels -= Mathf.Abs(startPositionX);
            startPositionX = 0;
        }

        if (startPositionZ < 0)
        {
            numberOfZPixels -= Mathf.Abs(startPositionZ);
            startPositionZ = 0;
        }

        if (startPositionX + numberOfXPixels + 1 > terrainData.heightmapResolution)
        {
            numberOfXPixels = terrainData.heightmapResolution - startPositionX - 1;
        }

        if (startPositionZ + numberOfZPixels + 1 > terrainData.heightmapResolution)
        {
            numberOfZPixels = terrainData.heightmapResolution - startPositionZ - 1;
        }

        // Retrieve the heights of the tile we are in and all overlapped tiles
        var heights = terrainData.GetHeights(startPositionX, startPositionZ, numberOfXPixels + 1, numberOfZPixels + 1);

        // Pre-scale the heights data to be world-scale instead of 0...1
        for (int i = 0; i < numberOfXPixels + 1; i++)
        {
            for (int j = 0; j < numberOfZPixels + 1; j++)
            {
                heights[j, i] *= terrainData.size.y;
            }
        }

        // Find the shortest distance to any triangle in the set gathered
        float shortestDistance = float.MaxValue;

        Vector3 shortestPoint = Vector3.zero;

        for (int x = 0; x < numberOfXPixels; x++)
        {
            for (int z = 0; z < numberOfZPixels; z++)
            {
                // Build the set of points that creates the two triangles that form this tile
                Vector3 a = new Vector3((startPositionX + x) * pixelSizeX, heights[z, x], (startPositionZ + z) * pixelSizeZ);
                Vector3 b = new Vector3((startPositionX + x + 1) * pixelSizeX, heights[z, x + 1], (startPositionZ + z) * pixelSizeZ);
                Vector3 c = new Vector3((startPositionX + x) * pixelSizeX, heights[z + 1, x], (startPositionZ + z + 1) * pixelSizeZ);
                Vector3 d = new Vector3((startPositionX + x + 1) * pixelSizeX, heights[z + 1, x + 1], (startPositionZ + z + 1) * pixelSizeZ);

                Vector3 nearest;

                ForcesStaticMembers.ClosestPointOnTriangleToPoint(ref a, ref d, ref c, ref local, out nearest);

                float distance = (local - nearest).sqrMagnitude;

                if (distance <= shortestDistance)
                {
                    shortestDistance = distance;
                    shortestPoint = nearest;
                }

                // check if calculating this twice is necessary
                ForcesStaticMembers.ClosestPointOnTriangleToPoint(ref a, ref b, ref d, ref local, out nearest);

                distance = (local - nearest).sqrMagnitude;

                if (distance <= shortestDistance)
                {
                    shortestDistance = distance;
                    shortestPoint = nearest;
                }

                /*
                if (debug)
                {
                    DebugDraw.DrawTriangle(a, d, c, Color.cyan);
                    DebugDraw.DrawTriangle(a, b, d, Color.red);
                }
                */
            }
        }

        return collider.transform.TransformPoint(shortestPoint);
    }
    public override Vector3 ClosestPoint(Vector3 to, out Vector3 normal)
    {
        // come up with better solution for this
        normal = Vector3.up;
        return ClosestPoint(to);
    }
}
public class ConcaveColliderCalculator : ColliderCalculator
{
    private MeshKDTree meshKDTree;
    public ConcaveColliderCalculator(Collider collider) : base(collider)
    {
        meshKDTree = new MeshKDTree(((MeshCollider)collider).sharedMesh, collider.transform);
    }
    public override Vector3 ClosestPoint(Vector3 to)
    {
        //runs if the MeshKDTree exists, meaning the KDTree has baked data
        return meshKDTree.ClosestPointOnSurface(to);
    }
    public override Vector3 ClosestPoint(Vector3 to, out Vector3 normal)
    {
        return meshKDTree.ClosestPointOnSurface(to, out normal);
    }
}

