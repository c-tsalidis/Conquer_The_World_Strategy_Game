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

    private void Start() {
        _rb = GetComponent<Rigidbody>();
    }


    private void OnCollisionEnter(Collision other) {
        // Debug.Log(gameObject.name +  " collided with " + other.gameObject.name);
        if (other.gameObject.CompareTag(Init.Tags.Troop)) {
            if (Troop.troopPlayer == Init.localPlayer) {
                var troop = other.gameObject.GetComponent<Troop>();
                troop.TakeDamage(Troop._damage, troop);
                CollideArrow();
            }
        }
        else if (other.gameObject.CompareTag(Init.Tags.Map)) {
            CollideArrow();
        }
    }

    private void CollideArrow() {
        _rb.velocity = Vector3.zero;
        _rb.useGravity = false;
        _rb.angularVelocity = Vector3.zero;
        if(weaponType == WeaponType.Arrow) StartCoroutine(Pool(2.0f));
    }

    /// <summary>
    /// Once the launched object has reached its place, pool it
    /// </summary>
    /// <returns></returns>
    public IEnumerator Pool(float seconds) {
        yield return new WaitForSeconds(seconds);
        gameObject.SetActive(false);
    }
}