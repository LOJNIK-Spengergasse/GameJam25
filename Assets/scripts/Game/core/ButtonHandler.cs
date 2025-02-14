using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour
{
    [Header("From PlayerButtonHandler")]

    [SerializeField]
    private GameObject player, Dealer;
    [SerializeField]
    public Button hitBtn, standBtn;
    [SerializeField]
    private GameObject GameHandler;
    private GameHandler GameHandlerScript;
    private AudioManager audioManager;
    private bool isHitButtonCooldown = false;

    [Header("BetButttonHandler")]
    [SerializeField]
    int amount;
    [SerializeField]
    private TMP_Text Bet_text;
    [SerializeField]
    private GameObject Bet_UI;

    private int Bet => PlayerPrefs.GetInt("Bet");
    private int Money => PlayerPrefs.GetInt("Money");

    // Start is called before the first frame update
    void Start()
    {
        GameHandlerScript = GameHandler.GetComponent<GameHandler>();
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
        Bet_text.text = Bet.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if (hitBtn != null && player != null)
        {
            hitBtn.interactable = player.GetComponent<Player>().TotalValue < 21 && !isHitButtonCooldown;
        }
        if (standBtn != null && GameHandlerScript != null)
        {
            standBtn.interactable = GameHandlerScript.stand != 3 && GameHandlerScript.stand != 4;
        }
    }

    //player buttons
    public void Hit()
    {
        player.GetComponent<Player>().PullCard();
        StartCoroutine(ReenableButtonAfterDelay(hitBtn, 0.1f));
        isHitButtonCooldown = true;
        audioManager.PlaySFX(audioManager.buttonClickSound);
    }

    public void Stand()
    {
        GameHandlerScript.stand = 1;
        Dealer.GetComponent<Dealer>().PullRest();
        audioManager.PlaySFX(audioManager.buttonClickSound);
    }

    public void Deal()
    {
        audioManager.PlaySFX(audioManager.buttonClickSound);
        StartCoroutine(GameHandlerScript.StartRound());
    }

    private IEnumerator ReenableButtonAfterDelay(Button btn, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (btn == hitBtn)
        {
            isHitButtonCooldown = false;
        }
    }

    public void DisplaySPC()
    {
        player.GetComponent<Player>().DisplaySpecialCards();
    }

    //betBtn
    public void Raise()
    {
        if ((Bet) + amount < 99900 && (Bet) + amount <= Money)
        {
            audioManager.PlaySFX(audioManager.buttonClickSound);
            PlayerPrefs.SetInt("Bet", Bet + amount);
            Bet_text.text = Bet.ToString();
        }
    }
    public void Lower()
    {
        if ((Bet) - amount >= amount)
        {
            audioManager.PlaySFX(audioManager.buttonClickSound);
            PlayerPrefs.SetInt("Bet", int.Parse(Bet_text.text));
            PlayerPrefs.SetInt("Bet", Bet - amount);
            Bet_text.text = Bet.ToString();
        }
    }
    public void AllIn()
    {
        audioManager.PlaySFX(audioManager.buttonClickSound);
        PlayerPrefs.SetInt("Bet", Money);
        Bet_text.text = Bet.ToString();
    }

    public void Done()
    {
        audioManager.PlaySFX(audioManager.buttonClickSound);
        Bet_UI.SetActive(false);
        //BetUi.transform.GetChild(4).gameObject.SetActive(false);
    }
}
