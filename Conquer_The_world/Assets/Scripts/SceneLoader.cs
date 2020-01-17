using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour {
    public void LoadScene(string name) {
        if (name == null) throw new ArgumentNullException(nameof(name));
        // after the  exception has been thrown the rest of the code won't be executed
        SceneManager.LoadScene(name);
        Init.MainCamera = Camera.main;
    }
}