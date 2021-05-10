using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public enum WolfState
{
    RabbitSearching,
    MateSearching,
    Hunting,
    MateTargeting,
    Eating,
    Mating,
    Dying
}

public class Wolf : GroundCreature
{
    public WolfState state = WolfState.RabbitSearching;

    Rabbit targetRabbit;
    Wolf mate;
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

        float hungerRate = state == WolfState.Hunting ? 2 : 1;
        IncreaseHunger(hungerRate);
        SetPriority();
    }

    void PerformStateAction()
    {
        int x = targetRabbit ? targetRabbit.currentX : 0;
        int z = targetRabbit ? targetRabbit.currentZ : 0;

        switch (state)
        {
            case WolfState.RabbitSearching:
                HopToNearbySquareAfterDelay(2, x, z);
                break;
            case WolfState.Hunting:
                if (!targetRabbit)
                {
                    state = WolfState.RabbitSearching;
                    if (Mathf.Abs(targetZ) == 1 && Mathf.Abs(targetX) == 1)
                        targetX = 0;
                    return;
                }
                HopToNearbySquareAfterDelay(1, x, z);
                ClearRabbitWhenSafe();
                break;
            case WolfState.MateSearching:
                HopToNearbySquareAfterDelay(2, x, z);
                break;
            case WolfState.MateTargeting:
                if (!mate)
                {
                    state = WolfState.MateSearching;
                    if (Mathf.Abs(targetZ) == 1 && Mathf.Abs(targetX) == 1)
                        targetX = 0;
                    return;
                }
                x = mate.currentX;
                z = mate.currentZ;
                HopToNearbySquareAfterDelay(1, x, z);
                break;
            case WolfState.Dying:
                Die();
                break;
            case WolfState.Eating:
                EatRabbit();
                break;
            case WolfState.Mating:
                Mate();
                break;
        }
    }
    void SetPriority()
    {
        if (state == WolfState.RabbitSearching || state == WolfState.MateSearching)
        {
            if (reproduction > hunger)
                state = WolfState.MateSearching;
            else
                state = WolfState.RabbitSearching;
        }
    }


    protected override sealed void Target(int xCoord, int zCoord)
    {
        base.Target(xCoord, zCoord);

        if (currentX == xCoord && currentZ == zCoord)
        {
            if (state == WolfState.Hunting)
            {
                state = WolfState.Eating;
            }

            else if (state == WolfState.MateTargeting)
            {
                state = WolfState.Mating;
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

        if (hunger >= 100)
        {
            state = WolfState.Dying;
        }
    }

    protected override sealed void IncreaseReproductionValue()
    {
        base.IncreaseReproductionValue();
    }

    float eatTimer;
    void EatRabbit()
    {
        eatTimer += Time.deltaTime;

        if (eatTimer > 1)
        {
            hunger -= hungerLossOnEat;
            state = WolfState.RabbitSearching;
            eatTimer = 0;
            if (!ecosystem.creaturesToRemove.Contains(targetRabbit) && targetRabbit)
                ecosystem.creaturesToRemove.Add(targetRabbit);
        }
    }
    float mateTimer;
    void Mate()
    {
        mateTimer += Time.deltaTime;

        if (mateTimer > 1)
        {
            if (!gender)
            {
                CreateOffSpring(ecosystem.wolf);
            }

            mateTimer = 0;
            reproduction = 0;
            state = WolfState.RabbitSearching;
            mate = null;
        }
    }

    void ClearRabbitWhenSafe()
    {
        if (Vector3.Distance(transform.position, targetRabbit.transform.position) > 9f)
            targetRabbit = null;
    }
    void UpdateSpeed()
    {
        speed = state == WolfState.Hunting ? chaseSpeed * 0.5f : baseSpeed;
    }

    protected override sealed void UpdateLookDirection()
    {
        if(state != WolfState.Dying)
        {
            base.UpdateLookDirection();
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!targetRabbit)
        {
            if (other.gameObject.CompareTag("rabbit") && !other.isTrigger && state == WolfState.RabbitSearching)
            {
                targetRabbit = other.gameObject.GetComponent<Rabbit>();
                state = WolfState.Hunting;
            }
        }

        if (state == WolfState.MateSearching)
        {
            if (other.gameObject.CompareTag("wolf"))
            {
                //Generates garbage
                Wolf w = other.gameObject.GetComponent<Wolf>();

                if (w.state == WolfState.MateSearching && gender != w.gender)
                {
                    mate = w;
                    state = WolfState.MateTargeting;
                    w.mate = this;
                    w.state = WolfState.MateTargeting;
                }
            }
        }
    }
}
