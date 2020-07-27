using System;
using System.Collections.Generic;
using Cinemachine;
using GameStrings;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
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
    // public static PlayerData PlayerData;
    [SerializeField] private int numPlayers;
    [SerializeField] private GameObject playerPrefab;
    public static List<GameObject> Players = new List<GameObject>();
    public static GameObject [] playersCharacters;
    public static Player localPlayer;

    // instance of the camera movement controller
    private CameraController _cameraController;

    // main camera
    public static Camera MainCamera;

    // materials
    [SerializeField] private Material[] troopMaterials;

    // selection box image
    public RawImage selectionBoxImg;
    public Canvas canvas;

    // for the fps (frames per second)
    public float updateInterval = 0.5F;

    private float accum = 0; // FPS accumulated over the interval
    private int frames = 0; // Frames drawn over the interval
    private float timeleft; // Left time for current interval
    [SerializeField] private TextMeshProUGUI fpsText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI isAttackingText;

    public CinemachineFreeLook cinemachineFreeLook;
    [SerializeField] private GameObject thirdPersonGameObject;
    public int currentControlledTroop = 0;

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
        // PlayerData = gameObject.AddComponent<PlayerData>();

        // initialize camera controller
        _cameraController = gameObject.AddComponent<CameraController>();

        // finally, set up the game with the player progress
        SetUpGame();
    }

    private void SetUpGame() {
        // get the player saved data values
        // PlayerData.GetSavedDataValues();

        for (int i = 0; i < numPlayers; i++) {
            string playerName = "Player" + (i + 1);
            Players.Add(Instantiate(Resources.Load("Prefabs/Player")) as GameObject);
            var p = Players[i].GetComponent<Player>();
            p.playerName = playerName;
            Players[i].name = playerName;
            if (i == 0) {
                p.isLocalPlayer = true;
                localPlayer = p;
            }
        }
        playersCharacters = new GameObject[numPlayers];

        if (MainCamera == null) MainCamera = Camera.main;
        LoadBattle();
    }

    public void LoadBattle() {
        // create the troops
        CreateTroops();
    }

    // TODO --> Move this method to the Troop class
    private void CreateTroops() {
        int playerCounter = 0;
        // create the troops for each player
        foreach (var player in Players) {
            var p = player.GetComponent<Player>();
            for (int i = 0; i < 10; i++) {
                // var troop = Instantiate(Resources.Load("Prefabs/Troop"), transform.position + Vector3.right * (i + 1) + Vector3.up, Quaternion.identity) as GameObject;
                var troop = Instantiate(Resources.Load("Prefabs/Troop"),
                    new Vector3(Random.Range(-40, 40), 0, Random.Range(-40, 40)), Quaternion.identity) as GameObject;
                troop.transform.SetParent(player.transform);
                if (troop != null) {
                    Troop t = troop.GetComponent<Troop>();
                    if (i % 2 == 0 && i != 0) t.troopType = Troop.TroopType.Swordsman;
                    else t.troopType = Troop.TroopType.Archer;
                    if (i == 0 && p.isLocalPlayer) {
                        string chosenCharacter = PlayerPrefs.GetString("chosenCharacter");
                        if (chosenCharacter == "archer") t.troopType = Troop.TroopType.Archer;
                        else t.troopType = Troop.TroopType.Swordsman;
                        t.isControlled = true;
                        // troop.AddComponent<ThirdPersonMovement>();
                        t.PopulateInstance(p, 10);
                        playersCharacters[playerCounter] = troop;
                    }
                    else t.PopulateInstance(p, 1);
                    t.tag = Tags.Troop;
                    
                    p.troops.Add(troop);
                }
                else Debug.Log("Troop prefab is null");
            }

            playerCounter++;
        }
    }

    public static Player GetLocalPlayer() {
        Player currentPlayer = null;
        foreach (var player in Players) {
            var p = player.GetComponent<Player>();
            if (p.isLocalPlayer) {
                currentPlayer = p;
                break;
            }
        }

        return currentPlayer;
    }

    private void Update() {
        // if (MainCamera != null) _cameraController.CheckInput();
        CalculateFPS();
    }
    
    private void CalculateFPS() {
        timeleft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        frames++;

        // Interval ended - update GUI text and start new interval
        if (timeleft <= 0.0) {
            // display two fractional digits (f2 format)
            float fps = accum / frames;
            string format = System.String.Format("{0:F2} FPS", fps);
            fpsText.text = format;
            timeleft = updateInterval;
            accum = 0.0F;
            frames = 0;
        }
    }

    public void ChangeControlledTroop(Transform t, bool isTroop) {
        if (isTroop) {
            cinemachineFreeLook.Follow = t;
            cinemachineFreeLook.LookAt = t;
        }
        else {
            cinemachineFreeLook.Follow = thirdPersonGameObject.transform;
            cinemachineFreeLook.LookAt = thirdPersonGameObject.transform;
        }
    }
}