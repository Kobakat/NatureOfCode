using System.Collections.Generic;
using UnityEngine;

public enum RabbitState
{
    FoodSearching,
    MateSearching,
    FoodTargeting,
    MateTargeting,
    Eating,
    Mating,
    Fleeing,
    Dying
}

public class Rabbit : GroundCreature
{
    public RabbitState state = RabbitState.FoodSearching;

    Flower targetFlower;
    Rabbit mate;
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
        SetPriority();
    }

    void PerformStateAction()
    {
        int x = 0;
        int z = 0;

        switch(state)
        {
            case RabbitState.FoodSearching:
                HopToNearbySquareAfterDelay(2, x, z);
                break;
            case RabbitState.MateSearching:
                HopToNearbySquareAfterDelay(2, x, z);
                break;
            case RabbitState.MateTargeting:
                if(!mate)
                {
                    state = RabbitState.MateSearching;
                    if (Mathf.Abs(targetZ) == 1 && Mathf.Abs(targetX) == 1)
                        targetX = 0;
                    return;
                }
                x = mate.currentX;
                z = mate.currentZ;
                HopToNearbySquareAfterDelay(1, x, z);
                break;
            case RabbitState.FoodTargeting:
                if (!targetFlower)
                {
                    state = RabbitState.FoodSearching;
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
            case RabbitState.Mating:
                Mate();
                break;
            case RabbitState.Fleeing:
                if (!wolfToFleeFrom)
                {
                    state = RabbitState.FoodSearching;
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
            if(state == RabbitState.FoodTargeting)
            {
                state = RabbitState.Eating;
            }

            else if(state == RabbitState.MateTargeting)
            {
                state = RabbitState.Mating;
            }
            
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
    }

    float eatTimer;
    void EatFlower()
    {
        eatTimer += Time.deltaTime;

        if(eatTimer > 1)
        {
            hunger -= hungerLossOnEat;
            if (hunger < 0)
                hunger = 0;
            state = RabbitState.FoodSearching;
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

    void SetPriority()
    {
        if(state == RabbitState.FoodSearching || state == RabbitState.MateSearching)
        {
            if (reproduction > hunger)
                state = RabbitState.MateSearching;
            else
                state = RabbitState.FoodSearching;
        }
    }

    float mateTimer;
    void Mate()
    {
        mateTimer += Time.deltaTime;

        if(mateTimer > 1)
        {
            if(!gender)
            {
                CreateOffSpring(ecosystem.rabbit);
            }

            mateTimer = 0;
            reproduction = 0;
            state = RabbitState.FoodSearching;
            mate = null;
        }
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
                state = RabbitState.FoodTargeting;
            }
        }

        if(state == RabbitState.MateSearching)
        {
            if(other.gameObject.CompareTag("rabbit"))
            {
                //Generates garbage
                Rabbit r = other.gameObject.GetComponent<Rabbit>();

                if (r.state == RabbitState.MateSearching && gender != r.gender)
                {
                    mate = r;
                    state = RabbitState.MateTargeting;
                    r.mate = this;
                    r.state = RabbitState.MateTargeting;
                }
            }
        }
    }
}
