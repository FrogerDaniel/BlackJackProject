using System;
using Unity.Netcode;
using UnityEngine;

public class DeckScript : NetworkBehaviour
{
    
    public NetworkVariable<SpriteArrayWrapper> cardSprites = new NetworkVariable<SpriteArrayWrapper>(new SpriteArrayWrapper(new Sprite[] { }));
    public NetworkVariable<IntArrayWrapper> cardValues = new NetworkVariable<IntArrayWrapper>(new IntArrayWrapper(new int[] { }));
    private NetworkVariable<int> currentIndex = new NetworkVariable<int>(0, writePerm: NetworkVariableWritePermission.Server);

    void Start()
    {
        GetCardValues();
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
        // Standard array data swapping technique
        Sprite[] tempCardSprites = cardSprites.Value.GetSprites();
        int[] tempCardValues = cardValues.Value.Values;

        for (int i = tempCardSprites.Length - 1; i > 0; --i)
        {
            int j = Mathf.FloorToInt(UnityEngine.Random.Range(0.0f, 1.0f) * (tempCardSprites.Length - 1)) + 1;
            Sprite face = tempCardSprites[i];
            tempCardSprites[i] = tempCardSprites[j];
            tempCardSprites[j] = face;

            int value = tempCardValues[i];
            tempCardValues[i] = tempCardValues[j];
            tempCardValues[j] = value;
        }

        cardSprites.Value =  new SpriteArrayWrapper(tempCardSprites);
        cardValues.Value = new IntArrayWrapper(tempCardValues);
        currentIndex.Value = 1;
    }

    [ServerRpc(RequireOwnership = false)]
    public void DealCardServerRpc()
    {
        DealCardClientRpc();
    }

    [ClientRpc]
    private void DealCardClientRpc()
    {
        CardScript cardScript = FindObjectOfType<CardScript>();
        cardScript.SetSprite(cardSprites.Value.GetSprites()[currentIndex.Value]);
        cardScript.SetValueOfCard(cardValues.Value[currentIndex.Value++]);
    }
    public int DealCard(CardScript cardScript)
    {
        DealCardServerRpc();
        return cardScript.GetValueOfCard();
    }
    public Sprite GetCardBack()
    {
        return cardSprites.Value.GetSprites()[0];
    }
}



