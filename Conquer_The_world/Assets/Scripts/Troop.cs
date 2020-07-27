using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.PlayerLoop;
using UnityEngine.Windows.Speech;

[RequireComponent(typeof(NavMeshAgent))]
public class Troop : MonoBehaviour {
    public NavMeshAgent _pathFinder;

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
    private bool _controlledIsOnPursuitMode = true;
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

            visuals = Instantiate(archerVisuals, visuals.transform.position + Vector3.down * 1.1f,
                visuals.transform.rotation);
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

        if (isControlled) {
            Init.Instance.ChangeControlledTroop(transform, true);
            Init.Instance.healthText.text = "Health: " + _health;
            isSelected = true;
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
                _health = 10 * level;
                _damage *= level;
                break;
            }
            case TroopType.Archer: {
                InstantiateWeapon("Prefabs/ArcherWeapons");
                minAttackRange = 8.0f;
                _minEnemyDistToNotice = 16.0f + level;
                _speed = 3.5f;
                _health = 5 * level;
                _damage *= level;
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
            _arrow.SetActive(false);
        }
    }

    private void Update() {
        if (_health <= 0) {
            StartCoroutine(Die());
            return;
        }

        CheckInput();
    }

    private void CheckInput() {
        if (!isControlled) return;

        // make it so that if the user clicks on a target, attack him
        // or (preferably), on space --> it'll attack automatically

        if (Input.GetKeyDown(KeyCode.Space) && IsAlive) {
            _controlledIsOnPursuitMode = !_controlledIsOnPursuitMode;
            if(_controlledIsOnPursuitMode) Init.Instance.isAttackingText.text = "Attacking";
            else Init.Instance.isAttackingText.text = "Free movement";
        }

        /*
        if (!_isAttacking && (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)) {
            _isRunning = true;
            _isAttacking = false;
            target = null;
            _targetIsSet = false;
        }
        else _isRunning = false;
        */
    }

    private void FixedUpdate() {
        if (_targetIsSet) {
            if (!_isAttacking) {
                if (isControlled) {
                    if(_controlledIsOnPursuitMode) transform.LookAt(target);
                }
                else transform.LookAt(target);
            }
            // check if the target is visible
            RaycastHit hit;
            // Does the ray intersect the troop layer
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit,
                Mathf.Infinity,
                LayerMask.GetMask(Init.Tags.Troop))) {
                // the target is visible
                targetIsVisible = true;
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance,
                    Color.green);
            }
            else {
                targetIsVisible = false;
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.red);
            }
        }

        if (moveTo != null) {
            float distanceToTarget = CalculateDistance(moveTo.position);
            if (!_isAttacking) {
                if (distanceToTarget >= minAttackRange || target == null) {
                    if (!isControlled) _isRunning = true;
                    _isRunning = true;
                }
                else if (targetIsVisible) {
                    if (isControlled) {
                        if (_controlledIsOnPursuitMode) _isAttacking = true;
                        else _isAttacking = false;
                    }
                    else _isAttacking = true;
                }

                if (distanceToTarget < minAttackRange && target == null) {
                    _isRunning = true;
                }
            }
            else {
                _isRunning = false;
                _isAttacking = false;
            }

            previousTime = Time.time;
        }

        if (_isRunning) Run();
        else if (_isAttacking) Attack();
        else {
            Animator.SetBool("isAttacking", false);
            Animator.SetBool("isRunning", false);
        }
    }


    public void Run() {
        if (moveTo == null) return;
        Animator.SetBool("isRunning", true);
        _pathFinder.SetDestination(moveTo.position);
        _pathFinder.speed = _speed;
    }

    public void Attack() {
        if (target == null || !_targetIsSet) return;
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
                // if (t.troopType == TroopType.Archer) t.arrowPrefab.GetComponent<Weapon>().Reset();
                t._isRunning = false;
                t._isAttacking = false;
            }
        }

        targetedBy.Clear();
        if (Init.localPlayer.troops.Contains(gameObject)) Init.localPlayer.troops.Remove(gameObject);
        if (Init.localPlayer.troops.Count > 0)
            Init.Instance.ChangeControlledTroop(Init.localPlayer.troops[0].transform, true);
        else {
            Init.Instance.ChangeControlledTroop(null, false);
        }

        yield return new WaitForSeconds(20.0f);
        Destroy(gameObject);
    }

    public void TakeDamage(int damage, Troop damager) {
        // Debug.Log(damager.transform.name + " injured " + transform.name + " by " + damage);
        _health -= damage;
        if (isControlled) Init.Instance.healthText.text = "Health: " + _health;
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
}