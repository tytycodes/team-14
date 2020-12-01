using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneSwitch : MonoBehaviour
{
    GameController Controller;
    GameObject MainCanvas;
    GameObject LoadingCanvas;
    AsyncOperation loadingOperation;
    Slider progressBar;

    public void playGame()
    {
        Controller = GameObject.Find("GameController").GetComponent<GameController>();
        MainCanvas = GameObject.Find("MainCanvas");
        LoadingCanvas = GameObject.Find("LoadingCanvas");
        progressBar = GameObject.Find("LoadingBar").GetComponent<Slider>();
        
        //Check if the game and or difficulty are selected
        if(Controller.getGame() == -1 || (Controller.getDifficulty() == -1 && Controller.getGameMode() == 0))
        {
            UnityEngine.Debug.Log("No game/difficulty selected");
            return;
        }

        //Use Controller.getGame() to load the scene for the corresponding game
        string SceneName = "";
        switch (Controller.getGame())
        {
            case (0):   SceneName = "TicTacToe";
                        break;
            case (1):   SceneName = "Connect4";
                        break;
        }
        MainCanvas.SetActive(false);
        LoadingCanvas.SetActive(true);
        loadingOperation = SceneManager.LoadSceneAsync(SceneName);
    }

    void Update()
    {
        if(loadingOperation != null)
        {
            progressBar.value = Mathf.Clamp01(loadingOperation.progress / 0.9f);
        }
    }
}
