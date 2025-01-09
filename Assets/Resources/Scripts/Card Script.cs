using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class CardScript : MonoBehaviour
{
    //store value of a card 
    private int value = 0;

    public int GetValueOfCard()
    {
        return value;
    }

    public void SetValueOfCard(int newValue)
    {
        value = newValue;
        
    }
    public void SetSprite(Sprite newSprite)
    {
        gameObject.GetComponent<SpriteRenderer>().sprite = newSprite;
    }
    public string GetSpriteName()
    {
        return GetComponent<SpriteRenderer>().sprite.name;   
    }

    public void ResetCard()
    {
        Sprite back = GameObject.Find("Deck").GetComponent<DeckScript>().GetCardBack();
        gameObject.GetComponent<SpriteRenderer>().sprite = back;
        value = 0;
    }
}
