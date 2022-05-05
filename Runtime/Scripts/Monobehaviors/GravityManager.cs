using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityManager : MonoBehaviour
{
    // sort gravity attractors by importance to made detection faster
    private List<GravitySource> sources = new List<GravitySource>();
    private List<ApplyGravityObject> applyObjects = new List<ApplyGravityObject>();
    // how strong this planet's gravity is
    [SerializeField]
    [Tooltip("The direction that will be used for gravity if there aren't any Gravity Sources contributing")]
    private Vector3 defaultGravityDirection = Vector3.down;
    [SerializeField]
    [Tooltip("The strength that will be used for gravity if there aren't any Gravity Sources contributing")]
    private float defaultGravityStrength = 9.8f;
    // if set to true, gravity manager will find all gravity sources in the scene when gameplay starts
    // should probably be set to false most of the time
    //[SerializeField]
    //[Tooltip("If set to true, Gravity Manager will find all gravity sources in the scene when when gameplay starts.")]
    //private bool initializeSelf = false; 

    // listens to
    private GravitySourceEventsChannelSO gravitySourceEventsChannel;
    private ApplyGravityEventsChannelSO applyGravityEventsChannel;

    private void Awake()
    {
        gravitySourceEventsChannel = CustomGravityHelperFunctions._gravitySourceEventsChannel;
        applyGravityEventsChannel = CustomGravityHelperFunctions._applyGravityEventsChannel;
        defaultGravityDirection = defaultGravityDirection.normalized;
        /*
        if (initializeSelf)
        {
            ClearGravitySources();
            FindGravitySources();
        }
        */
    }

    /*
    public void ClearGravitySources()
    {
        sources.Clear();
    }

    public void FindGravitySources()
    {
        foreach (GravitySource source in FindObjectsOfType<GravitySource>())
        {
            if (!sources.Contains(source))
            {
                sources.Add(source);
            }
        }
        SortSources();
    }
    */

    private void OnEnable()
    {
        if (gravitySourceEventsChannel != null)
        {
            gravitySourceEventsChannel.OnAddGravitySourceRequested += AddGravitySource;
            gravitySourceEventsChannel.OnRemoveGravitySourceRequested += RemoveGravitySource;
        }
        
        if (applyGravityEventsChannel != null)
        {
            applyGravityEventsChannel.OnAddApplyObjectRequested += AddApplyGravityObject;
            applyGravityEventsChannel.OnRemoveApplyObjectRequested += RemoveApplyGravityObject;
        }
    }

    private void OnDisable()
    {
        if (gravitySourceEventsChannel != null)
        {
            gravitySourceEventsChannel.OnAddGravitySourceRequested -= AddGravitySource;
            gravitySourceEventsChannel.OnRemoveGravitySourceRequested -= RemoveGravitySource;
        }

        if (applyGravityEventsChannel != null)
        {
            applyGravityEventsChannel.OnAddApplyObjectRequested -= AddApplyGravityObject;
            applyGravityEventsChannel.OnRemoveApplyObjectRequested -= RemoveApplyGravityObject;
        }
    }

    private void FixedUpdate()
    {
        foreach (ApplyGravityObject applyObject in applyObjects)
        {
            applyObject.ApplyGravity(GetTotalVectorAtPoint(applyObject.GetPoint()));
        }
    }

    // use this from gravity attractors to add them in as they load with a scene
    private void AddGravitySource(GravitySource source)
    {
        if (!sources.Contains(source))
        {
            sources.Add(source);
        }
        // sort the sources every time one is added
        SortSources();
    }

    // use this from gravity attractors to remove them in as they unload with a scene
    private void RemoveGravitySource(GravitySource source)
    {
        sources.Remove(source);
    }
    
    // use this from gravity objects to add them in as they load with a scene
    private void AddApplyGravityObject(ApplyGravityObject applyObject)
    {
        if (!applyObjects.Contains(applyObject))
        {
            applyObjects.Add(applyObject);
        }
    }

    // use this from gravity attractors to remove them in as they unload with a scene
    private void RemoveApplyGravityObject(ApplyGravityObject applyObject)
    {
        applyObjects.Remove(applyObject);
    }

    // sorts source importance from low to high, with 0 at the end
    private void SortSources()
    {
        if(sources.Count > 0)
        {
            sources.Sort((x, y) => x.importance.CompareTo(y.importance));
            //moves all importance 0's to the end of the list
            if (sources[sources.Count - 1].importance > 0)
            {
                while (sources[0].importance <= 0)
                {
                    GravitySource s = sources[0];
                    sources.RemoveAt(0);
                    sources.Add(s);
                }

            }
        }
    }

    // gravity vector is gravity direction multiplied by gravity strength
    // calculates the currecnt gravity vector between default gravity, gravity attractors, and gravity zones
    // order of importance = gravity attractor, gravity zone, default gravity

    // gravity vector is only for non-additive sources and will mix based on falloff and importance
    public Vector3 GetGravityVectorAtPoint(Vector3 point)
    {
        Vector3 gravityVector = Vector3.zero;
        float alpha = 0;

        foreach(GravitySource source in sources)
        {
            //only calculate if gravity isn't at 100% and not additive
            if (!source.additive && alpha < 1)
            {
                Vector3 vector = source.GravityVector(point, out float strength);
                vector *= source.invert ? -1 : 1;
                if (strength > 0)
                {
                    CustomGravityHelperFunctions.GravityCompositeStraight(alpha, strength, gravityVector, vector, out alpha, out gravityVector);
                }
            }
        }
        if(alpha < 1)
        {
            // add in default gravity
            CustomGravityHelperFunctions.GravityCompositeStraight(alpha, 1, gravityVector, defaultGravityDirection * defaultGravityStrength, out alpha, out gravityVector);
        }

        return gravityVector;
    }

    // force vector is only for additive sources
    public Vector3 GetForceVectorAtPoint(Vector3 point)
    {
        Vector3 forceVector = Vector3.zero;

        foreach (GravitySource source in sources)
        {
            // only calculate if gravity isn't at 100% and not additive
            if (source.additive)
            {
                Vector3 vector = source.GravityVector(point, out float strength);
                vector *= source.invert ? -1 : 1;
                if (strength > 0)
                {
                    forceVector += vector * strength;
                }
            }
        }

        return forceVector;
    }

    // total vector is a combination of non-additive and additive sources together
    public Vector3 GetTotalVectorAtPoint(Vector3 point)
    {
        Vector3 gravityVector = Vector3.zero;
        Vector3 forceVector = Vector3.zero;
        float alpha = 0;

        foreach (GravitySource source in sources)
        {
            // only calculate if gravity isn't at 100% and not additive
            if (!source.additive && alpha < 1)
            {
                Vector3 vector = source.GravityVector(point, out float strength);
                vector *= source.invert ? -1 : 1;
                if (strength > 0)
                {
                    CustomGravityHelperFunctions.GravityCompositeStraight(alpha, strength, gravityVector, vector, out alpha, out gravityVector);
                }
            }
            else if (source.additive)
            {
                Vector3 vector = source.GravityVector(point, out float strength);
                vector *= source.invert ? -1 : 1;
                if (strength > 0)
                {
                    forceVector += vector * strength;
                }
            }
        }
        if (alpha < 1)
        {
            // add in default gravity
            CustomGravityHelperFunctions.GravityCompositeStraight(alpha, 1, gravityVector, defaultGravityDirection * defaultGravityStrength, out alpha, out gravityVector);
        }

        return (gravityVector + forceVector);
    }
}
