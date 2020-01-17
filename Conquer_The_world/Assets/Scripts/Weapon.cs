using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour {
    Transform weaponTransform;

    private void Start() {
        weaponTransform = transform;
    }

    private void Update() {
        // update its position to where his player is at
        // weaponTransform.position = weaponTransform.position + weaponTransform.parent.position;
    }

    private void OnCollisionEnter(Collision other) {
        Debug.Log("Collided with " + other.gameObject.name);
    }
}