using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDealerScript : MonoBehaviour
{
    //script for both player and dealer
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] CardScript cardScript;
    [SerializeField] DeckScript deckScript;
    //total value of cards in hand
    public int handValue = 0;
    //what we bet
    private int candyWrappers = 1000;
    //array of cards on the table
    public GameObject[] hand;
    // index of card to be turned over
    public int cardIndex = 0;
    // list to track aces
    List<CardScript> aceList = new List<CardScript>();
    public void StartHand()
    {
        GetCard();
        Debug.Log("Card Given");
        GetCard();
    }

    //add a card to the hand
    public int GetCard()
    {
        // Get a card, use deal card to assign sprite and value to card on table
        int cardValue = deckScript.DealCard(hand[cardIndex].GetComponent<CardScript>());
        // Show card on game screen
        hand[cardIndex].GetComponent<Renderer>().enabled = true;
        // Add card value to running total of the hand
        handValue += cardValue;
        // If value is 1, it is an ace
        if (cardValue == 1)
        {
            aceList.Add(hand[cardIndex].GetComponent<CardScript>());
        }
        // Cehck if we should use an 11 instead of a 1
        AceCheck();
        cardIndex++;
        return handValue;
    }

    public void AceCheck()
    {
        foreach (CardScript ace in aceList)
        {
            if(handValue + 10 < 22 && ace.GetValueOfCard() == 1)
            {
                ace.SetValueOfCard(11);
                handValue += 10;
            }
            else if(handValue > 21 && ace.GetValueOfCard() == 11)
            {
                ace.SetValueOfCard(1);
                handValue -= 10;
            }
        }
    }

    public void AdjustMoney(int amount)
    {
        candyWrappers += amount;
    }

    public int GetCW()
    {
        return candyWrappers;
    }

    public void ResetHand()
    {
        for(int i = 0; i < hand.Length; i++)
        {
            hand[i].GetComponent<CardScript>().ResetCard();
            hand[i].GetComponent<Renderer>().enabled = false;
        }
        cardIndex = 0;
        handValue = 0;
        aceList = new List<CardScript>();
    }
}
