
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using Unity.Netcode;
using Cinemachine;

public class GameManager : NetworkBehaviour
{
    [SerializeField] public Button dealBtn;
    [SerializeField] public Button hitBtn;
    [SerializeField] public Button foldBtn;
    [SerializeField] public Button betBtn;
    [SerializeField] Canvas pauseCanvas;
    [SerializeField] Canvas mainUICanvas;
    [SerializeField] Button exitBtn;
    [SerializeField] CinemachineVirtualCamera playerCamera;
    [SerializeField] CinemachineVirtualCamera dealerCamera;

    private int standClicks = 0;

    [SerializeField] public DealerScript dealerScript;
    [SerializeField] public PlayerScript playerScript;
    [SerializeField] SceneInit sceneInit;
    [SerializeField] UIManager uiManager;

    public TMP_Text scoreText;
    public TMP_Text dealerScoreText;
    public TMP_Text betsText;
    public TMP_Text cashText;
    public TMP_Text mainText;

    // Card hidden dealer's 2nd card
    public GameObject hiddenCard;
    // total bet
    //int pot = 0;
    public NetworkVariable<int> pot = new NetworkVariable<int>(0, readPerm: NetworkVariableReadPermission.Everyone);
    bool gameIsPaused = false;

    public NetworkVariable<bool> isDealer = new NetworkVariable<bool>(false);

    void Start()
    {
        dealBtn.onClick.AddListener(() => DealClicked());
        hitBtn.onClick.AddListener(() => HitClicked());
        foldBtn.onClick.AddListener(() => StandClicked());
        betBtn.onClick.AddListener(() => BetClicked());
        //exitBtn.onClick.AddListener(() => ExitClicked());
    }

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

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Assign the dealer role to one player, e.g., the first player who connects
        if (NetworkManager.Singleton.ConnectedClientsList.Count == 1)
        {
            isDealer.Value = true;
            ActivateDealerCamera();
            dealerScript = GameObject.FindGameObjectWithTag("Dealer").GetComponent<DealerScript>();
        }
        else
        {
            isDealer.Value = false;
            playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
            ActivatePlayerCamera();
        }
    }

    private void DealClicked()
    {

            DealClickedServerRpc();
        

    }

    [ServerRpc(RequireOwnership = false)]
    private void DealClickedServerRpc()
    {
        if (playerScript != null)
        {
            playerScript.ResetHand();
            Debug.Log("Player Script is not null");
        }
        else
        {
            Debug.Log("Player script was null");
            playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
            playerScript.ResetHand();
        }

        if (dealerScript != null)
        {
            dealerScript.ResetHand();
        }
        else
        {
            dealerScript = GameObject.FindGameObjectWithTag("Dealer").GetComponent<DealerScript>();
            Debug.Log("Dealer script was null");
        }

        betBtn.gameObject.SetActive(true);
        mainText.gameObject.SetActive(false);
        dealerScoreText.gameObject.SetActive(false);
        GameObject.Find("Deck").GetComponent<DeckScript>().ShuffleServerRpc();
        GameObject.FindGameObjectWithTag("Deck Dealer").GetComponent<DeckScript>().ShuffleServerRpc();
        playerScript.StartHand();
        dealerScript.StartHand();
        pot.Value = 40;
        playerScript.AdjustMoney(-20);
        DealClickedDealerClientRpc();
    }

    [ClientRpc]
    private void DealClickedDealerClientRpc()
    {
            uiManager.UpdateUIClientRpc();
        

    }


    private void HitClicked()
    {
        HitClickedServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void HitClickedServerRpc()
    {

            HitClickedClientRpc();
        

    }

    [ClientRpc]
    private void HitClickedClientRpc()
    {
        if (playerScript.cardIndex.Value <= 10)
        {
            playerScript.GetCard();
            scoreText.text = "Hand: " + playerScript.handValue.Value.ToString();
            if (playerScript.handValue.Value > 20)
            {
                RoundOver();
            }
        }
    }

    private void StandClicked()
    {
        StandClickedServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void StandClickedServerRpc()
    {
        StandClickedClientRpc();

    }

    [ClientRpc]
    private void StandClickedClientRpc()
    {
        standClicks++;
        if (standClicks > 1)
        {
            RoundOver();
        }
        HitDealer();
    }

    private void HitDealer()
    {
        while (dealerScript.handValueDealer.Value < 16 && dealerScript.cardIndexDealer.Value < 10)
        {
            dealerScript.GetCard();
            dealerScoreText.text = "Hand: " + dealerScript.handValueDealer.Value.ToString();
            if (dealerScript.handValueDealer.Value > 20)
            {
                RoundOver();
            }
            // show dealer score
        }
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
        bool playerLost = playerScript.handValue.Value > 21;
        bool dealerLost = dealerScript.handValueDealer.Value > 21;
        bool playerHit21 = playerScript.handValue.Value == 21;
        bool dealerHit21 = dealerScript.handValueDealer.Value == 21;

        // if stand was clicked less than twice, no 21s or lost, quit function
        if (standClicks < 2 && !playerLost && !dealerLost && !playerHit21 && !dealerHit21)
        {
            return;
        }
        bool roundOver = true;

        // all lost, bets returned
        if (playerLost && dealerLost)
        {
            mainText.text = "All Bust: Units returned";
            playerScript.AdjustMoney(pot.Value / 2);
        }
        // if player bust, or dealer has more points
        else if (playerLost || (!dealerLost && dealerScript.handValueDealer.Value > playerScript.handValue.Value))
        {
            mainText.text = "Dealer wins!";
            if (playerScript.GetUnits() == 0)
            {
                Debug.Log("Should change scene");
                StartCoroutine(sceneInit.LoadSceneAfterDelay("EndingScene", 2f));
            }
        }
        // if player has more points, or dealer bust
        else if (dealerLost || playerScript.handValue.Value > dealerScript.handValueDealer.Value)
        {
            mainText.text = "You got the Units!";
            playerScript.AdjustMoney(pot.Value);
        }
        // check for tie, return bets
        else if (playerScript.handValue.Value == dealerScript.handValueDealer.Value)
        {
            mainText.text = "Tie: Units returned";
            playerScript.AdjustMoney(pot.Value / 2);
        }
        else
        {
            roundOver = false;
        }

        // change UI for next round
        if (roundOver)
        {
            uiManager.RoundUiUpdate();
            standClicks = 0;
        }
    }

    private void BetClicked()
    {
        BetClickedServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void BetClickedServerRpc()
    {
        BetClickedClientRpc();
    }

    [ClientRpc]
    private void BetClickedClientRpc()
    {
        if (playerScript.GetUnits() > 0)
        {
            TMP_Text newBet = betBtn.GetComponentInChildren(typeof(TMP_Text)) as TMP_Text;
            int intBet = int.Parse(newBet.text.ToString().Remove(2, 2));
            playerScript.AdjustMoney(-intBet);
            cashText.text = playerScript.GetUnits().ToString() + " CW";
            pot.Value += (intBet * 2);
            betsText.text = "Bets: " + pot.Value.ToString() + "CW";
        }
        else
        {
            mainText.text = "No units left";
            mainText.gameObject.SetActive(true);
            StartCoroutine(DeleteTextAfterDelay(mainText, 2f));

        }
    }
    private IEnumerator DeleteTextAfterDelay(TMP_Text text, float delay)
    {
        yield return new WaitForSeconds(delay);

        text.gameObject.SetActive(false);
    }
}
