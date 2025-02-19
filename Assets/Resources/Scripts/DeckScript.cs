
using System;
using Unity.Netcode;
using UnityEngine;

public class DeckScript : NetworkBehaviour
{
    public NetworkVariable<SpriteArrayWrapper> cardSprites = new NetworkVariable<SpriteArrayWrapper>(new SpriteArrayWrapper(new Sprite[] { }), writePerm: NetworkVariableWritePermission.Server, readPerm: NetworkVariableReadPermission.Everyone);
    public Sprite[] cardInitialSprites = new Sprite[53];
    public NetworkVariable<IntArrayWrapper> cardValues = new NetworkVariable<IntArrayWrapper>(new IntArrayWrapper(new int[] { }), writePerm: NetworkVariableWritePermission.Server, readPerm: NetworkVariableReadPermission.Everyone);
    public NetworkVariable<int> currentIndex = new NetworkVariable<int>(0, writePerm: NetworkVariableWritePermission.Server);
    private bool deckIsShuffled = false;
    private CardScript targetCardScript;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (NetworkManager.Singleton.ConnectedClientsList.Count == 1)
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
        if(!deckIsShuffled)
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

    }

    [ServerRpc(RequireOwnership = false)]
    public void DealCardServerRpc()
    {
        DealCardClientRpc();
        currentIndex.Value++;
    }

    [ClientRpc]
    private void DealCardClientRpc()
    {
        targetCardScript.SetSprite(cardSprites.Value.GetSprites()[currentIndex.Value]);
        targetCardScript.SetValueOfCard(cardValues.Value[currentIndex.Value]);
        Debug.Log("currentCardIndex is " +  currentIndex.Value);    
    }

    public int DealCard(CardScript cardScript)
    {
        targetCardScript = cardScript;
        DealCardServerRpc();
        return cardScript.GetValueOfCard();
    }

    public Sprite GetCardBack()
    {
        return cardSprites.Value.GetSprites()[0];
    }
}
