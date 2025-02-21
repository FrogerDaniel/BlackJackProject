using Unity.Netcode;
using UnityEngine;

public class CardScript : NetworkBehaviour
{
    private NetworkVariable<int> value = new NetworkVariable<int>(0, readPerm: NetworkVariableReadPermission.Everyone); // Network variable to store the card's value
    private const string SpriteFolderPath = "Sprites/Cards Resized/"; // Path to the folder containing card sprites

    /***-------------------------------------------------------------------------
    * GET VALUE OF CARD
    * Returns the value of the card
    * ----------------------------------------------------------------------***/
    public int GetValueOfCard()
    {
        return value.Value; // Return the current value of the card
    }

    /***-------------------------------------------------------------------------
    * SET VALUE OF CARD
    * Sets the value of the card on the server or client
    * ----------------------------------------------------------------------***/
    public void SetValueOfCard(int newValue)
    {
        if (IsServer)
        {
            value.Value = newValue; // Set the value on the server
            UpdateValueClientRpc(newValue); // Update the value on all clients
        }
        else
        {
            SetValueServerRpc(newValue); // Request server to set the value
        }
    }

    /***-------------------------------------------------------------------------
    * SET VALUE SERVER RPC
    * Sets the value of the card on the server and updates clients
    * ----------------------------------------------------------------------***/
    [ServerRpc(RequireOwnership = false)]
    private void SetValueServerRpc(int newValue)
    {
        value.Value = newValue; // Set the value on the server
        UpdateValueClientRpc(newValue); // Update the value on all clients
    }

    /***-------------------------------------------------------------------------
    * UPDATE VALUE CLIENT RPC
    * Updates the value of the card on all clients
    * ----------------------------------------------------------------------***/
    [ClientRpc]
    private void UpdateValueClientRpc(int newValue)
    {
        value.Value = newValue; // Update the value on the client
    }

    /***-------------------------------------------------------------------------
    * SET SPRITE
    * Sets the sprite of the card on clients
    * ----------------------------------------------------------------------***/
    public void SetSprite(Sprite newSprite)
    {
        if (newSprite == null)
        {
            Debug.LogError("newSprite is null!"); // Log error if newSprite is null
            return;
        }

        SetSpriteClientRpc(newSprite.name); // Request clients to set the sprite
    }

    /***-------------------------------------------------------------------------
    * SET SPRITE CLIENT RPC
    * Updates the sprite of the card on all clients
    * ----------------------------------------------------------------------***/
    [ClientRpc]
    private void SetSpriteClientRpc(string spriteName)
    {
        string fullPath = SpriteFolderPath + spriteName; // Construct the full path to the sprite
        Sprite newSprite = Resources.Load<Sprite>(fullPath); // Load the sprite from resources
        if (newSprite != null)
        {
            SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>(); // Get the SpriteRenderer component
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = newSprite; // Set the sprite
            }
            else
            {
                Debug.LogError("SpriteRenderer component is missing on " + gameObject.name); // Log error if SpriteRenderer is missing
            }
        }
        else
        {
            Debug.LogError("Failed to load sprite: " + spriteName); // Log error if sprite loading fails
        }
    }

    /***-------------------------------------------------------------------------
    * RESET CARD
    * Resets the card to its initial state on clients
    * ----------------------------------------------------------------------***/
    public void ResetCard()
    {
        ResetCardClientRpc(); // Request clients to reset the card
    }

    /***-------------------------------------------------------------------------
    * RESET CARD CLIENT RPC
    * Resets the card to its initial state on all clients
    * ----------------------------------------------------------------------***/
    [ClientRpc]
    private void ResetCardClientRpc()
    {
        Sprite back = GameObject.FindGameObjectWithTag("Deck").GetComponent<DeckScript>().GetCardBack(); // Get the back sprite of the card
        gameObject.GetComponent<SpriteRenderer>().sprite = back; // Set the card back sprite
        SetValueOfCard(0); // Reset the value of the card to 0
    }
}
