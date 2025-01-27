using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class Dealer : MonoBehaviour
{
    [SerializeField]
    Deck deck;
    
    // Start is called before the first frame update
    void Start(){}
    // Update is called once per frame
    void Update(){}

    public void PullInit()
    {
        //Pull open first Card
        KeyValuePair<string, int> card = deck.PullCard();
        OpenCard = card.Key;
        DealerHand.Add(card.Key,card.Value);
        //Pull second hidden Card
        card = deck.PullCard();
        DealerHand.Add(card.Key, card.Value);
    }
    public void PullRest()
    {
        KeyValuePair<string, int> card;
        while (TotalValue < 17)
        {
            {
                card = deck.PullCard();
                if (!DealerHand.ContainsKey(card.Key))
                {
                    DealerHand.Add(card.Key, card.Value);
                }
            }
        }
        foreach (string key in DealerHand.Keys)
        {
            if (key.Contains("A") && DealerHand.Values.Sum() > 21)
            {
                DealerHand[key] = 1;
            }
        }
        if (TotalValue < 17) PullRest();
    }

    public void ClearHand()
    {
        DealerHand.Clear();
    }

    public Dictionary<string, int> DealerHand = new Dictionary<string, int>();
    public string OpenCard { get; set; }
    public int TotalValue => DealerHand.Values.Sum();

}
