using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyForceReceiver : MonoBehaviour
{
    [Tooltip("If this object should be affected by forces by default.")]
    public bool enableForces = true;
    [Tooltip("If this object should ignore checking Proveider layer masks when applying forces")]
    public bool ignoreLayerMask = false;

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
                if (ignoreLayerMask) layer = gameObject.layer;

                (ForceTypeSO, Vector3)[] forceVectors = forceManagerSO.GetTotalForcesAtPoint(rb.position, layer);
                foreach((ForceTypeSO forceType, Vector3 vector) forceVector in forceVectors)
                {
                    rb.AddForce(forceVector.vector, forceVector.forceType.forceMode);
                }
            }
        }
    }
}
