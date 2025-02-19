using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Netcode;
using Cinemachine;
using TMPro;
using UnityEditor.Experimental.GraphView;

public class UIManager : NetworkBehaviour
{
    private GameManager gameManager;
    [ClientRpc]
    public void UpdateUIClientRpc()
    {

        // update player score
        gameManager.scoreText.text = "Hand: " + gameManager.playerScript.handValue.Value.ToString();
        gameManager.dealerScoreText.text = "Hand: " + gameManager.dealerScript.handValueDealer.Value.ToString();
        gameManager.hiddenCard.GetComponent<Renderer>().enabled = true;

        // hide buttons 
        gameManager.dealBtn.gameObject.SetActive(false);
        gameManager.hitBtn.gameObject.SetActive(true);
        gameManager.foldBtn.gameObject.SetActive(true);

        // set standard pot size

        gameManager.betsText.text = "Bets: " + gameManager.pot.Value.ToString() + " CW";
        gameManager.cashText.text = gameManager.playerScript.GetUnits().ToString() + " CW";
    }

    public void RoundUiUpdate()
    {
        gameManager.hitBtn.gameObject.SetActive(false);
        gameManager.foldBtn.gameObject.SetActive(false);
        gameManager.dealBtn.gameObject.SetActive(true);
        gameManager.mainText.gameObject.SetActive(true);
        gameManager.betBtn.gameObject.SetActive(false);
        gameManager.dealerScoreText.gameObject.SetActive(true);
        gameManager.hiddenCard.GetComponent<Renderer>().enabled = false;
        gameManager.cashText.text = gameManager.playerScript.GetUnits().ToString() + " CW";
    }


}
