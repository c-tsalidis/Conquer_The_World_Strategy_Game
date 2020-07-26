using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// reference --> https://www.youtube.com/watch?v=4HpC--2iowE
[RequireComponent(typeof(CharacterController))]
public class ThirdPersonMovement : MonoBehaviour {
    public Transform camera;
    private CharacterController _characterController;
    public float speed = 6f;
    public float turnSmoothTime = 0.1f;
    private float _turnSmoothVelocity;

    private void Start() {
        _characterController = GetComponent<CharacterController>();
    }

    private void Update() {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
        if (direction.magnitude >= 0.1f) {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + camera.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity,
                turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            _characterController.Move(moveDirection.normalized * (speed * Time.deltaTime));
        }

        RaycastHit hit;
        if (Physics.Raycast(this.transform.position, Vector3.down, out hit, Mathf.Infinity, LayerMask.NameToLayer(Init.Tags.Ground))) {
            Debug.DrawRay(transform.position, Vector3.down * hit.distance, Color.green);
            // transform.position = new Vector3(transform.position.x, hit.normal.y, transform.position.z);
        }
    }
}