using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using IronPython.Hosting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Connect4 : MonoBehaviour
{
    //Singleton model for script. Helps minimize unwanted behavior
    private static Connect4 _instance = null;

    public static Connect4 Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType(typeof(Connect4)) as Connect4;
            }

            return _instance;
        }
    }

    public int playerTurn;
    public bool leagueChoice;
    public int turnCount;
    public int currBet;
    private int difficulty;
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
    private GameObject ContextMessage;
    private TextMeshProUGUI PairedAI;
    private string CurrName;
    public Sprite[] playerIcon;
    public UnityEngine.UI.Button[] connectFourSpace;
    static Dictionary<string, string> AgentNames;

    private GameObject Grid;
    private GameObject Call_Button;
    private GameObject Single_Button;
    private GameObject Double_Button;
    private GameObject Triple_Button;
    private GameObject Reset_Button;
    private GameObject Quit_Button;
    private GameObject Continue_Button;

    private Stopwatch stopwatch;

    //Set singleton instance and initialize all buttons
    void Awake()
    {
        _instance = this;
        Grid = GameObject.Find("Grid");
        Call_Button = GameObject.Find("Button_Call");
        Single_Button = GameObject.Find("Button_Single");
        Double_Button = GameObject.Find("Button_Double");
        Triple_Button = GameObject.Find("Button_Triple");
        Reset_Button = GameObject.Find("Button_Reset");
        Quit_Button = GameObject.Find("Button_Quit");
        Continue_Button = GameObject.Find("Button_Continue");
        ContextMessage = GameObject.Find("ContextMessage");
        PairedAI = GameObject.Find("PairedAI").GetComponent<TextMeshProUGUI>();
        AgentNames = new Dictionary<string, string>
        {
            {"learning strategy and tactics", "high strategy and tactics"},
            {"learning tactics only", "low strategy and high tactics"},
            {"learning strategy only", "high strategy and low tactics"},
            {"no learning", "low strategy and tactics"}
        };
        HideButtons();
    }

    //display league-level buttons depending on if you're calling a bet or increasing a bet
    public void ShowButtons(bool turn)
    {
        //Hide grid and activate/deactivate proper buttons
        Grid.SetActive(false);
        Continue_Button.SetActive(false);
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

    //hide league-level buttons to play the game
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

    //explicitly set the connect 4 spaces. Fails otherwise
    public void SetConnect4Board()
    {
        for (int i = 0; i < connectFourSpace.Length; i++)
        {
            connectFourSpace[i] = GameObject.Find(i.ToString()).GetComponent<Button>();
        }
    }

    //Initialize board settings based on game mode and difficulty sent from GameController
    public void C4Init(int gamemode, int diff)
    {
        difficulty = diff;
        //Single match mode
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
        //League play mode
        else
        {
            AIChips = 100;
            AIWins = 0;
            PlayerChips = 100;
            PlayerWins = 0;
            ties = 0;
            currBet = 5;
            Reset_Button.SetActive(false);
            Connect4League();
        }
    }

    //Single match specific setup. Call connect4setup so that the game can reset later from connect4setup
    public void Connect4Single()
    {
        //Create the IronPython runtime engine
        var engine = Python.CreateEngine();

        //Set searchpaths for IronPython
        ICollection<string> searchPaths = engine.GetSearchPaths();

        //Path to the folder of Agent and BoardEnvironment
        searchPaths.Add(Application.dataPath + @"\Resources\Connect4\");
        //Path to the Python standard library
        searchPaths.Add(Application.dataPath + @"\Plugins\Lib\");
        engine.SetSearchPaths(searchPaths);

        //Load in the difficulty q-table
        string diff = "";
        switch (difficulty)
        {
            case 0:
                diff = @"\Resources\Connect4\easy.txt";
                break;
            case 1:
                diff = @"\Resources\Connect4\medium.txt";
                break;
            case 2:
                diff = @"\Resources\Connect4\hard.txt";
                break;
        }

        //load in the python scripts
        dynamic tempagent = engine.ExecuteFile(Application.dataPath + @"\Resources\Connect4\Agent.py");
        dynamic tempboard = engine.ExecuteFile(Application.dataPath + @"\Resources\Connect4\BoardEnvironment.py");
        Board = tempboard.BoardEnvironment();
        Agent = tempagent.Agent(Board, Application.dataPath + diff);
        Connect4Setup();
    }

    //Connect4Setup point that will allow single match resets more easily
    public void Connect4Setup()
    {
        ContextMessage.SetActive(false);
        PairedAI.text = CurrName;
        Board.reset();

        //explicitly set the board
        SetConnect4Board();

        System.Random r = new System.Random();
        int num = r.Next(1, 100);
        playerTurn = (num > 50) ? 0 : 1;
        turnCount = 0;
        //reset the alpha levels just in case they've been changed (alpha is lowered so the board doesn't look weird after a filled board)
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
        //Let the AI have a turn (if applicable)
        C4_AITurn();
    }

    //League play specific setup. Call NewC4LeagueGame so that you can be paired against another AI more easily if the previous AI quits
    public void Connect4League()
    {
        //Create the IronPython runtime engine
        var engine = Python.CreateEngine();

        //Set searchpaths for IronPython
        ICollection<string> searchPaths = engine.GetSearchPaths();

        //Path to the folder of Agent and BoardEnvironment
        searchPaths.Add(Application.dataPath + @"\Resources\Connect4\");
        //Path to the Python standard library
        searchPaths.Add(Application.dataPath + @"\Plugins\Lib\");
        engine.SetSearchPaths(searchPaths);

        //load in the python scripts
        dynamic tempagent = engine.ExecuteFile(Application.dataPath + @"\Resources\Connect4\Agent.py");
        dynamic tempboard = engine.ExecuteFile(Application.dataPath + @"\Resources\Connect4\BoardEnvironment.py");
        dynamic templeague = engine.ExecuteFile(Application.dataPath + @"\Resources\Connect4\LeagueEnvironment.py");
        dynamic temputil = engine.ExecuteFile(Application.dataPath + @"\Resources\Connect4\LeagueUtil.py");

        Board = tempboard.BoardEnvironment();
        League = templeague.LeagueEnvironment(Board);
        Util = temputil.LeagueUtil(Board, League);

        League.set_players(Util.get_names(), Util.get_leagues(), Util.get_boards());

        NewC4LeagueGame();
    }

    //NewC4LeagueGame point that will allow league match resets more easily
    public void NewC4LeagueGame()
    {
        PlayerChips = 100;
        AIChips = 100;

        //Get the text component of the chip display so that it can easily be modified later
        pChips = GameObject.Find("PlayerChipsText").GetComponent<Text>();
        aChips = GameObject.Find("AIChipsText").GetComponent<Text>();

        pChips.text = Convert.ToString(PlayerChips);
        aChips.text = Convert.ToString(AIChips);

        //List to store the result of reset_pair in the python script, which consists of the league agent index and whether or not the player chooses first at the league level
        List<string> result = ((IList<object>)League.reset_pair()).Cast<string>().ToList();

        //Iterate through the result list and set appropriate variables for the corresponding list item
        int ListIndex = 0;
        foreach (string item in result)
        {
            switch (ListIndex)
            {
                case 0:
                    //Get the name of the paired AI and display it to the screen using the prebuilt dictionary
                    string temp = Util.get_agent_name(int.Parse(item));
                    AgentNames.TryGetValue(temp, out CurrName);
                    TextMeshProUGUI textmeshPro = ContextMessage.GetComponent<TextMeshProUGUI>();
                    textmeshPro.text = "Paired AI is " + CurrName;
                    PairedAI.text = "";
                    ContextMessage.SetActive(true);
                    Agent = Util.get_board_agent(int.Parse(item));
                    LeagueAgent = Util.get_league_agent(int.Parse(item));
                    break;
                case 1:
                    leagueChoice = (item == "False") ? false : true;
                    break;
            }
            ListIndex++;
        }
        PlayC4League();
    }

    //Play at the league level, depending on who is first to choose at league level
    public void PlayC4League()
    {
        bool quit = false;
        if (!leagueChoice)
        {
            string choice = LeagueAgent.select_action(!leagueChoice);

            TextMeshProUGUI textmeshPro = ContextMessage.GetComponent<TextMeshProUGUI>();
            textmeshPro.text = "AI chose " + choice;
            ContextMessage.SetActive(true);
            //Choices available to the AI if it chooses first
            switch (choice)
            {
                case "quit":
                    quit = true;
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
        }
        //Show buttons if the AI didn't quit
        if (!quit)
            ShowButtons(leagueChoice);
    }

    //Allow the AI to make a choice if it is the AI's turn
    public void C4_AITurn()
    {
        if (playerTurn == 1)
        {
            ConnectFourSpace(Agent.select_action(!leagueChoice));
        }
    }

    //Selection of a connect 4 space
    public void ConnectFourSpace(int cellNumber)
    {
        //The choice that will be understood by the Board python script
        char choice = playerTurn == 0 ? 'X' : 'O';
        //Get the lowest available row in the selected column on the connect 4 board
        int space = Board.get_lowest_column(cellNumber);
        Board.select_piece(space, choice);
        connectFourSpace[space].image.sprite = playerIcon[playerTurn];
        connectFourSpace[space].interactable = false;

        //Set next player's turn
        playerTurn++;
        playerTurn %= 2;

        TextMeshProUGUI textmeshPro = ContextMessage.GetComponent<TextMeshProUGUI>();
        //Check for a winner
        if (Board.winner() != "")
        {
            //Player has won
            if (playerTurn == 1)
            {
                textmeshPro.text = "Player wins!";
                ContextMessage.SetActive(true);
                //Make adjustments to league variables if applicable
                if (League != null)
                {
                    PlayerWins++;
                    PlayerChips += chip_mul * currBet;
                    AIChips -= chip_mul * currBet;
                    pChips.text = Convert.ToString(PlayerChips);
                    aChips.text = Convert.ToString(AIChips);
                }
            }
            //AI has won
            else
            {
                textmeshPro.text = "AI wins!";
                ContextMessage.SetActive(true);
                //Make adjustments to league variables if applicable
                if (League != null)
                {
                    AIWins++;
                    AIChips += chip_mul * currBet;
                    PlayerChips -= chip_mul * currBet;
                    pChips.text = Convert.ToString(PlayerChips);
                    aChips.text = Convert.ToString(AIChips);
                }
            }
            //Deactivate all spaces on the connect four board
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
            //Check for league specific disqualifiers, like chips running out
            if (League != null)
            {
                //Someone's chips ran out
                if (AIChips <= 0 || PlayerChips <= 0)
                {
                    if (AIChips <= 0) textmeshPro.text = "AI ran out of chips.";
                    else textmeshPro.text = "Player ran out of chips.";
                    ContextMessage.SetActive(true);
                    ContinueGame();
                }
                //No one's chips ran out
                else
                {
                    //Flip league choice
                    leagueChoice = !leagueChoice;
                    PlayC4League();
                }
            }
        }
        //Check for a tie
        else if (Board.is_full())
        {
            textmeshPro.text = "Tie game!";
            ContextMessage.SetActive(true);
            //Make adjustments to league variables if applicable
            if (League != null)
            {
                ties++;
                //Flip league choice
                leagueChoice = !leagueChoice;
                PlayC4League();
            }
        }
        else
        {
            //Select piece in the python script
            C4_AITurn();
        }
    }

    //Allow the player to either quit or continue once someone quits/loses all their chips
    public void ContinueGame()
    {
        HideButtons();
        Call_Button.SetActive(false);
        Grid.SetActive(false);
        Quit_Button.SetActive(true);
        Continue_Button.SetActive(true);
    }

    //Set chip multiplier based on the selected button
    public void setChipMul(int cellNumber)
    {
        chip_mul = cellNumber;
        string choice = LeagueAgent.select_action(!leagueChoice);

        TextMeshProUGUI textmeshPro = ContextMessage.GetComponent<TextMeshProUGUI>();
        textmeshPro.text = "AI chose " + choice;
        ContextMessage.SetActive(true);

        if (choice == "quit")
        {
            ContinueGame();
        }
        else
        {
            HideButtons();
            Connect4Setup();
        }
    }

    //Keep the chip multiplier and play the match
    public void CallBet()
    {
        HideButtons();
        Connect4Setup();
    }
}