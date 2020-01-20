using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelectionManager : MonoBehaviour {
    public static Vector3 moveTo;
    private bool isDragging;
    private Vector3 startScreenPosition;

    void Update() {
        CheckSelection();
    }

    private void CheckSelection() {
        // if the user clicks somewhere on the screen
        // if (Input.GetButtonDown("Fire1")) {
        if (Input.GetMouseButtonDown(0)) {
            isDragging = true; // by default the user is dragging the mouse
            startScreenPosition = Input.mousePosition;
            // if the user clicks somewhere on the screen
            RaycastHit hit;
            Ray ray = Init.MainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit)) {
                // maybe get the rays that hit only a specific layer??
                Transform objectHit = hit.transform;
                if (objectHit.CompareTag(Init.Tags.PlayerTroop)) {
                    Debug.Log("Selected " + objectHit.transform.name);
                    var t = objectHit.GetComponent<Troop>();
                    t.isSelected = true;
                }
                else if (objectHit.CompareTag(Init.Tags.Ground)) {
                    // get the point where the ray hits the ground plane
                    moveTo = hit.point;
                    Debug.Log("Map hit - Set pos to " + moveTo);
                    // go through all the selected units and move them
                    foreach (var troop in Init.PlayerData.troops) {
                        Troop t = troop.GetComponent<Troop>();
                        if (t.isSelected) {
                            t.moveTo = moveTo;
                        }
                    }
                }
            }
        }

        // if the user releases the mouse left button then it means he has stopped dragging the mouse to select units
        if (Input.GetMouseButtonUp(0)) {
            isDragging = false;
        }

        // selectionBox.gameObject.SetActive(isSelecting);
        if (isDragging) {
            // check this code --> https://wiki.unity3d.com/index.php/SelectionBox
            Bounds b = new Bounds();
            // The center of the bounds is inbetween startpos and current pos
            b.center = Vector3.Lerp(startScreenPosition, Input.mousePosition, 0.5f);
            // We make the size absolute (negative bounds don't contain anything)
            b.size = new Vector3(Mathf.Abs(startScreenPosition.x - Input.mousePosition.x),
                Mathf.Abs(startScreenPosition.y - Input.mousePosition.y), 0);

            // To display our selection box image in the same place as our bounds
            // rt.position = b.center;
            // rt.sizeDelta = canvas.transform.InverseTransformVector(b.size);

            // Looping through all the troops
            foreach (var troop in Init.PlayerData.troops) {
                //If the screenPosition of the worldobject is within our selection bounds, we can add it to our selection
                Vector3 screenPos = Init.MainCamera.WorldToScreenPoint(troop.transform.position);
                screenPos.z = 0;
                if (b.Contains(screenPos)) {
                    // it means that the troop is inside the selecting box
                    var t = troop.GetComponent<Troop>();
                    t.isSelected = true;
                }
            }
        }

        // if the players wants to deselect troops, they will press the key "q" from the keyboard
        if (Input.GetKeyDown("q")) {
            foreach (var troop in Init.PlayerData.troops) {
                Troop t = troop.GetComponent<Troop>();
                if (t != null) {
                    if (t.isSelected) t.isSelected = false;
                }
            }
        }
    }
}