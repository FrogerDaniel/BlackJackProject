using Mono.Cecil;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;

public class CardScript : NetworkBehaviour
{
    //store value of a card 
    private NetworkVariable<int> value = new NetworkVariable<int>(0);

    public int GetValueOfCard()
    {
        return value.Value;
    }

    public void SetValueOfCard(int newValue)
    {

        SetValueServerRpc(newValue);
    }
    [ServerRpc(RequireOwnership = false)]
    private void SetValueServerRpc(int newValue)
    {
        SetValueClientRpc(newValue);
    }
    [ClientRpc]
    private void SetValueClientRpc(int newValue)
    {
        value.Value = newValue;
    }
    public void SetSprite(Sprite newSprite)
    {
        SetSpriteClientRpc(newSprite.name);
    }
    [ClientRpc]
    private void SetSpriteClientRpc(string spriteName)
    {
        Sprite newSprite = Resources.Load<Sprite>(spriteName);
        gameObject.GetComponent<SpriteRenderer>().sprite = newSprite;
    }
    public string GetSpriteName()
    {
        return GetComponent<SpriteRenderer>().sprite.name;   
    }

    public void ResetCard()
    {
        ResetCardClientRpc();
    }
    [ClientRpc]
    private void ResetCardClientRpc()
    {
        Sprite back = GameObject.Find("Deck").GetComponent<DeckScript>().GetCardBack();
        gameObject.GetComponent<SpriteRenderer>().sprite = back;
        SetValueOfCard(0);
    }
}
