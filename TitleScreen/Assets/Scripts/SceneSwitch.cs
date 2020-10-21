using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitch : MonoBehaviour
{
    GameObject Button;
    GameController Controller;
    public void playGame()
    {
        Button = GameObject.Find("GameController");
        Controller = Button.GetComponent<GameController>();
        
        if(Controller.game == -1 || Controller.difficulty == -1)
        {
            UnityEngine.Debug.Log("No game/difficulty selected");
            return;
        }

        //Use Controller.game to load the scene for the corresponding game
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
