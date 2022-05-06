using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ForceReceiver : MonoBehaviour
{
    [Tooltip("If this object should be affected by forces by default.")]
    public bool enableForces = true;
    [Tooltip("If this object should check against proveiders layer masks when applying forces")]
    public bool useLayerMask = true;

    protected Rigidbody rb;

    // broadcasts to
    private ForceReceiverEventsChannelSO forceReceiverEventsChannel;

    // Start is called before the first frame update
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        // turns off unity's implementation of gravity on the object, just in case
        rb.useGravity = false;
    }
    private void OnEnable()
    {
        forceReceiverEventsChannel = ForcesStaticMembers.forceReceiverEventsChannel;
        StartCoroutine(AddEventWait());
    }

    // needs to wait a frame before adding to channel because force manager might not have run first
    private IEnumerator AddEventWait()
    {
        yield return new WaitForEndOfFrame();
        if (forceReceiverEventsChannel != null && enableForces)
        {
            forceReceiverEventsChannel.RaiseAddEvent(this);
        }
    }

    //use to remove from gravity manager
    private void OnDisable()
    {
        if (forceReceiverEventsChannel != null && enableForces)
        {
            forceReceiverEventsChannel.RaiseRemoveEvent(this);
        }
    }

    public void EnableForces(bool enabled)
    {
        enableForces = enabled;
    }

    public virtual Vector3 GetPosition()
    {
        return rb.position;
    }

    public virtual void ApplyForce(Vector3 totalVector, ForceType forceType = ForceType.Force)
    {
        switch (forceType)
        {
            case ForceType.Force:
                rb.AddForce(totalVector, ForceMode.Force);
                break;
            case ForceType.Acceleration:
                rb.AddForce(totalVector, ForceMode.Acceleration);
                break;
            case ForceType.Impulse:
                rb.AddForce(totalVector, ForceMode.Impulse);
                break;
            case ForceType.VelocityChang:
                rb.AddForce(totalVector, ForceMode.VelocityChange);
                break;
            case ForceType.Gravity:
                rb.AddForce(totalVector, ForceMode.Acceleration);
                break;
            case ForceType.Generic:
                rb.AddForce(totalVector, ForceMode.Force);
                break;
        }
    }
}
