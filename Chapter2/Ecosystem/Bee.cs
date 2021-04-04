using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BeeState
{
    Searching,
    Targeting
}

public class Bee : Creature
{
    public float searchSpeed = 5;
    public float targetSpeed = 10;
    public float maxVision = 10;
    public float maxTime = 15;
    public float maxPush = 10;


    List<Flower> flowers;
    List<Flower> visitedFlowers;
    Flower targetFlower;
    BeeState state = BeeState.Searching;

    Vector3 respawnPos;

    protected sealed override void Awake()
    {
        base.Awake();
        SpawnInRandomLocation();
        FindAllFlowers();
    }

    private void FixedUpdate()
    {
        ExecuteBeeActionBasedOnState();
        KeepInBounds();
        LiftAwayFromFloor();
        ClampVelocity();
        SetRotation();

        ResetWhenFallingBelowTerrain();
    }

    void SpawnInRandomLocation()
    {
        Ecosystem ecosystem = FindObjectOfType<Ecosystem>();

        int posX = Random.Range(0, (int)ecosystem.terrainManager.terrainSize.x);
        int posY = Random.Range(0, (int)ecosystem.terrainManager.terrainSize.y);
        float rotY = Random.Range(-180f, 180f);

        transform.position = new Vector3(posX, ecosystem.terrainManager.maxHeight + 1, posY);

        RaycastHit newHit;
        Ray newRay = new Ray(transform.position, Vector3.down);
        Physics.Raycast(newRay, out newHit, 50f);

        respawnPos = newHit.transform.position + (newHit.normal * 3f);
        transform.position = respawnPos;
        transform.rotation = Quaternion.Euler(new Vector3(0, rotY, 0));
    }

    void FindAllFlowers()
    {
        flowers = new List<Flower>();
        visitedFlowers = new List<Flower>();

        foreach (Flower f in FindObjectOfType<Ecosystem>().resources)
        {
            flowers.Add(f);
        }
    }

    void SetRotation()
    {
        Vector3 velocityNoY = body.velocity;
        velocityNoY.y = 0;
        velocityNoY = velocityNoY.normalized;

        transform.rotation = Quaternion.LookRotation(velocityNoY);
    }

    void ExecuteBeeActionBasedOnState()
    {
        switch (state)
        {
            case BeeState.Searching:
                SearchForFlower();
                FindNearestFlower();         
                break;
            case BeeState.Targeting:
                MoveTowardsTargetFlower();
                break;
        }
    }

    void SearchForFlower()
    {
        body.AddForce(searchSpeed * transform.forward);
    }

    float targetTimer;
    void MoveTowardsTargetFlower()
    {
        //Add force towards the flower
        Vector3 directionalVector = targetFlower.transform.position - this.transform.position;
        directionalVector.y = 0;
        directionalVector.Normalize();
        body.AddForce(directionalVector * targetSpeed);

        //Count up on the timer until maxTime is reached
        //This time isn't accurate since its being called in the FixedUpdate cycle
        targetTimer += Time.deltaTime;

        Debug.DrawLine(transform.position, targetFlower.transform.position, Color.red);

        if (targetTimer >= maxTime)
        {
            visitedFlowers.Add(targetFlower);
            state = BeeState.Searching;
            targetFlower = null;
        }     
    }

    void FindNearestFlower()
    {
        foreach (Flower f in flowers)
        {
            if (f != targetFlower && !visitedFlowers.Contains(f))
            {
                float distance = Vector3.Distance(this.transform.position, f.transform.position);
                float currentTargetDistance =
                    targetFlower ?
                    Vector3.Distance(this.transform.position, targetFlower.transform.position)
                    : 10000;

                if (distance < currentTargetDistance && distance < maxVision)
                {
                    state = BeeState.Targeting;
                    targetFlower = f;             
                    targetTimer = 0;
                }
            }
        }
    }

    void KeepInBounds()
    {
        float topWallForce = Mathf.Min(maxPush, maxPush / (body.position.z - maxBound.y));
        float bottomWallForce = Mathf.Min(maxPush, maxPush / (body.position.z - minBound.y));
        float leftWallForce = Mathf.Min(maxPush, maxPush / (body.position.x - maxBound.x));
        float rightWallForce = Mathf.Min(maxPush, maxPush / (body.position.x - minBound.x));

        Vector3 zForce = Vector3.forward * (topWallForce + bottomWallForce);
        Vector3 xForce = Vector3.right * (leftWallForce + rightWallForce);

        Vector3 pushForce = xForce + zForce;

        body.AddForce(pushForce);
    }

    RaycastHit hit;
    Ray ray;
    void LiftAwayFromFloor()
    {
        ray = new Ray(transform.position, Vector3.down);
        bool rayHit = Physics.Raycast(ray, out hit, 2f);

        if(rayHit)
        {
            float upwardsForce = Mathf.Min(maxPush, maxPush / hit.distance);
            body.AddForce(Vector3.up * upwardsForce);
        }
    }

    void ClampVelocity()
    {
        Vector3 xzVelocity = new Vector3(body.velocity.x, 0, body.velocity.z);
        Vector3 yVelocity = Vector3.up * body.velocity.y;

        xzVelocity = Vector3.ClampMagnitude(xzVelocity, maxXZSpeed);
        yVelocity = Vector3.ClampMagnitude(yVelocity, maxYSpeed);

        body.velocity = xzVelocity + yVelocity;
    }

    void ResetWhenFallingBelowTerrain()
    {
        if(body.position.y < -10)
        {
            transform.position = respawnPos;
        }
    }
}
