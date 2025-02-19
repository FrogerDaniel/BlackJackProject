using Unity.Netcode;
using UnityEngine;

public class CardScript : NetworkBehaviour
{
    private NetworkVariable<int> value = new NetworkVariable<int>(0, readPerm: NetworkVariableReadPermission.Everyone);
    private const string SpriteFolderPath = "Sprites/Cards Resized/";

    public int GetValueOfCard()
    {
        return value.Value;
    }

    public void SetValueOfCard(int newValue)
    {
        if (IsServer)
        {
            value.Value = newValue;
            UpdateValueClientRpc(newValue);
        }
        else
        {
            SetValueServerRpc(newValue);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetValueServerRpc(int newValue)
    {
        value.Value = newValue;
        UpdateValueClientRpc(newValue);
    }

    [ClientRpc]
    private void UpdateValueClientRpc(int newValue)
    {
        value.Value = newValue;
    }

    public void SetSprite(Sprite newSprite)
    {
        if (newSprite == null)
        {
            Debug.LogError("newSprite is null!");
            return;
        }

        SetSpriteClientRpc(newSprite.name);
    }

    [ClientRpc]
    private void SetSpriteClientRpc(string spriteName)
    {
        string fullPath = SpriteFolderPath + spriteName;
        Sprite newSprite = Resources.Load<Sprite>(fullPath);
        if (newSprite != null)
        {
            SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = newSprite;
            }
            else
            {
                Debug.LogError("SpriteRenderer component is missing on " + gameObject.name);
            }
        }
        else
        {
            Debug.LogError("Failed to load sprite: " + spriteName);
        }
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
