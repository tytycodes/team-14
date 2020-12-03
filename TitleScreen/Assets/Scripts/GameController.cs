using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using IronPython.Hosting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    //Singleton model for script. Helps minimize unwanted behavior
    private static GameController _instance = null;

    public static GameController Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = GameObject.FindObjectOfType(typeof(GameController)) as GameController;
            }

            return _instance;
        }
    }

    private static int game;
    private static int difficulty;
    private static int gamemode;
    public UnityEngine.UI.Button[] gameButton;
    public UnityEngine.UI.Button[] difficultyButton;
    public UnityEngine.UI.Button[] modeButton;

    private GameObject E_Button;
    private GameObject M_Button;
    private GameObject H_Button;
    private GameObject Grid;
    private GameObject Call_Button;
    private GameObject Single_Button;
    private GameObject Double_Button;
    private GameObject Triple_Button;
    private GameObject Reset_Button;
    private GameObject Quit_Button;
    private GameObject Continue_Button;

    void Awake()
    {
        //Set the singleton gamecontroller
        _instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        E_Button = GameObject.Find("E_Button");
        M_Button = GameObject.Find("M_Button");
        H_Button = GameObject.Find("H_Button");
    }

    // called first
    void OnEnable()
    {
        //Add event to load scene
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // called second
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //return if this isn't the singleton gamecontroller
        if (this != _instance) return;
        switch (scene.name)
        {
            case ("TicTacToe"):
                //Start TicTacToe
                GameObject.Find("TicTacToe").GetComponent<TicTacToe>().TTTInit(gamemode, difficulty);
                break;
            case ("Connect4"):
                //Start Connect4
                GameObject.Find("Connect4").GetComponent<Connect4>().C4Init(gamemode, difficulty);
                break;
            case ("MainMenu"):
                //Set buttons to be automatically selected
                gameButton[0].interactable = false;
                game = 0;
                difficultyButton[0].interactable = false;
                difficulty = 0;
                modeButton[0].interactable = false;
                gamemode = 0;
                break;
        }
    }

    //return the selected game
    public int getGame()
    {
        return game;
    }

    //return the selected difficulty
    public int getDifficulty()
    {
        return difficulty;
    }

    //return the selected game mode
    public int getGameMode()
    {
        return gamemode;
    }
    
    //React to game selection buttons
    public void gameButtonSelect(int cellNumber)
    {
        //set current button to be uninteractable and set the selected game
        gameButton[cellNumber].interactable = false;
        game = cellNumber;
        for(int i = 0; i < 2; i++)
        {
            //set all buttons that aren't selected to be interactable
            if (i != cellNumber)
            {
                gameButton[i].interactable = true;
            }
        }
    }

    //React to difficulty selection buttons
    public void difficultyButtonSelect(int cellNumber)
    {
        //set current button to be uninteractable and set the selected difficulty
        difficultyButton[cellNumber].interactable = false;
        difficulty = cellNumber;
        for (int i = 0; i < 3; i++)
        {
            //set all buttons that aren't selected to be interactable
            if (i != cellNumber)
            {
                difficultyButton[i].interactable = true;
            }
        }
    }

    //React to the selected game mode
    public void modeButtonSelect(int cellNumber)
    {

        //set current button to be uninteractable and set the selected gamemode
        modeButton[cellNumber].interactable = false;
        gamemode = cellNumber;
        //Set other mode to be interactable
        modeButton[1 - cellNumber].interactable = true;
        //Disable the difficulty selections if league mode is selected
        if(cellNumber == 1)
        {
            E_Button.SetActive(false);
            M_Button.SetActive(false);
            H_Button.SetActive(false);
        }
        //Enable the difficulty selections if single match mode is selected
        else
        {
            E_Button.SetActive(true);
            M_Button.SetActive(true);
            H_Button.SetActive(true);
        }
    }
}
