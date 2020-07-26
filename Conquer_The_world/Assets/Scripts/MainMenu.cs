using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void ChooseCharacter(string character) {
        PlayerPrefs.SetString("chosenCharacter", character);
        SceneManager.LoadScene("Scenes/BattleScene");
    }
}
