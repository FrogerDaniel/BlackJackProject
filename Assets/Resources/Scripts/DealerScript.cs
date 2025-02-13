using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class DealerScript : MonoBehaviour
{
    //script for both player and dealer
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] CardScript cardScript;
    [SerializeField] DeckScript deckScript;
    [SerializeField] GameManager gameManager;
    [SerializeField] GameObject hiddenCard;
    //total value of cards in hand
    public int handValue = 0;
    //array of cards on the table
    public GameObject[] hand;
    // index of card to be turned over
    public int cardIndex = 0;
    // list to track aces
    List<CardScript> aceList = new List<CardScript>();
    //var for pot of bets
    public int pot = 0;
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
            //for each ace in the list of aces check if the total hand value would be less than bust value
            if (handValue + 10 < 22 && ace.GetValueOfCard() == 1)
            {
                //if player will not bust, make ace equal 11
                ace.SetValueOfCard(11);
                handValue += 10;
            }
            else if (handValue > 21 && ace.GetValueOfCard() == 11)
            {
                //if player will bust make ace equal 1
                ace.SetValueOfCard(1);
                handValue -= 10;
            }
        }
    }

    public void ResetHand()
    {
        //goes through hand and resets card sprites
        for (int i = 0; i < hand.Length; i++)
        {
            hand[i].GetComponent<CardScript>().ResetCard();
            hand[i].GetComponent<Renderer>().enabled = false;
        }
        //resets all indexes and values to use for new hand
        cardIndex = 0;
        handValue = 0;
        //recreate a list for aces
        aceList = new List<CardScript>();
    }

    public void DealCards()
    {
        //deal cards to dealer
        ResetHand();
        //deal cards to players, implement function when player script is done
        //implement enabling UI for players when player script is done
        GameObject.Find("Deck").GetComponent<DeckScript>().Shuffle();
        //start hand for player

        //start hand for dealer
        StartHand();
        //update player and dealer score  score with UI Manager
        //UpdateUI()

        hiddenCard.GetComponent<Renderer>().enabled = true;
        //hide deal button and enable hit and stand buttons for dealer and player via UI Manager
        
        //set standard pot size
        pot = 40;
        //update text with total bets via UI Manager
        //player script will handle betting on the player side
    }

    private void HitDealer()
    {
        while (handValue < 16 && cardIndex < 10)
        {
            GetCard();
            //show dealer score
            //dealerScoreText.text = "Hand: " + dealerScript.handValue.ToString();
            if (handValue > 20)
            {
                //call for round over
                gameManager.RoundOver();
            }
        }
    }
}
