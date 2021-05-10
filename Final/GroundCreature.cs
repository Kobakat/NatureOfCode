using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class GroundCreature : Creature
{
    public float hungerIncreaseRate = 2f;
    public int hungerLossOnEat = 20;
    public float reproductionIncreaseRate = 1f;
    public int minOffspring = 1;
    public int maxOffspring = 3;
    public float minSpeed = 0.5f;
    public float maxSpeed = 1.0f;
    public float turnSpeed = 360f;
    public float chaseSpeed = 3f;
    public float hopHeight = 1.25f;
    
    public bool gender { get; set; } //0 female, 1 male

    protected float hopTimer;
    protected bool currentlyHopping;
    protected Vector3 previousHopPoint;
    protected Vector3 targetHopPoint;

    protected int hunger;
    protected float hungerTimer;

    protected int reproduction;
    protected float reproductionTimer;

    protected float baseSpeed;
    protected float speed;
    protected int targetX;
    protected int targetZ;

    [SerializeField] protected Material mMat;
    [SerializeField] protected Material fMat;

    public int currentX;
    public int currentZ;

    protected List<int> possibleTiles;
    protected override void Start()
    {
        base.Start();

        //hacky and lazy way to make sure new borns dont spawn randomly but instead on their parent
        if (Time.time < 3f)
        {
            SpawnInRandomLocation(0.5f, false);
        }

        gender = System.Convert.ToBoolean(Random.Range(0, 2));

        MeshRenderer renderer = GetComponent<MeshRenderer>();

        if (gender)
            renderer.material = fMat;
        else
            renderer.material = mMat;


        targetX = (int)(transform.forward.x + 0.01f);
        targetZ = (int)(transform.forward.z + 0.01f);
        currentX = (int)(transform.position.x + 0.01f);
        currentZ = (int)(transform.position.z + 0.01f);

        baseSpeed = Random.Range(minSpeed, maxSpeed);
        speed = baseSpeed;

        possibleTiles = new List<int>();
    }

    protected virtual void HopToNearbySquareAfterDelay(int action, int xCoord, int zCoord) //dumb, lazy enum.
    {
        if (currentlyHopping)
        {
            Hop();
        }

        else
        {
            hopTimer += Time.deltaTime;

            if (hopTimer >= speed)
            {
                hopTimer = 0;

                switch(action) //Dumb and lazy
                {
                    case 1:
                        Target(xCoord, zCoord);
                        break;
                    case 2:
                        Search();
                        break;
                    case 3:
                        Flee(xCoord, zCoord);
                        break;
                }
            }
        }
    }

    protected virtual void Search()
    {
        //If the creature is looking forward on the X axis, sample 3 points near it.
        if (Mathf.Abs(targetX) > 0.1)
        {
            int possibleForward = currentX + targetX * 1;
            int possibleLeft = currentZ - 1;
            int possibleRight = currentZ + 1;

            if (possibleForward < maxBound.x && possibleForward > minBound.x)
                possibleTiles.AddRange(Enumerable.Repeat(possibleForward, 8));

            if (possibleLeft < maxBound.y && possibleLeft > minBound.y)
                possibleTiles.Add(possibleLeft);

            if (possibleRight < maxBound.y && possibleRight > minBound.y)
                possibleTiles.Add(possibleRight);

            int selection = Random.Range(0, possibleTiles.Count);

            if (possibleTiles[selection] == possibleForward)
            {
                currentX = possibleForward;
            }

            else if (possibleTiles[selection] == possibleRight)
            {
                currentZ = possibleRight;
                targetX = 0;
                targetZ = 1;
            }

            else if (possibleTiles[selection] == possibleLeft)
            {
                currentZ = possibleLeft;
                targetX = 0;
                targetZ = -1;
            }
        }

        //If the creature is looking along the Z axis, sample points along it
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

    protected virtual void Target(int xCoord, int zCoord)
    {
        int xChange = 0;
        int zChange = 0;

        if (currentX != xCoord)
            xChange = currentX > xCoord ? -1 : 1;

        if (currentZ != zCoord)
            zChange = currentZ > zCoord ? -1 : 1;

        currentX += xChange;
        targetX = xChange;
        currentZ += zChange;
        targetZ = zChange;     
    }

    protected virtual void Hop()
    {
        hopTimer += Time.deltaTime;
        float frac = hopTimer / baseSpeed;
        Vector3 newPoint = Vector3.Lerp(previousHopPoint, targetHopPoint, frac);

        float peakHeight = Mathf.Max(targetHopPoint.y + hopHeight, previousHopPoint.y + hopHeight);
        float newY;

        if (frac <= 0.5f)
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

        if (frac >= 1)
        {
            transform.position = targetHopPoint;
            hopTimer = 0;
            currentlyHopping = false;
        }
    }

    protected virtual void UpdateLookDirection()
    {
        Quaternion targetRot = Quaternion.LookRotation(new Vector3(targetX, 0, targetZ));
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
    }

    protected float deathTimer;
    protected virtual void Die()
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

    protected virtual void IncreaseHunger(float rate)
    {
        hungerTimer += Time.deltaTime * rate;

        if (hungerTimer > hungerIncreaseRate)
        {
            hungerTimer = 0;
            hunger++;
        }
    }

    protected virtual void IncreaseReproductionValue()
    {
        reproductionTimer += Time.deltaTime;

        if (reproductionTimer > reproductionIncreaseRate)
        {
            reproductionTimer = 0;
            reproduction++;

            if (reproduction > 70)
                reproduction = 70;
        }      
    }

    protected virtual void CreateOffSpring(GameObject type)
    {
        int offspringCount = Random.Range(minOffspring, maxOffspring + 1);
        reproduction = 0;

        for (int i = 0; i < offspringCount; i++)
        {
            Creature childComponent = Instantiate(type, transform.position, transform.rotation, this.transform.parent).GetComponent<Creature>();
            childComponent.ecosystem = this.ecosystem;
            this.ecosystem.creatures.Add(childComponent);
        }
    }

    protected virtual void Flee(int xCoord, int zCoord) 
    {
        int xChange = 0;
        int zChange = 0;

        if (currentX != xCoord)
            xChange = currentX > xCoord ? 1 : -1;

        if (currentZ != zCoord)
            zChange = currentZ > zCoord ? 1 : -1;

        xChange = (currentX + xChange > maxBound.x - 1 || currentX + xChange < minBound.x) ? 0 : xChange;
        zChange = (currentZ + zChange > maxBound.y - 1 || currentZ + zChange < minBound.y) ? 0 : zChange;

        currentX += xChange;
        targetX = xChange;
        currentZ += zChange;
        targetZ = zChange;
    }  
}
