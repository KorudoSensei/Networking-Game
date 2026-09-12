using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine.UI;

public class PlayerNetworkData : NetworkBehaviour
{

    public NetworkVariable<FixedString64Bytes> username = new NetworkVariable<FixedString64Bytes>
    ("Unassigned", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] private Text nameDisplayText;
    

    public override void OnNetworkSpawn()
    {
        username.OnValueChanged += OnNameChanged;
        UpdateNameDisplay(username.Value.ToString());

        if(IsOwner)
        {
            string typedName = NetworkUiManager.EnteredUsername;
            SetUsernameServerRpc(typedName);
        }
    }

    public override void OnNetworkDespawn()
    {
        username.OnValueChanged -= OnNameChanged;
    }
    private void UpdateNameDisplay(string name)
    {
        if (nameDisplayText != null)
        {
            nameDisplayText.text = name;
        }
    }

    private void OnNameChanged(FixedString64Bytes oldval, FixedString64Bytes newval)
    {
        UpdateNameDisplay(newval.ToString());
    }

    [ServerRpc]
    private void SetUsernameServerRpc(string name)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            username.Value = name.Trim();
        }
    }
}
