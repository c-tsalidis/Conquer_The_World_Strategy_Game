using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    
    public string playerName;
    public List<GameObject> troops;
    public int [] citiesConquered;
    public bool isLocalPlayer;

    public List<Troop> selectedTroops = new List<Troop>();
    
    public Player() {
        troops = new List<GameObject>();
    }

    public void GetSavedDataValues() {
        // this will be used to get the saved data values --> player information (game progress...)
    }
}