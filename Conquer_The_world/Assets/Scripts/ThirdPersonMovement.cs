using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// reference --> https://www.youtube.com/watch?v=4HpC--2iowE
[RequireComponent(typeof(CharacterController))]
public class ThirdPersonMovement : MonoBehaviour {
    private Transform _camera;
    private CharacterController _characterController;
    public float speed = 6f;
    public float turnSmoothTime = 0.1f;
    private float _turnSmoothVelocity;
    private Troop _troop;

    private void Start() {
        _camera = GameObject.FindGameObjectWithTag("MainCamera").transform;
        _characterController = GetComponent<CharacterController>();
        _troop = transform.GetComponent<Troop>();
        _troop._pathFinder.stoppingDistance = 0.5f;
    }

    private void Update() {
        
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        // TODO --> Check if I can move with the nav mesh agent path finder instead

        if (direction.magnitude >= 0.1f) {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + _camera.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity,
                turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            _characterController.Move(moveDirection.normalized * (speed * Time.deltaTime));
        }
        
        
    }
}