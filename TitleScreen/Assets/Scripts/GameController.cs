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
    //Singleton GameController so initialization doesn't occur twice
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

    public int playerTurn;
    public bool leagueChoice;
    public int turnCount;
    public int currBet;
    private static int game;
    private static int difficulty;
    private static int gamemode;
    public int AIChips;
    public int AIWins;
    public int PlayerChips;
    public int PlayerWins;
    public int ties;
    public int chip_mul;
    private dynamic Agent;
    private dynamic Board;
    private dynamic League;
    private dynamic LeagueAgent;
    private dynamic Util;
    private UnityEngine.UI.Text pChips;
    private UnityEngine.UI.Text aChips;
    public Sprite[] playerIcon;
    public UnityEngine.UI.Button[] ticTacToeSpace;
    public UnityEngine.UI.Button[] gameButton;
    public UnityEngine.UI.Button[] difficultyButton;
    public UnityEngine.UI.Button[] modeButton;

    private GameObject E;
    private GameObject E_Button;
    private GameObject M;
    private GameObject M_Button;
    private GameObject H;
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
        E = GameObject.Find("E");
        E_Button = GameObject.Find("E_Button");
        M = GameObject.Find("M");
        M_Button = GameObject.Find("M_Button");
        H = GameObject.Find("H");
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
                Grid = GameObject.Find("Grid");
                Call_Button = GameObject.Find("Button_Call");
                Single_Button = GameObject.Find("Button_Single");
                Double_Button = GameObject.Find("Button_Double");
                Triple_Button = GameObject.Find("Button_Triple");
                Reset_Button = GameObject.Find("Button_Reset");
                Quit_Button = GameObject.Find("Button_Quit");
                Continue_Button = GameObject.Find("Button_Continue");
                if (gamemode == 0)
                {
                    League = null;
                    HideButtons();
                    Reset_Button.SetActive(true);
                    Quit_Button.SetActive(true);
                    GameObject.Find("PlayerChips").SetActive(false);
                    GameObject.Find("AIChips").SetActive(false);
                    TicTacToeSingle();
                }
                else 
                {
                    AIChips = 100;
                    AIWins = 0;
                    PlayerChips = 100;
                    PlayerWins = 0;
                    ties = 0;
                    currBet = 5;
                    Reset_Button.SetActive(false);
                    TicTacToeLeague(); 
                }
                break;
            case ("SampleScene"):
                gameButton[0].interactable = false;
                game = 0;
                difficultyButton[0].interactable = false;
                difficulty = 0;
                modeButton[0].interactable = false;
                gamemode = 0;
                break;
        }
    }

    public int getGame()
    {
        return game;
    }

    public int getDifficulty()
    {
        return difficulty;
    }

    public int getGameMode()
    {
        return gamemode;
    }

    public void SetTicTacToeBoard()
    {
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
    }

    public void ShowButtons(bool turn)
    {
        Grid.SetActive(false);
        if (turn)
        {
            Call_Button.SetActive(false);
            Single_Button.SetActive(true);
            Double_Button.SetActive(true);
            Triple_Button.SetActive(true);
            Quit_Button.SetActive(true);
        }
        else
        {
            Call_Button.SetActive(true);
            Single_Button.SetActive(false);
            Double_Button.SetActive(false);
            Triple_Button.SetActive(false);
            Quit_Button.SetActive(true);
        }
    }

    public void HideButtons()
    {
        Grid.SetActive(true);
        Call_Button.SetActive(false);
        Single_Button.SetActive(false);
        Double_Button.SetActive(false);
        Triple_Button.SetActive(false);
        Reset_Button.SetActive(false);
        Quit_Button.SetActive(false);
        Continue_Button.SetActive(false);
    }

    public void TicTacToeSingle()
    {
        var engine = Python.CreateEngine();

        ICollection<string> searchPaths = engine.GetSearchPaths();

        //Path to the folder of Agent and BoardEnvironment
        searchPaths.Add(Application.dataPath + @"\Scripts\");
        //Path to the Python standard library
        searchPaths.Add(Application.dataPath + @"\Plugins\Lib\");
        engine.SetSearchPaths(searchPaths);

        //Load in the difficulty q-table
        string diff = "";
        switch (difficulty)
        {
            case 0:
                diff = @"\Scripts\easy.txt";
                break;
            case 1:
                diff = @"\Scripts\medium.txt";
                break;
            case 2:
                diff = @"\Scripts\hard.txt";
                break;
        }

        //load in the python scripts
        dynamic tempagent = engine.ExecuteFile(Application.dataPath + @"\Scripts\Agent.py");
        dynamic tempboard = engine.ExecuteFile(Application.dataPath + @"\Scripts\BoardEnvironment.py");
        Board = tempboard.BoardEnvironment();
        Agent = tempagent.Agent(Board, Application.dataPath + diff);
        TicTacToeSetup();
    }

    public void TicTacToeSetup()
    {
        Board.reset();

        SetTicTacToeBoard();

        System.Random r = new System.Random();
        int num = r.Next(1, 100);
        playerTurn = (num > 50) ? 0 : 1;
        //UnityEngine.Debug.Log(num);
        turnCount = 0;
        for(int i = 0; i < ticTacToeSpace.Length; i++)
        {
            ticTacToeSpace[i].interactable = true;
            ticTacToeSpace[i].GetComponent<UnityEngine.UI.Image>().sprite = null;
            dynamic Color1 = ticTacToeSpace[i].colors;
            Color1.disabledColor = new Color(ticTacToeSpace[i].colors.normalColor.r,
                                             ticTacToeSpace[i].colors.normalColor.g,
                                             ticTacToeSpace[i].colors.normalColor.b,
                                             0.502f);
            ticTacToeSpace[i].colors = Color1;
        }
        AITurn();
    }

    public void NewLeagueGame()
    {
        PlayerChips = 100;
        AIChips = 100;

        pChips = GameObject.Find("PlayerChipsText").GetComponent<Text>();
        aChips = GameObject.Find("AIChipsText").GetComponent<Text>();

        pChips.text = Convert.ToString(PlayerChips);
        aChips.text = Convert.ToString(AIChips);

        List<string> result = ((IList<object>)League.reset_pair()).Cast<string>().ToList();

        int ListIndex = 0;
        foreach (string item in result)
        {
            switch (ListIndex)
            {
                case 0:
                    UnityEngine.Debug.Log("New AI at index " + item);
                    Agent = Util.get_board_agent(int.Parse(item));
                    LeagueAgent = Util.get_league_agent(int.Parse(item));
                    break;
                case 1:
                    leagueChoice = (item == "False") ? false : true;
                    break;
            }
            ListIndex++;
        }
        PlayLeague();
    }

    public void PlayLeague()
    {
        if (!leagueChoice)
        {
            string choice = LeagueAgent.select_action(!leagueChoice);
            switch (choice)
            {
                case "quit":
                    UnityEngine.Debug.Log("AI chose " + choice);
                    ContinueGame();
                    break;
                case "single bet":
                    chip_mul = 1;
                    break;
                case "double bet":
                    chip_mul = 2;
                    break;
                case "triple bet":
                    chip_mul = 3;
                    break;
                default: break;
            }
            UnityEngine.Debug.Log("AI chose " + choice);
        }
        ShowButtons(leagueChoice);
    }

    public void TicTacToeLeague()
    {
        var engine = Python.CreateEngine();

        ICollection<string> searchPaths = engine.GetSearchPaths();

        //Path to the folder of Agent and BoardEnvironment
        searchPaths.Add(Application.dataPath + @"\Scripts\");
        //Path to the Python standard library
        searchPaths.Add(Application.dataPath + @"\Plugins\Lib\");
        engine.SetSearchPaths(searchPaths);

        //load in the python scripts
        dynamic tempagent = engine.ExecuteFile(Application.dataPath + @"\Scripts\Agent.py");
        dynamic tempboard = engine.ExecuteFile(Application.dataPath + @"\Scripts\BoardEnvironment.py");
        dynamic templeague = engine.ExecuteFile(Application.dataPath + @"\Scripts\LeagueEnvironment.py");
        dynamic temputil = engine.ExecuteFile(Application.dataPath + @"\Scripts\LeagueUtil.py");

        Board = tempboard.BoardEnvironment();
        League = templeague.LeagueEnvironment(Board);
        Util = temputil.LeagueUtil(Board, League);

        League.set_players(Util.get_names(), Util.get_leagues(), Util.get_boards());

        NewLeagueGame();
    }

    void AITurn()
    {
        //Check if it is the AI's turn
        if (playerTurn == 1)
        {
            TicTacToeButton(Agent.select_action(!leagueChoice));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TicTacToeButton(int cellNumber)
    {
        //Select space
        ticTacToeSpace[cellNumber].image.sprite = playerIcon[playerTurn];
        ticTacToeSpace[cellNumber].interactable = false;

        //Set next player's turn
        char choice = playerTurn == 0 ? 'X' : 'O';

        //Select piece in the python script
        Board.select_piece(cellNumber, choice);
        playerTurn++;
        playerTurn %= 2;

        //Check for a winner
        if (Board.winner() != "")
        {
            if (playerTurn == 1)
            {
                UnityEngine.Debug.Log("Player wins");
                if (League != null)
                {
                    PlayerWins++;
                    PlayerChips += chip_mul * currBet;
                    AIChips -= chip_mul * currBet;
                    pChips.text = Convert.ToString(PlayerChips);
                    aChips.text = Convert.ToString(AIChips);
                }
            }
            else
            {
                UnityEngine.Debug.Log("AI wins");
                if (League != null)
                {
                    AIWins++;
                    AIChips += chip_mul * currBet;
                    PlayerChips -= chip_mul * currBet;
                    pChips.text = Convert.ToString(PlayerChips);
                    aChips.text = Convert.ToString(AIChips);
                }
            }
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
            if (League != null)
            {
                if (AIChips <= 0 || PlayerChips <= 0)
                {
                    UnityEngine.Debug.Log("Chips ran out");
                    ContinueGame();
                }
                else
                {
                    leagueChoice = !leagueChoice;
                    PlayLeague();
                }
            }
        }
        //Check for ties
        else if (Board.is_full())
        {
            UnityEngine.Debug.Log("Tie");
            if (League != null)
            {
                ties++;
                leagueChoice = !leagueChoice;
                PlayLeague();
            }
        }
        else
        {
            //Otherwise, make the next turn
            AITurn();
        }
    }

    public void ContinueGame()
    {
        HideButtons();
        Call_Button.SetActive(false);
        Grid.SetActive(false);
        Quit_Button.SetActive(true);
        Continue_Button.SetActive(true);
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

    public void modeButtonSelect(int cellNumber)
    {
        modeButton[cellNumber].interactable = false;
        gamemode = cellNumber;
        modeButton[1 - cellNumber].interactable = true;
        if(cellNumber == 1)
        {
            E.SetActive(false);
            E_Button.SetActive(false);
            M.SetActive(false);
            M_Button.SetActive(false);
            H.SetActive(false);
            H_Button.SetActive(false);
        }
        else
        {
            E.SetActive(true);
            E_Button.SetActive(true);
            M.SetActive(true);
            M_Button.SetActive(true);
            H.SetActive(true);
            H_Button.SetActive(true);
        }
    }

    public void setChipMul(int cellNumber)
    {
        chip_mul = cellNumber;
        string choice = LeagueAgent.select_action(!leagueChoice);
        UnityEngine.Debug.Log("AI chose " + choice);
        if (choice == "quit")
        {
            ContinueGame();
        }
        else
        {
            HideButtons();
            TicTacToeSetup();
        }
    }

    public void CallBet()
    {
        HideButtons();
        TicTacToeSetup();
    }
}
