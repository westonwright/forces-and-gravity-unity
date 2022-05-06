# Custom Forces and Gravity for Unity

This package gives you the ability to apply custom forces and gravity
to objects in your scene based on arbitrary meshes, zones, and points.

## Installation
To install this package, add [package](https://github.com/westonwright/forces-and-gravity-unity.git) from git URL in Unity's package manager

### 

## Features
### Force Producers
*Force Producers* calculate the force applied to a point in space depending on the type of producer.  
Producers can individually apply a different type of force (Force, Acceleration, Impulse, VelocityChange, etc.)  
and can use additive or weighted blending to mix with other producers (of any type) that are using the same type of force.

These producers have multiple shared variables:
- *Preview*
  > Displays a Gizmo for the producer being displayed and its falloff. Changes Color based on Force Type and if the force is enabled.

- *Enable Force*
  > Togglable enables or disables producers from applying forces.

- *Force Type*
  > Determines which force type to apply from this producer. Force type includes some unique types for use in custom scripting.

- *Layer Mask*
  > Which layers to apply forces to.

- *Force Strength*
  > The strength of the forces being applied.

- *Importance*
  > Used to order producers for mixing and overwriting. The order of importance is (low to high): 1 to Infinity, then -1 to -Infinity, the 0 at the lowest importance.

- *Additive*
  > Toggles if this producer mixes with other producers. If true, it will always apply its force instead of mixing with other producers.

- *Invert*
  > Toggles if the direction of force should be inverted. Can be used to push objects away instead of pulling them in.

- *Falloff Range*
  > Determines how far out falloff reaches. Falloff creates a smooth transition from full-strength to 0-strength from the producer based on distance.

Currently, there are 5 types of Producers:

#### Force Producer  
This is the base class of all other force producers. All it does is pull objects towards its origin from any distance.

![Force Surface Screenshot](https://drive.google.com/uc?export=view&id=1U76GsJSwFsdjoPuv7OhXRCz8E9zZTsl7) 

#### Force Surface  
Creates forces based on a mesh from a mesh collider or any other default collider shape. Force is determined by the distance from the surface and normal data from the mesh/collider.

![Force Surface Screenshot](https://drive.google.com/uc?export=view&id=1U76GsJSwFsdjoPuv7OhXRCz8E9zZTsl7) 

Unique Variables:  
- *Force Range*
  > Determines how far from the surface of the mesh the full strength force will reach.
#### Force Zone
Creates a force in one direction within a bounding box.

![Force Zone Screenshot](https://drive.google.com/uc?export=view&id=1IGFyb28UckiMKOhU0cgOsxE7sJcpIBSA) 

Unique Variables:  
- *Force Direction*
  > The direction in which force will be applied within this zone. Normalized at runtime. 
#### Force Point
Does essentially the same this as Force Producer but with an inner radius and falloff. Creates spherical forces based on distance from a point in space. The inner range is determined by scale.

![Force Point Screenshot](https://drive.google.com/uc?export=view&id=1Kh_1rVQsLDGgmdnEX3hI-bGeKaMb61pc) 
#### Force Global
Creates a force in one direction for the entire scene.

![Force Global Screenshot](https://drive.google.com/uc?export=view&id=1MmDqgKgHV_Py-xWy5ZM2K9cii6MSDaCe) 

(very good gif)  

Unique Variables:  
- *Force Direction*
  > The direction in which force will be applied within this zone. Normalized at runtime.  

### Rigidbody Force Receiver
Apply a forces to a rigidbody from *Force Providers* in the scene.

Variables:
- *Enable Forces*
  > If this object can actively receive forces from producers.

- *Use Layer Mask*
  > If this object should only receive forces from producers with correct layer masks, or if it should receive forces regardless of layer mask.

## Setup
There are a few steps to get custom forces working:
1. Add one of the Force Producers to the scene. I'd recommend starting with a Force Global but be sure to read the Tips!
2. Add a Force Receiver to a rigidbody in the scene. (or use a custom script you've created)
3. Press play!

## Tips
If using a mesh for a **Force Surface**, it must be from a **mesh collider**, not from any other mesh component.

**Force Surfaces** can become very resource intensive with high-poly meshes. It is recommended to **use low-poly meshes** when possible. One option could be to use a low-poly mesh for the Force Surface and a high-poly mesh above it for collision.

**Force Surfaces** provide better results with meshes that have **smoothed normals**. If you are using the same mesh for collision as your display mesh and it doesn't have smoothed normals, it might be better to make two versions of the mesh with different normals.

Be careful of priority when using the **Force Global** provider. It is recommended that you **set the priority to 0** on global providers. Default priority for all providers is set to 1 (the highest option), which means it could overwrite all other colliders in the scene. 

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.

## Code Examples

### Creating Your Own Force Producer

```c#
// This Force Producer will create a force in the 
// direction of a given point from the world origin

// must inherit from ForceProducer!
public class ForceProducerExample : ForceProducer
{
    // you can declare any custom variables or methods
    // consider adding some Gizmos to make it more clear what your ForceProducer is doing
    
    // adding and removing this Force Producer to the ForceManagerSO is critical!
    // the base Force Producer class does this for us in OnEnable and OnDisable
    // if you override OnEnable or OnDisable you must add 
    // this Force Producer to the ForceManagerSO manually!
    // this is what that process looks like:
    /*
    protected override void OnEnable()
    {
        forceManagerSO = ForcesStaticMembers.forceManagerSO;
        if (forceManagerSO != null)
        {
            forceManagerSO.AddForceProducer(this);
        }
    }

    protected override void OnDisable()
    {
        if (forceManagerSO != null)
        {
            forceManagerSO.RemoveForceProducer(this);
        }
    }
    */

    // called from ForceManager
    // return force vector for provided point. Use whatever calculation you want
    // set strength for weighted mixin
    // if strength is 0, the force will have no effect
    // if strength is 1, the force will completely saturate the weighted mixing
    // in other force producers, strength is determined by falloff
    public override Vector3 ForceVector(Vector3 point, out float strength)
    {
        strength = 1;
        // this would just move the point further from the world origin
        return point.normalized * forceStrength;
    }

    // returns the full gravity vector regardless of if the point is in the collider or not
    // not currently called by anything
    // will usually be the same calculation as function above but without strength
    public override Vector3 ForceVector(Vector3 point)
    {
        return point.normalized * forceStrength;
    }
}
```

### Accessing Force Data From Your Own Script
```c#
// This Script will Output the force at its position to the Debug Log
public class ForceDetectorExample : MonoBehaviour
{
    // reads from
    // you MUST have this to get data
    // we will get it in Start/Awake
    private ForceManagerSO forceManagerSO;
    
    // Start is called before the first frame update
    private void Start()
    {
        // ForcesStaticMembers contains a refrence to the Scriptable Object we need!
        forceManagerSO = ForcesStaticMembers.forceManagerSO;
    }
    
    private void Update()
    {
        if(forceManagerSO != null)
        {
            // ForceManagerSO has multiple functions you can use to get information about
            // forces at a given point in the scene. Learn more in the Documentation.
            // this function returns an array of force vectors for all of the types of force
            Vector3[] forceVectors = forceManagerSO.GetTotalForcesAtPoint(rb.position, layer);
            
            // ForcesStaticMembers also stores the count for how many types of forces there are
            // we want to loop through for each type of force
            for (int i = 0; i < ForcesStaticMembers.forceTypeCount; i++)
            {
                // ForceType is an enum with the default 4 force modes (Force, Acceleration, Impulse, VelocityChange)
                // as well as two additional types (Gravity, Generic) for use in scripts like there.
                // Gravity ordinarily would act idential to Acceleration but we might need the distinction for some acitons
                switch ((ForceType)i)
                {
                    case ForceType.Force:
                        Debug.Log("Force Vector: " + forceVectors[i]);
                        break;
                    case ForceType.Acceleration:
                        Debug.Log("Acceleration Vector: " + forceVectors[i]);
                        break;
                    case ForceType.Impulse:
                        Debug.Log("Impulse Vector: " + forceVectors[i]);
                        break;
                    case ForceType.VelocityChang:
                        Debug.Log("Velocity Change Vector: " + forceVectors[i]);
                        break;
                    case ForceType.Gravity:
                        Debug.Log("Gravity Vector: " + forceVectors[i]);
                        break;
                    case ForceType.Generic:
                        Debug.Log("Generic Vector: " + forceVectors[i]);
                        break;
                }
            }
            Debug.Log()
        }
    }
}
```

## Roadmap
* Complete Documentation.
* Support for Capsule and Terrain collider previews

## License
[MIT](https://choosealicense.com/licenses/mit/)

## Links
[twitter](https://twitter.com/WestonWright_)
[github](https://github.com/westonwright)
