using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Cinemachine;
using TMPro;

public class UIManager : NetworkBehaviour
{
    private GameManager gameManager;

    private void Awake()
    {
        StartCoroutine(FindWithDelay());
    }
    private IEnumerator FindWithDelay()
    {
        yield return new WaitForSeconds(5f);
        gameManager = GameObject.Find("GameManager(Clone)").GetComponent<GameManager>();
    }
    public void DealUi()
    {
        Debug.Log(gameManager.playerScript.handValueLocal.ToString() + "Value before updating UI");
        gameManager.scoreText.text = "Hand: " + gameManager.playerScript.handValueLocal.ToString();
        gameManager.mainText.gameObject.SetActive(false);
        gameManager.dealerScoreText.text = "Hand: " + gameManager.dealerScript.handValueDealerLocal.ToString();
        gameManager.hiddenCard.GetComponent<Renderer>().enabled = true;
        gameManager.dealBtn.gameObject.SetActive(false);
        if (!IsHost)
        {
            gameManager.dealerScoreText.gameObject.SetActive(false);
            gameManager.betBtn.gameObject.SetActive(true);
            gameManager.hitBtn.gameObject.SetActive(true);
            gameManager.standBtn.gameObject.SetActive(true);
        }
        else
        {
            gameManager.hitDealerBtn.gameObject.SetActive(true);
        }
        gameManager.betsText.text = "Bets: " + gameManager.localPotvalue.ToString() + " CW";
        gameManager.cashText.text = gameManager.playerScript.GetUnits().ToString() + " CW";
    }

    public void RoundOverUi()
    {

        if(!IsHost)
        {
            gameManager.hitBtn.gameObject.SetActive(false);
            gameManager.standBtn.gameObject.SetActive(false);
            gameManager.betBtn.gameObject.SetActive(false);
        }
        else
        {
            gameManager.dealBtn.gameObject.SetActive(true);

        }
        gameManager.mainText.gameObject.SetActive(true);
        gameManager.hitDealerBtn.gameObject.SetActive(false);
        gameManager.dealerScoreText.gameObject.SetActive(true);
        gameManager.hiddenCard.GetComponent<Renderer>().enabled = false;
        gameManager.cashText.text = gameManager.playerScript.GetUnits().ToString() + " CW";
    }
    public void ChangePlayerScore()
    {
        gameManager.scoreText.text = "Hand: " + gameManager.playerScript.handValueLocal.ToString();
    }

    public void ChangeDealerScore()
    {
        gameManager.dealerScoreText.text = "Hand: " + gameManager.dealerScript.handValueDealerLocal.ToString();
    }
    public void ChangeBetsText()
    {
        gameManager.cashText.text = gameManager.playerScript.GetUnits().ToString() + " CW";

        gameManager.betsText.text = "Bets: " + gameManager.localPotvalue.ToString() + "CW";
    }
    public void ChangeMainText(string text)
    {
        gameManager.mainText.text = text;
    }

}
