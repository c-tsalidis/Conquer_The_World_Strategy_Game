using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour {
    private Troop _troop;
    
    private void Start() {
        _troop = transform.parent.parent.GetComponent<Troop>();
    }

    public void ArrowShotEvent() {
        // _troop.ShootArrow();
        _troop._arrow.GetComponent<Weapon>().ShootArrow();
    }

    public void AttackFinishedEvent() {
        // Debug.Log("Animation finished ");
        _troop.Animator.SetBool("isAttacking", false);
        _troop._isAttacking = false;
        if(_troop.target != null) _troop.target.gameObject.GetComponent<Troop>().TakeDamage(_troop._damage, _troop); // make damage to the target
        _troop.Attack();
    }

    public void PrintEvent(string s) {
        Debug.Log("PrintEvent: " + s + " called at: " + Time.time + " from + " + _troop.gameObject.name);
    }
}