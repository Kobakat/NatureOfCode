using System.Collections.Generic;
using UnityEngine;

public enum RabbitState
{
    Searching,
    Targeting,
    Eating,
    Fleeing,
    Dying
}

public class Rabbit : GroundCreature
{
    public RabbitState state = RabbitState.Searching;

    Flower targetFlower;
    Wolf wolfToFleeFrom;
    
    protected sealed override void Start()
    {
        base.Start();
    }

    void Update()
    {
        UpdateSpeed();
        PerformStateAction();
        UpdateLookDirection();
        IncreaseReproductionValue();

        float hungerRate = state == RabbitState.Fleeing ? 2 : 1;
        IncreaseHunger(hungerRate);     
    }

    void PerformStateAction()
    {
        int x = 0;
        int z = 0;

        switch(state)
        {
            case RabbitState.Searching:
                HopToNearbySquareAfterDelay(2, x, z);
                break;
            case RabbitState.Targeting:
                if (!targetFlower)
                {
                    state = RabbitState.Searching;
                    if (Mathf.Abs(targetZ) == 1 && Mathf.Abs(targetX) == 1)
                        targetX = 0;
                    return;
                }
                x = targetFlower.xCoord;
                z = targetFlower.zCoord;
                HopToNearbySquareAfterDelay(1, x, z);
                break;
            case RabbitState.Dying:
                Die();
                break;
            case RabbitState.Eating:
                EatFlower();
                break;
            case RabbitState.Fleeing:
                if (!wolfToFleeFrom)
                {
                    state = RabbitState.Searching;
                    if (Mathf.Abs(targetZ) == 1 && Mathf.Abs(targetX) == 1)
                        targetX = 0;
                    return;
                }
                x = wolfToFleeFrom.currentX;
                z = wolfToFleeFrom.currentZ;
                HopToNearbySquareAfterDelay(3, x, z);
                ClearWolfWhenSafe();
                break;
        }
    }

    protected override sealed void Target(int xCoord, int zCoord)
    {
        base.Target(xCoord, zCoord);

        if (currentX == xCoord && currentZ == zCoord)
        {
            state = RabbitState.Eating;
            if (Mathf.Abs(targetZ) == 1 && Mathf.Abs(targetX) == 1)
                targetX = 0;
        }

        else
        {
            previousHopPoint = transform.position;
            targetHopPoint = ecosystem.terrainManager.terrain[currentX, currentZ] + Vector3.up * 0.5f;
            currentlyHopping = true;
        }
    }

    protected override sealed void IncreaseHunger(float rate)
    {
        base.IncreaseHunger(rate);

        if(hunger >= 100)
        {
            state = RabbitState.Dying;
        }
    }

    protected override sealed void IncreaseReproductionValue()
    {
        base.IncreaseReproductionValue();

        if (reproduction >= 100)
        {
            CreateOffSpring(ecosystem.rabbit);
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

    protected override sealed void Flee(int xCoord, int zCoord)
    {
        base.Flee(xCoord, zCoord);

        if (currentX == xCoord && currentZ == zCoord)
        {
            state = RabbitState.Dying;
            if (Mathf.Abs(targetZ) == 1 && Mathf.Abs(targetX) == 1)
                targetX = 0;
        }

        else
        {
            previousHopPoint = transform.position;
            targetHopPoint = ecosystem.terrainManager.terrain[currentX, currentZ] + Vector3.up * 0.5f;
            currentlyHopping = true;
        }
    }

    void ClearWolfWhenSafe()
    {
        if (Vector3.Distance(transform.position, wolfToFleeFrom.transform.position) > 9f)
            wolfToFleeFrom = null;
    }
    void UpdateSpeed()
    {
        speed = state == RabbitState.Fleeing ? baseSpeed * chaseSpeed : baseSpeed;
    }

    protected override sealed void UpdateLookDirection()
    {
        if (state != RabbitState.Dying)
        {
            base.UpdateLookDirection();
        }
    }

    void OnTriggerStay(Collider other)
    {       
        if(!wolfToFleeFrom && state != RabbitState.Dying)
        {
            if(other.gameObject.CompareTag("wolf") && !other.isTrigger)
            {
                state = RabbitState.Fleeing;
                wolfToFleeFrom = other.gameObject.GetComponent<Wolf>();
            }
        }

        if(!targetFlower && state != RabbitState.Fleeing)
        {
            if (other.gameObject.CompareTag("flower"))
            {
                targetFlower = other.gameObject.GetComponent<Flower>();
                state = RabbitState.Targeting;
            }
        }
    }
}
