using Unity.Netcode;
using UnityEngine;

public class CarController : NetworkBehaviour
{
    [Header("Car Settings")]
    public float driftFactor;
    public float accelerationFactor = 30.0f;
    public float turnFactor = 5.0f;
    public float friction = 3.0f;
    public float maxSpeed = 20;

    //Variables
    private float accelerationInput = 0;
    private float steeringInput = 0;
    private float rotationAngle = 0;
    private float forwardVelocity = 0;
    


    //Components
    Rigidbody2D carRigidbody2D;

    public override void OnNetworkSpawn()
    {
        carRigidbody2D = GetComponent<Rigidbody2D>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!IsOwner)
        {
            return;
        }
        ApplyEngineForce();
        ApplySteering();
        KillOrthogonalVeloicity();
    }

    void ApplyEngineForce()
    {
        forwardVelocity = Vector2.Dot(transform.up, carRigidbody2D.linearVelocity);

        if(forwardVelocity > maxSpeed && accelerationInput > 0)
            return;
            
        if(forwardVelocity < -maxSpeed * 0.5f && accelerationInput < 0)
            return;

        if(carRigidbody2D.linearVelocity.sqrMagnitude > maxSpeed * maxSpeed && accelerationInput > 0)
            return;

        if (accelerationInput == 0)
            carRigidbody2D.linearDamping = Mathf.Lerp(carRigidbody2D.linearDamping, friction, Time.fixedDeltaTime * 3);
        else
            carRigidbody2D.linearDamping = 0f;  
        Vector2 engineForceVector = transform.up * accelerationInput * accelerationFactor;
        carRigidbody2D.AddForce(engineForceVector, ForceMode2D.Force);    
    }

    void ApplySteering()
    {
        float minSpeedToTurn = carRigidbody2D.linearVelocity.magnitude / 12;
        minSpeedToTurn = Mathf.Clamp01(minSpeedToTurn);
        float directionMultiplier = forwardVelocity >= 0 ? 1.0f : -1.0f;

        rotationAngle -= steeringInput * turnFactor * minSpeedToTurn * directionMultiplier;

        carRigidbody2D.MoveRotation(rotationAngle);
    }

    void KillOrthogonalVeloicity()
    {
        Vector2 verticalVelocity = transform.up * Vector2.Dot(carRigidbody2D.linearVelocity, transform.up);
        Vector2 horizontalVelocity = transform.right * Vector2.Dot(carRigidbody2D.linearVelocity, transform.right);

        carRigidbody2D.linearVelocity = verticalVelocity + horizontalVelocity * driftFactor;
    }

    public void SetInputVector(Vector2 inputVector)
    {
        steeringInput = inputVector.x;
        accelerationInput = inputVector.y;
    }


}
