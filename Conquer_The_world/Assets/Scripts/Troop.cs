using System;
using System.Collections;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Troop : MonoBehaviour, ITroop {
    private NavMeshAgent _pathFinder;
    public enum TroopType {
        Swordsman,
        Archer
    };

    public TroopType troopType;
    // [SerializeField] private Camera mainCamera;

    public bool IsAlive { get; set; }
    private int _damage;
    private int _health;
    private float _speed;
    private float _range;
    public bool isSelected;
    public bool isControlled;

    // position to go to
    public Vector3 moveTo;
    private Vector3 previousPos;


    public Troop() {
        troopType = TroopType.Swordsman;
        IsAlive = true;
        _damage = 1;
        _health = 5;
        _speed = 2.0f;
        _range = 2;
        isControlled = false;
    }

    private void Start() {
        _pathFinder = GetComponent<NavMeshAgent>();
    }

    private void Update() {
        Check();
    }

    public void PopulateInstance(int level) {
        gameObject.name = troopType.ToString();
        switch (troopType) {
            case TroopType.Swordsman: {
                GameObject swordsManWeapons = Instantiate(Resources.Load("Prefabs/SwordsManWeapons"), transform) as GameObject;
                if (swordsManWeapons != null) {swordsManWeapons.transform.SetParent(gameObject.transform);}
                break;
            }
            case TroopType.Archer: {
                GameObject archerWeapons = Instantiate(Resources.Load("Prefabs/ArcherWeapons"), transform) as GameObject;
                if (archerWeapons != null) archerWeapons.transform.SetParent(gameObject.transform);
                break;
            }
        }
    }

    public int Damage { get; }
    public void Move() {
        /*
        if (isControlled) {
            float horizontal = Input.GetAxisRaw("Horizontal") * Time.fixedDeltaTime * speed;
            float vertical = Input.GetAxisRaw("Vertical") * Time.fixedDeltaTime * speed;
            Vector3 moveInput = new Vector3(horizontal, 0, vertical);
            gameObject.transform.Translate(moveInput);
            // mainCamera.transform.position = new Vector3(mainCamera.transform.position.x, transform.position.y, mainCamera.transform.position.z);
            mainCamera.transform.position = mainCamera.transform.position + Vector3.up * cameraOffset;
        }
        */
        Debug.Log(gameObject.name + " moving to " + moveTo);
        // _pathFinder.Warp(moveTo);
        _pathFinder.SetDestination(moveTo);
        // transform.position = moveTo;
    }

    public void Attack() {
        throw new NotImplementedException();
    }

    public void Die() {
        throw new NotImplementedException();
    }

    public void Check() {
        if (isSelected) {
            if (previousPos != moveTo) {
                Move();
                previousPos = moveTo;
            }
        }
    }

    public void TakeDamage(int damage) {
        throw new NotImplementedException();
    }


    private void OnCollisionEnter(Collision other) {
        Debug.Log("Collided with " + other.gameObject.name);
    }
}
