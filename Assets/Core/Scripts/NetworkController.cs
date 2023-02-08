using FishNet;
using UnityEngine;

public class NetworkController : MonoBehaviour
{
    [SerializeField] private bool _isClient;

    private void Start()
    {
        if (_isClient)
            InstanceFinder.ClientManager.StartConnection();
    }
}