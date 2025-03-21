
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class DeckScript : NetworkBehaviour
{
    //array to access sprites through network vars
    public NetworkVariable<SpriteArrayWrapper> cardSprites = new NetworkVariable<SpriteArrayWrapper>(new SpriteArrayWrapper(new Sprite[] { }), writePerm: NetworkVariableWritePermission.Server, readPerm: NetworkVariableReadPermission.Everyone);
    //array of initial sprites to set up in inspector
    public Sprite[] cardInitialSprites = new Sprite[53];
    //array to access values of sprites through network vars
    public NetworkVariable<IntArrayWrapper> cardValues = new NetworkVariable<IntArrayWrapper>(new IntArrayWrapper(new int[] { }), writePerm: NetworkVariableWritePermission.Server, readPerm: NetworkVariableReadPermission.Everyone);
    //
    public NetworkVariable<int> currentIndex = new NetworkVariable<int>(0, writePerm: NetworkVariableWritePermission.Server);
    
    private CardScript[] cardScriptsArray;
    //hand that holds cards
    private GameObject[] handForDeck = new GameObject[11];
    private PlayerScript playerScript;


    private void Awake()
    {
        StartCoroutine(FindWithDelay());
    }

    /***-------------------------------------------------------------------------
    * FINDWITHDELAY
    * uses player script to set up array of scripts of cards from hand object
    * made for loading up scripts simultaneously for all clients
    * ----------------------------------------------------------------------***/
    private IEnumerator FindWithDelay()
    {
        yield return new WaitForSeconds(2f);
        playerScript = GameObject.Find("PlayerNew(Clone)").GetComponent<PlayerScript>();
        //set up hand array from players hand
        for (int counter = 0; counter < handForDeck.Length; counter++)
        {
            handForDeck[counter] = playerScript.hand[counter];
        }
        //set up scripts from hand
        cardScriptsArray = new CardScript[11];
        for (int counter = 0; counter < cardScriptsArray.Length; counter++)
        {
            cardScriptsArray[counter] = handForDeck[counter].GetComponent<CardScript>();
        }
    }

    /***-------------------------------------------------------------------------
* ONNETWORKSPAWN
* When all players join server set up card values in array according to its sprite
* ----------------------------------------------------------------------***/
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
    /***-------------------------------------------------------------------------
* GETCARDVALUES
* Updates networ array with values of cards
* ----------------------------------------------------------------------***/
    void GetCardValues()
    {
        int num = 0;
        int[] tempCardValues = new int[cardSprites.Value.Length()];
        for (int counter = 0; counter < cardSprites.Value.Length(); counter++)
        {
            num = counter;
            //get the remainder of division from counter
            num %= 13;
            //if remainder is 10 or more, then its 10, or J,Q,K
            if (num > 10 || num == 0)
            {
                num = 10;
            }
            //if remainder is <10, assign it to the card, (++ to avoid card back at 0 spot)
            tempCardValues[counter] = num++;
        }
        //assign those values to network var
        cardValues.Value = new IntArrayWrapper(tempCardValues);
    }
    /***-------------------------------------------------------------------------
* ShuffleServerRpc
* Shuffle cards on server using standard swapping technique
* ----------------------------------------------------------------------***/
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
        //assign shuffled values and sprites
        cardSprites.Value = new SpriteArrayWrapper(tempCardSprites);
        cardValues.Value = new IntArrayWrapper(tempCardValues);
        Debug.Log("Deck was shuffled");
        currentIndex.Value = 1;
    }

    [ServerRpc(RequireOwnership = false)]
    public void DealCardServerRpc(int cardIndex)
    {
        DealCardClientRpc(cardIndex);
        currentIndex.Value++;
    }
    /***-------------------------------------------------------------------------
* DEALCARDCLIENTRPC
* Update sprites and values of cards on client side
* ----------------------------------------------------------------------***/
    [ClientRpc]
    private void DealCardClientRpc(int cardIndex)
    {
        cardScriptsArray[cardIndex].SetSprite(cardSprites.Value.GetSprites()[currentIndex.Value]);
        cardScriptsArray[cardIndex].SetValueOfCard(cardValues.Value[currentIndex.Value]);
        Debug.Log("currentCardIndex is " + currentIndex.Value);  
    }

    public int DealCard(int cardIndex)
    {
        DealCardServerRpc(cardIndex);
        return cardScriptsArray[cardIndex].GetValueOfCard();
    }
    /***-------------------------------------------------------------------------
* GETCARDBACK
* returns value of sprite
* ----------------------------------------------------------------------***/
    public Sprite GetCardBack()
    {
        return cardSprites.Value.GetSprites()[0];
    }
}
