using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DealerScript : NetworkBehaviour
{
    [SerializeField] private CardScript cardScript;
    [SerializeField]private DeckScript deckDealerScript;

    public NetworkVariable<int> handValueDealer = new NetworkVariable<int>(0, writePerm: NetworkVariableWritePermission.Server, readPerm: NetworkVariableReadPermission.Everyone);
    public GameObject[] hand;
    public NetworkVariable<int> cardIndexDealer = new NetworkVariable<int>(0, writePerm: NetworkVariableWritePermission.Server, readPerm: NetworkVariableReadPermission.Everyone);
    private List<CardScript> aceList = new List<CardScript>();

    private void Awake()
    {

        deckDealerScript = GameObject.FindGameObjectWithTag("Deck Dealer").GetComponent<DeckScript>();
    }

    public void StartHand()
    {
        if (IsServer)
        {
            cardIndexDealer.Value = 1;
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
        return handValueDealer.Value;
    }

    //[ServerRpc(RequireOwnership = false)]
    //private void RequestCardServerRpc()
    //{
    //    int cardValue = DealCardOnServer();
    //    UpdateHandValueClientRpc(cardValue);
    //}

    private int DealCardOnServer()
    {
        int cardValue = deckDealerScript.DealCard(hand[cardIndexDealer.Value].GetComponent<CardScript>());
        hand[cardIndexDealer.Value].GetComponent<Renderer>().enabled = true;
        //handValueDealer.Value += cardValue;
        if (cardValue == 1)
        {
            aceList.Add(hand[cardIndexDealer.Value].GetComponent<CardScript>());
        }
        AceCheck();
        UpdateCardClientRpc(cardIndexDealer.Value, cardValue);
        cardIndexDealer.Value++;
        Debug.Log("Card was given to dealer with value of " + cardValue);
        return cardValue;
    }

    [ClientRpc]
    private void UpdateCardClientRpc(int index, int cardValue)
    {
        hand[index].GetComponent<CardScript>().SetSprite(deckDealerScript.cardSprites.Value.GetSprites()[index]);
        hand[index].GetComponent<CardScript>().SetValueOfCard(cardValue);
        hand[index].GetComponent<Renderer>().enabled = true;
    }

    [ClientRpc]
    private void UpdateHandValueClientRpc(int cardValue)
    {
        handValueDealer.Value += cardValue;
    }

    private void AceCheck()
    {
        foreach (CardScript ace in aceList)
        {
            if (handValueDealer.Value + 10 < 22 && ace.GetValueOfCard() == 1)
            {
                ace.SetValueOfCard(11);
                handValueDealer.Value += 10;
            }
            else if (handValueDealer.Value > 21 && ace.GetValueOfCard() == 11)
            {
                ace.SetValueOfCard(1);
                handValueDealer.Value -= 10;
            }
        }
    }

    public void ResetHand()
    {
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
        cardIndexDealer.Value = 0;
        handValueDealer.Value = 0;
        aceList = new List<CardScript>();
    }
}

