using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{
    /// <summary>
    /// Normal speed of camera movement.
    /// </summary>
    public float movementSpeed = 1.0f;

    public float rotationSpeed = 10.0f;
    public float zoomSpeed = 1.0f;
    public float minDistance = 50.0f;
    public float maxDistance = 1000.0f;
    public float minAngle = 20.0f;
    public float maxAngle = 80.0f;
    public float scrollBorderWidth = 20.0f;

    private float distance = 200.0f;
    private Vector3 centerPosition = new Vector3(0.0f, 100.0f, 0.0f);
    private Vector3 rotation = new Vector3(45.0f, 0.0f, 0.0f);

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 movement = new Vector3(0.0f, 0.0f, 0.0f);
        if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) || Input.mousePosition.x < scrollBorderWidth)
        {
            movement += -transform.right;
        }

        if(Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) || Input.mousePosition.x > Screen.width - scrollBorderWidth)
        {
            movement += transform.right;
        }

        if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) || Input.mousePosition.y > Screen.height - scrollBorderWidth)
        {
            movement += transform.forward;
        }

        if(Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) || Input.mousePosition.y < scrollBorderWidth)
        {
            movement += -transform.forward;
        }

        movement.y = 0.0f;
        centerPosition += movement * movementSpeed * Time.deltaTime * distance;

        if(Input.GetKey(KeyCode.Mouse1))
        {
            rotation.x -= Input.GetAxis("Mouse Y") * rotationSpeed;
            rotation.y += Input.GetAxis("Mouse X") * rotationSpeed;
            rotation.x = Mathf.Clamp(rotation.x, minAngle, maxAngle);
        }

        float axis = Input.GetAxis("Mouse ScrollWheel");
        Debug.Log(axis);
        distance -= axis * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);
        Debug.Log(distance);

        Quaternion rotationQuat = Quaternion.Euler(rotation);
        Vector3 distanceVec = new Vector3(0.0f, 0.0f, -distance);
        transform.position = centerPosition + rotationQuat * distanceVec;
        transform.LookAt(centerPosition);
    }
}
