using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    public GameObject barrelPrefab, moneyPrefab;
    public float distBetweenBarrels = 3.9f, distBetweenCoins = 2; //can be modified within Unity; note that values less than the z dimension of an object may lead to similar objects spawning within each other; note that values less than the sum of the car's length (which is 7) and the object's z dimension creates the chance (assuming the object can fall anywhere on the width of the track) for a section of track to be impassible without the car encountering the object, however as this also depends on the car's ability to move horizontally between obstacles, for the sake of simplicity for this project, it is noted that it is possible for sections of track to be untraversable without the car encountering the object
    public const int numObstacles = 40, numMoney = 60; //how many objects to render at a time; can be modified within Unity (for performance testing)
    private Car car; //position is used to tell objects when to despawn
    public float barrelDist = 80, coinDist = 80; //initialized to the distance to the first obstacle; can be modified within Unity; note that the limitations of the size of a floating point variable does mean the game is not truly endless (as eventually the variable will overflow)
    public float barrelVariance = 7.8f, coinVariance = 2; //the range of z values behind where the object would otherwise spawn that the object can actually spawn within; can be modified within Unity; when the z distance between objects plus the object's variance is greater than the z-dimension of object, there is a chance for similar objects to spawn within each other; note that a road width (which is 12) less than the number of objects that can fall within the car's length (which Jeffrey suspects = max((car's length (which is 7) + the object's variance + the object's z dimension) / z distance between objects, 1)) times the sum of the car's width (which is 3) and the object's x dimension, creates the chance for a section of track to be untraversable without the car encountering the object

    [HideInInspector] public List<GameObject> obstacles, money;
    
    private const float trackLength = 80f; //to be updated as required

    // Start is called before the first frame update
    void Start()
    {
        car = FindObjectOfType<Car>();
        for (int i = 0; i < numObstacles; ++i) //initialize the array of obstacles objects
        {
            obstacles.Add(Instantiate(barrelPrefab, transform));
            obstacles[i].GetComponent<Obstacle>().index = i;
            obstacles[i].GetComponent<Obstacle>().spawnTime = Time.time;
            obstacles[i].transform.localPosition = new Vector3(Random.Range(-6f, 6f), 0.05f, barrelDist + Random.Range(0, barrelVariance)); //x position is random on the road, y position is ground level, z position is at the back within some range
            obstacles[i].SetActive(true);
            obstacles[i].transform.GetChild(0).gameObject.SetActive(false); //during development, in the Unity editor, the first child of the Barrel prefab is active
            obstacles[i].transform.GetChild(Random.Range(0, 9)).gameObject.SetActive(true);
            barrelDist += distBetweenBarrels; //the next object to spawn will do so slightly further back
        }

        for (int i = 0; i < numMoney; ++i) //initialize the array of money objects
        {
            float variance = Random.Range(0, coinVariance); //used to turn distBetweenCoins into a minimum distance between coins
            money.Add(Instantiate(moneyPrefab, transform));
            money[i].GetComponent<Obstacle>().index = i;
            money[i].GetComponent<Obstacle>().spawnTime = Time.time;
            money[i].transform.localPosition = new Vector3(Random.Range(-5.75f, 5.75f), 1, coinDist + variance); //x position is random on the road, y position is just above ground level, z position is at the back within some range
            money[i].SetActive(true);
            coinDist += distBetweenCoins + variance; //the next object to spawn will do so slightly further back; this (adding the variance) is what makes distBetweenCoins a minimum allowed distance
        }
    }

    // Update is called once per frame
    void Update()
    {
        //FIXME: consider running in background (as in don't update every frame)
        for (int i = 0; i < numObstacles; ++i)
        {
            if (obstacles[i].transform.localPosition.y < -5 || obstacles[i].transform.localPosition.z < car.transform.localPosition.z - 5) //if the barrel is in an irrelevant position then it needs to be respawned
            {
                for (int j = 0; j < 9; ++j)
                {
                    if (obstacles[i].transform.GetChild(j).gameObject.activeSelf)
                    {
                        obstacles[i].transform.GetChild(j).gameObject.SetActive(false); //ensures only one barrel model is active at a time
                        break; //only one barrel model should be active at a time so breaking should do no harm
                    }
                }
                obstacles[i].transform.GetChild(Random.Range(0, 9)).gameObject.SetActive(true); //re-randomize the barrel's prefab model
                obstacles[i].GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0); //set the barrel's velocity to zero
                obstacles[i].GetComponent<Rigidbody>().angularVelocity = new Vector3(0, 0, 0); //set the barrel's velocity to zero
                obstacles[i].transform.eulerAngles = new Vector3(270, 0, 0); //set the barrel standing upright
                obstacles[i].transform.localPosition = new Vector3(Random.Range(-6f, 6f), 0.05f, barrelDist + Random.Range(0, barrelVariance)); //x position is random on the road, y position is ground level, z position is at the back within some range
                barrelDist += distBetweenBarrels; //the next object to spawn will do so slightly further back
            }
            if (obstacles[i].transform.localPosition.z > car.transform.localPosition.z + trackLength && obstacles[i].GetComponent<Rigidbody>().velocity.magnitude < 2) // the velocity check is to verify that this barrel hasn't been launched by the player
            {
                obstacles[i].GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll; //otherwise the barrel will fall through the floor as it may not be generated there yet
            }
            else
            {
                obstacles[i].GetComponent<Obstacle>().spawnTime = Time.time;
                obstacles[i].GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            }
        }
        
        for (int i = 0; i < numMoney; ++i)
        {
            if (money[i].transform.localPosition.z < car.transform.localPosition.z - 5)
            {
                float variance = Random.Range(0, coinVariance); //used to turn distBetweenCoins into a minimum distance between coins
                money[i].GetComponent<Obstacle>().spawnTime = Time.time;
                money[i].transform.localPosition = new Vector3(Random.Range(-5.75f, 5.75f), 1, coinDist + variance); //x position is random on the road, y position is just above ground level, z position is at the back within some range
                coinDist += distBetweenCoins + variance; //the next object to spawn will do so slightly further back; this (adding the variance) is what makes distBetweenCoins a minimum allowed distance
            }
        }
    }

    public void Respawn(int index, string tag)
    {
        if (tag == "Obstacle")
        {
            //re-randomizing the barrel's prefab model and location checking are unecessary as this function is only called by Obstacle.cs in the instance of a collision at spawn
            obstacles[index].transform.eulerAngles = new Vector3(270, 0, 0); //set the barrel standing upright
            obstacles[index].GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0); //set the barrel's velocity to zero
            obstacles[index].transform.localPosition = new Vector3(Random.Range(-6f, 6f), 0.05f, barrelDist + Random.Range(0, barrelVariance)); //x position is random on the road, y position is ground level, z position is at the back within some range
            barrelDist += distBetweenBarrels; //the next object to spawn will do so slightly further back

            if (obstacles[index].transform.localPosition.z > car.transform.localPosition.z + trackLength)
            {
                obstacles[index].SetActive(false); //otherwise the barrel will fall through the floor as it may not be generated there yet
            }
            else
            {
                obstacles[index].SetActive(true);
            }
        }
        else //tag == "Money"; when it comes to tags, money isn't called an "Obstacle"
        {
            //location checking the coin is unecessary as this function is only called by Obstacle.cs in the instance of a collision at spawn
            float variance = Random.Range(0, coinVariance); //used to turn distBetweenCoins into a minimum distance between coins
            money[index].GetComponent<Obstacle>().spawnTime = Time.time;
            money[index].transform.localPosition = new Vector3(Random.Range(-5.75f, 5.75f), 1, coinDist + variance); //x position is random on the road, y position is just above ground level, z position is at the back within some range
            coinDist += distBetweenCoins + variance; //the next object to spawn will do so slightly further back; this (adding the variance) is what makes distBetweenCoins a minimum allowed distance
        }
    }
}
