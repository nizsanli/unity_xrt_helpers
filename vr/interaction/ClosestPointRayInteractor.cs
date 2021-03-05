using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.XR.Interaction.Toolkit;

[Serializable]
public class ClosestPointRayInteractor : XRRayInteractor
{
    XRBaseInteractable grabbed;

    Vector3 grabPositionOffset;
    Quaternion grabRotationOffset;

    [Range(0f, 1f)]
    public float positionEasing = 1f;
    [Range(0f, 1f)]
    public float rotationEasing = 1f;

    public Renderer handMesh;
    private XRInteractorLineVisual lineVisual;

    protected override void Start()
    {
        base.Start();

        lineVisual = GetComponent<XRInteractorLineVisual>();
    }

    public void GrabAtClosestPoint(XRBaseInteractable interactable)
    {
        // get closest point
        RaycastHit hitInfo;
        bool didHit = Physics.Raycast(transform.position, transform.forward, out hitInfo, maxRaycastDistance);

        // store reference
        grabbed = interactable;

        // rigidbody under full control of this interactor now
        // forces, collisions, joints will not affect this body
        grabbed.GetComponent<Rigidbody>().isKinematic = true;

        // store offsets
        // position
        grabPositionOffset = grabbed.transform.InverseTransformVector(hitInfo.point - hitInfo.transform.position);
        // rotation: Quaternion.Inverse(A) * B  =  B - A  = A to B
        grabRotationOffset = Quaternion.Inverse(transform.rotation) * grabbed.transform.rotation;

        // hide hand mesh if defined
        if (handMesh != null)
            handMesh.enabled = false;

        // hide line visual if defined
        if (lineVisual != null)
            lineVisual.enabled = false;
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        GrabAtClosestPoint(args.interactable);
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);

        Rigidbody rb = grabbed.GetComponent<Rigidbody>();

        // enable external forces
        rb.isKinematic = false;

        // drop reference
        grabbed = null;

        // show hand mesh if defined
        if (handMesh != null)
            handMesh.enabled = true;

        // show line visual if defined
        if (lineVisual != null)
            lineVisual.enabled = true;
    }

    private void FixedUpdate()
    {
        if (grabbed)
        {
            Rigidbody rb = grabbed.GetComponent<Rigidbody>();

            // get target position and rotation based on offsets collected during OnSelectEnter
            Quaternion targetRotation = transform.rotation * grabRotationOffset;
            Vector3 targetPosition = transform.position - grabbed.transform.rotation * grabPositionOffset;

            // interpolate target rotation
            rb.MoveRotation(Quaternion.Lerp(rb.transform.rotation, targetRotation, rotationEasing));
            // interpolate position rotation
            rb.MovePosition(Vector3.Lerp(rb.transform.position, targetPosition, positionEasing));
        }
    }
}
