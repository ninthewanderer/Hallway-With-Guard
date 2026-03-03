using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

/*
 2/28/2026 PROGRESS:
 - Added text to indicate when the player has been spotted by the cat.
 
 - Still need to implement optional mousetrap checking & eating behaviors.
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
    [System.NonSerialized] public bool playerSpotted =  false; 
    public float detectionDelay = 0.2f;
    public float viewRadius;
    [Range(0, 360)] public float viewAngle;
    public LayerMask targetMask;
    public LayerMask obstacleMask;
    public Text spottedText;
    
    // Variables needed for navigation.
    public GameObject bed;
    public GameObject[] patrolPoints;
    private bool finishedPatrol = false;
    private bool finishedRest = false;
    public float actionDelay = 5f;
    public float catSpeed;
    
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
        
        // Starts the coroutine for constant player detection.
        StartCoroutine(Hunting());
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

    private void FieldOfViewCheck()
    {
        // Collider array that will be constantly searching for the player on the layer targetMask.
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, viewRadius, targetMask);
        
        // If length isn't 0, that means something has been detected (the player).
        if (rangeChecks.Length != 0)
        {
            Transform target = rangeChecks[0].transform;
            
            // Calculates the direction of the target's position.
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            
            // Calculates the angle from the player to the enemy and checks it against viewAngle.
            if (Vector3.Angle(transform.forward, directionToTarget) < viewAngle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);
                
                // Creates a raycast from the cat's to the target and checks if there are any obstacles in the way.
                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask))
                {
                    playerSpotted = true;
                    spottedText.text = "Spotted!";
                    spottedText.color = Color.red;
                    agent.speed = playerSpeed + 5;
                    transform.LookAt(player.transform.position);
                }
                else
                {
                    playerSpotted = false;
                    spottedText.text = "Not Spotted.";
                    spottedText.color = Color.green;
                    agent.speed = catSpeed;
                }
            }
            else
            {
                spottedText.text = "Not Spotted.";
                spottedText.color = Color.green;
                playerSpotted = false;
            }
        }
        else if (playerSpotted) 
        { // Ensures that playerSpotted won't be infinitely set to true after 1 loop of this coroutine.
            spottedText.text = "Not Spotted.";
            spottedText.color = Color.green;
            playerSpotted = false;
        }
    }
    
    private IEnumerator Hunting()
    {
        // The delay between each run of this coroutine.
        WaitForSeconds wait = new WaitForSeconds(detectionDelay);

        // infinite loop to ensure there is constant player detection
        while (true)
        {
            yield return wait;
            FieldOfViewCheck();
        }
    }
    
    // FIXME: still needs to be fixed when win conditions are implemented
    private Node.Status Hunt()
    {
        if (!playerSpotted)
        {
            return Node.Status.FAILURE;
        }
        
        // Since the player has been spotted, the cat's state must be HUNTING and their last action must be hunting.
        state = ActionState.HUNTING;
        lastAction = LastAction.HUNT;

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
        // The delay until the cat moves to the next patrol point.
        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(actionDelay);
        
        // Begins iterating through the patrolPoints array.
        foreach (GameObject waypoint in patrolPoints)
        {
            // Sets cat's path to the waypoint.
            agent.SetDestination(waypoint.transform.position);
            
            // Waits for the cat to arrive at the patrol point before continuing or ending its patrol.
            yield return wait;
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
            lastAction = LastAction.PATROL;
            finishedPatrol = false;
            state = ActionState.IDLE;
            return Node.Status.SUCCESS;
        }
        
        // At default, set to running for every loop through the tree that the cat is working.
        return Node.Status.RUNNING;
    }

    private IEnumerator Resting()
    {
        // The delay until the cat moves away from the bed.
        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(actionDelay);
        
        // Sets the cat's destination to its bed.
        agent.SetDestination(bed.transform.position);
        
        // Waits for the cat to get to the bed and "rest".
        yield return wait;

        // After the cat finishes resting, this lets Rest() know that Resting() was successful.
        finishedRest = true;
    }
    
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
        
        // This will execute the first run-through of this node if it gets this far. 
        if (state == ActionState.IDLE)
        {
            state = ActionState.WORKING;
            StartCoroutine(Resting());
        }
        
        // When the Resting() coroutine sets finishedRest to true, this runs.
        if (finishedRest)
        {
            StopCoroutine(Resting());
            lastAction = LastAction.REST;
            finishedRest = false;
            state = ActionState.IDLE;
            return Node.Status.SUCCESS;
        }
        
        // At default, set to running for every loop through the tree that the cat is working.
        return Node.Status.RUNNING;
    }
}
