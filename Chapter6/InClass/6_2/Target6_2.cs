using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target6_2 : MonoBehaviour
{
    public Rigidbody body { get; set; }
    public float pushSpeed = 5.0f;
    Vector4 screenBounds;
    void Awake()
    {
        body = GetComponent<Rigidbody>();
        screenBounds = Utility.GetOrthoGraphicScreenBounds();

        Vector3 initialForce = new Vector3(
            Random.Range(0.5f, 1f),
            Random.Range(0.5f, 1f),
            0);

        initialForce = initialForce.normalized;

        body.AddForce(initialForce * pushSpeed, ForceMode.VelocityChange);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        KeepAwayFromEdges();
    }

    void KeepAwayFromEdges()
    {
        if (body.position.x > screenBounds.y)
        {
            body.position = new Vector3(screenBounds.y, body.position.y, 0);
            body.velocity = new Vector3(body.velocity.x * -1f, body.velocity.y, 0);
        }

        if (body.position.x < screenBounds.x)
        {
            body.position = new Vector3(screenBounds.x, body.position.y, 0);
            body.velocity = new Vector3(body.velocity.x * -1f, body.velocity.y, 0);
        }

        if (body.position.y > screenBounds.w)
        {
            body.position = new Vector3(body.position.x, screenBounds.w, 0);
            body.velocity = new Vector3(body.velocity.x, body.velocity.y * -1f, 0);
        }

        if (body.position.y < screenBounds.z)
        {
            body.position = new Vector3(body.position.x, screenBounds.z, 0);
            body.velocity = new Vector3(body.velocity.x, body.velocity.y * -1f, 0);
        }
    }
}
