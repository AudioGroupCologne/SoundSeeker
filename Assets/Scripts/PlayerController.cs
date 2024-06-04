using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static float speed = 2f;
    private Rigidbody rb;
    private float movementX;
    private float movementY;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.transform.Translate(movementX * speed * Time.deltaTime,0, movementY * speed * Time.deltaTime);
    }

    public void SetMovement(float x, float y)
    {
        movementX = -x;
        movementY = -y;
    }

    //  for xr controller pad movement
    private void OnMove(InputValue val)
    {
        Vector2 movementVector = val.Get<Vector2>();
        movementX = -movementVector.y;
        movementY = movementVector.x;   
    }

    public void ActivatePlayerInput(bool activate)
    {
        GetComponent<PlayerInput>().enabled = activate;
    }

    public void SetSpeed(float val)
    {
        speed = val;
    }
    

}
