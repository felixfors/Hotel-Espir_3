using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyGrab : MonoBehaviour
{
    public Camera cam;
    public float grabDistance = 5f;
    public float spring = 50f;
    public float damper = 5f;

    private Rigidbody grabbedBody;
    private SpringJoint joint;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            TryGrab();
        else if (Input.GetMouseButtonUp(0))
            Release();
        else if (grabbedBody)
            MoveGrabPoint();
    }

    void TryGrab()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, grabDistance))
        {
            if (hit.rigidbody != null)
            {
                grabbedBody = hit.rigidbody;

                // Skapa ett "dummy-objekt" som jointen fäster vid
                GameObject grabAnchor = new GameObject("GrabAnchor");
                grabAnchor.transform.position = hit.point;
                grabAnchor.transform.parent = grabbedBody.transform;
                joint = grabAnchor.AddComponent<SpringJoint>();
                joint.connectedBody = grabbedBody;
                joint.anchor = Vector3.zero;

                // Här är tricket — fäst i objektets lokala punkt
                Vector3 localHit = grabbedBody.transform.InverseTransformPoint(hit.point);
                joint.connectedAnchor = localHit;

                joint.spring = spring;
                joint.damper = damper;
                joint.maxDistance = 0f;
            }
        }
    }

    void MoveGrabPoint()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Vector3 targetPos = ray.GetPoint(grabDistance);
        joint.transform.position = targetPos;
    }

    void Release()
    {
        if (joint)
            Destroy(joint.gameObject);
        grabbedBody = null;
        joint = null;
    }
}
