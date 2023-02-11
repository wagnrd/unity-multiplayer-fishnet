using FishNet;
using UnityEngine;

internal enum StartOption
{
    Server,
    Client,
    Host
}

public class NetworkController : MonoBehaviour
{
    [Header("Development")]
    [SerializeField] private StartOption _editorStartOption;
    [SerializeField] private StartOption _standaloneStartOption;

    private void Start()
    {
#if UNITY_EDITOR
        StartConnectionByOption(_editorStartOption);
#elif UNITY_STANDALONE && DEVELOPMENT_BUILD
        StartConnectionByOption(_standaloneStartOption);
#elif UNITY_SERVER
        InstanceFinder.ServerManager.StartConnection();
#else
        InstanceFinder.ClientManager.StartConnection();
#endif
    }

    private void StartConnectionByOption(StartOption startOption)
    {
        if (startOption is StartOption.Server or StartOption.Host)
            InstanceFinder.ServerManager.StartConnection();
        if (startOption is StartOption.Client or StartOption.Host)
            InstanceFinder.ClientManager.StartConnection();
    }
}