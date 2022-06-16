using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour//controls position and rotation of the attached camera
{
    public float moveSpeed = 2f;
    public float rotationSpeed = 2f;
    public float floatiness = 0.1f;
    float yaw = 0;
    float pitch = 0;
    float roll = 0;
    public bool disableCameraMovement = false;
    private float speedMultiplier = 1;
    void Update()
    {
        if (!disableCameraMovement)
        {
            bool moving = false;

            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            {
                transform.position += transform.rotation*Vector3.left * moveSpeed * Time.deltaTime * speedMultiplier;
                moving = true;
            }
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            {
                transform.position += transform.rotation * Vector3.right * moveSpeed * Time.deltaTime * speedMultiplier;
                moving = true;
            }
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            {
                transform.position += transform.rotation * new Vector3(0, 0, 1) * moveSpeed * Time.deltaTime * speedMultiplier;
                moving = true;
            }
            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            {
                transform.position += transform.rotation * new Vector3(0, 0, -1) * moveSpeed * Time.deltaTime * speedMultiplier;
                moving = true;
            }
            if (Input.GetKey(KeyCode.Space))
            {
                transform.position += transform.rotation * Vector3.up * moveSpeed * Time.deltaTime * speedMultiplier;
                moving = true;
            }
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                transform.position += transform.rotation * Vector3.down  * moveSpeed * Time.deltaTime * speedMultiplier;
                moving = true;
            }
            if (Input.GetKey(KeyCode.Q)||Input.GetKey(KeyCode.Comma))
            {
                roll += moveSpeed * Time.deltaTime * speedMultiplier * rotationSpeed * 10;
                moving = true;
            }
            if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Period))
            {
                roll -= moveSpeed * Time.deltaTime * speedMultiplier * rotationSpeed * 10;
                moving = true;
            }

            if (moving)// by doing it down here, it still allows for small camera movements when lagging
            {
                speedMultiplier = 1 - (1 - speedMultiplier) * Mathf.Exp(Time.deltaTime / -floatiness);   
            }
            else
            {
                speedMultiplier = speedMultiplier * Mathf.Exp(Time.deltaTime / -floatiness*4);
            }
            if (Input.GetKey(KeyCode.Mouse1))
            {
                yaw -= Input.GetAxis("Mouse Y") * rotationSpeed;
                pitch += Input.GetAxis("Mouse X") * rotationSpeed;
            }
            if (Input.GetKey(KeyCode.Mouse2))// reset rotation and look at the board
            {
                yaw = 0;
                pitch = 0;
                roll = 0;
                transform.position -= new Vector3(0, 0, transform.position.z + 9.99f);
            }
            if (Input.GetKey(KeyCode.R))//reset position and rotation
            {
                yaw = 0;
                pitch = 0;
                roll = 0;
                transform.position = new Vector3(0.6f, 4f, -9.99f);
            }
            transform.rotation = Quaternion.Euler(0, 0, roll);
            transform.rotation *= Quaternion.Euler(yaw, pitch, 0);
        }
    }
}
