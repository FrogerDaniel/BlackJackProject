using System.Threading;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PlayerSpawner : MonoBehaviour
{
    private NetworkManager networkManager; // Reference to the NetworkManager component
    [SerializeField] GameObject dealerPrefab; // Prefab for the dealer
    [SerializeField] GameObject playerPrefab; // Prefab for the player
    [SerializeField] GameObject gameManager; // Prefab for the GameManager
    [SerializeField] GameObject mainUi; // Prefab for the main UI
    [SerializeField] GameObject deck; // Prefab for the deck
    [SerializeField] GameObject deckDealer; // Prefab for the deck dealer
    [SerializeField] GameObject UiManager; // Prefab for the UI Manager

    void Start()
    {
        Debug.Log(gameObject.name); // Log the name of the game object
        networkManager = this.GetComponent<NetworkManager>(); // Get the NetworkManager component
        networkManager.OnClientConnectedCallback += OnClientConnected; // Register the client connected callback
    }

    /***-------------------------------------------------------------------------
    * ON CLIENT CONNECTED
    * Called when a client connects to the server
    * ----------------------------------------------------------------------***/
    private void OnClientConnected(ulong clientId)
    {
        if (networkManager.ConnectedClientsList.Count == 2)
        {
            if (networkManager.IsServer)
            {
                SpawnPlayer(0, dealerPrefab); // Spawn the dealer
                SpawnPlayer(1, playerPrefab); // Spawn the player
                Thread.Sleep(1000); // Pause for 1 second to ensure players prefab spawn

                var UiManagerInstance = Instantiate(UiManager); // Instantiate the UI Manager
                UiManagerInstance.GetComponent<NetworkObject>().Spawn(); // Spawn the UI Manager network object

                var UiInstance = Instantiate(mainUi); // Instantiate the main UI
                UiInstance.GetComponent<NetworkObject>().Spawn(); // Spawn the main UI network object

                var deckInstance = Instantiate(deck); // Instantiate the deck
                deckInstance.GetComponent<NetworkObject>().Spawn(); // Spawn the deck network object

                var deckDealerInstance = Instantiate(deckDealer); // Instantiate the deck dealer
                deckDealerInstance.GetComponent<NetworkObject>().Spawn(); // Spawn the deck dealer network object

                var gameManagerInstance = Instantiate(gameManager); // Instantiate the GameManager
                gameManagerInstance.GetComponent<NetworkObject>().Spawn(); // Spawn the GameManager network object
            }
        }
        Debug.Log("Player connected with ID " + clientId); // Log the client ID
    }

    /***-------------------------------------------------------------------------
    * SPAWN PLAYER
    * Spawns the player or dealer prefab and assigns it to the connected client
    * ----------------------------------------------------------------------***/
    private void SpawnPlayer(ulong clientId, GameObject prefab)
    {
        if (networkManager.IsServer)
        {
            var playerInstance = Instantiate(prefab); // Instantiate the correct prefab (dealer or player)
            playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId); // Spawn the network object and assign it to the connected client
        }
    }
}
