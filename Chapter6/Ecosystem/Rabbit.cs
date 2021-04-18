using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public enum RabbitState
{
    Searching,
    Targeting,
    Eating,
    Dying
}

public class Rabbit : Creature
{
    public RabbitState state = RabbitState.Searching;

    public float hungerIncreaseRate = 2f;
    public int hungerLossOnEat = 20;
    public float reproductionIncreaseRate = 1f;
    public int minOffspring = 1;
    public int maxOffspring =3;

    public float turnSpeed = 360f;
    public float hopDelay = 1f;
    public float hopSpeed = 0.4f;
    public float hopHeight = 1.25f;

    float hopTimer;
    bool currentlyHopping;
    Vector3 previousHopPoint;
    Vector3 targetHopPoint;

    int hunger;
    float hungerTimer;
    Flower targetFlower;

    int reproduction;
    float reproductionTimer;

    int targetX;
    int targetZ;
    int currentX;
    int currentZ;

    List<int> possibleTiles;
    protected sealed override void Start()
    {
        base.Start();
 
        //hacky and lazy way to make sure new born rabbits dont spawn randomly but instead on their parent
        if(Time.time < 3f)
        {
            SpawnInRandomLocation(0.5f, false);
        }

        targetX = (int)(transform.forward.x + 0.01f);
        targetZ = (int)(transform.forward.z + 0.01f);
        currentX = (int)(transform.position.x + 0.01f);
        currentZ = (int)(transform.position.z + 0.01f);

        possibleTiles = new List<int>();
    }

    void Update()
    {
        PerformStateAction();
        UpdateLookDirection();
        IncreaseReproductionValue();
        IncreaseHunger();      
    }

    void PerformStateAction()
    {
        switch(state)
        {
            case RabbitState.Searching:
                HopToNearbySquareAfterDelay(false);
                break;
            case RabbitState.Targeting:
                HopToNearbySquareAfterDelay(true);
                break;
            case RabbitState.Dying:
                Die();
                break;
            case RabbitState.Eating:
                EatFlower();
                break;
        }
    }

    void HopToNearbySquareAfterDelay(bool targeting)
    {
        if(currentlyHopping)
        {
            Hop();
        }

        else
        {
            hopTimer += Time.deltaTime;

            if(hopTimer >= hopDelay)
            {
                hopTimer = 0;

                if (targeting)
                    Target();
                else
                    Search();
            }
        }
    }

    void Search()
    {
        //If the rabbit is looking forward on the X axis, sample 3 points near it.
        if(Mathf.Abs(targetX) > 0.1)
        {
            int possibleForward = currentX + targetX * 1;
            int possibleLeft = currentZ - 1;
            int possibleRight = currentZ + 1;

            if (possibleForward < maxBound.x && possibleForward > minBound.x)
                possibleTiles.Add(possibleForward);

            if(possibleLeft < maxBound.y && possibleLeft > minBound.y)
                possibleTiles.Add(possibleLeft);

            if (possibleRight < maxBound.y && possibleRight > minBound.y)
                possibleTiles.Add(possibleRight);

            int selection = Random.Range(0, possibleTiles.Count);

            if(possibleTiles[selection] == possibleForward)
                possibleTiles.AddRange(Enumerable.Repeat(possibleForward, 8));

            else if(possibleTiles[selection] == possibleRight)
            {
                currentZ = possibleRight;
                targetX = 0;
                targetZ = 1;
            }

            else if(possibleTiles[selection] == possibleLeft)
            {
                currentZ = possibleLeft;
                targetX = 0;
                targetZ = -1;
            }
        }

        //If the rabbit is looking along the Z axis, sample points along it
        else
        {
            int possibleForward = currentZ + targetZ * 1;
            int possibleLeft = currentX - 1;
            int possibleRight = currentX + 1;

            if (possibleForward < maxBound.y && possibleForward > minBound.y)
                possibleTiles.AddRange(Enumerable.Repeat(possibleForward, 8));
               
            if (possibleLeft < maxBound.x && possibleLeft > minBound.x)
                possibleTiles.Add(possibleLeft);

            if (possibleRight < maxBound.x && possibleRight > minBound.x)
                possibleTiles.Add(possibleRight);

            int selection = Random.Range(0, possibleTiles.Count);

            if (possibleTiles[selection] == possibleForward)
            {
                currentZ = possibleForward;
            }

            else if (possibleTiles[selection] == possibleRight)
            {
                currentX = possibleRight;
                targetZ = 0;
                targetX = 1;
            }

            else if (possibleTiles[selection] == possibleLeft)
            {
                currentX = possibleLeft;
                targetZ = 0;
                targetX = -1;
            }
        }

        previousHopPoint = transform.position;
        targetHopPoint = ecosystem.terrainManager.terrain[currentX, currentZ] + Vector3.up * 0.5f;       
        currentlyHopping = true;

        possibleTiles.Clear();
    }

    void Target()
    {
        if(!targetFlower)
        {
            state = RabbitState.Searching;
            if (Mathf.Abs(targetZ) == 1 && Mathf.Abs(targetX) == 1)
                targetX = 0;
            return;
        }

        int x = targetFlower.xCoord;
        int z = targetFlower.zCoord;

        int xChange = 0;
        int zChange = 0;

        if(currentX != x)
            xChange = currentX > x ? -1 : 1;
            
        if (currentZ != z)
            zChange = currentZ > z ? -1 : 1;

        currentX += xChange;
        targetX = xChange;
        currentZ += zChange;
        targetZ = zChange;

        if (currentX == x && currentZ == z)
        {
            state = RabbitState.Eating;
            if(Mathf.Abs(targetZ) == 1 && Mathf.Abs(targetX) == 1)
                targetX = 0;
        }

        else
        {
            previousHopPoint = transform.position;
            targetHopPoint = ecosystem.terrainManager.terrain[currentX, currentZ] + Vector3.up * 0.5f;
            currentlyHopping = true;
        }
    }

    void Hop()
    {
        hopTimer += Time.deltaTime;
        float frac = hopTimer / hopSpeed;
        Vector3 newPoint = Vector3.Lerp(previousHopPoint, targetHopPoint, frac);

        float peakHeight = Mathf.Max(targetHopPoint.y + hopHeight, previousHopPoint.y + hopHeight);
        float newY;

        if(frac <= 0.5f)
        {
            float smoothFrac = (2f * frac);
            newY = Mathf.Lerp(previousHopPoint.y, peakHeight, smoothFrac);
        }
        else
        {
            float smoothFrac = (frac - 0.5f) * 2f;
            newY = Mathf.Lerp(peakHeight, targetHopPoint.y, smoothFrac);
        }

        newPoint.y = newY;
        transform.position = newPoint;

        if(frac >= 1)
        {
            transform.position = targetHopPoint;
            hopTimer = 0;
            currentlyHopping = false;
        }
    }

    void IncreaseHunger()
    {
        hungerTimer += Time.deltaTime;

        if(hungerTimer > hungerIncreaseRate)
        {
            hungerTimer = 0;
            hunger++;
        }

        if(hunger >= 100)
        {
            state = RabbitState.Dying;
        }
    }

    void IncreaseReproductionValue()
    {
        reproductionTimer += Time.deltaTime;

        if (reproductionTimer > reproductionIncreaseRate)
        {
            reproductionTimer = 0;
            reproduction++;
        }

        if (reproduction >= 100)
        {
            CreateOffSpring();
        }
    }

    float eatTimer;
    void EatFlower()
    {
        eatTimer += Time.deltaTime;

        if(eatTimer > 1)
        {
            hunger -= hungerLossOnEat;
            state = RabbitState.Searching;
            eatTimer = 0;
            if(!ecosystem.resourcesToRemove.Contains(targetFlower) && targetFlower)
                ecosystem.resourcesToRemove.Add(targetFlower);
        }

    }

    float deathTimer;
    void Die()
    {
        deathTimer += Time.deltaTime;

        Quaternion targetRot = Quaternion.Euler(0, 0, 90);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * Time.deltaTime);

        if (deathTimer > 2f)
        {
            ecosystem.creatures.Remove(this);
            Destroy(this.gameObject);
        }
    }

    void UpdateLookDirection()
    {
        if(state != RabbitState.Dying)
        {
            Quaternion targetRot = Quaternion.LookRotation(new Vector3(targetX, 0, targetZ));
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
        }      
    }

    void CreateOffSpring()
    {
        int offspringCount = Random.Range(minOffspring, maxOffspring + 1);
        reproduction = 0;

        for(int i = 0; i < offspringCount; i++)
        {
            Creature childComponent = Instantiate(ecosystem.rabbit, transform.position, transform.rotation, this.transform.parent).GetComponent<Creature>();
            childComponent.ecosystem = this.ecosystem;
            this.ecosystem.creatures.Add(childComponent);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if(!targetFlower)
        {
            if (other.gameObject.CompareTag("flower"))
            {
                targetFlower = other.gameObject.GetComponent<Flower>();
                state = RabbitState.Targeting;
            }
        }
    }
}
