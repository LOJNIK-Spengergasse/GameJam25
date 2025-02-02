using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Unity.Collections;
using UnityEngine;
//using static UnityEditor.Progress;

public class Dealer : MonoBehaviour
{
    //General
    [SerializeField]
    private GameObject GameHandler;
    private AudioManager audio;
    //General End

    //Dealer AI
    [Tooltip("Max Value where Dealer pulls another Card")]
    [SerializeField] private int MaxVal; //17 standart
    //Dealer AI

    //Dealer Display
    [SerializeField]
    public Sprite[] Dealers;
    private SpriteRenderer SpriteRenderer;
    //Dealer Display End

    //Dealer Cards
    private Dictionary<string, int> DealerHand = new Dictionary<string, int>(); //Dealer Cards
    public IReadOnlyDictionary<string,int> GetDealerHand()
    {
        return new ReadOnlyDictionary<string, int>(DealerHand);
    }
    public KeyValuePair<string, int> OpenCard { get; private set; } //The first card the Dealer pulls, is displayed to Player
    public int ValueModifier { get; set; }
    public int TotalValue => DealerHand.Values.Sum() + ValueModifier; //Total Value of Dealers Handcards + Modifier
    //Dealer Cards End

    //Dealer Abilities
    private List<string> Abilities;

    [SerializeField]
    public GameObject Player, SPCSlotL; //Slot is ugly af, aber es geht fast und das brauchma jetzt
    private DropHandler DropHandler;
    private SpecialCardsList SpecialCardsList;
    private int dealerName;
    //Dealer Abilities End

    //Display Cards
    [SerializeField]
    private GameObject DealerCardParent, CardPrefab;
    [SerializeField]
    private Sprite CardBack;
    private Vector3 leftCardPos;
    //Display Cards End

    // Start is called before the first frame update
    void Start()
    {
        //Load Components & Objects
        DropHandler = SPCSlotL.GetComponent<DropHandler>();
        SpecialCardsList = GameHandler.GetComponent<SpecialCardsList>();
        SpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        audio = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
        //Initialize First Dealer
        ChangeDealer();
    }

    //Ace Check (TEMP HERE, cause used by PlayerHandler as well)
    private bool AceCheck(bool addCard) //returnValue = if something changed
    {
        bool Changed=false;
        int AceCheckValue = 1;
        int TempFixValue = 10;
        int BiggerValue = TotalValue;
        int SmallerValue = GlobalData.DealerWinCond;
        if (!addCard)
        {
            AceCheckValue = 11;
            TempFixValue = -TempFixValue;
            SmallerValue = TotalValue;
            BiggerValue = GlobalData.DealerWinCond;
        }

        List<string> keysToModify = new List<string>();
        foreach (string key in DealerHand.Keys)
        {
            if (key.Contains("A") && BiggerValue > SmallerValue && !(DealerHand[key] == AceCheckValue))
            {
                Changed = true;
                ValueModifier -= TempFixValue;
                keysToModify.Add(key);
            }
        }
        foreach (string key in keysToModify)
        {
            if (addCard) ValueModifier += TempFixValue;
            DealerHand[key] = AceCheckValue;
        }
        return Changed;
    }

    //Edit DealerHand
    public void PullInit()
    {
        //leftCardPos
        leftCardPos = new Vector3(0, 0, 0);
        //Pull open first Card
        KeyValuePair<string, int> card = Deck.PullCard();
        OpenCard = card;
        DealerHand.Add(card.Key,card.Value);
        StartCoroutine(DisplayDealerCards(card.Key));
        //Pull second hidden Card
        card = Deck.PullCard();
        DealerHand.Add(card.Key, card.Value);
        StartCoroutine(DisplayDealerCards(card.Key));
    }
    
    public IEnumerator PullRest()
    {
        StartCoroutine(BounceEffect(gameObject));
        yield return StartCoroutine(PullRestInternally());
    }
    public IEnumerator PullRestInternally()
    {
        TurnCardsOver();
        KeyValuePair<string, int> card;
        while (TotalValue < MaxVal)
        {
            {
                card = Deck.PullCard();
                if (!DealerHand.ContainsKey(card.Key)) //Working Card
                {
                    yield return StartCoroutine(DisplayDealerCardsWithDelay(card));
                }
            }
        }
        AceCheck(true);
        if (TotalValue < MaxVal) yield return StartCoroutine(PullRestInternally());
    }

    public IEnumerator ClearHand()
    {
        //DealerHand.Clear();
        DealerHand = new Dictionary<string, int>();
        ValueModifier = 0;
        OpenCard = new KeyValuePair<string, int>();
        foreach (Transform card in DealerCardParent.transform)
        {
            GameObject.Destroy(card.gameObject);
        }
        yield return null;
    }
    public void PullMulti(int count) //wtf wer hat das so dumm benannt
    {
        while (count > 0)
        {
            KeyValuePair<string, int> card = Deck.PullCard();
            DealerHand.Add(card.Key, card.Value);
            StartCoroutine(DisplayDealerCards(card.Key));
            count--;
        }
        AceCheck(true);
    }

    public void AddCard(string key, int value)
    {
        KeyValuePair<string, int> card = new KeyValuePair<string, int>(key, value);
        DealerHand.Add(card.Key, card.Value);
        DisplayDealerCards(card.Key);
        AceCheck(true);
    }

    public void RemoveCard(string cardKey)
    {
        DealerHand.Remove(cardKey);
        //checks for aces too :)
        if (TotalValue < 12 && DealerHand.Keys.Any(k => k.Contains("A")))
        {
            AceCheck(false);
        }
    }
    //Edit DealerHand END


    public void ChangeDealer()
    {
        System.Random rand = new System.Random();
        int index = rand.Next(Dealers.Length);
        SpriteRenderer.sprite = Dealers[index];
        index++;
        if (index < 3) Abilities = DealerAbilities[index-1].ToList();
        else Abilities = DealerAbilities[index/3-1].ToList();
        dealerName = index;
        Abilities = DealerAbilities[index % 3].ToList();
    }

    private string[][] DealerAbilities = new string[][]
    {
        new string[] { "Double-Sided Blade", "ThreeKings", "TheTwins"},
        new string[] { "Player+1", "Switcheroo" }, //Restart is pain so no
        new string[] { "Ass", "Joker" } //Destroy makes no sense
    };
    public IEnumerator UseAbilities()
    {
        for (int i = 0; i < 2; i++)
        {
            System.Random rand = new System.Random();
            PlayerHandler playerHandler = Player.GetComponent<PlayerHandler>();
            // Over 21 abilities
            string[] Filter = new string[] { };
            if (TotalValue > 21)
            {
                Filter = new string[] { "ThreeKings" }; //"Switcheroo"
            }

            // Player High Number
            else if (playerHandler != null && playerHandler.curSum >= 17 && playerHandler.curSum <= 21)
            {
                Debug.Log("whyy");
                Filter = new string[] { "Player+1" }; //"Switcheroo"
            }

            // Under 17 abilities
            else if (TotalValue <= 17)
            {
                Filter = new string[] { "TheTwins", "Joker", "Ass" }; //"Switcheroo"
            }
            else if (TotalValue == 20)
            {
                Filter = new string[] { "Joker", "Ass" };
            }
            else Filter = new string[] { };
            yield return StartCoroutine(PickAndRemoveAbility(Filter));
        }
        yield return null;
    }

    private IEnumerator PickAndRemoveAbility(string[] abilityPool)
    {
        // Ensure abilityPool is not null or empty
        if (abilityPool == null || abilityPool.Length == 0)
        {
            Debug.LogWarning("Ability pool is null or empty.");
            yield break; // Exit coroutine early
        }

        // Ensure Abilities is not null or empty
        if (Abilities == null || Abilities.Count == 0)
        {
            Debug.LogWarning("Abilities list is null or empty.");
            yield break; // Exit coroutine early
        }

        // Filter abilities that exist in both `abilityPool` and `Abilities`
        var validAbilities = abilityPool.Where(a => Abilities.Contains(a)).ToList();
        // Check if there are no valid abilities to use
        if (validAbilities.Count == 0)
        {
            Debug.LogWarning("No valid abilities found to use.");
            yield break; // Exit coroutine early
        }
        // Pick a random ability
        string selectedAbility = validAbilities[UnityEngine.Random.Range(0, validAbilities.Count)];
        // Call Ability

        if (SpecialCardsList.DealerSpecialCardsUi.ContainsKey(selectedAbility))
        {
            DropHandler.TriggerSPCEffect(SpecialCardsList.DealerSpecialCardsUi[selectedAbility]);
            yield return new WaitForSeconds(2f);
            Debug.Log($"Used ability: {selectedAbility}");
        }
        else
        {
            Debug.LogError($"Key '{selectedAbility}' not found in SpecialCardsUi!");
            yield break;
        }
        // Remove from Abilities (convert array to list first if needed)
        List<string> abilitiesList = Abilities.ToList();
        if (abilitiesList.Remove(selectedAbility))
        {
            Abilities = abilitiesList; // Update back if Abilities is an array
            Debug.Log($"Successfully removed ability: {selectedAbility}");
        }
        else Debug.LogError($"Failed to remove ability: {selectedAbility}");
        yield return null;
    }


    public IEnumerator DisplayDealerCards(string cardKey)
    {
        GameObject card = Instantiate(CardPrefab);
        card.transform.localScale = new Vector3(2.5f, 3f, 2.5f);
        card.name = cardKey;
        card.transform.SetParent(DealerCardParent.transform);
        Vector3 cardPos = new Vector3(1.3f, 0f, 0f);
        int childrenAmt = DealerCardParent.transform.childCount;
        // Check if first
        if (childrenAmt == 1)
        {
            cardPos = new Vector3(0f, 0f, 0f);
            leftCardPos = cardPos;
        } else leftCardPos += cardPos;
        if (childrenAmt == 1 || GameHandler.GetComponent<GameHandler>().stand >= 3)
        {
            CardManager.DeckConverter(cardKey, out string suit, out int rank);
            Sprite s = DealerCardParent.GetComponent<CardManager>().GetCardSprite(suit, rank);
            card.GetComponent<SpriteRenderer>().sprite = s;
        }
        else card.GetComponent<SpriteRenderer>().sprite = CardBack;
        card.transform.localPosition = leftCardPos;
        yield return null;
    }

    public void TurnCardsOver()
    {
        foreach (Transform child in DealerCardParent.transform)
        {
            string cardKey = child.name;
            CardManager.DeckConverter(cardKey, out string suit, out int rank);
            Sprite s = DealerCardParent.GetComponent<CardManager>().GetCardSprite(suit, rank);
            child.GetComponent<SpriteRenderer>().sprite = s;
        }
    }
    //Animation
    private IEnumerator DisplayDealerCardsWithDelay(KeyValuePair<string, int> card)
    {
        yield return new WaitForSeconds(0.6f);
        DealerHand.Add(card.Key, card.Value);
        yield return StartCoroutine(DisplayDealerCards(card.Key));
    }

    private IEnumerator BounceEffect(GameObject cardObject)
    {
        Vector3 originalPos = cardObject.transform.localPosition;
        Vector3 bouncePos = originalPos + new Vector3(0, 0.5f, 0);

        float duration = 0.1f;
        float elapsedTime = 0f;

        switch (dealerName%3)
        {
            case 0:
                audio.PlaySFX(audio.DragonLaugh);
                break;
            case 1:
                audio.PlaySFX(audio.DevilLaugh);
                break;
            case 2:
                audio.PlaySFX(audio.DinoLaugh);
                break;
        }
        // Move up
        while (elapsedTime < duration)
        {
            cardObject.transform.localPosition = Vector3.Lerp(originalPos, bouncePos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Move down
        elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            cardObject.transform.localPosition = Vector3.Lerp(bouncePos, originalPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        cardObject.transform.localPosition = originalPos; // Ensure exact original position
    }




}
