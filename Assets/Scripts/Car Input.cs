using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class CarInput : NetworkBehaviour
{
    public InputActionAsset inputActionAsset;
    //Components
    CarController carController;

    public override void OnNetworkSpawn()
    {
        carController = GetComponent<CarController>();  
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner)
            return;

        InputAction moveAction = inputActionAsset.FindAction("Move");
        Vector2 inputVector = moveAction.ReadValue<Vector2>();

        carController.SetInputVector(inputVector);
    }
}
