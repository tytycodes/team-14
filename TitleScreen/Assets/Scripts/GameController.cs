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
    public int playerTurn2;
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
    public Sprite[] playerIcon2;
    public UnityEngine.UI.Button[] ticTacToeSpace;
    public UnityEngine.UI.Button[] gameButton;
    public UnityEngine.UI.Button[] difficultyButton;
    public UnityEngine.UI.Button[] modeButton;
    public UnityEngine.UI.Button[] connectFourSpace;

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
            case ("Connect4"):
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
                    Connect4Single();
                }
                /*else
                {
                    AIChips = 100;
                    AIWins = 0;
                    PlayerChips = 100;
                    PlayerWins = 0;
                    ties = 0;
                    currBet = 5;
                    Reset_Button.SetActive(false);
                    //Connect4League();
                }*/
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
        for(int i = 0; i < 9; i++)
        {
            ticTacToeSpace[i] = GameObject.Find(i.ToString()).GetComponent<Button>();
        }
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
        searchPaths.Add(Application.dataPath + @"\Scripts\TicTacToe\");
        //Path to the Python standard library
        searchPaths.Add(Application.dataPath + @"\Plugins\Lib\");
        engine.SetSearchPaths(searchPaths);

        //Load in the difficulty q-table
        string diff = "";
        switch (difficulty)
        {
            case 0:
                diff = @"\Scripts\TicTacToe\easy.txt";
                break;
            case 1:
                diff = @"\Scripts\TicTacToe\medium.txt";
                break;
            case 2:
                diff = @"\Scripts\TicTacToe\hard.txt";
                break;
        }

        //load in the python scripts
        dynamic tempagent = engine.ExecuteFile(Application.dataPath + @"\Scripts\TicTacToe\Agent.py");
        dynamic tempboard = engine.ExecuteFile(Application.dataPath + @"\Scripts\TicTacToe\BoardEnvironment.py");
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
        TTT_AITurn();
    }

    public void NewTTTLeagueGame()
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
        PlayTTTLeague();
    }

    public void PlayTTTLeague()
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

        NewTTTLeagueGame();
    }

    void TTT_AITurn()
    {
        //Check if it is the AI's turn
        if (playerTurn == 1)
        {
            TicTacToeButton(Agent.select_action(!leagueChoice));
        }
    }

    public void Connect4Single()
    {
        var engine = Python.CreateEngine();

        ICollection<string> searchPaths = engine.GetSearchPaths();

        //Path to the folder of Agent and BoardEnvironment
        searchPaths.Add(Application.dataPath + @"\Scripts\Connect4\");
        //Path to the Python standard library
        searchPaths.Add(Application.dataPath + @"\Plugins\Lib\");
        engine.SetSearchPaths(searchPaths);

        //Load in the difficulty q-table
        string diff = "";
        switch (difficulty)
        {
            case 0:
                diff = @"\Scripts\Connect4\easy.txt";
                break;
            case 1:
                diff = @"\Scripts\Connect4\medium.txt";
                break;
            case 2:
                diff = @"\Scripts\Connect4\hard.txt";
                break;
        }

        //load in the python scripts
        dynamic tempagent = engine.ExecuteFile(Application.dataPath + @"\Scripts\Connect4\Agent.py");
        dynamic tempboard = engine.ExecuteFile(Application.dataPath + @"\Scripts\Connect4\BoardEnvironment.py");
        Board = tempboard.BoardEnvironment();
        Agent = tempagent.Agent(Board, Application.dataPath + diff);
        Connect4Setup();
    }

    public void Connect4Setup()
    {
        Board.reset();

        SetConnect4Board();

        System.Random r = new System.Random();
        int num = r.Next(1, 100);
        playerTurn2 = (num > 50) ? 0 : 1;
        //UnityEngine.Debug.Log(num);
        turnCount = 0;
        for (int i = 0; i < connectFourSpace.Length; i++)
        {
            connectFourSpace[i].interactable = true;
            connectFourSpace[i].GetComponent<UnityEngine.UI.Image>().sprite = null;
            dynamic Color1 = connectFourSpace[i].colors;
            Color1.disabledColor = new Color(connectFourSpace[i].colors.normalColor.r,
                                             connectFourSpace[i].colors.normalColor.g,
                                             connectFourSpace[i].colors.normalColor.b,
                                             0.502f);
            connectFourSpace[i].colors = Color1;
        }
        C4_AITurn();
    }

    public void C4_AITurn()
    {
        if (playerTurn2 == 1)
        {
            ConnectFourSpace(Agent.select_action());
        }
    }

    public void SetConnect4Board()
    {
        for (int i = 0; i < connectFourSpace.Length; i++)
        {
            connectFourSpace[i] = GameObject.Find(i.ToString()).GetComponent<Button>();
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
                    PlayTTTLeague();
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
                PlayTTTLeague();
            }
        }
        else
        {
            //Otherwise, make the next turn
            TTT_AITurn();
        }
    }

    public void ConnectFourSpace(int cellNumber)
    {
        char choice = playerTurn2 == 0 ? 'X' : 'O';
        int space = Board.get_lowest_column(cellNumber);
        Board.select_piece(space, choice);
        connectFourSpace[space].image.sprite = playerIcon2[playerTurn2];
        connectFourSpace[space].interactable = false;

        //Set next player's turn
        playerTurn2++;
        playerTurn2 %= 2;

        if (Board.winner() != "")
        {
            if(playerTurn2 == 1)
            {
                UnityEngine.Debug.Log("Player wins!");
            }
            else
            {
                UnityEngine.Debug.Log("AI wins!");
            }
            for (int i = 0; i < 25; i++)
            {
                connectFourSpace[i].interactable = false;
                //Set null sprite images to have 0 alpha to prevent weird box showing up on uninteractable
                if (connectFourSpace[i].image.sprite == null)
                {
                    dynamic Color1 = connectFourSpace[i].colors;
                    Color1.disabledColor = new Color(connectFourSpace[i].colors.normalColor.r,
                                                     connectFourSpace[i].colors.normalColor.g,
                                                     connectFourSpace[i].colors.normalColor.b,
                                                     0.0f);
                    connectFourSpace[i].colors = Color1;
                }
            }
        }
        else if (Board.is_full())
        {
            UnityEngine.Debug.Log("Tie!");
        }
        else {
            //Select piece in the python script
            C4_AITurn();
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
        for(int i = 0; i < 2; i++)
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
