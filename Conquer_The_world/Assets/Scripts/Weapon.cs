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

    public bool isShot = false;

    private void Start() {
        _rb = GetComponent<Rigidbody>();
    }

    private void Update() {
        /*
        if(Troop == null) return;
        if (Troop.target != null && isShot) {
            var target = Troop.target;
            float speed = 25.0f;
            // Move our position a step closer to the target.
            float step = speed * Time.deltaTime; // calculate distance to move
            transform.position = Vector3.MoveTowards(transform.position, target.position, step);
            transform.LookAt(target);

            // Check if the position of the cube and sphere are approximately equal.
            if (Vector3.Distance(transform.position, target.position) < 0.001f) {
                gameObject.SetActive(false);
            }
        }
        else if(isShot) _rb.AddForce(transform.forward * 10.0f);
        */
    }


    private void OnTriggerEnter(Collider other) {
        
        if (other.gameObject.CompareTag(Init.Tags.Troop)) {
            var troop = other.gameObject.GetComponent<Troop>();
            if (Troop.troopPlayer != troop.troopPlayer) {
                troop.TakeDamage(Troop._damage, troop);
                CollideArrow();
                if(troop.isControlled) Debug.Log("DAMAGED");
            }
        }
        
        if (other.gameObject.layer == LayerMask.NameToLayer(Init.Tags.Map)) {
            CollideArrow();
        }
    }

    public void ShootArrow() {
        isShot = true;
        Reset();
        isShot = true;
        prevPosition = Troop.arrowSpawner.transform.position;
        prevRotation = Troop.arrowSpawner.transform.rotation;
        var _rb = GetComponent<Rigidbody>();
        var thrust = 10.0f;
        _rb.isKinematic = false;
        // transform.LookAt(Troop.target.transform);
        _rb.AddForce(transform.forward * thrust);
    }

    private void CollideArrow() {
        _rb.isKinematic = true;
    }

    public void Reset() {
        gameObject.SetActive(true);
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        // _rb.isKinematic = false;
        transform.position = prevPosition;
        transform.rotation = prevRotation;
    }
}