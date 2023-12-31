using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public abstract class AIBoat : MonoBehaviour {
    public int waitMin, waitMax;
    public GameObject LocationPrefab;

    [HideInInspector] public NavMeshAgent navAgent;
    protected NavMeshPath navPath;
    protected System.Random random;
    protected GameObject targetObject;

    private Bounds waterBounds;

    public void Awake() {
        random = new System.Random();
        navAgent = GetComponent<NavMeshAgent>();
        navPath = new NavMeshPath();
        waterBounds = GameObject.Find("WaterSurface").GetComponent<Renderer>().bounds;
        InitalizeAtPoint(new Vector3(0, 0, 0)); //Automatically sets target object


        Debug.Assert(navAgent != null); // make sure all boats have nav mesh       
        Debug.Assert(waterBounds != null);
        Debug.Assert(LocationPrefab != null);
        Debug.Assert(targetObject != null);
    }

    /// <summary>
    /// Picks a new location using PickRandomPoint()
    /// then moves the already initalized targetObject 
    /// to that location and begins the coroutine to change
    /// the boats path. 
    /// </summary>
    /// <param name="minWait"></param>
    /// <param name="maxWait"></param>
    public void SetRandomDestination(int minWait, int maxWait) {
        Vector3 newLocation = PickRandomPoint();
        targetObject.transform.position = newLocation;
        StartCoroutine(Move(random.Next(minWait, maxWait)));
    }

    /// <summary>
    /// Takes a given location
    /// then moves the already initalized targetObject 
    /// to that location and begins the coroutine to change
    /// the boats path. 
    /// </summary>
    /// <param name="minWait"></param>
    /// <param name="maxWait"></param>
    public void SetGivenDestination(int minWait, int maxWait, Vector3 location) {
        targetObject.transform.position = location;
        navAgent.CalculatePath(location, navPath);
        navAgent.SetDestination(location);
        StartCoroutine(Move(random.Next(minWait, maxWait)));
    }

    /// <summary>
    /// General move funtion that sets destiantion
    ///and waits a certain amount of time before moving to it.
    /// </summary>
    /// <param name="waitTime"></param>
    /// <returns></returns>    
    public IEnumerator Move(float waitTime) {
        yield return new WaitForSeconds(waitTime);
        Debug.Assert(navPath.status == NavMeshPathStatus.PathComplete);
        navAgent.SetPath(navPath);
    }

    /// <summary>
    /// Initalize the LocationPrefab to a given Vector3 location with a y value of 0.
    /// Then sets the parent of the object to a temp gameObject.
    /// </summary>
    /// <param name="point"></param>
    protected void InitalizeAtPoint(Vector3 point) {
        Debug.Assert(LocationPrefab != null);
        targetObject = Instantiate(LocationPrefab, new Vector3(point.x, 0, point.z), LocationPrefab.transform.rotation);
        targetObject.transform.SetParent(GameObject.FindGameObjectWithTag("Temp").transform);
    }

    /// <summary>
    /// Returns a valid Vector3 path within the bounds
    /// of the water texture. 
    /// </summary>
    /// <returns></returns>
    protected Vector3 PickRandomPoint() {
        Debug.Assert(targetObject != null);
        Vector3 rp;
        bool tooClose;
        do {
            rp = new Vector3(
            UnityEngine.Random.Range(waterBounds.min.x, waterBounds.max.x),
            transform.position.y,
            UnityEngine.Random.Range(waterBounds.min.z, waterBounds.max.z)
            );
            tooClose = Vector3.Distance(rp, transform.position) < LocationPrefab.GetComponent<SphereCollider>().radius;
        } while (tooClose || !IsPathValid(rp));
        return rp;
    }

    protected bool IsPathValid(Vector3 location) {
        navAgent.CalculatePath(location, navPath);
        return (navPath.status == NavMeshPathStatus.PathComplete);
    }

    public void MoveTargetObject(Vector3 location) {
        navAgent.CalculatePath(location, navPath);
        Debug.Assert(navPath.status == NavMeshPathStatus.PathComplete);
        navAgent.SetPath(navPath);
    }
    public IEnumerator DestroyAfterTime(float time) {
        yield return new WaitForSeconds(time);
        CancelInvoke(); // Prevent null error when setting path to main boat
        StopAllCoroutines();
        
        Destroy(this.gameObject);
        Destroy(targetObject);
    }

    /// <summary>
    /// Set NavMeshAgent AngularSpeed
    /// </summary>
    /// <param name="turnSpeed"></param>
    public void SetTurnSpeed(float turnSpeed) {
        Debug.Assert(navAgent != null);
        navAgent.angularSpeed = turnSpeed;
    }

    /// <summary>
    /// Set NavMeshAgent speed
    /// </summary>
    /// <param name="speed"></param>
    public void SetSpeed(float speed) {
        Debug.Assert(navAgent != null);
        navAgent.speed = speed;
    }

}
