using NUnit.Framework;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DealerScript : NetworkBehaviour
{
    [SerializeField] CardScript cardScript;
    [SerializeField] DeckScript deckScript;

    public NetworkVariable<int> handValue = new NetworkVariable<int>(0);
    public GameObject[] hand;
    public NetworkVariable<int> cardIndex = new NetworkVariable<int>(0);
    private List<CardScript> aceList = new List<CardScript>();

    private void Awake()
    {
        deckScript = GameObject.FindGameObjectWithTag("Deck").gameObject.GetComponent<DeckScript>();
    }

    public void StartHand()
    {
        if (IsServer)
        {
            StartHandServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void StartHandServerRpc()
    {
        GetCard();
        Debug.Log("Card Given");
        GetCard();
    }

    public int GetCard()
    {
        GetCardServerRpc();
        return handValue.Value;
    }

    [ServerRpc(RequireOwnership = false)]
    private void GetCardServerRpc()
    {
        int cardValue = deckScript.DealCard(hand[cardIndex.Value].GetComponent<CardScript>());
        hand[cardIndex.Value].GetComponent<Renderer>().enabled = true;
        handValue.Value += cardValue;
        if (cardValue == 1)
        {
            aceList.Add(hand[cardIndex.Value].GetComponent<CardScript>());
        }
        AceCheck();
        cardIndex.Value++;
        GetCardClientRpc(cardIndex.Value - 1, cardValue, handValue.Value);
    }

    [ClientRpc]
    private void GetCardClientRpc(int index, int cardValue, int newHandValue)
    {
        hand[index].GetComponent<CardScript>().SetSprite(deckScript.cardSprites.Value.GetSprites()[index]);
        hand[index].GetComponent<CardScript>().SetValueOfCard(cardValue);
        hand[index].GetComponent<Renderer>().enabled = true;
        handValue.Value = newHandValue;
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
    public void ResetHand()
    {
        if (IsServer)
        {
            ResetHandServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ResetHandServerRpc()
    {
        for (int i = 0; i < hand.Length; i++)
        {
            hand[i].GetComponent<CardScript>().ResetCard();
            hand[i].GetComponent<Renderer>().enabled = false;
        }
        cardIndex.Value = 0;
        handValue.Value = 0;
        aceList = new List<CardScript>();
        ResetHandClientRpc();
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
