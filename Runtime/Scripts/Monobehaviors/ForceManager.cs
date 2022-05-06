using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ForceManager : MonoBehaviour
{
    private List<ForceProducer> producers = new List<ForceProducer>();
    private List<ForceReceiver> receivers = new List<ForceReceiver>();

    // listens to
    private ForceProducerEventsChannelSO forceProducerEventsChannel;
    private ForceReceiverEventsChannelSO forceReceiverEventsChannel;

    private void Awake()
    {
        forceProducerEventsChannel = ForcesStaticMembers.forceProducerEventsChannel;
        forceReceiverEventsChannel = ForcesStaticMembers.forceReceiverEventsChannel;
        //defaultGravityDirection = defaultGravityDirection.normalized;
    }

    private void OnEnable()
    {
        if (forceProducerEventsChannel != null)
        {
            forceProducerEventsChannel.OnAddForceProducerRequested += AddForceProducer;
            forceProducerEventsChannel.OnRemoveForceProducerRequested += RemoveForceProducer;
        }
        
        if (forceReceiverEventsChannel != null)
        {
            forceReceiverEventsChannel.OnAddForceReceiverRequested += AddForceReceiver;
            forceReceiverEventsChannel.OnRemoveForceReceiverRequested += RemoveForceReceiver;
        }
    }

    private void OnDisable()
    {
        if (forceProducerEventsChannel != null)
        {
            forceProducerEventsChannel.OnAddForceProducerRequested -= AddForceProducer;
            forceProducerEventsChannel.OnRemoveForceProducerRequested -= RemoveForceProducer;
        }

        if (forceReceiverEventsChannel != null)
        {
            forceReceiverEventsChannel.OnAddForceReceiverRequested -= AddForceReceiver;
            forceReceiverEventsChannel.OnRemoveForceReceiverRequested -= RemoveForceReceiver;
        }
    }

    private void FixedUpdate()
    {
        foreach (ForceReceiver receiver in receivers)
        {
            if (receiver.enableForces)
            {
                int? layer = null;
                if (receiver.useLayerMask) layer = receiver.gameObject.layer;


                Vector3[] forceVectors = GetTotalForcesAtPoint(receiver.GetPosition(), layer);
                for (int i = 0; i < 6; i++)
                {
                    receiver.ApplyForce(forceVectors[i], (ForceType)i);
                }
            }
        }
    }

    // use this from gravity attractors to add them in as they load with a scene
    private void AddForceProducer(ForceProducer producer)
    {
        if (!producers.Contains(producer))
        {
            producers.Add(producer);
        }
        // sort the sources every time one is added
        SortProducers();
    }

    // use this from gravity attractors to remove them in as they unload with a scene
    private void RemoveForceProducer(ForceProducer producer)
    {
        producers.Remove(producer);
    }
    
    // use this from gravity objects to add them in as they load with a scene
    private void AddForceReceiver(ForceReceiver receiver)
    {
        if (!receivers.Contains(receiver))
        {
            receivers.Add(receiver);
        }
    }

    // use this from gravity attractors to remove them in as they unload with a scene
    private void RemoveForceReceiver(ForceReceiver receiver)
    {
        receivers.Remove(receiver);
    }

    // sorts source importance from low to high, with 0 at the end
    private void SortProducers()
    {
        if(producers.Count > 0)
        {
            List<ForceProducer> positives = new List<ForceProducer>();
            List<ForceProducer> negatives = new List<ForceProducer>(); 
            List<ForceProducer> zeroes = new List<ForceProducer>();

            foreach (ForceProducer producer in producers)
            {
                if(producer.importance > 0)
                {
                    positives.Add(producer);
                }
                else if(producer.importance < 0)
                {
                    negatives.Add(producer);
                }
                else
                {
                    zeroes.Add(producer);
                }
            }
            producers.Clear();
            negatives.Sort((x, y) => x.importance.CompareTo(y.importance));
            positives.Sort((x, y) => x.importance.CompareTo(y.importance));
            for (int i = 0; i < positives.Count; i++)
            {
                producers.Add(positives[i]);
            }
            
            for (int i = negatives.Count - 1; i >= 0; i--)
            {
                producers.Add(negatives[i]);
            }

            foreach (ForceProducer zero in zeroes)
            {
                producers.Add(zero);
            }

            positives.Clear();
            negatives.Clear();
            zeroes.Clear();
        }
    }

    // force vector is force direction multiplied by force strength
    // calculates the currecnt force vector between default gravity, force surfaces, force point, and force zones

    // force vector is only for non-additive sources and will mix based on falloff and importance
    /// <summary>
    /// Returns an array with the forces for each force type at a given position, weighted by importance and falloff. Force, Acceleration, Impulse, VelocityChange, Gravity, and Generic.
    /// </summary>
    /// <param name="point">The point in World Space to use check against all Force Producers</param>
    /// <returns></returns>
    public Vector3[] GetWeightedForcesAtPoint(Vector3 point, int? layer = null)
    {

        Vector3[] forceVectors = new Vector3[ForcesStaticMembers.forceTypeCount];
        for (int i = 0; i < ForcesStaticMembers.forceTypeCount; i++)
        {
            forceVectors[i] = GetWeightedForceTypeAtPoint(point, (ForceType)i, layer);
        }

        return forceVectors;
    }

    /// <summary>
    /// returns the force of a specific type at a given position, weighted by importance and falloff.
    /// </summary>
    /// <param name="point">The point in World Space to use check against all Force Producers</param>
    /// <param name="forceType">The type of force to check for</param>
    /// <param name="layer">The layer to check against</param>
    /// <returns></returns>
    public Vector3 GetWeightedForceTypeAtPoint(Vector3 point, ForceType forceType, int? layer = null)
    {
        Vector3 forceVector = Vector3.zero;
        float alpha = 0;

        foreach(ForceProducer producer in producers)
        {
            if (!producer.enableForce || (producer.forceType != forceType)) continue;
            // if layer is set, check against layer mask
            if (layer.HasValue ? !(producer.layerMask == (producer.layerMask | (1 << layer.Value))) : false) continue;


            //only calculate if gravity isn't at 100% and not additive
            if (!producer.additive && alpha < 1)
            {
                Vector3 vector = producer.ForceVector(point, out float strength);
                vector *= producer.invert ? -1 : 1;
                if (strength > 0)
                {
                    ForcesStaticMembers.VectorCompositeStraight(alpha, strength, forceVector, vector, out alpha, out forceVector);
                }
            }
        }
        if (alpha < 1)
        {
            // add in default gravity
            ForcesStaticMembers.VectorCompositeStraight(alpha, 1, forceVector, Vector3.zero, out alpha, out forceVector);
        }

        return forceVector;
    }

    /// <summary>
    /// Returns an array with the forces for each force type at a given position, combined additively. Force, Acceleration, Impulse, VelocityChange, Gravity, and Generic.
    /// </summary>
    /// <param name="point">The point in World Space to use check against all Force Producers</param>
    /// <returns></returns>
    public Vector3[] GetAdditiveForcesAtPoint(Vector3 point, int? layer = null)
    {
        Vector3[] forceVectors = new Vector3[ForcesStaticMembers.forceTypeCount];
        for (int i = 0; i < ForcesStaticMembers.forceTypeCount; i++)
        {
            forceVectors[i] = GetAdditiveForceTypeAtPoint(point, (ForceType)i, layer);
        }

        return forceVectors;
    }

    // force vector is only for additive producers
    /// <summary>
    /// returns the force of a specific type at a given position, combined additively.
    /// </summary>
    /// <param name="point">The point in World Space to use check against all Force Producers</param>
    /// <param name="forceType">The type of force to check for</param>
    /// <returns></returns>
    public Vector3 GetAdditiveForceTypeAtPoint(Vector3 point, ForceType forceType, int? layer = null)
    {
        Vector3 forceVector = Vector3.zero;

        foreach (ForceProducer producer in producers)
        {
            if (!producer.enableForce || (producer.forceType != forceType)) continue;
            // if layer is set, check against layer mask
            if (layer.HasValue ? !(producer.layerMask == (producer.layerMask | (1 << layer.Value))) : false) continue;


            // only calculate if gravity isn't at 100% and not additive
            if (producer.additive)
            {
                Vector3 vector = producer.ForceVector(point, out float strength);
                vector *= producer.invert ? -1 : 1;
                if (strength > 0)
                {
                    forceVector += vector * strength;
                }
            }
        }

        return forceVector;
    }

    /// <summary>
    /// Returns an array with the forces for each force type at a given position, combineding additive and weighted results. Force, Acceleration, Impulse, VelocityChange, Gravity, and Generic.
    /// </summary>
    /// <param name="point">The point in World Space to use check against all Force Producers</param>
    /// <returns></returns>
    public Vector3[] GetTotalForcesAtPoint(Vector3 point, int? layer = null)
    {
        Vector3[] forceVectors = new Vector3[ForcesStaticMembers.forceTypeCount];
        for (int i = 0; i < ForcesStaticMembers.forceTypeCount; i++)
        {
            forceVectors[i] = GetTotalForceTypeAtPoint(point, (ForceType)i, layer);
        }

        return forceVectors;
    }

    /// <summary>
    /// returns the force of a specific type at a given position, combineding additive and weighted results.
    /// </summary>
    /// <param name="point">The point in World Space to use check against all Force Producers</param>
    /// <param name="forceType">The type of force to check for</param>
    /// <returns></returns>
    public Vector3 GetTotalForceTypeAtPoint(Vector3 point, ForceType forceType, int? layer = null)
    {
        Vector3 weightecVector = Vector3.zero;
        Vector3 additiveVector = Vector3.zero;
        float alpha = 0;

        foreach (ForceProducer producer in producers)
        {
            if (!producer.enableForce || (producer.forceType != forceType)) continue;
            // if layer is set, check against layer mask
            if (layer.HasValue ? !(producer.layerMask == (producer.layerMask | (1 << layer.Value))) : false) continue;

            // only calculate if gravity isn't at 100% and not additive
            if (!producer.additive && alpha < 1)
            {
                Vector3 vector = producer.ForceVector(point, out float strength);
                vector *= producer.invert ? -1 : 1;
                if (strength > 0)
                {
                    ForcesStaticMembers.VectorCompositeStraight(alpha, strength, weightecVector, vector, out alpha, out weightecVector);
                }
            }
            else if (producer.additive)
            {
                Vector3 vector = producer.ForceVector(point, out float strength);
                vector *= producer.invert ? -1 : 1;
                if (strength > 0)
                {
                    additiveVector += vector * strength;
                }
            }
        }
        if (alpha < 1)
        {
            // add in default gravity
            ForcesStaticMembers.VectorCompositeStraight(alpha, 1, weightecVector, Vector3.zero, out alpha, out weightecVector);
        }

        return (weightecVector + additiveVector);
    }
}
