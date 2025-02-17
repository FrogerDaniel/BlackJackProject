using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    [SerializeField] Button dealBtn;
    [SerializeField] Button hitBtn;
    [SerializeField] Button standBtn;
    [SerializeField] Button betBtn;
    [SerializeField] Canvas pauseCanvas;
    [SerializeField] Canvas mainUICanvas;
    [SerializeField] Button exitBtn;
    private int standClicks = 0;

    [SerializeField]DealerScript dealerScript;
    [SerializeField]PlayerScript playerScript;
    [SerializeField] SceneInit sceneInit;

    public TMP_Text scoreText;
    public TMP_Text dealerScoreText;
    public TMP_Text betsText;
    public TMP_Text cashText;
    public TMP_Text mainText;
    public TMP_Text standBtnText;

    // Card hidden dealer's 2nd card

    public GameObject hiddenCard;
    //total bet
    int pot = 0;
    bool gameIsPaused = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        dealBtn.onClick.AddListener(() => DealClicked());
        hitBtn.onClick.AddListener(() => HitClicked());
        standBtn.onClick.AddListener(() => StandClicked());
        betBtn.onClick.AddListener(() => BetClicked());
        //exitBtn.onClick.AddListener(() => ExitClicked());
    }

    //private void Update()
    //{
    //    PauseTheGame();
    //}

    private void DealClicked()
    {
        playerScript.ResetHand();
        dealerScript.ResetHand();
        betBtn.gameObject.SetActive(true);
        mainText.gameObject.SetActive(false);
        dealerScoreText.gameObject.SetActive(false);
        GameObject.Find("Deck").GetComponent<DeckScript>().ShuffleServerRpc();
        playerScript.StartHand();
        dealerScript.StartHand();
        //update player score
        scoreText.text = "Hand: " +  playerScript.handValue.Value.ToString();
        dealerScoreText.text = "Hand: " + dealerScript.handValue.ToString();
        hiddenCard.GetComponent<Renderer>().enabled = true;
        //hide buttons 
        dealBtn.gameObject.SetActive(false);
        hitBtn.gameObject.SetActive(true);
        standBtn.gameObject.SetActive(true);
        standBtnText.text = "Stand";
        //set standard pot size
        pot = 40;
        betsText.text = "Bets: " + pot.ToString() + " CW";
        playerScript.AdjustMoney(-20);
        cashText.text = playerScript.GetUnits().ToString() + " CW";
    }
    private void HitClicked()
    {
        //check if there is still room for cards
        if(playerScript.cardIndex.Value <= 10)
        {
            playerScript.GetCard();
            scoreText.text = "Hand: " + playerScript.handValue.ToString();
            if(playerScript.handValue.Value > 20)
            {
                RoundOver();
            }
        }
    }
    private void StandClicked()
    {
        standClicks++;
        if (standClicks > 1)
        {
            RoundOver();
        }
        HitDealer();
        standBtnText.text = "Call";
      
    }

    private void HitDealer()
    {
        while(dealerScript.handValue.Value < 16 && dealerScript.cardIndex.Value < 10)
        {
            dealerScript.GetCard();
            dealerScoreText.text = "Hand: " + dealerScript.handValue.ToString();
            if(dealerScript.handValue.Value > 20)
            {
                RoundOver();
            }
            //show dealer score
        }
    }

    public void RoundOver()
    {
        bool playerLost = playerScript.handValue.Value > 21;
        bool dealerLost = dealerScript.handValue.Value > 21;
        bool playerHit21 = playerScript.handValue.Value == 21;
        bool dealerHit21 = dealerScript.handValue.Value == 21;
        //if stand was clicked less than twice, no 21s or lost, quit function
        if(standClicks < 2 && !playerLost && !dealerLost && !playerHit21 && !dealerHit21)
        {
            return;
        }
        bool roundOver = true;
        // all lost, bets returned
        if(playerLost && dealerLost)
        {
            mainText.text = "All Bust: Wrappers returned";
            playerScript.AdjustMoney(pot / 2);
        }
        //if player bust, or dealer has more ponts
        else if (playerLost || (!dealerLost && dealerScript.handValue.Value > playerScript.handValue.Value))
        {
            mainText.text = "Dealer wins!";
            if (playerScript.GetUnits() == 0)
            {
                Debug.Log("Should change scene");
                StartCoroutine(sceneInit.LoadSceneAfterDelay("EndingScene", 2f)); 
                
            }
        }
        //if player has more points, or dealer bust
        else if (dealerLost || playerScript.handValue.Value > dealerScript.handValue.Value)
        {
            mainText.text = "You got the wrappers!";
            playerScript.AdjustMoney(pot);
        }
        //check for tie, return bets
        else if (playerScript.handValue.Value == dealerScript.handValue.Value)
        {
            mainText.text = "Tie: Wrappers returned";
            playerScript.AdjustMoney(pot / 2);
        }
        else
        {
            roundOver = false;
        }
        //change ui for next round
        if(roundOver)
        {
            hitBtn.gameObject.SetActive(false);
            standBtn.gameObject.SetActive(false);
            dealBtn.gameObject.SetActive(true);
            mainText.gameObject.SetActive(true);
            betBtn.gameObject.SetActive(false);
            dealerScoreText.gameObject.SetActive(true);
            hiddenCard.GetComponent<Renderer>().enabled = false;
            cashText.text = playerScript.GetUnits().ToString() + " CW";
            standClicks = 0;
        }
    }

    private void BetClicked()
    {
        if(playerScript.GetUnits() > 0)
        {
            TMP_Text newBet = betBtn.GetComponentInChildren(typeof(TMP_Text)) as TMP_Text;
            int intBet = int.Parse(newBet.text.ToString().Remove(2, 2));
            playerScript.AdjustMoney(-intBet);
            cashText.text = playerScript.GetUnits().ToString() + " CW";
            pot += (intBet * 2);
            betsText.text = "Bets: " + pot.ToString() + "CW";
        }
        else
        {
            mainText.text = "No candy wrappers left";
            mainText.gameObject.SetActive(true);
            StartCoroutine(DeleteTextAfterDelay(mainText, 2f));
        }

    }

    //private void PauseTheGame()
    //{
        
    //    if (Input.GetKeyDown(KeyCode.Escape) && !gameIsPaused)
    //    {
    //        Debug.Log("Pause");
    //        pauseCanvas.gameObject.SetActive(true);
    //        mainUICanvas.gameObject.SetActive(false);
    //        gameIsPaused = true;
    //        Time.timeScale = 0;
    //        exitBtn.gameObject.SetActive(true);
    //    }
    //    else if (Input.GetKeyDown(KeyCode.Escape) && gameIsPaused)
    //    {
    //        Debug.Log("Unpause");
    //        pauseCanvas.gameObject.SetActive(false);
    //        mainUICanvas.gameObject.SetActive(true);
    //        gameIsPaused = false;
    //        Time.timeScale = 1;
    //        exitBtn.gameObject.SetActive(false);
    //    }
    //}

    private IEnumerator DeleteTextAfterDelay(TMP_Text text, float delay)
    {
        yield return new WaitForSeconds(delay);

        text.gameObject.SetActive(false);
    }

    private void ExitClicked()
    {
        Debug.Log("Exit Game");
        Application.Quit();
    }
}
