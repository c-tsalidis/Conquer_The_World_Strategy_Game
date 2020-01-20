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

    public bool IsAlive { get; set; }
    private int _damage;
    private int _health;
    private float _speed;
    private float _range;
    public bool isSelected;
    public bool isControlled;
    public string targetTag;

    private GameObject _weapon;
    private Animator _weaponAnimation;

    // position to go to
    public Vector3 moveTo;
    private Vector3 _previousPos;

    private bool _reachedDestination;

    private bool _targetIsSet;
    private Transform _target;
    private bool _canAttack; // has the time interval between attacks finished??

    [SerializeField] private float minDistance = 2.0f;
    [SerializeField] private WaitForSeconds _waitForSeconds;
    private LayerMask _targetMask;

    public Troop() {
        troopType = TroopType.Swordsman;
        IsAlive = true;
        _damage = 1;
        _health = 5;
        _speed = 2.0f;
        _range = 2;
        isControlled = false;
        _waitForSeconds = new WaitForSeconds(1.0f);
        _canAttack = true;
    }

    private void Start() {
        _pathFinder = GetComponent<NavMeshAgent>();
    }

    private void Update() {
        if (_health <= 0) {
            IsAlive = false;
            StartCoroutine(Die());
        }

        StartCoroutine(Check());
    }

    public void PopulateInstance(int level) {
        // set the gameobject's name to the troop type
        gameObject.name = troopType.ToString();
        // set the target tag
        if (gameObject.CompareTag(Init.Tags.PlayerTroop)) {
            // set this gameobject's layer
            gameObject.layer = LayerMask.NameToLayer(Init.Tags.PlayerTroop);
            // set the target
            targetTag = Init.Tags.Enemy;
            _targetMask = LayerMask.GetMask(Init.Tags.Enemy);
        }
        else {
            // set this gameobject's layer
            gameObject.layer = LayerMask.NameToLayer(Init.Tags.Enemy);
            // set the target
            targetTag = Init.Tags.PlayerTroop;
            _targetMask = LayerMask.GetMask(Init.Tags.PlayerTroop);
        }

        // populate if either swordsman or archer
        switch (troopType) {
            case TroopType.Swordsman: {
                GameObject swordsManWeapons =
                    Instantiate(Resources.Load("Prefabs/SwordsManWeapons"), transform) as GameObject;
                if (swordsManWeapons != null) {
                    swordsManWeapons.transform.SetParent(gameObject.transform);
                }

                _weapon = swordsManWeapons;
                break;
            }
            case TroopType.Archer: {
                GameObject archerWeapons =
                    Instantiate(Resources.Load("Prefabs/ArcherWeapons"), transform) as GameObject;
                if (archerWeapons != null) archerWeapons.transform.SetParent(gameObject.transform);
                _weapon = archerWeapons;
                break;
            }
        }

        if (_weapon != null) _weaponAnimation = _weapon.GetComponent<Animator>();
    }

    public void Move() {
        _pathFinder.SetDestination(moveTo);
    }

    public IEnumerator Attack() {
        // the wait for seconds should be higher than the attack animation --> like around twice or so
        Debug.Log(gameObject.name + " attacking " + _target.name);
        if (_target != null) _target.GetComponent<Troop>().TakeDamage(_damage);
        _weaponAnimation.SetBool("IsAttacking", _canAttack);
        _canAttack = false;
        yield return new WaitForSeconds(2.0f);
        // play attack animation
        _canAttack = true;
    }

    public IEnumerator Die() {
        Init.PlayerData.troops.Remove(gameObject);
        yield return _waitForSeconds;
        Destroy(gameObject);
    }

    public IEnumerator Check() {
        if (gameObject == null) yield break;
        yield return _waitForSeconds;
        if (isSelected) {
            // if (CalculateDistance(moveTo) >= minDistance) {
            if (_previousPos != moveTo) {
                Move();
                _previousPos = moveTo;
            }

            // }
            // else _pathFinder.isStopped = true;
        }

        if (!_targetIsSet) CheckForEnemiesNearby();
        else {
            if (_target != null) {
                if (_target.GetComponent<Troop>().IsAlive) {
                    transform.LookAt(_target);
                    if (_canAttack) {
                        StartCoroutine(Attack());
                    }
                }
                else {
                    _target = null;
                    _targetIsSet = false;
                }
            }
        }

        // check if there are any enemies in range
        // StartCoroutine(CheckRange());
    }

    
    
    private void CheckForEnemiesNearby() {
        // maybe replace the hitColliders with an OnTriggerEnter(Collider other) event??

        // Use the OverlapBox to detect if there are any other colliders within this box area.
        // Use the GameObject's centre, half the size (as a radius) and rotation. This creates an invisible box around your GameObject.
        Collider [] hitColliders = Physics.OverlapBox(transform.position, transform.localScale * _range,
            Quaternion.identity, _targetMask);
        // foreach (var c in hitColliders) {
        // Debug.Log(gameObject.name + " is nearby " + c.name);
        // }
        if (hitColliders.Length > 0) {
            _targetIsSet = true;
            _target = hitColliders[0].transform;
        }
    }

    public void TakeDamage(int damage) {
        _health -= damage;
    }

    /// <summary>
    /// Calculates the distance from Troop game object position to another position in the x and z coordinates
    /// </summary>
    private float CalculateDistance(Vector3 pos) {
        var position = transform.position;
        var xDif = position.x - pos.x;
        var zDif = position.z - pos.z;
        var distanceSquared = xDif * xDif + zDif * zDif;
        return (float) Math.Sqrt(distanceSquared);
    }
}