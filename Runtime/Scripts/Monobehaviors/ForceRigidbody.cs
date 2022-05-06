using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ForceRigidbody : MonoBehaviour
{
    [Tooltip("If this object should be affected by forces by default.")]
    public bool enableForces = true;
    [Tooltip("If this object should check against proveiders layer masks when applying forces")]
    public bool useLayerMask = true;

    protected Rigidbody rb;

    // reads from
    private ForceManagerSO forceManagerSO;

    // Start is called before the first frame update
    private void Start()
    {
        forceManagerSO = ForcesStaticMembers.forceManagerSO;

        rb = GetComponent<Rigidbody>();
        // turns off unity's implementation of gravity on the object, just in case
        rb.useGravity = false;
    }

    private void FixedUpdate()
    {
        if (forceManagerSO != null)
        {
            if (enableForces)
            {
                int? layer = null;
                if (useLayerMask) layer = gameObject.layer;


                Vector3[] forceVectors = forceManagerSO.GetTotalForcesAtPoint(rb.position, layer);
                for (int i = 0; i < ForcesStaticMembers.forceTypeCount; i++)
                {
                    switch ((ForceType)i)
                    {
                        case ForceType.Force:
                            rb.AddForce(forceVectors[i], ForceMode.Force);
                            break;
                        case ForceType.Acceleration:
                            rb.AddForce(forceVectors[i], ForceMode.Acceleration);
                            break;
                        case ForceType.Impulse:
                            rb.AddForce(forceVectors[i], ForceMode.Impulse);
                            break;
                        case ForceType.VelocityChang:
                            rb.AddForce(forceVectors[i], ForceMode.VelocityChange);
                            break;
                        case ForceType.Gravity:
                            rb.AddForce(forceVectors[i], ForceMode.Acceleration);
                            break;
                        case ForceType.Generic:
                            rb.AddForce(forceVectors[i], ForceMode.Force);
                            break;
                    }
                }
            }
        }
    }
}
