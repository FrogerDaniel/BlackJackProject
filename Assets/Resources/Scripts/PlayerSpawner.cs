using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class PlayerSpawner : MonoBehaviour
{
    private NetworkManager networkManager;
    [SerializeField] GameObject dealerPrefab;
    [SerializeField] GameObject playerPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        networkManager = this.GetComponent<NetworkManager>();
        networkManager.OnClientConnectedCallback += OnClientConnected;
    }
    private void OnClientConnected(ulong clientId)
    {
        networkManager.NetworkConfig.PlayerPrefab = playerPrefab;
        Debug.Log("Player connected with ID" +  clientId);
    }
}
