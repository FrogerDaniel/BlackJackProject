
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerScript : NetworkBehaviour
{
    [SerializeField] CardScript cardScript;
    [SerializeField]DeckScript deckScript;

    public NetworkVariable<int> handValue = new NetworkVariable<int>(0, readPerm: NetworkVariableReadPermission.Everyone);
    public NetworkVariable<int> units = new NetworkVariable<int>(1000, readPerm: NetworkVariableReadPermission.Everyone);
    public GameObject[] hand;
    public NetworkVariable<int> cardIndex = new NetworkVariable<int>(0, readPerm: NetworkVariableReadPermission.Everyone);
    private List<CardScript> aceList = new List<CardScript>();

    private void Awake()
    {
        deckScript = GameObject.FindGameObjectWithTag("Deck").gameObject.GetComponent<DeckScript>();
    }

    public void StartHand()
    {
        if (IsServer)
        {
            cardIndex.Value = 1;
            StartHandClientRpc();
        }
    }

    [ClientRpc]
    private void StartHandClientRpc()
    {
        GetCard();
        Debug.Log("Card Given");
        GetCard();
    }

    public int GetCard()
    {
        if (IsServer)
        {
            int cardValue = DealCardOnServer();
            UpdateHandValueClientRpc(cardValue);
        }
        return handValue.Value;
    }

    //[ServerRpc(RequireOwnership = false)]
    //private void RequestCardServerRpc()
    //{
    //    int cardValue = DealCardOnServer();
    //    UpdateHandValueClientRpc(cardValue);
    //}

    private int DealCardOnServer()
    {
        int cardValue = deckScript.DealCard(hand[cardIndex.Value].GetComponent<CardScript>());
        hand[cardIndex.Value].GetComponent<Renderer>().enabled = true;
        //handValue.Value += cardValue;
        if (cardValue == 1)
        {
            aceList.Add(hand[cardIndex.Value].GetComponent<CardScript>());
        }
        AceCheck();
        UpdateCardClientRpc(cardIndex.Value, cardValue);
        cardIndex.Value++;
        Debug.Log("Card was given with value of " + cardValue);
        return cardValue;
    }

    [ClientRpc]
    private void UpdateCardClientRpc(int index, int cardValue)
    {

        hand[index].GetComponent<CardScript>().SetSprite(deckScript.cardSprites.Value.GetSprites()[index]);
        hand[index].GetComponent<CardScript>().SetValueOfCard(cardValue);
        hand[index].GetComponent<Renderer>().enabled = true;
    }

    [ClientRpc]
    private void UpdateHandValueClientRpc(int cardValue)
    {
        handValue.Value += cardValue;
    }

    private void AceCheck()
    {
        foreach (CardScript ace in aceList)
        {
            if (handValue.Value + 10 < 22 && ace.GetValueOfCard() == 1)
            {
                ace.SetValueOfCard(11);
                handValue.Value += 10;
            }
            else if (handValue.Value > 21 && ace.GetValueOfCard() == 11)
            {
                ace.SetValueOfCard(1);
                handValue.Value -= 10;
            }
        }
    }

    public void AdjustMoney(int amount)
    {
        if (IsServer)
        {
            units.Value += amount;
            AdjustMoneyClientRpc(amount);
        }
    }

    [ClientRpc]
    private void AdjustMoneyClientRpc(int unitsToGive)
    {
        units.Value += unitsToGive;
    }

    public int GetUnits()
    {
        return units.Value;
    }

    public void ResetHand()
    {
        if (IsServer)
        {
            ResetHandClientRpc();
        }
    }

    [ClientRpc]
    private void ResetHandClientRpc()
    {
        for (int i = 0; i < hand.Length; i++)
        {
            hand[i].GetComponent<CardScript>().ResetCard();
            hand[i].GetComponent<Renderer>().enabled = false;
        }
        cardIndex.Value = 0;
        handValue.Value = 0;
        aceList = new List<CardScript>();
    }
}

