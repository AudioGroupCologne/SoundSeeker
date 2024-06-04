using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class PlayerMovement : MonoBehaviour
{
    public InputActionProperty gripButtonAction;
    public InputAction confirmButtonAction;
    public GameObject rightHand;
    public GameObject playerCube; //Case: Remote control blue cube
    private PlayerController playerController; //Case: Remote control blue cube
    public CharacterController characterController; //Case: Control player
    private float gripPressed;
    private bool rotationSet = false;
    public float speed = 0.01f;
    [SerializeField]
    private LocomotionMode locomotionMode;
    public bool useVignette = false;
    private bool targetPositionConfirmed = false;
    public delegate void OnConfirmPosition(bool value);
    public static OnConfirmPosition onConfirmPosition;


    Quaternion initRotation;
    Quaternion lastRotation;

    // Start is called before the first frame update
    void Start()
    {
        playerController = playerCube.GetComponent<PlayerController>();
        gripButtonAction.action.Enable();
        confirmButtonAction = new InputAction("confirm", binding: "<XRController>{RightHand}/triggerPressed");
        confirmButtonAction.performed += OnConfirmButtonPress;
        confirmButtonAction.Enable();

        initRotation = rightHand.transform.rotation;
        lastRotation = rightHand.transform.rotation;

        //Deactivate pad controls if tilt controls are active
        if (locomotionMode == LocomotionMode.RemoteTilt)
        {
            GetComponent<ActionBasedContinuousMoveProvider>().enabled = false;
            playerController.ActivatePlayerInput(false);
        }
        //Ugly hack to enable both vignette and tilt controls and cont. move provider 
        else if (locomotionMode == LocomotionMode.SelfTilt)
        {
            GetComponent<ActionBasedContinuousMoveProvider>().enabled = true;
        }
        else if (locomotionMode == LocomotionMode.SelfPad)
        {
            GetComponent<ActionBasedContinuousMoveProvider>().enabled = true;
            GetComponent<ActionBasedContinuousMoveProvider>().moveSpeed = speed;

        }
        else if (locomotionMode == LocomotionMode.RemotePad)
        {
            GetComponent<ActionBasedContinuousMoveProvider>().enabled = false;
            playerController.ActivatePlayerInput(true);
            playerController.SetSpeed(2f);
        }

        GameObject.FindGameObjectWithTag("Vignette").SetActive(useVignette);
    }

    // Update is called once per frame
    void Update()
    {
        if ((locomotionMode != LocomotionMode.SelfTilt) || (locomotionMode != LocomotionMode.RemoteTilt))
        {
            gripPressed = gripButtonAction.action.ReadValue<float>();
            if (!rotationSet && gripPressed == 1)
            {
                initRotation = rightHand.transform.rotation;
                lastRotation = rightHand.transform.rotation;
                rotationSet = true;
            }
            if (rotationSet)
            {
                if (gripPressed == 1)
                {
                    Quaternion deltaRotation = rightHand.transform.rotation * Quaternion.Inverse(initRotation);
                    lastRotation = rightHand.transform.rotation;
                    deltaRotation.ToAngleAxis(out var angle, out var axis);
                    angle *= Mathf.Deg2Rad;
                    var direction = (1.0f / Time.deltaTime) * angle * axis;
                    //Debug.Log(direction);

                    switch (locomotionMode)
                    {
                        case LocomotionMode.RemoteTilt:
                            playerController.SetMovement(direction.x, direction.z);
                            break;
                        case LocomotionMode.SelfTilt:
                            MovePlayerController(direction);
                            break;
                    }
                }
            }

        }

    }

    private void OnConfirmButtonPress(InputAction.CallbackContext ctx)
    {
        onConfirmPosition?.Invoke(true);
    }

    private void MovePlayerController(Vector3 dir)
    {
        characterController.transform.Translate(-dir.z * speed * Time.deltaTime, 0, dir.x * speed * Time.deltaTime);
    }

    public enum LocomotionMode
    {
        RemoteTilt = 0, RemotePad = 1, SelfTilt = 2, SelfPad = 3
    }

    public bool GetTargetPositionConfirmed()
    {
        return targetPositionConfirmed;
    }
   

}




/**
 * //gripPressed = gripButtonAction.action.ReadValue<bool>();
        if (gripPressed && !rotationSet)
        {
            rotationSet = true;
            initRotation = rotationAction.action.ReadValue<Quaternion>();
            //eulerAngles = initRotation.eulerAngles;
            
        }
        Quaternion newRotation = rotationAction.action.ReadValue<Quaternion>();
        float quatAngle = Quaternion.Angle(initRotation, newRotation);
        Debug.Log(quatAngle);
        //Vector3 newEulerAngles = newRotation.eulerAngles;

        //Debug.Log("X:" + newEulerAngles.x + "; Y: " + newEulerAngles.y + "; Z:" + newEulerAngles.z);

        if (!initRotation.Equals(Quaternion.identity))
        {
            
            //newEulerAngles = newRotation.eulerAngles;
            //Vector3 difference = newEulerAngles - eulerAngles;
            //Debug.Log("X:" + difference.x + "; Y: " + difference.y + "; Z:" + difference.z);
        }
        //Quaternion rotation = rotationAction.action.ReadValue<Quaternion>();
        //Debug.Log(rotation);
**/