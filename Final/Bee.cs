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
 
    List<Flower> visitedFlowers;
    Flower targetFlower;
    [SerializeField] Transform mesh;

    BeeState state = BeeState.Searching;

    RaycastHit hit;

    float targetHeight;
    float heightOffset;
    float dampVelocity;

    protected sealed override void Start()
    {
        base.Start();
        SpawnInRandomLocation(3f, false);
        visitedFlowers = new List<Flower>();
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
                CreateNewFlowerWhenPossible();
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
        if(!targetFlower)
        {
            state = BeeState.Searching;
            targetTimer = 0;
        }

        else
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
                targetTimer = 0;
                targetFlower = null;
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
        bool rayHit = Physics.Raycast(ray, out hit, 10f, ~0);

        if (rayHit)
        {
            float newTarget = hit.transform.position.y + 2f;
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

    float flowerCreationTimer;
    void CreateNewFlowerWhenPossible()
    {
        if(visitedFlowers.Count >= 2)
        {
            flowerCreationTimer += Time.deltaTime;

            if(flowerCreationTimer > 5f)
            {
                RaycastHit flowerCheck;
                Ray ray = new Ray(body.position + transform.forward + (Vector3.up * 2f), Vector3.down);
                bool rayHit = Physics.Raycast(ray, out flowerCheck, 10f, ~0);

                if(rayHit)
                {
                    int x = (int)(hit.transform.position.x + .01f);
                    int z = (int)(hit.transform.position.z + .01f);

                    bool flowerExists = false;
                    foreach(Flower f in ecosystem.resources)
                    {
                        if (f.xCoord == x && f.zCoord == z)
                            flowerExists = true;
                    }

                    if(!flowerExists)
                    {
                        Vector3 spawnPos = new Vector3(x, 50, z); // hack hard coded y value

                        Flower childComponent = Instantiate(ecosystem.flower, spawnPos, transform.rotation, this.transform.parent).GetComponent<Flower>();
                        childComponent.ecosystem = this.ecosystem;
                        childComponent.SnapToTerrain(x, z);                     
                        this.ecosystem.resources.Add(childComponent);
                        flowerCreationTimer = 0;
                        visitedFlowers.Clear();
                    }                    
                }
            }

        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!targetFlower)
        {
            if(other.gameObject.CompareTag("flower"))
            {
                if(!visitedFlowers.Contains(other.gameObject.GetComponent<Flower>()))
                {
                    targetFlower = other.gameObject.GetComponent<Flower>();
                    state = BeeState.Targeting;
                }
            }
        }
    }
}
