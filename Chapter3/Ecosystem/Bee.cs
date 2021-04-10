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
    public float heightShiftSpeed = 0.1f;
    public float oscillationAmplitude = 0.1f;
    public float oscillationFrequency = 20f;
    public float maxVision = 10;
    public float maxTime = 15;
    public float maxPush = 10;
 
    List<Flower> flowers;
    List<Flower> visitedFlowers;
    Flower targetFlower;
    [SerializeField] Transform mesh;

    BeeState state = BeeState.Searching;

    Vector3 respawnPos; 
    RaycastHit hit;

    float targetHeight;
    float heightOffset;
    float dampVelocity;

    protected sealed override void Start()
    {
        base.Start();
        SpawnInRandomLocation();
        FindAllFlowers();
        hit = new RaycastHit();
    }

    void Update()
    {
        Oscillate();
    }

    protected sealed override void FixedUpdate()
    {
        base.FixedUpdate();

        ExecuteBeeActionBasedOnState();
        KeepInBounds();
        FlyAboveFloor();
        ApplyForceAndClampVelocity();
        SetRotation();

        ResetWhenFallingBelowTerrain();
    }

    void SpawnInRandomLocation()
    {
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
        moveForce += (searchSpeed * transform.forward);
    }

    float targetTimer;
    void MoveTowardsTargetFlower()
    {
        //Add force towards the flower
        Vector3 directionalVector = targetFlower.transform.position - this.transform.position;
        directionalVector.y = 0;
        directionalVector.Normalize();
        moveForce += (directionalVector * targetSpeed);

        //Count up on the timer until maxTime is reached
        //This time isn't accurate since its being called in the FixedUpdate cycle
        targetTimer += Time.deltaTime;

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

        moveForce += xForce + zForce;   
    }

    
    void FlyAboveFloor()
    {
        Ray ray = new Ray(body.position + transform.forward + (Vector3.up * 2f), Vector3.down);
        bool rayHit = Physics.Raycast(ray, out hit, 10f);

        if (rayHit)
        {
            float newTarget = hit.transform.position.y + 4f;
            if(targetHeight != newTarget)
                targetHeight = newTarget;
        }

        FlyToTargetHeight();
    }

    void ApplyForceAndClampVelocity()
    {
        body.AddForce(moveForce);

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

    void FlyToTargetHeight()
    {
        float newHeight = Mathf.SmoothDamp(body.position.y, targetHeight, ref dampVelocity, heightShiftSpeed);
        body.position = new Vector3(body.position.x, newHeight, body.position.z);
    }

    void Oscillate()
    {
        heightOffset += Time.deltaTime * oscillationFrequency;
        float heightChange = Mathf.Sin(heightOffset) * oscillationAmplitude;

        mesh.localPosition = Vector3.up * heightChange;
    }
}
