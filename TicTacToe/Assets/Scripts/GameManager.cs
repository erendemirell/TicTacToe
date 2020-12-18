using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject[] holes;
    public Sprite[] xoSprite;
    public Text gameFinishedText;

    bool isGameFinished = false;
    int isX;

    DBManager DB;

    // Start is called before the first frame update
    void Start()
    {
        DB = FindObjectOfType<DBManager>();

        isX = DB.GetXOSprite();
    }

    private void Update()
    {
        if (isGameFinished)
        {
            //gameFinishedText.text = "oyun bitti";
        }
    }

    public void WriteGameStatusText(string gameStatusText)
    {
        gameFinishedText.text = gameStatusText;
    }

    public void HoleClick(int index)
    {
        if (!isGameFinished)
        {
            if (DB.isMyTurn)
            {
                if (holes[index].GetComponent<Image>().sprite == null)
                {
                    holes[index].GetComponent<Image>().sprite = xoSprite[isX];
                    holes[index].GetComponent<Image>().color = Color.white;

                    DB.ChangeTurn();
                    DB.SetDBHoles(index, isX);
                }
            }
        }
    }

    public void UpdateHoles(Dictionary<string, object> dbHoles)
    {
        for (int i = 0; i < holes.Length; i++)
        {
            switch (dbHoles[i.ToString()])
            {
                case "x":
                    holes[i].GetComponent<Image>().sprite = xoSprite[0];
                    holes[i].GetComponent<Image>().color = Color.white;
                    holes[i].GetComponent<Button>().interactable = false;
                    break;
                case "o":
                    holes[i].GetComponent<Image>().sprite = xoSprite[1];
                    holes[i].GetComponent<Image>().color = Color.white;
                    holes[i].GetComponent<Button>().interactable = false;
                    break;
                case "":
                    holes[i].GetComponent<Image>().sprite = null;
                    break;
                default:
                    break;
            }
        }

        CheckIsGameFinished();
    }

    private void CheckIsGameFinished()
    {
        if (CheckEquality(0, 1, 2)) // 1 2 3 
        {
            FinishGame();
        }
        else if (CheckEquality(3, 4, 5)) // 4 5 6
        {
            FinishGame();
        }
        else if (CheckEquality(6, 7, 8)) // 7 8 9
        {
            FinishGame();
        }
        else if (CheckEquality(0, 4, 8)) // 1 5 9
        {
            FinishGame();
        }
        else if (CheckEquality(2, 4, 6)) // 3 5 7
        {
            FinishGame();
        }
        else if (CheckEquality(0, 3, 6)) // 1 4 7
        {
            FinishGame();
        }
        else if (CheckEquality(1, 4, 7)) // 2 5 8
        {
            FinishGame();
        }
        else if (CheckEquality(2, 5, 8)) // 3 6 9
        {
            FinishGame();
        }
        else
        {
            print("game is not finish goo!!");
        }

    }

    private bool CheckEquality(int i, int j, int k)
    {
        bool isEqual = (holes[i].GetComponent<Image>().sprite == holes[j].GetComponent<Image>().sprite && holes[j].GetComponent<Image>().sprite == holes[k].GetComponent<Image>().sprite && holes[k].GetComponent<Image>().sprite != null);

        return isEqual;
    }

    private void FinishGame()
    {
        isGameFinished = true;
        DB.FinishGame("finished");
    }
}
