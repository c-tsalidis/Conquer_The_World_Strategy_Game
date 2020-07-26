using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitSelectionManager : MonoBehaviour {
    private GameObject _moveTo; // position to where the selected units are supposed to move to
    private bool _isDragging; // is the user dragging their mouse accross the screen to select units?

    private Vector3 _mouseStartScreenPosition; // the start position of the mouse when the user holds down the left mouse button

    private Canvas _canvas;
    private RectTransform _selectionBoxImgRectTransform;

    private void Awake() {
        if (_canvas == null) _canvas = Init.Instance.canvas;
        if (Init.Instance.selectionBoxImg != null) {
            var materialColor = Init.Instance.selectionBoxImg.material.color;
            materialColor.a = 50;
            _selectionBoxImgRectTransform = Init.Instance.selectionBoxImg.GetComponent<RectTransform>();
            Init.Instance.selectionBoxImg.gameObject.SetActive(false);
        }
        _moveTo = new GameObject("Cursor Move to Position");
    }

    private void Update() {
        CheckSelection();
    }

    private void CheckSelection() {
        // if the user clicks somewhere on the screen
        // if (Input.GetButtonDown("Fire1")) {
        if (Input.GetMouseButtonDown(0)) {
            _isDragging = true; // by default the user is dragging the mouse
            _mouseStartScreenPosition = Input.mousePosition;
            // if the user clicks somewhere on the screen
            RaycastHit hit;
            Ray ray = Init.MainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask(Init.Tags.Troop, Init.Tags.Ground))) {
                // Debug.Log("hit " + LayerMask.LayerToName(hit.transform.gameObject.layer));
                // maybe get the rays that hit only a specific layer??
                Transform objectHit = hit.transform;
                if (objectHit.CompareTag(Init.Tags.Troop)) {
                    foreach (var player in Init.Players) {
                        var p = Init.localPlayer;
                        if (p.isLocalPlayer) {
                            // select troop if it's the player's troop
                            var t = objectHit.GetComponent<Troop>();
                            t.isSelected = true;
                            if(!p.selectedTroops.Contains(t)) p.selectedTroops.Add(t);
                        }
                        else {
                            foreach (var selectedTroop in p.selectedTroops) {
                                if (selectedTroop.target == null) {
                                    selectedTroop.target = objectHit;
                                    objectHit.GetComponent<Troop>().targetedBy.Add(selectedTroop);
                                }
                            }
                        }
                    }
                }
                
                if (objectHit.gameObject.layer == LayerMask.NameToLayer(Init.Tags.Ground)) {
                    // get the point where the ray hits the ground plane
                    _moveTo.transform.position = hit.point;
                    // Debug.Log("Map hit - Set pos to " + _moveTo);
                    // go through all the selected units and move them
                    foreach (var troop in Init.localPlayer.troops) {
                        Troop t = troop.GetComponent<Troop>();
                        if (t.isSelected) {
                            t.moveTo = _moveTo.transform;
                        }
                    }
                }
            }
        }

        // if the user releases the mouse left button then it means he has stopped dragging the mouse to select units
        if (Input.GetMouseButtonUp(0)) _isDragging = false;

        Init.Instance.selectionBoxImg.gameObject.SetActive(_isDragging);
        if (_isDragging) {
            // check this code --> https://wiki.unity3d.com/index.php/SelectionBox
            Bounds b = new Bounds();
            // The center of the bounds is in between the start position and current position of the mouse
            // The value returned equals (b - a) * t. When t = 0 returns a. When t = 1 returns b. When t = 0.5 returns the point midway between a and b
            b.center = Vector3.Lerp(_mouseStartScreenPosition, Input.mousePosition, 0.5f);
            // We make the size absolute (negative bounds don't contain anything)
            b.size = new Vector3(Mathf.Abs(_mouseStartScreenPosition.x - Input.mousePosition.x),
                Mathf.Abs(_mouseStartScreenPosition.y - Input.mousePosition.y), 0);

            // To display our selection box image in the same place as our bounds
            if (_selectionBoxImgRectTransform != null) {
                _selectionBoxImgRectTransform.position = b.center;
                _selectionBoxImgRectTransform.sizeDelta = _canvas.transform.InverseTransformVector(b.size);
            }

            // Looping through all the troops
            foreach (var troop in Init.localPlayer.troops) {
                // If the screenPosition of the world object troop is within our selection bounds, we can add it to our selection
                Vector3 screenPos = Init.MainCamera.WorldToScreenPoint(troop.transform.position);
                screenPos.z = 0;
                if (b.Contains(screenPos)) {
                    // it means that the troop is inside the selecting box
                    var t = troop.GetComponent<Troop>();
                    t.isSelected = true;
                    if(!Init.localPlayer.selectedTroops.Contains(t)) Init.localPlayer.selectedTroops.Add(t);
                }
            }
        }

        // if the players wants to deselect troops, they will press the key "q" from the keyboard
        if (Input.GetKeyDown("q")) {
            foreach (var troop in Init.localPlayer.selectedTroops) {
                troop.GetComponent<Troop>().isSelected = false;
            }
            Init.localPlayer.selectedTroops.Clear();
        }

        /*
        // change from controlled troop to third person
        if (Input.GetKeyDown(KeyCode.Space)) {
            Init.Instance.ChangeControlledTroop(null, false);
        }
        
        // change controlled troop
        if (Input.GetKeyDown(KeyCode.Tab)) {
            
            if (Init.localPlayer.troops.Count > 0) {
                if (Init.Instance.currentControlledTroop + 1 == Init.localPlayer.troops.Count) {
                    Init.Instance.currentControlledTroop = 0;
                }
                else Init.Instance.currentControlledTroop++;
                Init.Instance.ChangeControlledTroop(Init.localPlayer.troops[Init.Instance.currentControlledTroop].transform, true);
            }
        }
        */
    }
}