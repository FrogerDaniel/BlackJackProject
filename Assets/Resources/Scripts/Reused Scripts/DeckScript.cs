using UnityEngine;

public class DeckScript : MonoBehaviour
{
    public Sprite[] cardSprites;
    int[] cardValues = new int[53];
    int currentIndex = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetCardValues();
    }
 
    void GetCardValues()
    {
        int num = 0;
        //num to use in for loop
        for (int i = 0; i < cardSprites.Length; i++)
        {
            //assign value from a counter
            num = i;
            //get the remainder from division
            num %= 13;
            //check if a card is 10, or J,Q,K
            if(num > 10 || num == 0)
            {
                //if true assign them 10
                num = 10;
            }
            //add a value to the array
            cardValues[i] = num++;
        }
    }
    public void Shuffle()
    {
        // Standard array data swapping technique
        for (int i = cardSprites.Length - 1; i > 0; --i)
        {
            int j = Mathf.FloorToInt(Random.Range(0.0f, 1.0f) * (cardSprites.Length - 1)) + 1;
            Sprite face = cardSprites[i];
            cardSprites[i] = cardSprites[j];
            cardSprites[j] = face;

            int value = cardValues[i];
            cardValues[i] = cardValues[j];
            cardValues[j] = value;
        }
        currentIndex = 1;
    }

    public int DealCard(CardScript cardScript)
    {
        cardScript.SetSprite(cardSprites[currentIndex]);
        cardScript.SetValueOfCard(cardValues[currentIndex++]);
        return cardScript.GetValueOfCard();
    }

    public Sprite GetCardBack()
    {
        return cardSprites[0];
    }
}
