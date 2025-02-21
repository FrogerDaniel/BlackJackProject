using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DealerScript : NetworkBehaviour
{
    /***-------------------------------------------------------------------------
* DEALERSCRIPT
* SAME SCRIPTS AS PLAYERS SCRIPT BUT USED FOR DEALERS PLAYER OBJECT, EXCLUDING ADJUST MONEY
* ----------------------------------------------------------------------***/
    [SerializeField]private DeckScriptDealer deckDealerScript;

    public NetworkVariable<int> handValueDealer = new NetworkVariable<int>(0, writePerm: NetworkVariableWritePermission.Server, readPerm: NetworkVariableReadPermission.Everyone);
    public GameObject[] hand;
    private CardScript[] cardScripts;
    public NetworkVariable<int> cardIndexDealer = new NetworkVariable<int>(0, writePerm: NetworkVariableWritePermission.Server, readPerm: NetworkVariableReadPermission.Everyone);
    private List<CardScript> aceList = new List<CardScript>();
    public int handValueDealerLocal = 0;

    private void Awake()
    {
        cardIndexDealer.Value = 1;
        StartCoroutine(FindWithDelay());
    }

    private IEnumerator FindWithDelay()
    {
        yield return new WaitForSeconds(2f);
        deckDealerScript = GameObject.FindGameObjectWithTag("Deck Dealer").gameObject.GetComponent<DeckScriptDealer>();
        cardScripts = new CardScript[hand.Length];
        for(int i = 0; i < hand.Length; i++)
        {
            cardScripts[i] = hand[i].GetComponent<CardScript>();
        }

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
        RequestCardServerRpc();
        return handValueDealer.Value;
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestCardServerRpc()
    {

        int cardValue = DealCardOnServer();
        UpdateHandValueServerRpc(cardValue);
    }

    private int DealCardOnServer()
    {
        int cardValue = deckDealerScript.DealCard(cardIndexDealer.Value);
        hand[cardIndexDealer.Value].GetComponent<Renderer>().enabled = true;
        //handValueDealer.Value += cardValue;
        if (cardValue == 1)
        {
            aceList.Add(hand[cardIndexDealer.Value].GetComponent<CardScript>());
        }
        UpdateCardClientRpc(cardIndexDealer.Value, cardValue);
        cardIndexDealer.Value++;
        Debug.Log("Card was given to dealer with value of " + cardValue);
        return cardValue;
    }

    [ClientRpc]
    private void UpdateCardClientRpc(int index, int cardValue)
    {
        cardScripts[index].SetSprite(deckDealerScript.cardSprites.Value.GetSprites()[index]);
        cardScripts[index].SetValueOfCard(cardValue);
        hand[index].GetComponent<Renderer>().enabled = true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateHandValueServerRpc(int cardValue)
    {
        handValueDealer.Value += cardValue;
        AceCheck();
        UpdateHandValueClientRpc(handValueDealer.Value);
    }

    [ClientRpc]
    private void UpdateHandValueClientRpc(int newHandValue)
    {
        handValueDealerLocal = newHandValue;
        Debug.Log(handValueDealerLocal.ToString() + " this is local hand value of dealer");
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
        ResetHandServerRpc(0);
        ResetHandClientRpc(handValueDealer.Value);
    }

    [ServerRpc]
    private void ResetHandServerRpc(int newHandValue)
    {
        cardIndexDealer.Value = 1;
        handValueDealer.Value = newHandValue;
    }

    [ClientRpc]
    private void ResetHandClientRpc(int newHandValue)
    {
        for (int i = 0; i < hand.Length; i++)
        {
            hand[i].GetComponent<CardScript>().ResetCard();
            hand[i].GetComponent<Renderer>().enabled = false;
        }
        handValueDealerLocal = newHandValue;
        aceList = new List<CardScript>();
    }
}

