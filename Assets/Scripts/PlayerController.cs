using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class PlayerController : MonoBehaviour
{
    public SteamVR_Action_Vector2 input;
    public float speed = 1;
    private CharacterController characterController;

    private CameraRotate cameraRotate;
    private ShowVRController showVRController;

    public void EnablePlayer()
    {
        characterController.enabled = true;
        cameraRotate.enabled = false;
        showVRController.enabled = true;
    }

    public void DisablePlayer()
    {
        characterController.enabled = false;
        cameraRotate.enabled = true;
        showVRController.enabled = false;
    }

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        cameraRotate = GetComponent<CameraRotate>();
        showVRController = GetComponent<ShowVRController>();
    }

    private void Update()
    {
        if (input.axis.magnitude > 0.4f)
        {
            Vector3 direction = Player.instance.hmdTransform.TransformDirection(new Vector3(input.axis.x, 0, input.axis.y));
            characterController.Move(speed * Time.deltaTime * Vector3.ProjectOnPlane(direction, Vector3.up));
        }
        characterController.Move(-new Vector3(0, 9.81f, 0) * Time.deltaTime);
    }
}
