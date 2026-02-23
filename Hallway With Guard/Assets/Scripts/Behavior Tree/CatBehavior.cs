using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/*
 2/23/2026 PROGRESS:
 - Basic ability to navigate to specified positions using NavMesh.
 - Confirmed basic btree functionality. 
 - Patrol() is complete.
 - Rest() is (mostly) complete.
 - Hunt() runs at base & interrupts Patrol() & Rest().
 
 - Rest() has a bug where it infinitely loops after the first time it is called.
 - Hunt() is not done.
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
    private LastAction lastAction = LastAction.PATROL; // FIXME: set to PATROL to test Rest -> Patrol loop
    
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
    private bool finishedRest = false;
    
    // Debugging bool.
    private bool gameOver = false;

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
        if (!gameOver)
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
            gameOver = true;
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
            StopCoroutine(Patrolling());
            state = ActionState.IDLE;
            lastAction = LastAction.PATROL;
            return Node.Status.SUCCESS;
        }
        
        // At default, set to running for every loop through the tree that the cat is working.
        return Node.Status.RUNNING;
    }

    private IEnumerator Resting()
    {
        // Sets the cat's destination to its bed.
        agent.SetDestination(bed.transform.position);
        bool arrivedAtBed = false;
        
        // While the cat hasn't arrived at the bed and is on their way to it, this will execute.
        while (!arrivedAtBed && agent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            // Calculates the distance between the bed and the cat.
            float distToBed = Vector3.Distance(bed.transform.position, transform.position);
                
            // If the cat reaches the bed, arrivedAtBed will be set to true.
            if (distToBed <= agent.stoppingDistance)
            { 
                arrivedAtBed = true;
            }
        }
        
        // After the cat arrives at the bed, it will "rest" for 5 seconds.
        yield return new WaitForSeconds(5f); 

        // After the cat finishes resting, this lets Rest() know that Resting() was successful.
        finishedRest = true;
    }
    
    // FIXME: breaks the Patrol -> Rest -> Patrol loop
    private Node.Status Rest()
    {
        // If the last action the cat performed was resting or hunting, fail to execute node.
        String lastActionString = GetLastAction();
        if (lastActionString.Equals("REST") || lastActionString.Equals("HUNT"))
        {
            Debug.Log("Failed to rest!");
            return Node.Status.FAILURE;
        }
        
        // If the player was recently spotted, fail to execute node.
        if (playerSpotted)
        {
            Debug.Log("Failed to rest!");
            return Node.Status.FAILURE;
        }
        
        // This will execute the first run-through of this node if it gets this far. 
        if (state == ActionState.IDLE)
        {
            Debug.Log("Starting to rest...");
            state = ActionState.WORKING;
            StartCoroutine(Resting());
        }
        
        // When the Resting() coroutine sets finishedRest to true, this runs.
        if (finishedRest)
        {
            Debug.Log("Finished resting!");
            StopCoroutine(Resting());
            lastAction = LastAction.REST;
            finishedRest = false;
            state = ActionState.IDLE;
            return Node.Status.SUCCESS;
        }
        
        // At default, set to running for every loop through the tree that the cat is working.
        Debug.Log("Resting...");
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
