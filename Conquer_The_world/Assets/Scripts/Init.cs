using System;
using GameStrings;
using UnityEngine;

public class Init : MonoBehaviour {
    // instance of the Init
    public static Init Instance;

    // instances of all the game strings
    public static SceneNames sceneNames;
    public static Tags tags;
    private static UnitSelectionManager _unitSelectionManager;

    // instance of the Player
    public static PlayerData playerData;

    // main camera
    public static Camera MainCamera;

    private void Awake() {
        // Create Init if it doesn't exist --> singleton
        if (Instance == null) Instance = this;
        else if (Instance == this) Destroy(gameObject);
        // Don't delete when switching scenes
        DontDestroyOnLoad(gameObject);

        // initialize the game strings instances
        sceneNames = new SceneNames();
        tags = new Tags();

        // initialize the unit selection manager instance and add it to the Init game object
        _unitSelectionManager = gameObject.AddComponent<UnitSelectionManager>();

        // initialize the player instance
        playerData = gameObject.AddComponent<PlayerData>();

        // finally, set up the game with the player progress
        SetUpGame();
    }

    private void SetUpGame() {
        // get the player saved data values
        playerData.GetSavedDataValues();
        
        if(MainCamera == null) MainCamera = Camera.main;
        LoadBattle();
    }

    public void LoadBattle() {
        // load the city prefab
        GameObject map = Instantiate(Resources.Load("Prefabs/Map")) as GameObject;
        if (map != null) map.tag = tags.Map;
        // create the troops
        CreateTroops();
    }

    private void CreateTroops() {
        // if player doesn't have any troops, give him 5 of each troop type: 5 swordsmen and 5 archers
        if (playerData.troops.Count <= 0) {
            for (int i = 0; i < 10; i++) {
                var troop = Instantiate(Resources.Load("Prefabs/Troop"), transform.position + Vector3.right * (i + 1), Quaternion.identity) as GameObject;
                if (troop != null) {
                    Troop t = troop.GetComponent<Troop>();
                    if (i < 5) t.troopType = Troop.TroopType.Swordsman;
                    else t.troopType = Troop.TroopType.Archer;
                    t.PopulateInstance(1);
                    t.tag = tags.PlayerTroop;
                    playerData.troops.Add(troop);
                }
                else Debug.Log("Troop prefab is null");
            }
        }

        // foreach (var troop in playerData.troops) { }
    }
}