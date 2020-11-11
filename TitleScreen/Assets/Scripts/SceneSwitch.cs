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
        if(Controller.getGame() == -1 || (Controller.getDifficulty() == -1 && Controller.getGameMode() == 0))
        {
            UnityEngine.Debug.Log("No game/difficulty selected");
            return;
        }

        //League mode for Connect4
        if(Controller.getGame() == 1 && Controller.getGameMode() == 1)
        {
            UnityEngine.Debug.Log("Not currently implemented");
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
        SceneManager.LoadScene(SceneName);
    }
}
