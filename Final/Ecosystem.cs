using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ecosystem : MonoBehaviour
{
    public int beeCount;
    public int rabbitCount;
    public int wolfCount;
    public int resourceCount;
    
    public List<Creature> creatures { get; set; }
    public List<IResource> resources { get; set; }

    GameObject creatureManager;
    GameObject resourceManager;

    public PerlinTerrain terrainManager;
    public GameObject bee;
    public GameObject flower;
    public GameObject rabbit;
    public GameObject wolf;

    public List<IResource> resourcesToRemove { get; set; }
    public List<Creature> creaturesToRemove { get; set; }

    void Awake()
    {
        terrainManager = Instantiate(terrainManager, this.transform);
        creatures = new List<Creature>();
        resources = new List<IResource>();
        resourcesToRemove = new List<IResource>();
        creaturesToRemove = new List<Creature>();
        creatureManager = new GameObject("Creatures");
        resourceManager = new GameObject("Resources");
        creatureManager.transform.SetParent(this.transform);    
        resourceManager.transform.SetParent(this.transform);              
    }

    void Start()
    {
        terrainManager.GenerateTerrain();
        CreateResources();
        CreateCreatures();
    }

    void LateUpdate()
    {
        RemoveResources();
        RemoveCreatures();
    }

    void CreateCreature(GameObject type)
    {
        GameObject creature = Instantiate(type, creatureManager.transform);
        Creature component = creature.GetComponent<Creature>();
        component.ecosystem = this;
        creatures.Add(component);

        //Set gameObject values
        creature.name = type.name;
        creature.transform.SetParent(creatureManager.transform);

    }

    void CreateResource(GameObject type)
    {
        GameObject resource = Instantiate(type, resourceManager.transform);
        resources.Add(resource.GetComponent<IResource>());

        //Set transform values
        resource.name = type.name;
        resource.transform.SetParent(resourceManager.transform);
    }

    void CreateCreatures()
    {
        for(int i = 0; i < beeCount; i++)
        {
            CreateCreature(bee);       
        }

        for (int i = 0; i < rabbitCount; i++)
        {
            CreateCreature(rabbit);
        }

        for (int i = 0; i < wolfCount; i++)
        {
            CreateCreature(wolf);
        }
    }

    void CreateResources()
    {
        for (int i = 0; i < resourceCount; i++)
        {
            CreateResource(flower);
            resources[i].ecosystem = this;
        }
    }

    void RemoveResources()
    {
        foreach(Flower resource in resourcesToRemove)
        {
            resources.Remove(resource);
            Destroy(resource.gameObject);
        }
        resourcesToRemove.Clear();
    }

    void RemoveCreatures()
    {
        foreach(Creature creature in creaturesToRemove)
        {
            creatures.Remove(creature);
            Destroy(creature.gameObject);
        }
        creaturesToRemove.Clear();
    }
}
