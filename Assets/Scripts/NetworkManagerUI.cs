using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;

    private void Awake()
    {
        hostButton.onClick.AddListener(() => JoinHost());
        clientButton.onClick.AddListener(() => JoinClient());
    }

    private void JoinHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    private void JoinClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    private void OnDestroy()
    {
        hostButton.onClick.RemoveAllListeners();
        clientButton.onClick.RemoveAllListeners();
    }
}
