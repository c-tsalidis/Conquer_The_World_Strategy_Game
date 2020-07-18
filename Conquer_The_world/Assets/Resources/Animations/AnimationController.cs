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
        _troop.Attack();
    }
    
    public void PrintEvent(string s) {
        Debug.Log("PrintEvent: " + s + " called at: " + Time.time + " from + " + _troop.gameObject.name);
    }
}