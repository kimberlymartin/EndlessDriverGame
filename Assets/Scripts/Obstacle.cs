using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [HideInInspector] public int index; //the position in the GameObject array in ObstacleManager.cs used to tell ObstacleManager.cs which (this) obstacle to respawn when required
    [HideInInspector] public float spawnTime; //upon (re)spawn, if this obstacle is collided with another recently (re)spawned obstacle (of a different type), one or both objects are respawned
    private ObstacleManager obstacleManager;
    [HideInInspector] public bool needToReset = false; //protected objects will be spared from one respawn

    // Start is called before the first frame update
    void Start()
    {
        obstacleManager = FindObjectOfType<ObstacleManager>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car") && this.CompareTag("Money"))
        {
            obstacleManager.Respawn(index, this.tag); //tell ObstacleManager.cs to respawn this object
            this.needToReset = false; //just in case; in preparation for another possible spawn collision
            this.spawnTime = Time.time;
            return; //the other object is a car so the remainder of this function will not be utilized anyway
        }
        if ((Time.time - spawnTime < 5) && (other.CompareTag("Obstacle") || other.CompareTag("Money")) && (Time.time - other.GetComponent<Obstacle>().spawnTime < 5)) //barrels and coins don't seem to detect if they're colliding with (spawned inside) an object of the same type, and since there is (as yet) no user concern with barrels or coins spawning within objects of the same type, verifying that exactly one of the tags is "Obstacle" and the other is "Money" is not performed as is assumed to be true
        {

            /*There are multiple ways to determine which objects should be respawned in response to a collision detected including respawning objects near the
            ObstacleManager's barrelDist and/or coinDist, using a time-since-(re)spawn timer on each object, going off the collided barrels' velocity, or using an
            infection-style (or cheese-touch) system with flags to show which barrels have experienced physics in response to a collision with the car or other
            barrels that have collided with the car (or barrels that have collided with those barrels, etc.) and not respawning barrels with their flag set.
            However, it was finally decided that which coin-barrel collisions constituted a respawn would be determined by whether the barrel(s) in question were
            upright (which would be defined as their center of mass residing (approximately) straight above their bottom, circular surface) along with having a
            spawn timer. And hence (with a barrel height of ~1 and diameter of ~0.8) a rotation of 38.7 degrees (= inverse cotangent of 1 / 0.8) (in either
            direction of standing straight up, (that is an x rotation of 270 +/- 38.7 degrees)) with a time since spawn of less than 5 seconds were decided to be
            the determining factors.*/
            if ((this.CompareTag("Obstacle") && this.transform.rotation.x % 360 < 308.7 && this.transform.rotation.x % 360 > 231.3) || (other.transform.rotation.x % 360 < 308.7 || other.transform.rotation.x % 360 > 231.3)) //if this or the other object is determined to be a barrel that has recently spawned
            {

                /*In addition to resolving the question of how to figure which collisions should be considered for respawn, there was the matter of deciding which
                of the objects should be sent for respawn. Ways of possibly accomplishing this include straight up prioritizing one type of object over the other
                (e.g. respawn all coins or all barrels), respawning all collided objects, prioritizing performance (that is, deleting whichever object is fewer in
                number as it is more likely to be collided with other/multiple instances of the other type of object), prioritizing object size (respawning the
                larger object for performance (again for the reason it is more likely to be collided with other/multiple instances of the other type of object),
                or respawning the smaller as the larger is more likely to be collided with enough other objects that it simply can never spawn (or rather has a
                significantly diminished chance of spawning)), the option chosen for this project which was prioritizing based on preserving the ratio of the
                collided game objects (as determined by the expected average of each type of object otherwise appearing in a section of track), prioritizing based
                on functions of time or other variables (including properties of game objects) (or such as preserving the ratio of the running total of previously
                spawned object types), or finally for prioritizing based on some combination of any of the aforementioned factors.*/
                if (this.needToReset)
                {
                    if (obstacleManager == null) //at launch, ObstacleManager's Start function is called and then this OnTriggerEnter function and then this Start function sometimes
                    {
                        obstacleManager = FindObjectOfType<ObstacleManager>();
                    }
                    obstacleManager.Respawn(index, this.tag); //tell ObstacleManager.cs to respawn this object
                    spawnTime = Time.time;
                    this.needToReset = false; //in preparation for another possible spawn collision
                }
                else if (!other.GetComponent<Obstacle>().needToReset) //then neither reset flag has been set so this obstacle needs to generate a random number to figure out who needs to respawn
                {
                    if (obstacleManager == null) //at launch, ObstacleManager's Start function is called and then this OnTriggerEnter function and then this Start function sometimes
                    {
                        obstacleManager = FindObjectOfType<ObstacleManager>();
                    }
                    if (this.CompareTag("Obstacle"))
                    {
                        if (Random.Range(0f, 1f) < (obstacleManager.distBetweenCoins + obstacleManager.coinVariance) / (obstacleManager.distBetweenBarrels + obstacleManager.distBetweenCoins + obstacleManager.coinVariance)) //calculating the ratio of object prevalence to determine which object gets respawned
                        {
                            obstacleManager.Respawn(index, this.tag); //tell ObstacleManager.cs to respawn this object
                            spawnTime = Time.time;
                        }
                        else //let the other respawn itself based on its needToReset flag when it calls its version of this script
                        {
                            other.GetComponent<Obstacle>().needToReset = true;
                        }
                    }
                    else //this.CompareTag("Money")
                    {
                        if (Random.Range(0f, 1f) < obstacleManager.distBetweenBarrels / (obstacleManager.distBetweenBarrels + obstacleManager.distBetweenCoins + obstacleManager.coinVariance)) //calculating the ratio of object prevalence to determine which object gets respawned
                        {
                            obstacleManager.Respawn(index, this.tag); //tell ObstacleManager.cs to respawn this object
                            spawnTime = Time.time;
                        }
                        else //let the other respawn itself based on its needToReset flag when it calls its version of this script
                        {
                            other.GetComponent<Obstacle>().needToReset = true;
                        }
                    }
                }
            }
        }
    }
}
