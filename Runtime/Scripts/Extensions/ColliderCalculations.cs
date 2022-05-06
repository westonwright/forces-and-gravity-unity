using UnityEngine;
using System.Collections;
using System.Collections.Generic;
// with refrence from Roystan Ross: https://roystanross.wordpress.com/category/unity-character-controller-series/
// and posts here: https://forum.unity.com/threads/get-the-collision-points-in-physics-overlapsphere.395176/#post-2581349


//calculates the closest point on the surface for various colliders
public static class ColliderCalculations
{
    public static Vector3 ClosestPointOnSurface(Collider collider, Vector3 to, float radius, ref Vector3 normal)
    {
        if (collider is BoxCollider)
        {
            return ColliderCalculations.ClosestPointOnSurface((BoxCollider)collider, to, ref normal);
        }
        else if (collider is SphereCollider)
        {
            return ColliderCalculations.ClosestPointOnSurface((SphereCollider)collider, to, ref normal);
        }
        else if (collider is CapsuleCollider)
        {
            return ColliderCalculations.ClosestPointOnSurface((CapsuleCollider)collider, to, ref normal);
        }
        else if (collider is MeshCollider)
        {
            return ColliderCalculations.ClosestPointOnSurface((MeshCollider)collider, to, ref normal);
        }
        else if (collider is TerrainCollider)
        {
            //return SuperCollider.ClosestPointOnSurface((TerrainCollider)collider, to, radius, ref normal, false);
            return ColliderCalculations.ClosestPointOnSurface((TerrainCollider)collider, to, radius, ref normal);
        }

        return Vector3.zero;
    }

    public static Vector3 ClosestPointOnSurface(SphereCollider collider, Vector3 to, ref Vector3 normal)
    {
        Vector3 p;

        p = to - collider.transform.position;
        p.Normalize();
        
        //set normal for gravity
        normal = p;

        p *= collider.radius * collider.transform.localScale.x;
        p += collider.transform.position;

        //Debug.DrawRay(p, normal, Color.red);

        return p;
    }

    public static Vector3 ClosestPointOnSurface(BoxCollider collider, Vector3 to, ref Vector3 normal)
    {
        // Cache the collider transform
        var ct = collider.transform;

        // Firstly, transform the point into the space of the collider
        var local = ct.InverseTransformPoint(to);

        // Now, shift it to be in the center of the box
        local -= collider.center;

        //Pre multiply to save operations.
        var halfSize = collider.size * 0.5f;

        // Clamp the points to the collider's extents
        var localNorm = new Vector3(
                Mathf.Clamp(local.x, -halfSize.x, halfSize.x),
                Mathf.Clamp(local.y, -halfSize.y, halfSize.y),
                Mathf.Clamp(local.z, -halfSize.z, halfSize.z)
            );

        //Calculate distances from each edge
        var dx = Mathf.Min(Mathf.Abs(halfSize.x - localNorm.x), Mathf.Abs(-halfSize.x - localNorm.x));
        var dy = Mathf.Min(Mathf.Abs(halfSize.y - localNorm.y), Mathf.Abs(-halfSize.y - localNorm.y));
        var dz = Mathf.Min(Mathf.Abs(halfSize.z - localNorm.z), Mathf.Abs(-halfSize.z - localNorm.z));

        // Select a face to project on
        if (dx < dy && dx < dz)
        {
            localNorm.x = Mathf.Sign(localNorm.x) * halfSize.x;
        }
        else if (dy < dx && dy < dz)
        {
            localNorm.y = Mathf.Sign(localNorm.y) * halfSize.y;
        }
        else if (dz < dx && dz < dy)
        {
            localNorm.z = Mathf.Sign(localNorm.z) * halfSize.z;
        }

        //set normal for gravity
        //this could prob use some work
        normal = localNorm;
        if((Mathf.Abs(normal.x) > Mathf.Abs(normal.y)) &&
            (Mathf.Abs(normal.x) > Mathf.Abs(normal.z)))
        {
            normal = new Vector3(normal.x, 0, 0);
        }
        else if((Mathf.Abs(normal.y) > Mathf.Abs(normal.x)) &&
            (Mathf.Abs(normal.y) > Mathf.Abs(normal.z)))
        {
            normal = new Vector3(0, normal.y, 0);
        }
        else
        {
            normal = new Vector3(0, 0, normal.z);
        }
        normal = normal.normalized;
        normal = ct.TransformDirection(normal);

        // Now we undo our transformations
        localNorm += collider.center;

        //Debug.DrawRay(ct.TransformPoint(localNorm), normal, Color.red);

        // Return resulting point
        return ct.TransformPoint(localNorm);
    }

    // Courtesy of Moodie
    public static Vector3 ClosestPointOnSurface(CapsuleCollider collider, Vector3 to, ref Vector3 normal)
    {
        Transform ct = collider.transform; // Transform of the collider

        float lineLength = collider.height - collider.radius * 2; // The length of the line connecting the center of both sphere
        Vector3 dir = Vector3.up;

        Vector3 upperSphere = dir * lineLength * 0.5f + collider.center; // The position of the radius of the upper sphere in local coordinates
        Vector3 lowerSphere = -dir * lineLength * 0.5f + collider.center; // The position of the radius of the lower sphere in local coordinates

        Vector3 local = ct.InverseTransformPoint(to); // The position of the controller in local coordinates

        Vector3 p = Vector3.zero; // Contact point
        Vector3 pt = Vector3.zero; // The point we need to use to get a direction vector with the controller to calculate contact point

        if (local.y < lineLength * 0.5f && local.y > -lineLength * 0.5f) // Controller is contacting with cylinder, not spheres
        {
            pt = dir * local.y + collider.center;
            normal = dir * local.y;
        }
        else if (local.y > lineLength * 0.5f) // Controller is contacting with the upper sphere
        {
            pt = upperSphere;
            normal = dir * lineLength * 0.5f;
        }
        else if (local.y < -lineLength * 0.5f)// Controller is contacting with lower sphere
        { 

            pt = lowerSphere;
            normal = -dir * lineLength * 0.5f;
        }

        //set normal for gravity
        normal = ct.TransformDirection((local - normal).normalized);

        //Calculate contact point in local coordinates and return it in world coordinates
        p = local - pt;
        p.Normalize();
        p = p * collider.radius + pt;

        //Debug.DrawRay(ct.TransformPoint(p), normal, Color.red);

        return ct.TransformPoint(p);
    }
    
    public static Vector3 ClosestPointOnSurface(MeshCollider collider, Vector3 to, ref Vector3 normal)
    {
        MeshKDTree meshKDTree = collider.GetComponent<MeshKDTree>();

        //runs if the MeshKDTree exists, meaning the KDTree has baked data
        if (meshKDTree != null)
        {
            return meshKDTree.ClosestPointOnSurface(to, ref normal);
        }
        //runs if the MeshKDTree does not exist and must calculate data each frame
        else
        {
            Mesh mesh = collider.sharedMesh;
            int[] tris = mesh.triangles;
            Vector3[] verts = mesh.vertices;
            Vector3[] norms = mesh.normals;
            KDTree kd = KDTree.MakeFromPoints(verts);
            VertTriList vt = new VertTriList(mesh);
            Transform ct = collider.transform;
            
            to = ct.InverseTransformPoint(to);

            //First, find the nearest vertex (the nearest point must be on one of the triangles
            //that uses this vertex if the mesh is convex).
            //Since there can be multiple vertices on a single spot, we need to find the correct vert and triangle.
            List<int> nearests = new List<int>();

            kd.FindNearestEpsilon(to, nearests);

            Vector3 nearestPt = Vector3.zero;
            float nearestSqDist = 100000000f;
            Vector3 possNearestPt;

            Vector3 A = Vector3.zero;
            Vector3 B = Vector3.zero;
            Vector3 C = Vector3.zero;
            int T = 0;
            for (int i = 0; i < nearests.Count; i++)
            {
                //    Get the list of triangles in which the nearest vert "participates".
                int[] nearTris = vt[nearests[i]];

                for (int j = 0; j < nearTris.Length; j++)
                {
                    int triOff = nearTris[j] * 3;
                    Vector3 a = verts[tris[triOff]];
                    Vector3 b = verts[tris[triOff + 1]];
                    Vector3 c = verts[tris[triOff + 2]];

                    ForcesStaticMembers.ClosestPointOnTriangleToPoint(ref to, ref a, ref b, ref c, out possNearestPt);
                    float possNearestSqDist = (to - possNearestPt).sqrMagnitude;

                    if (possNearestSqDist < nearestSqDist)
                    {
                        A = a;
                        B = b;
                        C = c;
                        T = triOff;
                        
                        nearestPt = possNearestPt;
                        nearestSqDist = possNearestSqDist;
                    }
                }
            }
            //set normal vector for gravity
            normal = ForcesStaticMembers.SmoothedNormalVector(nearestPt, A, B, C, norms[tris[T]], norms[tris[T + 1]], norms[tris[T + 2]], ct);

            return ct.TransformPoint(nearestPt);
        }
    }

    public static Vector3 ClosestPointOnSurface(TerrainCollider collider, Vector3 to, float radius, ref Vector3 normal)//, bool debug = false)
    {
        var terrainData = collider.terrainData;

        var local = collider.transform.InverseTransformPoint(to);

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

        //set normal for gravity
        //need to come up with actual solution for this
        normal = Vector3.up;

        return collider.transform.TransformPoint(shortestPoint);
    }
}