using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerScript : NetworkBehaviour
{
    [SerializeField] DeckScript deckScript; // Reference to the DeckScript component

    public NetworkVariable<int> handValue = new NetworkVariable<int>(0, readPerm: NetworkVariableReadPermission.Everyone); // Network variable for the hand value
    public int handValueLocal = 0; // Local hand value
    public NetworkVariable<int> units = new NetworkVariable<int>(1000, readPerm: NetworkVariableReadPermission.Everyone); // Network variable for the units
    public int unitsLocal = 0; // Local units value
    public GameObject[] hand; // Array of GameObjects representing the player's hand
    private CardScript[] cardScripts; // Array of CardScript components
    public NetworkVariable<int> cardIndex = new NetworkVariable<int>(0, readPerm: NetworkVariableReadPermission.Everyone); // Network variable for the card index
    private List<CardScript> aceList = new List<CardScript>(); // List of ace cards

    private void Awake()
    {
        cardIndex.Value = 1; // Initialize card index to 1
        StartCoroutine(FindWithDelay()); // Start coroutine to find components with delay
    }

    private IEnumerator FindWithDelay()
    {
        yield return new WaitForSeconds(2f); // Wait for 2 seconds to avoid spawn conflicts resulting in null ref 
        deckScript = GameObject.FindGameObjectWithTag("Deck").gameObject.GetComponent<DeckScript>(); // Find the DeckScript component
        cardScripts = new CardScript[hand.Length]; // Initialize the cardScripts array
        for (int i = 0; i < hand.Length; i++)
        {
            cardScripts[i] = hand[i].GetComponent<CardScript>(); // Get the CardScript component for each hand GameObject
        }
    }

    /***-------------------------------------------------------------------------
    * START HAND
    * Initiates the start of the hand
    * ----------------------------------------------------------------------***/
    public void StartHand()
    {
        StartHandServerRpc(); // Request the server to start the hand
    }

    /***-------------------------------------------------------------------------
    * START HAND SERVER RPC
    * Handles starting the hand on the server
    * ----------------------------------------------------------------------***/
    [ServerRpc(RequireOwnership = false)]
    private void StartHandServerRpc()
    {
        GetCard(); // Get the first card
        Debug.Log("Card Given");
        GetCard(); // Get the second card
    }

    /***-------------------------------------------------------------------------
    * GET CARD
    * Requests a card from the server
    * ----------------------------------------------------------------------***/
    public int GetCard()
    {
        RequestCardServerRpc(); // Request a card from the server
        return handValue.Value; // Return the current hand value
    }

    /***-------------------------------------------------------------------------
    * REQUEST CARD SERVER RPC
    * Handles card requests on the server
    * ----------------------------------------------------------------------***/
    [ServerRpc(RequireOwnership = false)]
    private void RequestCardServerRpc()
    {
        int cardValue = DealCardOnServer(); // Deal a card on the server
        UpdateHandValueServerRpc(cardValue); // Update the hand value on the server
    }

    /***-------------------------------------------------------------------------
    * DEAL CARD ON SERVER
    * Deals a card on the server
    * ----------------------------------------------------------------------***/
    private int DealCardOnServer()
    {
        int cardValue = deckScript.DealCard(cardIndex.Value); // Deal a card from the deck
        hand[cardIndex.Value].GetComponent<Renderer>().enabled = true; // Enable the card renderer
        if (cardValue == 1)
        {
            aceList.Add(hand[cardIndex.Value].GetComponent<CardScript>()); // Add ace to the ace list
        }
        UpdateCardClientRpc(cardIndex.Value, cardValue); // Update the card on the client
        cardIndex.Value++; // Increment the card index
        Debug.Log("Card was given with value of " + cardValue);
        return cardValue; // Return the card value
    }

    /***-------------------------------------------------------------------------
    * UPDATE CARD CLIENT RPC
    * Updates the card on the client
    * ----------------------------------------------------------------------***/
    [ClientRpc]
    private void UpdateCardClientRpc(int index, int cardValue)
    {
        cardScripts[index].SetSprite(deckScript.cardSprites.Value.GetSprites()[index]); // Set the card sprite
        cardScripts[index].SetValueOfCard(cardValue); // Set the card value
        hand[index].GetComponent<Renderer>().enabled = true; // Enable the card renderer
    }

    /***-------------------------------------------------------------------------
    * UPDATE HAND VALUE SERVER RPC
    * Updates the hand value on the server
    * ----------------------------------------------------------------------***/
    [ServerRpc(RequireOwnership = false)]
    private void UpdateHandValueServerRpc(int cardValue)
    {
        handValue.Value += cardValue; // Update the hand value
        AceCheck(); // Check for aces
        UpdateHandValueClientRpc(handValue.Value); // Update the hand value on the client
    }

    /***-------------------------------------------------------------------------
    * UPDATE HAND VALUE CLIENT RPC
    * Updates the hand value on the client
    * ----------------------------------------------------------------------***/
    [ClientRpc]
    private void UpdateHandValueClientRpc(int newHandValue)
    {
        handValueLocal = newHandValue; // Update the local hand value
        Debug.Log(handValueLocal.ToString() + " this is local hand value");
    }

    /***-------------------------------------------------------------------------
    * ACE CHECK
    * Checks and adjusts the value of aces
    * ----------------------------------------------------------------------***/
    private void AceCheck()
    {
        foreach (CardScript ace in aceList)
        {
            if (handValue.Value + 10 < 22 && ace.GetValueOfCard() == 1)
            {
                ace.SetValueOfCard(11); // Adjust ace value to 11
                handValue.Value += 10; // Update hand value
            }
            else if (handValue.Value > 21 && ace.GetValueOfCard() == 11)
            {
                ace.SetValueOfCard(1); // Adjust ace value to 1
                handValue.Value -= 10; // Update hand value
            }
        }
    }

    /***-------------------------------------------------------------------------
    * ADJUST MONEY
    * Adjusts the player's money
    * ----------------------------------------------------------------------***/
    public void AdjustMoney(int amount)
    {
        AdjustMoneyServerRpc(amount); // Request the server to adjust money
    }

    /***-------------------------------------------------------------------------
    * ADJUST MONEY SERVER RPC
    * Adjusts the player's money on the server
    * ----------------------------------------------------------------------***/
    [ServerRpc(RequireOwnership = false)]
    private void AdjustMoneyServerRpc(int amount)
    {
        units.Value += amount; // Update the units value
        AdjustMoneyClientRpc(units.Value); // Update the units value on the client
    }

    /***-------------------------------------------------------------------------
    * ADJUST MONEY CLIENT RPC
    * Updates the player's money on the client
    * ----------------------------------------------------------------------***/
    [ClientRpc]
    private void AdjustMoneyClientRpc(int newUnits)
    {
        unitsLocal = newUnits; // Update the local units value
        Debug.Log(unitsLocal.ToString() + "Those are local units");
    }

    /***-------------------------------------------------------------------------
    * GET UNITS
    * Returns the player's units
    * ----------------------------------------------------------------------***/
    public int GetUnits()
    {
        return unitsLocal; // Return the local units value
    }

    /***-------------------------------------------------------------------------
    * RESET HAND
    * Resets the player's hand
    * ----------------------------------------------------------------------***/
    public void ResetHand()
    {
        ResetHandServerRpc(0); // Request the server to reset the hand
        ResetHandClientRpc(handValue.Value); // Request the client to reset the hand
    }

    /***-------------------------------------------------------------------------
    * RESET HAND SERVER RPC
    * Resets the player's hand on the server
    * ----------------------------------------------------------------------***/
    [ServerRpc(RequireOwnership = false)]
    private void ResetHandServerRpc(int newHandValue)
    {
        cardIndex.Value = 1; // Reset the card index
        handValue.Value = newHandValue; // Reset the hand value
    }

    /***-------------------------------------------------------------------------
    * RESET HAND CLIENT RPC
    * Resets the player's hand on the client
    * ----------------------------------------------------------------------***/
    [ClientRpc]
    private void ResetHandClientRpc(int newHandValue)
    {
        for (int i = 0; i < hand.Length; i++)
        {
            hand[i].GetComponent<CardScript>().ResetCard(); // Reset each card
            hand[i].GetComponent<Renderer>().enabled = false; // Disable the card renderer
        }
        handValueLocal = newHandValue; // Update the local hand value
        Debug.LogError("This is value after reset " + handValueLocal.ToString());
        aceList = new List<CardScript>(); // Clear the ace list
    }
}
