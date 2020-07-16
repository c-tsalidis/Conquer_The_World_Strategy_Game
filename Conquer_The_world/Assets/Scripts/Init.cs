using System;
using System.Collections.Generic;
using GameStrings;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Init : MonoBehaviour {
    // instance of the Init
    public static Init Instance;

    // instances of all the game strings
    public static SceneNames SceneNames;
    public static Tags Tags;
    private static UnitSelectionManager _unitSelectionManager;

    // instance of the Player
    public static PlayerData PlayerData;
    
    // instance of the camera movement controller
    private CameraController _cameraController;

    // main camera
    public static Camera MainCamera;
    
    // materials
    [SerializeField] private Material [] troopMaterials;

    // selection box image
    public RawImage selectionBoxImg;
    public Canvas canvas;

    private void Awake() {
        // Create Init if it doesn't exist --> singleton
        if (Instance == null) Instance = this;
        else if (Instance == this) Destroy(gameObject);
        // Don't delete when switching scenes
        DontDestroyOnLoad(gameObject);

        // initialize the game strings instances
        SceneNames = new SceneNames();
        Tags = new Tags();

        // initialize the unit selection manager instance and add it to the Init game object
        _unitSelectionManager = gameObject.AddComponent<UnitSelectionManager>();

        // initialize the player instance
        PlayerData = gameObject.AddComponent<PlayerData>();
        
        // initialize camera controller
        _cameraController = gameObject.AddComponent<CameraController>();

        // finally, set up the game with the player progress
        SetUpGame();
    }

    private void SetUpGame() {
        // get the player saved data values
        PlayerData.GetSavedDataValues();
        
        if(MainCamera == null) MainCamera = Camera.main;
        LoadBattle();
    }

    public void LoadBattle() {
        // load the city prefab
        GameObject map = Instantiate(Resources.Load("Prefabs/Map")) as GameObject;
        if (map != null) {
            var t = map.transform.Find("Ground_Plane");
            if(t != null) t.tag = Tags.Ground;
        }
        // create the troops
        CreateTroops();
    }

    // TODO --> Move this method to the Troop class
    private void CreateTroops() {
        // if player doesn't have any troops, give him 5 of each troop type: 5 swordsmen and 5 archers
        if (PlayerData.troops.Count <= 0) {
            for (int i = 0; i < 10; i++) {
                // var troop = Instantiate(Resources.Load("Prefabs/Troop"), transform.position + Vector3.right * (i + 1) + Vector3.up, Quaternion.identity) as GameObject;
                var troop = Instantiate(Resources.Load("Prefabs/Troop"), new Vector3(Random.Range(-40, 40), 0, Random.Range(-40, 40)), Quaternion.identity) as GameObject;
                if (troop != null) {
                    Troop t = troop.GetComponent<Troop>();
                    if (i < 5) t.troopType = Troop.TroopType.Swordsman;
                    else t.troopType = Troop.TroopType.Archer;
                    t.tag = Tags.PlayerTroop;
                    t.PopulateInstance(1);
                    // t.transform.Find("Body_Visuals").GetComponent<Renderer>().material = troopMaterials[0]; // player troop material
                    PlayerData.troops.Add(troop);
                }
                else Debug.Log("Troop prefab is null");
            }
        }
        
        // create the enemies
        for (int i = 0; i < 10; i++) {
            // var troop = Instantiate(Resources.Load("Prefabs/Troop"), transform.position + (Vector3.left* 10) + Vector3.right * (i + 1) + Vector3.up, Quaternion.identity) as GameObject;
            var troop = Instantiate(Resources.Load("Prefabs/Troop"), new Vector3(Random.Range(-40, 40), 0, Random.Range(-40, 40)), Quaternion.identity) as GameObject;
            if (troop != null) {
                Troop t = troop.GetComponent<Troop>();
                if (i < 5) t.troopType = Troop.TroopType.Swordsman;
                else t.troopType = Troop.TroopType.Archer;
                t.PopulateInstance(1);
                // t.transform.Find("Body_Visuals").GetComponent<Renderer>().material = troopMaterials[1]; // enemy material
                t.tag = Tags.Enemy;
            }
            else Debug.Log("Troop prefab is null");
        }

        // foreach (var troop in playerData.troops) { }
    }

    private void Update() {
        if(MainCamera != null) _cameraController.CheckInput();
    }
}