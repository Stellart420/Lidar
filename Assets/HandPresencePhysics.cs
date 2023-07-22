// Hands physics follow the target (controllers)

using UnityEngine;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;

public class HandPresencePhysics : MonoBehaviour
{
    public Transform target;
    private Rigidbody rb;
    private Collider[] handColliders;

    public XRDirectInteractor hand;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        handColliders = GetComponentsInChildren<Collider>();

        hand.selectEntered.AddListener(DisableHandColliders);
        hand.selectExited.AddListener(EnableHandCollidersDelay);
    }

    void FixedUpdate()
    {
        // Follow the target position
        rb.velocity = (target.position - transform.position) / Time.fixedDeltaTime;

        // Calculate the target rotation difference
        Quaternion rotationDifference = target.rotation * Quaternion.Inverse(transform.rotation);
        rotationDifference.ToAngleAxis(out float angleInDegree, out Vector3 rotationAxis);
        Vector3 rotationDifferenceInDegree = angleInDegree * rotationAxis;

        // Follow the target rotation
        rb.angularVelocity = rotationDifferenceInDegree * Mathf.Deg2Rad / Time.fixedDeltaTime;
    }

    private void EnableHandCollidersDelay(SelectExitEventArgs arg0)
    {
        StartCoroutine(EnableHandColliders());
    }

    IEnumerator EnableHandColliders()
    {
        yield return new WaitForSeconds(0.5f);
        foreach (var item in handColliders)
        {
            item.enabled = true;
        }
    }

    private void DisableHandColliders(SelectEnterEventArgs arg0)
    {
        foreach (var item in handColliders)
        {
            item.enabled = false;
        }
    }
}
