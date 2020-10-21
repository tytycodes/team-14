using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using IronPython.Hosting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public int playerTurn;
    public int turnCount;
    public int game;
    private dynamic Agent;
    private dynamic Board;
    private dynamic xScore;
    private dynamic yScore;
    public int difficulty;
    public Sprite[] playerIcon;
    public UnityEngine.UI.Button[] ticTacToeSpace;
    public UnityEngine.UI.Button[] gameButton;
    public UnityEngine.UI.Button[] difficultyButton;

    // Start is called before the first frame update
    void Start()
    {
        game = -1;
        difficulty = -1;
    }

    // called first
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // called second
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case ("TicTacToe"):
                UnityEngine.Debug.Log("Setting up TicTacToe");
                GameSetup();
                break;
            default:
                UnityEngine.Debug.Log("Not TicTacToe");
                break;
        }
    }

    public void GameSetup()
    {
        var engine = Python.CreateEngine();

        ICollection<string> searchPaths = engine.GetSearchPaths();

        //Path to the folder of Agent and BoardEnvironment
        searchPaths.Add(Application.dataPath + @"\Scripts\");
        //Path to the Python standard library
        searchPaths.Add(Application.dataPath + @"\Plugins\Lib\");
        engine.SetSearchPaths(searchPaths);

        string diff = "";
        switch (difficulty) {
            case 0: diff = @"\Scripts\easy.txt";
                    break;
            case 1: diff = @"\Scripts\medium.txt";
                    break;
            case 2: diff = @"\Scripts\hard.txt";
                    break;
        }

        dynamic tempagent = engine.ExecuteFile(Application.dataPath + @"\Scripts\Agent.py");
        dynamic tempboard = engine.ExecuteFile(Application.dataPath + @"\Scripts\BoardEnvironment.py");
        Board = tempboard.BoardEnvironment();
        Agent = tempagent.Agent(Board, Application.dataPath + diff);

        xScore = GameObject.Find("xScoreText").GetComponent<Text>();
        yScore = GameObject.Find("yScoreText").GetComponent<Text>();

        xScore.text = "0";
        yScore.text = "0";

        //explicitly set the tictactoe spaces. Fails otherwise
        ticTacToeSpace[0] = GameObject.Find("7").GetComponent<Button>();
        ticTacToeSpace[1] = GameObject.Find("8").GetComponent<Button>();
        ticTacToeSpace[2] = GameObject.Find("9").GetComponent<Button>();
        ticTacToeSpace[3] = GameObject.Find("4").GetComponent<Button>();
        ticTacToeSpace[4] = GameObject.Find("5").GetComponent<Button>();
        ticTacToeSpace[5] = GameObject.Find("6").GetComponent<Button>();
        ticTacToeSpace[6] = GameObject.Find("1").GetComponent<Button>();
        ticTacToeSpace[7] = GameObject.Find("2").GetComponent<Button>();
        ticTacToeSpace[8] = GameObject.Find("3").GetComponent<Button>();

        System.Random r = new System.Random();
        int num = r.Next(1, 100);
        playerTurn = (num > 50) ? 0 : 1;
        //UnityEngine.Debug.Log(num);
        //UnityEngine.Debug.Log(playerTurn);
        turnCount = 0;
        for(int i = 0; i < ticTacToeSpace.Length; i++)
        {
            ticTacToeSpace[i].interactable = true;
            ticTacToeSpace[i].GetComponent<UnityEngine.UI.Image>().sprite = null;
        }
        AITurn();
    }

    void AITurn()
    {
        if (playerTurn == 1)
        {
            TicTacToeButton(Agent.select_action());
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TicTacToeButton(int cellNumber)
    {
        ticTacToeSpace[cellNumber].image.sprite = playerIcon[playerTurn];
        ticTacToeSpace[cellNumber].interactable = false;

        char choice = playerTurn == 0 ? 'X' : 'O';
        if(choice == 'X') { xScore.text = (System.Convert.ToInt32(xScore.text) + 1).ToString(); }
        else yScore.text = (System.Convert.ToInt32(yScore.text) + 1).ToString();
        Board.select_piece(cellNumber, choice);

        playerTurn++;
        playerTurn %= 2;

        if (Board.winner() != "")
        {
            UnityEngine.Debug.Log(Board.winner() + " wins!");
            for (int i = 0; i < 9; i++)
            {
                ticTacToeSpace[i].interactable = false;
                //Set null sprite images to have 0 alpha to prevent weird box showing up on uninteractable
                if (ticTacToeSpace[i].image.sprite == null)
                {
                    dynamic Color1 = ticTacToeSpace[i].colors;
                    Color1.disabledColor = new Color(ticTacToeSpace[i].colors.normalColor.r, 
                                                     ticTacToeSpace[i].colors.normalColor.g, 
                                                     ticTacToeSpace[i].colors.normalColor.b, 
                                                     0.0f);
                    ticTacToeSpace[i].colors = Color1;
                }
            }
        }
        else if (Board.is_full())
        {
            UnityEngine.Debug.Log("Tie!");
        }
        else
        {
            AITurn();
        }
    }

    public void gameButtonSelect(int cellNumber)
    {
        //gameButton[cellNumber].GetComponent<Image>().color = Color.white;
        //UnityEngine.Debug.Log(cellNumber);
        gameButton[cellNumber].interactable = false;
        game = cellNumber;
        for(int i = 0; i < 3; i++)
        {
            if (i != cellNumber)
            {
                //UnityEngine.Debug.Log(i + " deselected");
                gameButton[i].interactable = true;
            }
        }
    }

    public void difficultyButtonSelect(int cellNumber)
    {
        //difficultyButton[cellNumber].GetComponent<Image>().color = Color.white;
        //UnityEngine.Debug.Log("Selected a difficulty");
        difficultyButton[cellNumber].interactable = false;
        difficulty = cellNumber;
        for (int i = 0; i < 3; i++)
        {
            if (i != cellNumber)
            {
                //UnityEngine.Debug.Log(i + " deselected");
                difficultyButton[i].interactable = true;
            }
        }
    }
}
