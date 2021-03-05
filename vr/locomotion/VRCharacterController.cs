using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class VRCharacterController : MonoBehaviour
{
    public XRNode inputSource;
    public float moveSpeed = 5f;

    private Vector2 inputPrimary2DAxis;
    private CharacterController characterController;
    private XRRig rig;

    private InputDevice inputDevice;

    void Reset()
    {
        ValidateComponent<CharacterController>();
        ValidateComponent<XRRig>();
    }

    private void ValidateComponent<T>() where T : Component
    {
        if (GetComponent<T>() == null)
        {
            gameObject.AddComponent<T>();
        }
    }

    private void FixedUpdate()
    {
        ValidateInputDevice();

        MoveRig();
    }

    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        rig = GetComponent<XRRig>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void ValidateInputDevice()
    {
        if (!inputDevice.isValid)
        {
            inputDevice = InputDevices.GetDeviceAtXRNode(inputSource);
        }
    }

    private void MoveRig()
    {
        inputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out inputPrimary2DAxis);

        Vector3 moveVector = new Vector3(inputPrimary2DAxis.x, 0f, inputPrimary2DAxis.y);
        Quaternion headYaw = Quaternion.Euler(0f, rig.cameraGameObject.transform.eulerAngles.y, 0f);

        characterController.Move(headYaw * moveVector * moveSpeed * Time.fixedDeltaTime);
    }
}
