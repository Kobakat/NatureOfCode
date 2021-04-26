using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public enum WolfState
{
    Searching,
    Hunting,
    Eating,
    Dying
}

public class Wolf : GroundCreature
{
    public WolfState state = WolfState.Searching;

    Rabbit targetRabbit;

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
    }

    void PerformStateAction()
    {
        int x = targetRabbit ? targetRabbit.currentX : 0;
        int z = targetRabbit ? targetRabbit.currentZ : 0;

        switch (state)
        {
            case WolfState.Searching:
                HopToNearbySquareAfterDelay(2, x, z);
                break;
            case WolfState.Hunting:
                if (!targetRabbit)
                {
                    state = WolfState.Searching;
                    if (Mathf.Abs(targetZ) == 1 && Mathf.Abs(targetX) == 1)
                        targetX = 0;
                    return;
                }
                HopToNearbySquareAfterDelay(1, x, z);
                ClearRabbitWhenSafe();
                break;
            case WolfState.Dying:
                Die();
                break;
            case WolfState.Eating:
                EatRabbit();
                break;
        }
    }

    protected override sealed void Target(int xCoord, int zCoord)
    {
        base.Target(xCoord, zCoord);

        if (currentX == xCoord && currentZ == zCoord)
        {
            state = WolfState.Eating;
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

        if (reproduction >= 100)
        {
            CreateOffSpring(ecosystem.wolf);
        }
    }

    float eatTimer;
    void EatRabbit()
    {
        eatTimer += Time.deltaTime;

        if (eatTimer > 1)
        {
            hunger -= hungerLossOnEat;
            state = WolfState.Searching;
            eatTimer = 0;
            if (!ecosystem.creaturesToRemove.Contains(targetRabbit) && targetRabbit)
                ecosystem.creaturesToRemove.Add(targetRabbit);
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
            if (other.gameObject.CompareTag("rabbit") && !other.isTrigger)
            {
                targetRabbit = other.gameObject.GetComponent<Rabbit>();
                state = WolfState.Hunting;
            }
        }
    }
}
