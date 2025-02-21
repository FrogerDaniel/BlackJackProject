using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using Unity.Netcode;
using Cinemachine;
using System.ComponentModel;

public class GameManager : NetworkBehaviour
{
    [SerializeField] public Button dealBtn;
    [SerializeField] public Button hitBtn;
    public Button hitDealerBtn;
    [SerializeField] public Button standBtn;
    [SerializeField] public Button betBtn;
    [SerializeField] Button exitBtn;
    [SerializeField] CinemachineVirtualCamera playerCamera;
    [SerializeField] CinemachineVirtualCamera dealerCamera;

    private NetworkVariable<int> standClicks = new NetworkVariable<int>(0);
    private int standClicksLocal = 0;

    private DeckScript deck;
    private DeckScriptDealer deckDealer;

    [SerializeField] public DealerScript dealerScript;
    [SerializeField] public PlayerScript playerScript;
    [SerializeField] SceneInit sceneInit;
    private UIManager uiManager;

    public TMP_Text scoreText;
    public TMP_Text dealerScoreText;
    public TMP_Text betsText;
    public TMP_Text cashText;
    public TMP_Text mainText;

    // Card hidden dealer's 2nd card
    public GameObject hiddenCard;
    // total bet
    public NetworkVariable<int> pot = new NetworkVariable<int>(0, readPerm: NetworkVariableReadPermission.Everyone);
    public int localPotvalue = 0;
    bool gameIsPaused = false;
    void Awake()
    {
        StartCoroutine(FindWithDelay());
    }

    IEnumerator FindWithDelay()
    {
        //initialize all needed game objects with small delay to avoid null refs
        yield return new WaitForSeconds(5f);
        playerCamera = GameObject.Find("Player Camera").GetComponent<CinemachineVirtualCamera>();
        dealerCamera = GameObject.Find("Dealer Camera").GetComponent<CinemachineVirtualCamera>();
        dealerScoreText = GameObject.Find("Dealer Hand").GetComponent<TMP_Text>();

        dealBtn = GameObject.Find("Deal").GetComponent<Button>();
        hitBtn = GameObject.Find("Hit").GetComponent<Button>();
        hitDealerBtn = GameObject.Find("Hit Dealer").GetComponent<Button>();
        standBtn = GameObject.Find("Stand").GetComponent<Button>();
        betBtn = GameObject.Find("Bet").GetComponent<Button>();
        betBtn.gameObject.SetActive(false);


        dealerScript = GameObject.Find("Dealer(Clone)").GetComponent<DealerScript>();
        playerScript = GameObject.Find("PlayerNew(Clone)").GetComponent<PlayerScript>();
        deck = GameObject.FindGameObjectWithTag("Deck").GetComponent<DeckScript>();
        deckDealer = GameObject.FindGameObjectWithTag("Deck Dealer").GetComponent<DeckScriptDealer>();
        uiManager = GameObject.Find("UI Manager(Clone)").GetComponent<UIManager>();

        scoreText = GameObject.Find("Hand").GetComponent<TMP_Text>();

        betsText = GameObject.Find("Bets").GetComponent<TMP_Text>();
        cashText = GameObject.Find("Total").GetComponent<TMP_Text>();
        mainText = GameObject.Find("Main Text").GetComponent<TMP_Text>();

        hiddenCard = GameObject.Find("HideCard");

        sceneInit = GameObject.Find("SceneInit").GetComponent<SceneInit>();
        //activate cam and ui elements depending on player or host
        if (IsHost)
        {
            ActivateDealerCamera();
        }
        else
        {
            ActivatePlayerCamera();
            dealerScoreText.gameObject.SetActive(false);
            dealBtn.gameObject.SetActive(false);

        }
        hitDealerBtn.gameObject.SetActive(false);
        hitBtn.gameObject.SetActive(false);
        standBtn.gameObject.SetActive(false);
        betBtn.gameObject.SetActive(false);
        //subscribe to button click events
        dealBtn.onClick.AddListener(() => DealClicked());
        hitBtn.onClick.AddListener(() => HitClicked());
        hitDealerBtn.onClick.AddListener(() => HitDealerClicked());
        standBtn.onClick.AddListener(() => StandClicked());
        betBtn.onClick.AddListener(() => BetClicked());
    }
    //functions to activte camera depending on who player is
    private void ActivateDealerCamera()
    {
        dealerCamera.gameObject.SetActive(true);
        playerCamera.gameObject.SetActive(false);
    }

    private void ActivatePlayerCamera()
    {
        dealerCamera.gameObject.SetActive(false);
        playerCamera.gameObject.SetActive(true);
    }
    private void DealClicked()
    {
        if(IsServer)
        {
            DealClickedServerRpc();
            UpdateUiDealClientRpc();
        }

    }

    [ServerRpc(RequireOwnership = false)]
    private void DealClickedServerRpc()
    {
        //resets hand to avoid conflicts, shuffles deck, gives 2 cards to each player
        playerScript.ResetHand();
        dealerScript.ResetHand();
        deck.ShuffleServerRpc();
        deckDealer.ShuffleServerRpc();
        playerScript.StartHand();
        dealerScript.StartHand();

        //updates network pot
        pot.Value = 40;
        //call to update client pot
        UpdatePotClientRpc(pot.Value);
        //take money from player
        playerScript.AdjustMoney(-20);

    }

    [ClientRpc]
    private void UpdateUiDealClientRpc()
    {
        //call for update ui 
        uiManager.DealUi();
        if(dealerScript.handValueDealerLocal > 16)
        {
            //if dealer already has more than 16 disable hit button for him
            hitDealerBtn.gameObject.SetActive(false);
        }
    }

    [ClientRpc]
    private void UpdatePotClientRpc(int newPotValue)
    {
        localPotvalue = newPotValue;
    }
    private void HitClicked()
    {
        HitClickedServerRpc();
    }

    private void HitDealerClicked()
    {
        HitClickedDealerServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void HitClickedDealerServerRpc()
    {
        //check if there is space in hand
        if (dealerScript.cardIndexDealer.Value <= 10)
        {
            //if true get card
            dealerScript.GetCard();
            //update ui for new dealer score
            HitClickedDealerClientRpc();
            if (dealerScript.handValueDealerLocal > 16)
            {
                //if got more than 16, disable hit 
                DisableDealerHitClientRpc();
            }
            if(dealerScript.handValueDealerLocal > 20)
            {
                //if more than 21, finish round
                RoundOver();
            }
        }

    }

    [ClientRpc]
    private void HitClickedDealerClientRpc()
    {
        uiManager.ChangeDealerScore();
    }

    [ClientRpc]
    private void DisableDealerHitClientRpc()
    {
        hitDealerBtn.gameObject.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    private void HitClickedServerRpc()
    {
        //check if there is space in hand
        if (playerScript.cardIndex.Value <= 10)
        {
            //if true get card
            playerScript.GetCard();
            //update local values
            HitClickedClientRpc();
            //
            if (playerScript.handValueLocal > 20)
            {
                RoundOver();
            }
        }

    }

    [ClientRpc]
    private void HitClickedClientRpc()
    {
        uiManager.ChangePlayerScore();
    }

    private void StandClicked()
    {
        StandClickedServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void StandClickedServerRpc()
    {
        //allow up to 2 stands
        standClicks.Value++;
        StandClickedClientRpc(standClicks.Value);
        if (standClicksLocal > 1)
        {
            RoundOver();
        }
        //check for hit dealer  after each stand
        HitDealer();
    }

    [ClientRpc]
    private void StandClickedClientRpc(int newStandClicksValue)
    {
        standClicksLocal = newStandClicksValue;
    }

    private void HitDealer()
    {
        //if dealer has less than 16, take cards until false
        while (dealerScript.handValueDealerLocal < 16 && dealerScript.cardIndexDealer.Value < 10)
        {
            dealerScript.GetCard();
            UpdateUiHitDealerClientRpc();
            //
            if (dealerScript.handValueDealerLocal > 20)
            {
                RoundOver();
            }
        }
    }
    [ClientRpc]
    private void UpdateUiHitDealerClientRpc()
    {
        uiManager.ChangeDealerScore();
    }

    public void RoundOver()
    {
        RoundOverServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RoundOverServerRpc()
    {
        RoundOverClientRpc();
    }

    [ClientRpc]
    private void RoundOverClientRpc()
    {
        bool playerLost = playerScript.handValueLocal > 21;
        bool dealerLost = dealerScript.handValueDealerLocal > 21;
        bool playerHit21 = playerScript.handValueLocal == 21;
        bool dealerHit21 = dealerScript.handValueDealerLocal == 21;

        // if stand was clicked less than twice, no 21s or lost, quit function
        if (standClicksLocal < 2 && !playerLost && !dealerLost && !playerHit21 && !dealerHit21)
        {
            return;
        }
        bool roundOver = true;

        // all lost, bets returned
        if (playerLost && dealerLost)
        {
            uiManager.ChangeMainText("All Bust: Units returned");
            playerScript.AdjustMoney(pot.Value / 2);
        }
        // if player bust, or dealer has more points
        else if (playerLost || (!dealerLost && dealerScript.handValueDealerLocal > playerScript.handValueLocal))
        {
            uiManager.ChangeMainText("Dealer wins!");
            if (playerScript.GetUnits() == 0)
            {
                //Debug.Log("Should change scene");
                //StartCoroutine(sceneInit.LoadSceneAfterDelay("EndingScene", 2f));
                //No ending scene, close game
                Application.Quit();
            }
        }
        // if player has more points, or dealer bust
        else if (dealerLost || playerScript.handValueLocal > dealerScript.handValueDealerLocal)
        {
            uiManager.ChangeMainText("You got the Units!");
            playerScript.AdjustMoney(pot.Value);
        }
        // check for tie, return bets
        else if (playerScript.handValueLocal == dealerScript.handValueDealerLocal)
        {
            uiManager.ChangeMainText("Tie: Units returned");
            playerScript.AdjustMoney(pot.Value / 2);
        }
        else
        {
            roundOver = false;
        }

        // change UI for next round
        if (roundOver)
        {
            uiManager.RoundOverUi();
            standClicks.Value = 0;
            StandClickedClientRpc(standClicks.Value);
        }
    }

    private void BetClicked()
    {
        BetClickedServerRpc();

    }

    [ServerRpc(RequireOwnership = false)]
    private void BetClickedServerRpc()
    {
        if (playerScript.GetUnits() > 0)
        {
            int intBet = 20;
            playerScript.AdjustMoney(-intBet);
            pot.Value += (intBet * 2);

        }
        else
        {
            BetClickedRanOutOfUnitsClientRpc();
        }
        BetClickedClientRpc(pot.Value);
    }

    [ClientRpc]
    private void BetClickedClientRpc(int newPotValue)
    {
        localPotvalue = newPotValue;
        uiManager.ChangeBetsText();
    }
    [ClientRpc]
    private void BetClickedRanOutOfUnitsClientRpc()
    {
        uiManager.ChangeMainText("No units left");
        mainText.gameObject.SetActive(true);
        StartCoroutine(DeleteTextAfterDelay(mainText, 2f));
    }
    private IEnumerator DeleteTextAfterDelay(TMP_Text text, float delay)
    {
        yield return new WaitForSeconds(delay);

        text.gameObject.SetActive(false);
    }
}
