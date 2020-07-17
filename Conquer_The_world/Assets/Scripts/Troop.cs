using System;
using System.Collections;
using System.Collections.Generic;
using Interfaces;
using UnityEditor.Animations;
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
    public int _damage;
    public int _health;
    public float _speed;
    public float _minEnemyDistToNotice;
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
    public bool _isRunning = false;

    [SerializeField] private float minAttackRange = 2.0f;
    [SerializeField] private WaitForSeconds attackSpeedWaitForSeconds;
    private LayerMask _targetMask;

    // troops meshes (visuals/renderers)
    [SerializeField] private GameObject archerVisuals;
    [SerializeField] private GameObject swordsmanVisuals;

    private Animator _animator;
    private float _attackAnimationTimeLength;

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
        _pathFinder.stoppingDistance = minAttackRange - 2;
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

            visuals = Instantiate(swordsmanVisuals, visuals.transform.position + Vector3.down,
                visuals.transform.rotation);
        }

        visuals.transform.SetParent(bodyVisuals);
        _animator = visuals.GetComponent<Animator>();
    }

    // TODO --> If enemy in range, attack. Else, run

    private void Update() {
        if (_health <= 0) {
            StartCoroutine(Die());
        }

        if (_isAttacking) {
            _isRunning = false;
            _pathFinder.speed = 0;
            _animator.SetBool("isRunning", false);
            _animator.SetBool("isShooting", true);
        }
        else {
            _animator.SetBool("isShooting", false);
            if (_isRunning) {
                _pathFinder.speed = _speed;
                _animator.SetBool("isRunning", true);
            }
            else {
                _animator.SetBool("isRunning", false);
            }
        }

        StartCoroutine(Check());
    }

    public void PopulateInstance(int level) {
        // set the gameobject's name to the troop type
        gameObject.name = troopType + " | " + gameObject.tag;
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
                _health = 10;
                break;
            }
            case TroopType.Archer: {
                InstantiateWeapon("Prefabs/ArcherWeapons");
                minAttackRange = 8.0f;
                _minEnemyDistToNotice = 10.0f;
                _speed = 3.5f;
                _health = 5;
                break;
            }
        }
    }

    private void InstantiateWeapon(string weaponPathName) {
        _weapon = Instantiate(Resources.Load(weaponPathName), transform) as GameObject;
        if (_weapon != null) {
            _weapon.transform.SetParent(gameObject.transform);
            if (gameObject.tag == Init.Tags.PlayerTroop) _weapon.GetComponent<Renderer>().material.color = Color.blue;
            else _weapon.GetComponent<Renderer>().material.color = Color.red;
        }
    }

    public void Run() {
        _pathFinder.SetDestination(moveTo);
        _isRunning = true;
    }

    public IEnumerator Attack() {
        // the wait for seconds should be higher than the attack animation --> like around twice or so
        // Debug.Log(gameObject.name + " attacking " + _target.name);
        // if (target == null) yield return null;
        // _weaponAnimation.SetBool("IsAttacking", _canAttack);

        // play attack animation
        // _animator.SetBool("isRunning", false);
        // _animator.SetBool("isShooting", true);
        // _canAttack = false;
        _isAttacking = true;
        _animator.SetBool("isShooting", true);
        // wait until attack animation is finished
        yield return new WaitForSeconds(3.0f);
        // the enemy takes damage
        if (target != null && target.GetComponent<Troop>().IsAlive && this.IsAlive) target.GetComponent<Troop>(). TakeDamage(_damage, this);
        // _animator.SetBool("isShooting", false);
        // _canAttack = true;
    }

    public IEnumerator Die() {
        _pathFinder.speed = 0;
        _animator.SetBool("isDying", true);
        if (Init.PlayerData.selectedTroops.Contains(this)) Init.PlayerData.selectedTroops.Remove(this);
        target = null;
        IsAlive = false;
        targetedBy.Clear();
        Init.PlayerData.troops.Remove(gameObject);
        yield return new WaitForSeconds(20.0f);
        Destroy(gameObject);
    }

    public IEnumerator Check() {
        if (gameObject == null) yield break;
        yield return new WaitForSeconds(0.05f);
        if (isSelected && !_isAttacking) {
            if (CalculateDistance(moveTo) >= minAttackRange) {
                // if (_previousPos != moveTo) {
                // if (Vector3.Distance(Vector3.right * 2 + moveTo, moveTo) > 1) {
                Run();
            }
            else {
                // _pathFinder.isStopped = true;
                _isRunning = false;
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
        if(target != null) return;

        // Use the OverlapBox to detect if there are any other colliders within this box area.
        // Use the GameObject's centre, half the size (as a radius) and rotation. This creates an invisible box around your GameObject.
        Collider[] hitColliders = Physics.OverlapBox(transform.position, transform.localScale * _minEnemyDistToNotice,
            Quaternion.identity, _targetMask);
        // foreach (var c in hitColliders) {
        // Debug.Log(gameObject.name + " is nearby " + c.name);
        // }
        if (hitColliders.Length > 0 && hitColliders[0].transform.GetComponent<Troop>().IsAlive) {
            _targetIsSet = true;
            target = hitColliders[0].transform;
            target.GetComponent<Troop>().targetedBy.Add(this);
        }
    }

    public void TakeDamage(int damage, Troop damager) {
        Debug.Log(damager.transform.name +  " injured " + transform.name + " by " + damage);
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