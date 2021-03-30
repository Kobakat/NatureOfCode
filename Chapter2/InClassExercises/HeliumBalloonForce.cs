using UnityEngine;
/// ##########
/// EXERCISE 2.3
/// ##########
public class HeliumBalloonForce : MonoBehaviour
{
    GameObject Balloon;
    Rigidbody rb;

    Vector3 movementVector;
    Vector4 screenBounds;

    public float balloonSpeed = 5f;
    public float windSpeed = 5f;
    public float pushSpeed = 5f;

    public float maxSpeed = 5f;
    public float maxPush = 30f;
    
    [Range(-1, 1)]
    public float windDirection = 0;

    void Awake()
    {
        screenBounds = Utility.GetOrthoGraphicScreenBounds();

        Balloon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Balloon.AddComponent<Rigidbody>();

        rb = Balloon.GetComponent<Rigidbody>();
        rb.useGravity = false;
        movementVector = Vector3.zero;
    }

    void FixedUpdate()
    {
        AddWind();
        AddRisingForce();
        ClampForceAndSpeed();
        KeepAwayFromEdges();

        rb.AddForce(movementVector, ForceMode.Acceleration);
    }

    void AddWind()
    {
        Vector3 wind = new Vector3(
            windDirection,
            0,
            0);

        movementVector += (wind * windSpeed);
    }

    void AddRisingForce()
    {
        movementVector += (Vector3.up * balloonSpeed);
    }

    void ClampForceAndSpeed()
    {
        movementVector = Vector3.ClampMagnitude(movementVector, maxSpeed);
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
    }
    void KeepAwayFromEdges()
    {
        float topWallForce = Mathf.Min(maxPush, maxPush / (rb.position.y - screenBounds.w));
        float bottomWallForce = Mathf.Min(maxPush, maxPush / (rb.position.y - screenBounds.z));
        float leftWallForce = Mathf.Min(maxPush, maxPush / (rb.position.x - screenBounds.x));
        float rightWallForce = Mathf.Min(maxPush, maxPush / (rb.position.x - screenBounds.y));

        Vector3 yForce = Vector3.up * (topWallForce + bottomWallForce);
        Vector3 xForce = Vector3.right * (leftWallForce + rightWallForce);

        Vector3 pushForce = xForce + yForce;

        movementVector += pushForce;
    }
}
