using UnityEngine;
/// ########
/// EXERCISE 2.1
/// ########
public class HeliumBalloon : MonoBehaviour
{  
    GameObject Balloon;
    Rigidbody rb;

    Vector3 movementVector;
    Vector4 screenBounds;

    public float balloonSpeed = 5f;
    public float windSpeed = 5f;
    public float maxSpeed = 5f;
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
        KeepOnScreen();

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
    void KeepOnScreen()
    {
        if(rb.position.x > screenBounds.y)
        {
            rb.position = new Vector3(screenBounds.y, rb.position.y, 0);
            rb.velocity = new Vector3(rb.velocity.x * -1f, rb.velocity.y, 0);
        }

        if(rb.position.x < screenBounds.x)
        {
            rb.position = new Vector3(screenBounds.x, rb.position.y, 0);
            rb.velocity = new Vector3(rb.velocity.x * -1f, rb.velocity.y, 0);
        }

        if(rb.position.y > screenBounds.w)
        {
            rb.position = new Vector3(rb.position.x, screenBounds.w, 0);
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y * -1f, 0);
        }
    }
}
