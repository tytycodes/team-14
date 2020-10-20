using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{

    public int playerTurn;
    public int turnCount;
    public Sprite[] playerIcon;
    public UnityEngine.UI.Button[] ticTacToeSpace;
    public UnityEngine.UI.Button[] gameButton;
    public UnityEngine.UI.Button[] difficultyButton;

    // Start is called before the first frame update
    void Start()
    {
        GameSetup();
    }

    void GameSetup()
    {
        playerTurn = 0;
        turnCount = 0;
        for(int i = 0; i < ticTacToeSpace.Length; i++)
        {
            ticTacToeSpace[i].interactable = true;
            ticTacToeSpace[i].GetComponent<UnityEngine.UI.Image>().sprite = null;
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
        playerTurn++;
        playerTurn %= 2;
    }

    public void gameButtonSelect(int cellNumber)
    {
        //gameButton[cellNumber].GetComponent<Image>().color = Color.white;
        gameButton[cellNumber].interactable = false;
    }

    public void difficultyButtonSelect(int cellNumber)
    {
        //difficultyButton[cellNumber].GetComponent<Image>().color = Color.white;
        difficultyButton[cellNumber].interactable = false;
    }
}
