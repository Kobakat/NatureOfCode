using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle6_2 : MonoBehaviour
{
    public float r = 3.0f;
    public float maxforce = 1.0f;
    public float maxspeed = 4.0f;
    public float mass = 1.0f;

    public float predictionStrength = 5.0f;

    Vector4 screenBounds;

    public Target6_2 target { get; set; }
    Rigidbody body;

    Vector3 moveDir;
    // Start is called before the first frame update
    void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.mass = mass;
        body.drag = 0;
        body.useGravity = false;

        screenBounds = Utility.GetOrthoGraphicScreenBounds();
    }

    void FixedUpdate()
    {
        KeepAwayFromEdges();
        moveDir = Seek(target.body.position);
        applyForce(moveDir);

        body.velocity = new Vector3(
            Mathf.Clamp(body.velocity.x, -maxspeed, maxspeed),
            Mathf.Clamp(body.velocity.y, -maxspeed, maxspeed),
            Mathf.Clamp(body.velocity.z, -maxspeed, maxspeed));
    }

    public Vector3 Seek(Vector3 targetPos)
    {
        Vector3 targetVelocityNormal = target.body.velocity.normalized;
        Vector3 projectedTargetPosition = targetPos + (targetVelocityNormal * predictionStrength);

        Vector3 desired = projectedTargetPosition - body.transform.position;

        Debug.DrawLine(body.position, projectedTargetPosition, Color.red);
        

        desired.Normalize();
        desired *= maxspeed;
        Vector3 steer = desired - body.velocity;
        steer.x = Mathf.Clamp(steer.x, -maxforce, maxforce);
        steer.y = Mathf.Clamp(steer.y, -maxforce, maxforce);
        steer.z = Mathf.Clamp(steer.z, -maxforce, maxforce);

        return steer;
    }

  
    public void applyForce(Vector3 force)
    {
        body.AddForce(force * Time.fixedDeltaTime, ForceMode.VelocityChange);
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

    private void OnDrawGizmos()
    {
        Vector3 targetVelocityNormal = target.body.velocity.normalized;
        Vector3 projectedTargetPosition = target.body.position + (targetVelocityNormal * predictionStrength);

        Gizmos.DrawWireSphere(projectedTargetPosition, 0.3f);
    }

}
