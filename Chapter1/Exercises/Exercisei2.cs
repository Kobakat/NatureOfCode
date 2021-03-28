using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exercisei2 : MonoBehaviour
{
    exerciseintro2Mover walker;
    MovingObject chaser;

    void Start()
    {
        // Start is called before the first frame update
        walker = new exerciseintro2Mover();
        chaser = new MovingObject(walker.mover.transform);
    }

    // Update is called once per frame
    void FixedUpdate()
    {   //Have the walker choose a direction
        walker.step();
        walker.CheckEdges();
        chaser.step();
        chaser.CheckEdges();
    }
}

public class MovingObject
{
    Vector3 location;
    Vector2 minimumPos, maximumPos;

    Transform preyTransform;

    public GameObject mover = GameObject.CreatePrimitive(PrimitiveType.Cube);

    public MovingObject(Transform prey)
    {
        findWindowLimits();
        location = Vector2.zero;
        //We need to create a new material for WebGL
        Renderer r = mover.GetComponent<Renderer>();
        r.material = new Material(Shader.Find("Diffuse"));

        preyTransform = prey;
    }

    private void findWindowLimits()
    {
        // We want to start by setting the camera's projection to Orthographic mode
        Camera.main.orthographic = true;
        // Next we grab the minimum and maximum position for the screen
        minimumPos = Camera.main.ScreenToWorldPoint(Vector2.zero);
        maximumPos = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
    }

    public void step()
    {
        location = mover.transform.position;
        //Each frame choose a new Random number 0,1,2,3, 
        //If the number is equal to one of those values, take a step
 
        float choice = Random.Range(0, 1f);
        if (choice <= .2f)
        {
            location.y++;
        }
        else if (choice > .2f && choice <= .4f)
        {
            location.y--;
        }
        else if (choice > .4f && choice <= .6f)
        {
            location.x--;           
        }
        else
        {
            location = Vector3.MoveTowards(location, preyTransform.position, 5);
        }

        mover.transform.position += Time.deltaTime * location;
    }

    public void CheckEdges()
    {
        location = mover.transform.position;

        if (location.x > maximumPos.x)
        {
            location = Vector2.zero;
        }
        else if (location.x < minimumPos.x)
        {
            location = Vector2.zero;
        }
        if (location.y > maximumPos.y)
        {
            location = Vector2.zero;
        }
        else if (location.y < minimumPos.y)
        {
            location = Vector2.zero;
        }
        mover.transform.position = location;
    }
}

public class exerciseintro2Mover
{
    // The basic properties of a mover class
    private Vector3 location;

    // The window limits
    private Vector2 minimumPos, maximumPos;

    // Gives the class a GameObject to draw on the screen
    public GameObject mover = GameObject.CreatePrimitive(PrimitiveType.Sphere);

    public exerciseintro2Mover()
    {
        findWindowLimits();
        location = Vector2.zero;
        //We need to create a new material for WebGL
        Renderer r = mover.GetComponent<Renderer>();
        r.material = new Material(Shader.Find("Diffuse"));
    }

    public void step()
    {
        location = mover.transform.position;
        //Each frame choose a new Random number 0,1,2,3, 
        //If the number is equal to one of those values, take a step
        float choice = Random.Range(0, 4);
        if (choice == 0)
        {
            location.y++;

        }
        else if (choice == 1)
        {
            location.y--;
        }
        else if (choice == 2)
        {
            location.x--;
        }
        else
        {
            location.x++;
        }

        mover.transform.position += location * Time.deltaTime;
    }

    public void CheckEdges()
    {
        location = mover.transform.position;

        if (location.x > maximumPos.x)
        {
            location = Vector2.zero;
        }
        else if (location.x < minimumPos.x)
        {
            location = Vector2.zero;
        }
        if (location.y > maximumPos.y)
        {
            location = Vector2.zero;
        }
        else if (location.y < minimumPos.y)
        {
            location = Vector2.zero;
        }
        mover.transform.position = location;
    }

    private void findWindowLimits()
    {
        // We want to start by setting the camera's projection to Orthographic mode
        Camera.main.orthographic = true;
        // Next we grab the minimum and maximum position for the screen
        minimumPos = Camera.main.ScreenToWorldPoint(Vector2.zero);
        maximumPos = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
    }
}
