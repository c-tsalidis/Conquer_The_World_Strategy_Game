﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.PlayerLoop;
using UnityEngine.Windows.Speech;

[RequireComponent(typeof(NavMeshAgent))]
public class Troop : MonoBehaviour {
    private NavMeshAgent _pathFinder;

    public enum TroopType {
        Swordsman,
        Archer
    };

    public TroopType troopType;

    public Player troopPlayer;
    public bool IsAlive { get; set; }
    public int _damage;
    public int _health;
    public float _speed;
    public float _minEnemyDistToNotice;
    public bool isSelected;
    public bool isControlled;
    public string targetTag;

    private GameObject _identifier;

    // position to go to
    public Transform moveTo;
    private Vector3 _previousPos;

    private bool _reachedDestination;

    public bool _targetIsSet;
    public Transform target;
    private bool targetIsVisible = true;
    private bool _canAttack; // has the time interval between attacks finished??
    public bool _isAttacking = false;
    public bool _isRunning = false;

    [SerializeField] private float minAttackRange = 2.0f;
    [SerializeField] private WaitForSeconds attackSpeedWaitForSeconds;
    private LayerMask _targetMask;

    // troops meshes (visuals/renderers)
    [SerializeField] private GameObject archerVisuals;
    [SerializeField] private GameObject swordsmanVisuals;

    public Animator Animator { get; private set; }
    private float _attackAnimationTimeLength;

    public List<Troop> targetedBy = new List<Troop>();

    [SerializeField] private GameObject arrowPrefab;
    public GameObject arrowSpawner;
    public GameObject _arrow { get; set; }

    // private ObjectPooler _objectPooler;

    public float previousTime { get; set; }


    public Troop() {
        troopType = TroopType.Swordsman;
        IsAlive = true;
        _damage = 1;
        _health = 5;
        _speed = 3.5f;
        _minEnemyDistToNotice = 2;
        isControlled = false;
        attackSpeedWaitForSeconds = new WaitForSeconds(1.0f);
        _canAttack = true;
    }

    private void Start() {
        _pathFinder = GetComponent<NavMeshAgent>();
        _pathFinder.stoppingDistance = minAttackRange;
        var bodyVisuals = transform.Find("Body_Visuals"); // player troop visuals
        GameObject visuals = bodyVisuals.gameObject;
        if (troopType == TroopType.Archer) {
            if (archerVisuals == null) {
                Debug.LogError("Troop type archer is null");
            }

            visuals = Instantiate(archerVisuals, visuals.transform.position + Vector3.down * 1.1f, visuals.transform.rotation);
        }
        else if (troopType == TroopType.Swordsman) {
            if (swordsmanVisuals == null) {
                Debug.LogError("Troop type swordsman is null");
            }

            visuals = Instantiate(swordsmanVisuals, visuals.transform.position + Vector3.down * 1.1f,
                visuals.transform.rotation);
        }

        visuals.transform.SetParent(bodyVisuals);
        Animator = visuals.GetComponent<Animator>();

        moveTo = transform;
        
        if(isControlled) Init.Instance.ChangeControlledTroop(transform, true);

        /*
        _objectPooler = arrowSpawner.GetComponent<ObjectPooler>();
        _objectPooler.amountToPool = 15;
        _objectPooler.objectToPool = arrowPrefab;
        */
    }
    
    private void Update() {
        if (_health <= 0) {
            StartCoroutine(Die());
            return;
        }

        if (Time.time - previousTime > 0.05f) {
            
        }
    }

    private void FixedUpdate() {
        if (_targetIsSet) {
            if(!_isAttacking) transform.LookAt(target);
            // check if the target is visible
            RaycastHit hit;
            // Does the ray intersect the troop layer
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity,
                LayerMask.GetMask(Init.Tags.Troop))) {
                // the target is visible
                targetIsVisible = true;
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.green);
            }
            else {
                targetIsVisible = false;
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.red);
            }
        }
        if (moveTo != null) {
            if (CalculateDistance(moveTo.position) >= minAttackRange && !_isAttacking) Run();
            else if (!_isAttacking && targetIsVisible) Attack();
            previousTime = Time.time;
        }
    }

    public void PopulateInstance(Player player, int level) {
        // set the player of this troop
        troopPlayer = player;

        // set the gameobject's name to the troop type
        gameObject.name = troopType + " | " + gameObject.tag;
        // set the target tag
        // set this gameobject's layer
        gameObject.layer = LayerMask.NameToLayer(Init.Tags.Troop);
        // set the target
        targetTag = Init.Tags.Troop;
        _targetMask = LayerMask.GetMask(Init.Tags.Troop);

        // populate if either swordsman or archer
        switch (troopType) {
            case TroopType.Swordsman: {
                InstantiateWeapon("Prefabs/SwordsManWeapons");
                minAttackRange = 1.0f;
                _minEnemyDistToNotice = 8.0f;
                _speed = 2.0f;
                _health = 10;
                break;
            }
            case TroopType.Archer: {
                InstantiateWeapon("Prefabs/ArcherWeapons");
                minAttackRange = 8.0f;
                _minEnemyDistToNotice = 16.0f;
                _speed = 3.5f;
                _health = 5;
                break;
            }
        }
    }

    private void InstantiateWeapon(string identifierPathName) {
        _identifier = Instantiate(Resources.Load(identifierPathName), transform) as GameObject;
        if (_identifier != null) {
            _identifier.transform.SetParent(gameObject.transform);
            if (Init.localPlayer == troopPlayer) _identifier.GetComponent<Renderer>().material.color = Color.blue;
            else _identifier.GetComponent<Renderer>().material.color = Color.red;
        }

        if (troopType == TroopType.Archer) {
            _arrow = Instantiate(arrowPrefab, arrowSpawner.transform) as GameObject;
            _arrow.GetComponent<Weapon>().Troop = this;
        }
    }

    public void Run() {
        Animator.SetBool("isRunning", true);
        // transform.LookAt(moveTo);
        _pathFinder.SetDestination(moveTo.position);
        _pathFinder.speed = _speed;
        _isRunning = true;
    }

    public void Attack() {
        if (target == null || !_targetIsSet) return;
        _isAttacking = true;
        _pathFinder.speed = 0;
        transform.LookAt(target);
        Animator.SetBool("isRunning", false);
        Animator.SetBool("isAttacking", true);
    }

    public IEnumerator Die() {
        // _pathFinder.speed = 0;
        _pathFinder.isStopped = true;
        _identifier.SetActive(false);
        Animator.SetBool("isDying", true);
        if (Init.localPlayer.selectedTroops.Contains(this)) Init.localPlayer.selectedTroops.Remove(this);
        _targetIsSet = false;
        IsAlive = false;
        // tell the enemies that are targeting this troop that it's dead, and update its moveTo pos to their current moveTo pos
        foreach (var t in targetedBy) {
            if (t != null) {
                t.target = null;
                t._targetIsSet = false;
                t.moveTo = null;
            }
        }

        targetedBy.Clear();
        if (Init.localPlayer.troops.Contains(gameObject)) Init.localPlayer.troops.Remove(gameObject);
        if(Init.localPlayer.troops.Count > 0) Init.Instance.ChangeControlledTroop(Init.localPlayer.troops[0].transform, true);
        else {
            Init.Instance.ChangeControlledTroop(null, false);
        }
        yield return new WaitForSeconds(20.0f);
        Destroy(gameObject);
    }

    public void TakeDamage(int damage, Troop damager) {
        // Debug.Log(damager.transform.name + " injured " + transform.name + " by " + damage);
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

    public bool IsEnemy(Troop troop) {
        if (this.troopPlayer == troop.troopPlayer) return false;
        else return true;
    }

    public void ShootArrow() {
        // var arrow = _objectPooler.GetPooledObject();
        if (_arrow != null && target != null) {
            // _arrow.SetActive(true);
            // Debug.Log("Shooting arrow");
            var weapon = _arrow.GetComponent<Weapon>();
            weapon.Troop = this;
            var _rb = _arrow.GetComponent<Rigidbody>();
            var thrust = 10.0f;
            _arrow.transform.LookAt(target.transform);
            _rb.AddForce(arrowSpawner.transform.forward * thrust);
        }
    }
}