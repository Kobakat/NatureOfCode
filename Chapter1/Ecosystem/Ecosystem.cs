using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ecosystem : MonoBehaviour
{
    public int creatureCount;
    public int resourceCount;

    public List<Creature> creatures;
    public List<IResource> resources;

    GameObject creatureManager;
    GameObject resourceManager;

    void Awake()
    {
        creatures = new List<Creature>();
        resources = new List<IResource>();
      
        creatureManager = new GameObject("Creatures");
        resourceManager = new GameObject("Resources");
        creatureManager.transform.SetParent(this.transform);    
        resourceManager.transform.SetParent(this.transform);

        CreateResources();
        CreateCreatures();           
    }

    void CreateCreature(System.Type type)
    {
        GameObject creature = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Creature component = (Creature)creature.AddComponent(type);
        creatures.Add(component);
        

        //Set gameObject values
        creature.name = $"{type.Name}";
        creature.transform.SetParent(creatureManager.transform);
        creature.transform.position = Utility.GetRandomPositionOnScreen();
    }

    void CreateResource(System.Type type)
    {
        GameObject resource = GameObject.CreatePrimitive(PrimitiveType.Cube);
        IResource component = (IResource)resource.AddComponent(type);
        resources.Add(component);

        //Set transform values
        resource.name = $"{type.Name}";
        resource.transform.SetParent(resourceManager.transform);
        resource.transform.position = Utility.GetRandomPositionOnScreen();
    }

    void CreateCreatures()
    {
        for(int i = 0; i < creatureCount; i++)
        {
            CreateCreature(typeof(Fly));      
        }
    }

    void CreateResources()
    {
        for (int i = 0; i < resourceCount; i++)
        {
            CreateResource(typeof(Food));
        }
    }
}
