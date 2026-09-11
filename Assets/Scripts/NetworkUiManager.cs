using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUiManager : MonoBehaviour
{
    [SerializeField] private Button hostButton, clientButton;
    // private string targetIP, serverIP;
    // private ushort targetPort, serverPort;

    [Header("Client/Target Settings")]
    [SerializeField] private InputField targetIPInputField;
    [SerializeField] private InputField targetPortInputField;

    [Header("Server/Host Settings")]
    [SerializeField] private InputField serverIPInputField;
    [SerializeField] private InputField serverPortInputField;

    [Header("Defaults")]
    [SerializeField] private string defaultIP = "127.0.0.1";
    [SerializeField] private ushort defaultPort = 7777;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hostButton.onClick.AddListener(HostButtonOnClick);
        clientButton.onClick.AddListener(ClientButtonOnClick);
    }

    private void HostButtonOnClick()
    {
        string ip = string.IsNullOrWhiteSpace(serverIPInputField.text) ? "0.0.0.0" : serverIPInputField.text.Trim();
        ushort port = GetPortFromInput(serverIPInputField);

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ip, port, "0.0.0.0");
        NetworkManager.Singleton.StartHost();

       Debug.Log($"Hosting on Server IP: {ip}, Port: {port}");
    }

    private void ClientButtonOnClick()
    {
        string ip = string.IsNullOrWhiteSpace(targetIPInputField.text) ? defaultIP : targetIPInputField.text.Trim();
        ushort port = GetPortFromInput(targetPortInputField);

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ip, port);
        NetworkManager.Singleton.StartClient();
        
        Debug.Log($"Connecting to Target IP: {ip}, Port: {port}");
    }

    private ushort GetPortFromInput(InputField portField)
    {
        if(portField != null && !string.IsNullOrWhiteSpace(portField.text))
        {
            ushort.TryParse(portField.text.Trim(), out ushort parsedPort);
            return parsedPort;
        }

        return defaultPort;
    }
}
