using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour {
    // the player has
    // stats
    // troops
    // ...
    
    public string playerName;
    public List<GameObject> troops;
    public int [] citiesConquered;

    public List<Troop> selectedTroops = new List<Troop>();
    
    public PlayerData() {
        troops = new List<GameObject>();
    }

    public void GetSavedDataValues() {
        // this will be used to get the saved data values --> player information (game progress...)
    }
}