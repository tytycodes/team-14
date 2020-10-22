using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitch : MonoBehaviour
{
    GameController Controller;

    public void playGame()
    {
        Controller = GameObject.Find("GameController").GetComponent<GameController>();
        
        //Check if the game and or difficulty are selected
        if(Controller.game == -1 || Controller.difficulty == -1)
        {
            UnityEngine.Debug.Log("No game/difficulty selected");
            return;
        }

        //Use Controller.game to load the scene for the corresponding game
        string SceneName = "";
        switch (Controller.game)
        {
            case (0):   SceneName = "TicTacToe";
                        break;
        }
        SceneManager.LoadScene(SceneName);
    }
}
