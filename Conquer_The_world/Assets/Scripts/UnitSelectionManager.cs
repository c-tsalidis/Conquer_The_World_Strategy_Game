using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelectionManager : MonoBehaviour {
    public static Vector3 moveTo;

    void Update() {
        CheckSelection();
    }

    private void CheckSelection() {
        // if the user clicks somewhere on the screen
        if (Input.GetButtonDown("Fire1")) {
            // if the user clicks somewhere on the screen
            RaycastHit hit;
            Ray ray = Init.MainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit)) {
                Transform objectHit = hit.transform;
                // moveTo = ray.direction;
                if (objectHit.CompareTag(Init.tags.PlayerTroop)) {
                    Debug.Log("Selected " + objectHit.transform.name);
                    var t = objectHit.GetComponent<Troop>();
                    t.isSelected = true;
                }
                else if (objectHit.CompareTag(Init.tags.Map)) {
                    // get the point where the ray hits the ground plane
                    Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
                    if (groundPlane.Raycast(ray, out float rayDistance)) {
                        Vector3 point = ray.GetPoint(rayDistance);
                        moveTo = point;
                    }
                    Debug.Log("Map hit - Set pos to " + moveTo);
                    // go through all the selected units and move them
                    foreach (var troop in Init.playerData.troops) {
                        Troop t = troop.GetComponent<Troop>();
                        if (t.isSelected) {
                            t.moveTo = moveTo;
                        }
                    }
                }
            }
        }
        // if the players wants to deselect troops, they will press the key "q" from the keyboard
        if (Input.GetKeyDown("q")) {
            foreach (var troop in Init.playerData.troops) {
                Troop t = troop.GetComponent<Troop>();
                if (t != null) {
                    if(t.isSelected) t.isSelected = false;
                }
            }
        }
    }
}