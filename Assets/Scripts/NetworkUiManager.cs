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
    [SerializeField] private InputField clientUsernameInputField;

    [Header("Server/Host Settings")]
    [SerializeField] private InputField serverIPInputField;
    [SerializeField] private InputField serverPortInputField;
    [SerializeField] private InputField hostUsernameInputField;

    [Header("Defaults")]
    [SerializeField] private string defaultIP = "127.0.0.1";
    [SerializeField] private ushort defaultPort = 7777;


    public static string EnteredUsername;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hostButton.onClick.AddListener(HostButtonOnClick);
        clientButton.onClick.AddListener(ClientButtonOnClick);
    }

    private void HostButtonOnClick()
    {
        string ip = string.IsNullOrWhiteSpace(serverIPInputField.text) ? "0.0.0.0" : serverIPInputField.text.Trim();
        ushort port = GetPortFromInput(serverPortInputField);

        EnteredUsername = SaveUsername(hostUsernameInputField);
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

        EnteredUsername = SaveUsername(clientUsernameInputField);
        
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

    private string SaveUsername(InputField nameField)
    {
        if (nameField != null && !string.IsNullOrWhiteSpace(nameField.text))
        {
            return EnteredUsername = nameField.text.Trim();
        }
        else
        {
            return EnteredUsername = $"Player {Random.Range(10, 99)}";
        }
    }
}
