using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Weapon : MonoBehaviour {
    private enum WeaponType {
        Sword,
        Arrow
    };
    [SerializeField] private WeaponType weaponType;

    private LayerMask _layerMask;
    public Weapon(Troop troop) {
        Troop = troop;
    }

    public Troop Troop { get; set; }

    private Rigidbody _rb;

    private Vector3 prevPosition;
    private Quaternion prevRotation;

    private void Start() {
        _rb = GetComponent<Rigidbody>();
    }


    private void OnCollisionEnter(Collision other) {
        // Debug.Log(gameObject.name +  " collided with " + other.gameObject.name);
        if (other.gameObject.CompareTag(Init.Tags.Troop)) {
            var troop = other.gameObject.GetComponent<Troop>();
            if (Troop.troopPlayer != troop.troopPlayer) {
                troop.TakeDamage(Troop._damage, troop);
                CollideArrow();
            }
        }
        else if (other.gameObject.CompareTag(Init.Tags.Map)) {
            CollideArrow();
        }
    }

    public void ShootArrow() {
        prevPosition = transform.position;
        prevRotation = transform.rotation;
        var _rb = GetComponent<Rigidbody>();
        var thrust = 10.0f;
        // transform.LookAt(Troop.target.transform);
        _rb.AddForce(transform.forward * thrust);
    }

    private void CollideArrow() {
        // _rb.velocity = Vector3.zero;
        // _rb.angularVelocity = Vector3.zero;
        // _rb.isKinematic = true;
        // _rb.detectCollisions = false;
        // Reset();
    }
    
    public void Reset() {
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        // _rb.isKinematic = false;
        transform.position = prevPosition;
        transform.rotation = prevRotation;
    }
}