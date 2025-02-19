using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class PlayerSpawner : MonoBehaviour
{
    private NetworkManager networkManager;
    [SerializeField] GameObject dealerPrefab;
    [SerializeField] GameObject playerPrefab;

    void Start()
    {
        networkManager = this.GetComponent<NetworkManager>();
        networkManager.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        // Check if this is the first client connected, which will be the dealer
        if (networkManager.ConnectedClientsList.Count == 1)
        {
            // Spawn dealer
            SpawnPlayer(clientId, dealerPrefab);
        }
        else
        {
            // Spawn player
            SpawnPlayer(clientId, playerPrefab);
        }
        Debug.Log("Player connected with ID " + clientId);
    }

    private void SpawnPlayer(ulong clientId, GameObject prefab)
    {
        // Instantiate the correct prefab (dealer or player)
        var playerInstance = Instantiate(prefab);
        // Spawn the network object and assign it to the connected client
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }
}
