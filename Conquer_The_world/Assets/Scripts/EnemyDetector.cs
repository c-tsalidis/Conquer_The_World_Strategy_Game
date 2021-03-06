using System;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class EnemyDetector : MonoBehaviour {
    private BoxCollider _boxCollider;
    private Troop _troop;

    private void Start() {
        _boxCollider = GetComponent<BoxCollider>();
        _troop = transform.parent.GetComponent<Troop>();
        _boxCollider.size = new Vector3(_troop._minEnemyDistToNotice * 2, 1.0f, _troop._minEnemyDistToNotice * 2);
    }

    private void OnTriggerStay(Collider other) {
        // if this troop is already targeting another enemy troop then return void
        if (_troop.target != null || _troop._targetIsSet || _troop.isControlled)
            return;
        if (other.gameObject.CompareTag(Init.Tags.Troop)) {
            var t = other.gameObject.GetComponent<Troop>();
            if (!t.IsAlive) return;
            if (_troop.IsEnemy(t)) {
                _troop._targetIsSet = true;
                _troop.target = t.transform;
                _troop.moveTo = t.transform;
                t.GetComponent<Troop>().targetedBy.Add(_troop);
            }
        }
    }
}