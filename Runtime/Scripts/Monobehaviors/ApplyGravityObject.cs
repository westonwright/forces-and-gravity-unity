using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ApplyGravityObject : MonoBehaviour
{
    [SerializeField]
    [Tooltip("If this object should be affected by gravity by default.")]
    protected bool enableGravity = true;

    protected Rigidbody rb;

    // broadcasts to
    private ApplyGravityEventsChannelSO applyGravityEventsChannel;

    private void Awake()
    {
        applyGravityEventsChannel = CustomGravityHelperFunctions._applyGravityEventsChannel;
    }

    // Start is called before the first frame update
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        // turns off unity's implementation of gravity on the object, just in case
        rb.useGravity = false;

        if (applyGravityEventsChannel != null && enableGravity)
        {
            applyGravityEventsChannel.RaiseAddEvent(this);
        }
    }

    //use to remove from gravity manager
    private void OnDisable()
    {
        if (applyGravityEventsChannel != null && enableGravity)
        {
            applyGravityEventsChannel.RaiseRemoveEvent(this);
        }
    }

    public void EnableGravity(bool on)
    {
        enableGravity = on;
        if (applyGravityEventsChannel != null)
        {
            if (enableGravity)
            {
                applyGravityEventsChannel.RaiseAddEvent(this);
            }
            else
            {
                applyGravityEventsChannel.RaiseRemoveEvent(this);
            }
        }
    }

    public virtual Vector3 GetPoint()
    {
        return rb.position;
    }

    public virtual void ApplyGravity(Vector3 totalVector)
    {
        rb.AddForce(totalVector, ForceMode.Acceleration);
    }
}
