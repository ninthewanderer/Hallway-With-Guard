using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

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
    private bool playerSpotted =  false;
    
    // GameObjects needed for navigation.
    public GameObject bed;
    public GameObject[] patrolPoints;

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
        Selector patrolRestHunt = new Selector("Patrol, Rest, or Hunt");
        Leaf patrol = new Leaf("Patrol", Patrol);
        Leaf rest = new Leaf("Rest", Rest);
        Leaf hunt = new Leaf("Hunt", Hunt);
        
        patrolRestHunt.AddChild(patrol);
        patrolRestHunt.AddChild(rest);
        patrolRestHunt.AddChild(hunt);
        
        tree.AddChild(patrolRestHunt);
    }

    void Update()
    {
        // Calls Process() to start processing/running the nodes within the tree.
        if (treeStatus != Node.Status.SUCCESS)
        {
            Debug.Log("Processing tree.");
            treeStatus = tree.Process();
        }
    }

    private String GetLastAction()
    {
        return lastAction.ToString();
    }

    // FIXME: needs to incorporate patrol functionality and return SUCCESS
    private Node.Status Patrol()
    {
        // If the last action the cat performed was patrolling or eating, fail to execute node.
        String lastActionString = GetLastAction();
        if (lastActionString.Equals("PATROL") || lastActionString.Equals("EAT"))
        {
            Debug.Log("Patrol failed.");
            return Node.Status.FAILURE;
        }
        
        // If the player was recently spotted, fail to execute node.
        if (playerSpotted)
        {
            Debug.Log("Patrol failed.");
            return Node.Status.FAILURE;
        }
        
        // FIXME: temp code to test tree functionality.
        // Calculates the distance between the point and the cat.
        float distanceToPoint = Vector3.Distance(patrolPoints[0].transform.position, transform.position);
        
        // If the cat is idle, they will go to the point.
        if (state == ActionState.IDLE)
        {
            agent.SetDestination(patrolPoints[0].transform.position);
            state = ActionState.WORKING;
        }
        else if (Vector3.Distance(agent.pathEndPosition, patrolPoints[0].transform.position) >= agent.stoppingDistance)
        { // If the cat doesn't make it to the bed, the node fails.
            state = ActionState.IDLE;
            Debug.Log("Patrol failed.");
            return Node.Status.FAILURE;
        }
        else if (distanceToPoint <= agent.stoppingDistance)
        { // If the cat reaches the point, the node succeeds.
            state = ActionState.IDLE;
            lastAction = LastAction.PATROL;
            Debug.Log("Patrol was successful!");
            return Node.Status.SUCCESS;
        }
        
        //Debug.Log("Patrol is running...");
        return Node.Status.RUNNING;
    }

    // FIXME: needs to make cat wait after arriving at bed
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
        else if (Vector3.Distance(agent.pathEndPosition, bed.transform.position) >= agent.stoppingDistance)
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

    private Node.Status Hunt()
    {
        if (!playerSpotted)
        {
            return Node.Status.FAILURE;
        }
        
        // Since the player has been spotted, the coroutine needs to stop & the cat's state must change.
        if (state != ActionState.HUNTING) // Ensures this only happens once.
        {
            state = ActionState.HUNTING;
        }

        // Calculates the direction the player is heading towards.
        Vector3 playerDirection = player.transform.position - transform.position;
        float relativeDestination =
            Vector3.Angle(transform.forward, transform.TransformVector(player.transform.forward));
        float angleToTarget = Vector3.Angle(transform.forward, transform.TransformVector(playerDirection));

        // If the player isn't moving, the cat will "pounce".
        if ((angleToTarget > 90 && relativeDestination < 20) || playerController.velocity.magnitude < 0.1f)
        {
            agent.SetDestination(player.transform.position);
            
            // FIXME: actually Do something here to check if player is caught by the cat. maybe specific collider?
            Debug.Log("The player has been caught!");
        }
        
        // If the player is moving, calculates and goes to where they're heading based on their speed.
        float lookAhead = playerDirection.magnitude/(playerSpeed) + playerController.velocity.magnitude;
        agent.SetDestination(player.transform.position + player.transform.forward * lookAhead);
        // Do something here to check if player is caught by the cat
        
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
