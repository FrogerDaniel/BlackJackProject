using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR;

public class DeckScriptDealer : NetworkBehaviour
{
    /***-------------------------------------------------------------------------
* DECKSCRIPT
* SAME SCRIPTS AS DECK SCRIPT BUT USED FOR DEALERS OBJECT OF DECK
* ----------------------------------------------------------------------***/
    public NetworkVariable<SpriteArrayWrapper> cardSprites = new NetworkVariable<SpriteArrayWrapper>(new SpriteArrayWrapper(new Sprite[] { }), writePerm: NetworkVariableWritePermission.Server, readPerm: NetworkVariableReadPermission.Everyone);
    public Sprite[] cardInitialSprites = new Sprite[53];
    public NetworkVariable<IntArrayWrapper> cardValues = new NetworkVariable<IntArrayWrapper>(new IntArrayWrapper(new int[] { }), writePerm: NetworkVariableWritePermission.Server, readPerm: NetworkVariableReadPermission.Everyone);
    public NetworkVariable<int> currentIndex = new NetworkVariable<int>(0, writePerm: NetworkVariableWritePermission.Server);
    private bool deckIsShuffled = false;
    //private CardScript targetCardScript;
    private CardScript[] cardScriptsArray;
    private GameObject[] handForDeck = new GameObject[12];
    private DealerScript dealerScript;


    private void Awake()
    {
        StartCoroutine(FindWithDelay());
    }

    private IEnumerator FindWithDelay()
    {
        yield return new WaitForSeconds(2f);
        dealerScript = GameObject.Find("Dealer(Clone)").GetComponent<DealerScript>();
        for(int i = 0;  i < handForDeck.Length; i++)
        {
            handForDeck[i] = dealerScript.hand[i];
        }
        cardScriptsArray = new CardScript[12];
        for (int i = 0; i < cardScriptsArray.Length; i++)
        {
            cardScriptsArray[i] = handForDeck[i].GetComponent<CardScript>();
        }
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (NetworkManager.Singleton.ConnectedClientsList.Count == 2)
        {
            cardSprites.Value = new SpriteArrayWrapper(cardInitialSprites);
            foreach (var spriteName in cardSprites.Value.SpriteNames)
            {
                Debug.Log("Assigned Sprite: " + spriteName);
            }
            GetCardValues();
        }
    }

    void GetCardValues()
    {
        int num = 0;
        int[] tempCardValues = new int[cardSprites.Value.Length()];
        for (int i = 0; i < cardSprites.Value.Length(); i++)
        {
            num = i;
            num %= 13;
            if (num > 10 || num == 0)
            {
                num = 10;
            }
            tempCardValues[i] = num++;
        }
        cardValues.Value = new IntArrayWrapper(tempCardValues);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ShuffleServerRpc()
    {
        Sprite[] tempCardSprites = cardSprites.Value.GetSprites();
        int[] tempCardValues = cardValues.Value.Values;

        for (int i = tempCardSprites.Length - 1; i > 0; --i)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            Sprite face = tempCardSprites[i];
            tempCardSprites[i] = tempCardSprites[j];
            tempCardSprites[j] = face;

            int value = tempCardValues[i];
            tempCardValues[i] = tempCardValues[j];
            tempCardValues[j] = value;
        }

        cardSprites.Value = new SpriteArrayWrapper(tempCardSprites);
        cardValues.Value = new IntArrayWrapper(tempCardValues);
        Debug.Log("Deck was shuffled");
        deckIsShuffled = true;
        currentIndex.Value = 1;
    }

    [ServerRpc(RequireOwnership = false)]
    public void DealCardServerRpc(int cardIndex)
    {
        DealCardClientRpc(cardIndex);
        currentIndex.Value++;
    }

    [ClientRpc]
    private void DealCardClientRpc(int cardIndex)
    {
        cardScriptsArray[cardIndex].SetSprite(cardSprites.Value.GetSprites()[currentIndex.Value]);
        cardScriptsArray[cardIndex].SetValueOfCard(cardValues.Value[currentIndex.Value]);
        Debug.Log("currentCardIndex is " + currentIndex.Value);
    }

    public int DealCard(int cardIndex)
    {
        //targetCardScript = cardScriptsArray[cardIndex];
        DealCardServerRpc(cardIndex);
        return cardScriptsArray[cardIndex].GetValueOfCard();
    }

    public Sprite GetCardBack()
    {
        return cardSprites.Value.GetSprites()[0];
    }
}
