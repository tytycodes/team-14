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

public class TicTacToe : MonoBehaviour
{
    //Singleton model for script. Helps minimize unwanted behavior
    private static TicTacToe _instance = null;

    public static TicTacToe Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType(typeof(TicTacToe)) as TicTacToe;
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
    public UnityEngine.UI.Button[] ticTacToeSpace;
    static Dictionary<string, string> AgentNames;

    private GameObject Grid;
    private GameObject Call_Button;
    private GameObject Single_Button;
    private GameObject Double_Button;
    private GameObject Triple_Button;
    private GameObject Reset_Button;
    private GameObject Quit_Button;
    private GameObject Continue_Button;

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

    //explicitly set the tictactoe spaces. Fails otherwise
    public void SetTicTacToeBoard()
    {
        for (int i = 0; i < 9; i++)
        {
            ticTacToeSpace[i] = GameObject.Find(i.ToString()).GetComponent<Button>();
            //Change color multiplier back to 1 so that highlights look normal
            ColorBlock col = ticTacToeSpace[i].colors;
            col.colorMultiplier = 1;
            ticTacToeSpace[i].colors = col;
        }
    }

    //Initialize board settings based on game mode and difficulty sent from GameController
    public void TTTInit(int gamemode, int diff)
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
            TicTacToeSingle();
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
            TicTacToeLeague();
        }
    }

    //Single match specific setup. Call TicTacToeSetup so that the game can reset later from connect4setup
    public void TicTacToeSingle()
    {
        //Create the IronPython runtime engine
        var engine = Python.CreateEngine();

        //Set searchpaths for IronPython
        ICollection<string> searchPaths = engine.GetSearchPaths();

        //Path to the folder of Agent and BoardEnvironment
        searchPaths.Add(Application.dataPath + @"\Resources\TicTacToe\");
        //Path to the Python standard library
        searchPaths.Add(Application.dataPath + @"\Plugins\Lib\");
        engine.SetSearchPaths(searchPaths);

        //Load in the difficulty q-table
        string diff = "";
        switch (difficulty)
        {
            case 0:
                diff = @"\Resources\TicTacToe\easy.txt";
                break;
            case 1:
                diff = @"\Resources\TicTacToe\medium.txt";
                break;
            case 2:
                diff = @"\Resources\TicTacToe\hard.txt";
                break;
        }

        //load in the python scripts
        dynamic tempagent = engine.ExecuteFile(Application.dataPath + @"\Resources\TicTacToe\Agent.py");
        dynamic tempboard = engine.ExecuteFile(Application.dataPath + @"\Resources\TicTacToe\BoardEnvironment.py");
        Board = tempboard.BoardEnvironment();
        Agent = tempagent.Agent(Board, Application.dataPath + diff);
        TicTacToeSetup();
    }

    //TicTacToeSetup point that will allow single match resets more easily
    public void TicTacToeSetup()
    {
        ContextMessage.SetActive(false);
        PairedAI.text = CurrName;
        Board.reset();

        //explicitly set the board
        SetTicTacToeBoard();

        System.Random r = new System.Random();
        int num = r.Next(1, 100);
        playerTurn = (num > 50) ? 0 : 1;
        turnCount = 0;
        //reset the alpha levels just in case they've been changed (alpha is lowered so the board doesn't look weird after a filled board)
        for (int i = 0; i < ticTacToeSpace.Length; i++)
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
        //Let the AI have a turn (if applicable)
        TTT_AITurn();
    }

    //Allow the AI to make a choice if it is the AI's turn
    void TTT_AITurn()
    {
        //Check if it is the AI's turn
        if (playerTurn == 1)
        {
            TicTacToeButton(Agent.select_action(!leagueChoice));
        }
    }

    //League play specific setup. Call NewTTTLeagueGame so that you can be paired against another AI more easily if the previous AI quits
    public void TicTacToeLeague()
    {
        //Create the IronPython runtime engine
        var engine = Python.CreateEngine();

        //Set searchpaths for IronPython
        ICollection<string> searchPaths = engine.GetSearchPaths();

        //Path to the folder of Agent and BoardEnvironment
        searchPaths.Add(Application.dataPath + @"\Resources\TicTacToe\");
        //Path to the Python standard library
        searchPaths.Add(Application.dataPath + @"\Plugins\Lib\");
        engine.SetSearchPaths(searchPaths);

        //load in the python scripts
        dynamic tempagent = engine.ExecuteFile(Application.dataPath + @"\Resources\TicTacToe\Agent.py");
        dynamic tempboard = engine.ExecuteFile(Application.dataPath + @"\Resources\TicTacToe\BoardEnvironment.py");
        dynamic templeague = engine.ExecuteFile(Application.dataPath + @"\Resources\TicTacToe\LeagueEnvironment.py");
        dynamic temputil = engine.ExecuteFile(Application.dataPath + @"\Resources\TicTacToe\LeagueUtil.py");

        Board = tempboard.BoardEnvironment();
        League = templeague.LeagueEnvironment(Board);
        Util = temputil.LeagueUtil(Board, League);

        League.set_players(Util.get_names(), Util.get_leagues(), Util.get_boards());

        NewTTTLeagueGame();
    }

    //NewC4LeagueGame point that will allow league match resets more easily
    public void NewTTTLeagueGame()
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
        PlayTTTLeague();
    }

    //Play at the league level, depending on who is first to choose at league level
    public void PlayTTTLeague()
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

    //Selection of a TicTacToe space
    public void TicTacToeButton(int cellNumber)
    {
        //Select space
        ticTacToeSpace[cellNumber].image.sprite = playerIcon[playerTurn];
        //Change color multiplier to 5 so that board pieces aren't transparent
        ColorBlock col = ticTacToeSpace[cellNumber].colors;
        col.colorMultiplier = 5;
        ticTacToeSpace[cellNumber].colors = col;

        ticTacToeSpace[cellNumber].interactable = false;

        //Set next player's turn
        char choice = playerTurn == 0 ? 'X' : 'O';

        //Select piece in the python script
        Board.select_piece(cellNumber, choice);
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
            //Deactivate all spaces on the tic tac toe board
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
            //Check for league specific disqualifiers, like chips running out
            if (League != null)
            {
                //Someone's chips ran out
                if (AIChips <= 0 || PlayerChips <= 0)
                {
                    if(AIChips <= 0) textmeshPro.text = "AI ran out of chips.";
                    else textmeshPro.text = "Player ran out of chips.";
                    ContextMessage.SetActive(true);
                    ContinueGame();
                }
                //No one's chips ran out
                else
                {
                    //Flip league choice
                    leagueChoice = !leagueChoice;
                    PlayTTTLeague();
                }
            }
        }
        //Check for ties
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
                PlayTTTLeague();
            }
        }
        else
        {
            //Otherwise, make the next turn
            TTT_AITurn();
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
            TicTacToeSetup();
        }
    }

    //Keep the chip multiplier and play the match
    public void CallBet()
    {
        HideButtons();
        TicTacToeSetup();
    }
}