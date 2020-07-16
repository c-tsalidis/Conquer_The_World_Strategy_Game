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
    private float _minEnemyDistToNotice;
    public bool isSelected;
    public bool isControlled;
    public string targetTag;

    private GameObject _weapon;

    // position to go to
    public Vector3 moveTo;
    private Vector3 _previousPos;

    private bool _reachedDestination;

    private bool _targetIsSet;
    public Transform target;
    private bool _canAttack; // has the time interval between attacks finished??
    public bool _isAttacking = false;

    [SerializeField] private float minAttackRange = 2.0f;
    [SerializeField] private WaitForSeconds attackSpeedWaitForSeconds;
    private LayerMask _targetMask;
    
    // troops meshes (visuals/renderers)
    [SerializeField] private GameObject archerVisuals;
    [SerializeField] private GameObject swordsmanVisuals;

    private Animator _animator;
    private float _animationTimeLength;

    public List<Troop> targetedBy = new List<Troop>();

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
        var bodyVisuals = transform.Find("Body_Visuals"); // player troop visuals
        GameObject visuals = bodyVisuals.gameObject;
        if (troopType == TroopType.Archer) {
            if (archerVisuals == null) {
                Debug.LogError("Troop type archer is null");
            }
            visuals = Instantiate(archerVisuals, visuals.transform.position + Vector3.down, visuals.transform.rotation);
        }
        else if (troopType == TroopType.Swordsman) {
            if (swordsmanVisuals == null) {
                Debug.LogError("Troop type swordsman is null");
            }
            visuals = Instantiate(swordsmanVisuals, visuals.transform.position + Vector3.down, visuals.transform.rotation);
        }
        visuals.transform.SetParent(bodyVisuals);
        _animator = visuals.GetComponent<Animator>();
    }
    
    // TODO --> If enemy in range, attack. Else, run

    private void Update() {
        if (_health <= 0) {
            IsAlive = false;
            StartCoroutine(Die());
        }

        if (_isAttacking) _pathFinder.speed = 0;
        else _pathFinder.speed = _speed;
        
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
                InstantiateWeapon("Prefabs/SwordsManWeapons");
                minAttackRange = 2.0f;
                _minEnemyDistToNotice = 5.0f;
                _speed = 2.0f;
                break;
            }
            case TroopType.Archer: {
                InstantiateWeapon("Prefabs/ArcherWeapons");
                minAttackRange = 8.0f;
                _minEnemyDistToNotice = 10.0f;
                _speed = 3.5f;
                break;
            }
        }
        gameObject.GetComponent<NavMeshAgent>().stoppingDistance = minAttackRange;
    }

    private void InstantiateWeapon(string weaponPathName) {
        _weapon = Instantiate(Resources.Load(weaponPathName), transform) as GameObject;
        if (_weapon != null) {
            _weapon.transform.SetParent(gameObject.transform);
            if(gameObject.tag == Init.Tags.PlayerTroop) _weapon.GetComponent<Renderer>().material.color = Color.blue;
            else _weapon.GetComponent<Renderer>().material.color = Color.red;
        }
    }

    public void Move() {
        _pathFinder.SetDestination(moveTo);
    }

    public IEnumerator Attack() {
        if(_animationTimeLength == default) _animationTimeLength = _animator.runtimeAnimatorController.animationClips[0].length;
        // the wait for seconds should be higher than the attack animation --> like around twice or so
        // Debug.Log(gameObject.name + " attacking " + _target.name);
        if (target == null) yield return null;
        // _weaponAnimation.SetBool("IsAttacking", _canAttack);
        
        // play attack animation
        _animator.SetBool("isRunning", false);
        _animator.SetBool("isShooting", true);
        _canAttack = false;
        
        // wait until attack animation is finished
        yield return new WaitForSeconds(_animationTimeLength);
        // the enemy takes damage
        if(target != null) target.GetComponent<Troop>().TakeDamage(_damage);
        _animator.SetBool("isShooting", false);
        // _canAttack = true;
    }

    public IEnumerator Die() {
        if (Init.PlayerData.selectedTroops.Contains(this)) Init.PlayerData.selectedTroops.Remove(this);
        if(target.GetComponent<Troop>().targetedBy.Contains(this)) target.GetComponent<Troop>().targetedBy.Remove(this);
        Init.PlayerData.troops.Remove(gameObject);
        yield return attackSpeedWaitForSeconds;
        Destroy(gameObject);
    }

    public IEnumerator Check() {
        if (gameObject == null) yield break;
        yield return attackSpeedWaitForSeconds;
        if (isSelected && !_targetIsSet) {
            if (CalculateDistance(moveTo) >= minAttackRange) {
            // if (_previousPos != moveTo) {
            // if (Vector3.Distance(Vector3.right * 2 + moveTo, moveTo) > 1) {
                Move();
                // _previousPos = moveTo;
                _animator.SetBool("isRunning", true);
            }
            else {
                // _pathFinder.isStopped = true;
                _animator.SetBool("isRunning", false);
            }
        }

        if (!_targetIsSet) {
            CheckForEnemiesNearby();
            _isAttacking = false;
        }
        else {
            if (target != null) {
                if (target.GetComponent<Troop>().IsAlive) {
                    transform.LookAt(target);
                    _isAttacking = true;
                    // if (_canAttack) {
                        StartCoroutine(Attack());
                    // }
                }
                else {
                    target = null;
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
        Collider [] hitColliders = Physics.OverlapBox(transform.position, transform.localScale * _minEnemyDistToNotice,
            Quaternion.identity, _targetMask);
        // foreach (var c in hitColliders) {
        // Debug.Log(gameObject.name + " is nearby " + c.name);
        // }
        if (hitColliders.Length > 0) {
            _targetIsSet = true;
            target = hitColliders[0].transform;
            target.GetComponent<Troop>().targetedBy.Add(this);
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