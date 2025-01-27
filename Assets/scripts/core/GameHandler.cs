using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameHandler : MonoBehaviour
{
    [SerializeField]
    private GameObject Player,Dealer;
    private PlayerHandler playerHandler;
    private Dealer dealer;
    private long money=1000; //playermoney
    [SerializeField]
    private TMP_Text bet_text;
    private long Bet { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        playerHandler = Player.GetComponent<PlayerHandler>();
        dealer = Dealer.GetComponent<Dealer>();
    }

    //SetBet 
    public void SetBet()
    {
        Bet = long.Parse(bet_text.text);
    }

    public void EndGame()
    {
        if (dealer.TotalValue > 21)
        {
            money += Bet; //win
        }
        else if (playerHandler.curSum > 21)
        {
            money -= Bet; //loss
        }
        else if (playerHandler.curSum < dealer.TotalValue)
        {
            money -= Bet; //loss
        }
        else if (playerHandler.curSum > dealer.TotalValue)
        {
            money += Bet; //win
        }
        else if (playerHandler.curSum == dealer.TotalValue)
        {
            //draw
        }
        CheckGameOver();
    }

    public void CheckGameOver()
    {
        if (money <= 0)
        {
            SceneManager.LoadScene("GameOver"); //to be made UwU
        }
    }

    public void Stand()
    {
        dealer.PullRest();
        //special card from dealer
    }
}
