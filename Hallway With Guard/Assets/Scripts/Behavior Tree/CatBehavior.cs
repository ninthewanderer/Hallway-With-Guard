using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/*
 2/23/2026 PROGRESS:
 - Basic ability to navigate to specified positions using NavMesh.
 - Confirmed basic btree functionality. 
 - Patrol() is complete.
 - Hunt() has been tested and runs at base.
 - Hunt() interrupts Patrol() successfully.
 
 - Rest() and Hunt() are not done.
 - Ensure Hunt() interrupts Rest().
 - Create POV colliders for cat & test collision functionality.
 - Ensure btree repeatedly loops rather than ending after one iteration (current setup is for debugging).
 */

public class CatBehavior : MonoBehaviour
{
    // Cat's behavior tree.
    private BehaviorTree tree;
    
    // Used to store NavMesh info.
    private NavMeshAgent agent;
    
    // Tracks the cat's currently state in the behavior tree. (Idle by default).
    private enum ActionState { IDLE, WORKING, HUNTING };
    private ActionState state = ActionState.IDLE;
    
    // Tracks the cat's last action in the behavior tree. (Rest by default).
    private enum LastAction { REST, PATROL, HUNT, EAT };
    private LastAction lastAction = LastAction.REST;
    
    // Tracks the current state of the behavior tree. (Running by default).
    private Node.Status treeStatus = Node.Status.RUNNING;
    
    // Variables needed to track the player.
    public GameObject player;
    private float playerSpeed;
    private CharacterController playerController;
    public bool playerSpotted =  false; // FIXME: make sure this gets changed back to private.
    
    // Variables needed for navigation.
    public GameObject bed;
    public GameObject[] patrolPoints;
    private bool finishedPatrol = false;

    void Start()
    {
        // Obtains NavMeshAgent component from the Inspector.
        agent = GetComponent<NavMeshAgent>();
        
        // Obtains the player's CharacterController component & movement speed.
        playerController = player.GetComponent<CharacterController>();
        playerSpeed = player.GetComponent<PlayerMovement>().moveSpeed;
        
        // Creates a new behavior tree.
        tree = new BehaviorTree();
        
        // Creates default 3 behaviors of the tree.
        Selector huntPatrolRest = new Selector("Hunt, Patrol, or Rest");
        Leaf hunt = new Leaf("Hunt", Hunt);
        Leaf patrol = new Leaf("Patrol", Patrol);
        Leaf rest = new Leaf("Rest", Rest);
        
        huntPatrolRest.AddChild(hunt);
        huntPatrolRest.AddChild(patrol);
        huntPatrolRest.AddChild(rest);
        
        tree.AddChild(huntPatrolRest);
    }

    void Update()
    {
        // Calls Process() to start processing/running the nodes within the tree.
        if (treeStatus != Node.Status.SUCCESS)
        {
            treeStatus = tree.Process();
        }
    }

    private String GetLastAction()
    {
        return lastAction.ToString();
    }

    // FIXME: still needs win conditions, collider detections, and a way to turn playerSpotted off.
    private Node.Status Hunt()
    {
        if (!playerSpotted)
        {
            return Node.Status.FAILURE;
        }
        
        // Since the player has been spotted, the cat's state must be HUNTING no matter what and all coroutines stop.
        StopAllCoroutines();
        state = ActionState.HUNTING;

        // Calculates the direction the player is heading towards.
        Vector3 playerDirection = player.transform.position - transform.position;
        float relativeDestination =
            Vector3.Angle(transform.forward, transform.TransformVector(player.transform.forward));
        float angleToTarget = Vector3.Angle(transform.forward, transform.TransformVector(playerDirection));

        // If the player isn't moving, the cat will "pounce".
        if ((angleToTarget > 90 && relativeDestination < 20) || playerController.velocity.magnitude < 0.1f)
        {
            agent.SetDestination(player.transform.position);
        }
        
        // If the player is moving, calculates and goes to where they're heading based on their speed.
        float lookAhead = playerDirection.magnitude/(playerSpeed) + playerController.velocity.magnitude;
        agent.SetDestination(player.transform.position + player.transform.forward * lookAhead);
        // Do something here to check if player is caught by the cat

        if (Vector3.Distance(player.transform.position, transform.position) <= agent.stoppingDistance)
        {
            Debug.Log("The player has been caught!");
            return Node.Status.SUCCESS;
        }
        
        return Node.Status.RUNNING;
    }
    
    private IEnumerator Patrolling()
    {
        // Begins iterating through the patrolPoints array.
        foreach (GameObject waypoint in patrolPoints)
        {
            // Sets cat's path to the waypoint.
            agent.SetDestination(waypoint.transform.position);
            bool arrivedAtPatrol = false;
            
            // While the cat hasn't arrived at the patrol and is on their way to it, this will execute.
            while (!arrivedAtPatrol && agent.pathStatus != NavMeshPathStatus.PathComplete)
            {
                // Calculates the distance between the current patrol and the cat.
                float distToPatrol = Vector3.Distance(waypoint.transform.position, transform.position);
                
                // If the cat reaches the patrol point, arrivedAtPatrol will be set to true.
                if (distToPatrol <= agent.stoppingDistance)
                { 
                    arrivedAtPatrol = true;
                }
            }
            
            // After the cat arrives at the patrol point, it will wait 5 seconds before continuing or ending its patrol.
            yield return new WaitForSeconds(5f); 
        }

        // After the cat iterates through all of its patrol points, this lets Patrol() know that the patrol was successful.
        finishedPatrol = true;
    }
    
    private Node.Status Patrol()
    {
        // If the last action the cat performed was patrolling or eating, fail to execute node.
        String lastActionString = GetLastAction();
        if (lastActionString.Equals("PATROL") || lastActionString.Equals("EAT"))
        {
            return Node.Status.FAILURE;
        }
        
        // If the player was recently spotted, fail to execute node.
        if (playerSpotted)
        {
            return Node.Status.FAILURE;
        }

        // This will execute the first run-through of this node if it gets this far. 
        if (state == ActionState.IDLE)
        {
            state = ActionState.WORKING;
            StartCoroutine(Patrolling());
        }

        // When the Patrolling() coroutine sets finishedPatrol to true, this runs.
        if (finishedPatrol)
        {
            StopAllCoroutines();
            state = ActionState.IDLE;
            lastAction = LastAction.PATROL;
            return Node.Status.SUCCESS;
        }
        
        // At default, set to running for every loop through the tree that the cat is working.
        return Node.Status.RUNNING;
    }

    // FIXME: needs to make cat wait after arriving at bed.
    private Node.Status Rest()
    {
        // If the last action the cat performed was resting or hunting, fail to execute node.
        String lastActionString = GetLastAction();
        if (lastActionString.Equals("REST") || lastActionString.Equals("HUNT"))
        {
            return Node.Status.FAILURE;
        }
        
        // If the player was recently spotted, fail to execute node.
        if (playerSpotted)
        {
            return Node.Status.FAILURE;
        }

        // Calculates the distance between the bed and the cat.
        float distanceToBed = Vector3.Distance(bed.transform.position, transform.position);
        
        // If the cat is idle, they will go to the bed.
        if (state == ActionState.IDLE)
        {
            agent.SetDestination(bed.transform.position);
            state = ActionState.WORKING;
        }
        else if (Vector3.Distance(agent.pathEndPosition, bed.transform.position) > agent.stoppingDistance)
        { // If the cat doesn't make it to the bed, the node fails.
            state = ActionState.IDLE;
            return Node.Status.FAILURE;
        }
        else if (distanceToBed <= agent.stoppingDistance)
        { // If the cat reaches the bed, the node succeeds.
            state = ActionState.IDLE;
            lastAction = LastAction.REST;
            Debug.Log("Rest was successful!");
            return Node.Status.SUCCESS;
        }
        
        Debug.Log("Rest is running...");
        return Node.Status.RUNNING;
    }

    // See if this can be specific colliders?
    private void OnTriggerEnter(Collider other)
    {
        // If the player enters any of the cat's colliders, it will immediately start hunting them down.
        if (other.CompareTag("Player"))
        {
            playerSpotted = true;
        }
    }
}
