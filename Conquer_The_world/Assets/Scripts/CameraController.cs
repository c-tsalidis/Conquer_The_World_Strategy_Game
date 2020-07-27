using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    private Camera _camera;
    [SerializeField] private float speed = 15;
    [SerializeField] private float rotateSpeed = 2;

    private void Start() {
        _camera = Init.MainCamera;
    }

    public void CheckInput() {
        /*
        if (Input.anyKey) {
            float horizontal = Input.GetAxisRaw("Horizontal") * Time.fixedDeltaTime * speed;
            float vertical = Input.GetAxisRaw("Vertical") * Time.fixedDeltaTime * speed;
            _camera.transform.position += (Vector3.right * horizontal) + (Vector3.forward * vertical);
        }
        // move camera up or down when the user is mouse scrolling
        if (Math.Abs(Input.mouseScrollDelta.y) > 0) {
            _camera.transform.position += (Vector3.up * Input.mouseScrollDelta.y);
        }
        */
        /*
         // to rotate the camera in the 2D x axis (3D y axis):
        float mouseInputXAxis = Input.GetAxis("Mouse X") * rotateSpeed * Time.fixedTime;
        Vector3 rotationVector = new Vector3(0, mouseInputXAxis, 0);
        _camera.transform.Rotate(rotationVector);
        */
        
    }
}