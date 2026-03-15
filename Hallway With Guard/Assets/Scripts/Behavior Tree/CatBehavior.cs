using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.UI;

/*
 3/15/2026 PROGRESS
 Completed Tasks:
 - Implemented cat animations. Woohoo!
*/

public class CatBehavior : MonoBehaviour
{
    // Cat's behavior tree.
    private BehaviorTree tree;
    
    // Tracks the cat's currently state in the behavior tree. (Idle by default).
    private enum ActionState { IDLE, PATROLLING, RESTING, LOOKING, HUNTING };
    private ActionState state = ActionState.IDLE;
    
    // Tracks the cat's last action in the behavior tree. (Rest by default).
    private enum LastAction { REST, PATROL, HUNT };
    private LastAction lastAction = LastAction.REST;
    
    // For debug purposes, tracks the current state of the behavior tree. (Running by default).
    private Node.Status treeStatus = Node.Status.RUNNING;
    
    // Used to store NavMesh info & Animation info.
    private NavMeshAgent agent;
    private Animator catAnimator;
    
    [Header("────── Player Tracking ──────")]
    // Variables needed to track the player.
    public GameObject player;
    private float playerSpeed;
    [System.NonSerialized] public bool playerSpotted =  false; 
    public float detectionDelay = 0.2f;
    public float huntCooldown = 5f;
    public float viewRadius;
    [Range(0, 360)] public float viewAngle;
    public LayerMask targetMask;
    public LayerMask obstacleMask;
    [Space(10)]
    
    [Header("────── Navigation ──────")]
    // Variables needed for navigation.
    public GameObject bed;
    public GameObject[] patrolPoints;
    private bool finishedPatrol = false;
    private bool finishedRest = false;
    private bool finishedLooking = false;
    public float patrolDelay = 5f;
    public float lookDelay = 5f;
    public float restDelay = 5f;
    public float rotateSpeed = 0.5f;
    public int rotateAmount = 90;
    public float catSpeed;
    public float catSpeedOffset = 5f;
    private bool isEndgame = false;
    [Space(10)]
    
    [Header("────── UI & Audio ──────")]
    // Image + sprites to change the indication UI.
    public Image eyes;
    public Sprite spottedSprite;
    public Sprite unspottedSprite;
    
    // Audio source & clips needed for playing sounds.
    private AudioSource catAudioSource;
    public AudioClip detectionSound;
    public AudioClip actionCue;
    private bool clipHasPlayed = false;
    
    // Bool to let the tree & certain coroutines to know when to stop running.
    [System.NonSerialized] public bool gameOver = false;

    void Start()
    {
        // Obtains needed components from the Inspector.
        agent = GetComponent<NavMeshAgent>();
        catAnimator = GetComponent<Animator>();
        catAudioSource = GetComponent<AudioSource>();
        
        // Obtains the player's CharacterController component & movement speed.
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
        
        // Starts the coroutines for constant player detection, detectionSound audio, and animation.
        StartCoroutine(Hunting());
        StartCoroutine(DetectionSound());
        StartCoroutine(Animate());
    }

    void Update()
    {
        // Calls Process() to start processing/running the nodes within the tree.
        if (!gameOver)
        {
            treeStatus = tree.Process();
            if (playerSpotted)
            {
                transform.LookAt(player.transform.position);
            }
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
                    eyes.sprite = spottedSprite;
                    agent.speed = playerSpeed + catSpeedOffset;
                    agent.acceleration = agent.speed;
                }
                else
                {
                    if (!isEndgame)
                    {
                        playerSpotted = false;
                        clipHasPlayed = false;
                        eyes.sprite = unspottedSprite;
                        agent.speed = catSpeed;
                        agent.acceleration = agent.speed;
                    }
                }
            }
            else
            {
                if (!isEndgame)
                {
                    eyes.sprite = unspottedSprite;
                    playerSpotted = false;
                    clipHasPlayed = false;
                }
            }
        }
        else if (playerSpotted && !isEndgame) 
        { // Ensures that playerSpotted won't be infinitely set to true after 1 loop of this coroutine.
            eyes.sprite = unspottedSprite;
            playerSpotted = false;
            clipHasPlayed = false;
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
    
    private Node.Status Hunt()
    {
        if (!playerSpotted)
        {
            if (lastAction == LastAction.HUNT)
            {
                StartCoroutine(LookAfterHunting());
            }
            
            return Node.Status.FAILURE;
        }
        
        // Since the player has been spotted, the cat's state must be HUNTING and their last action must be hunting.
        state = ActionState.HUNTING;
        lastAction = LastAction.HUNT;
        
        // Moves the cat towards the player.
        agent.SetDestination(player.transform.position); 
        
        // If the player is close enough to the cat, they have been caught.
        if (gameOver)
        {
            Debug.Log("Player has been caught!");
            return Node.Status.SUCCESS;
        }
        
        return Node.Status.RUNNING;
    }
    
    private IEnumerator Patrolling()
    {
        // The delay until the cat moves to the next patrol point.
        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(patrolDelay);
        
        // Begins iterating through the patrolPoints array.
        foreach (GameObject waypoint in patrolPoints)
        {
            // Ensures cat is set to patrolling state so the next walking animation plays.
            state = ActionState.PATROLLING;
            
            // Sets cat's path to the waypoint.
            agent.SetDestination(waypoint.transform.position);
            
            // Waits for the cat to arrive at the patrol point before continuing or ending its patrol.
            yield return new WaitUntil(() => Vector3.Distance(agent.transform.position, waypoint.transform.position) 
                                             <= agent.stoppingDistance);
            
            StartCoroutine(LookAround());
            yield return new WaitUntil(() => finishedLooking);
            finishedLooking = false;
            yield return wait;
        }

        // After the cat iterates through all of its patrol points, finishedPatrol is marked true.
        state = ActionState.PATROLLING;
        finishedPatrol = true;
        
        // Plays an audio cue to let the player know the cat has finished its patrol.
        catAudioSource.clip = actionCue;
        catAudioSource.PlayOneShot(actionCue);
    }

    private IEnumerator LookAround()
    {
        state = ActionState.LOOKING; 
        
        // Rotates the cat to the right.
        for (int i = 0; i < rotateAmount; i++)
        {
            transform.Rotate(0, rotateSpeed, 0);
            yield return 0;
        }

        yield return new WaitForSecondsRealtime(lookDelay);

        // Rotates the cat to the left.
        for (int i = 0; i < rotateAmount; i++)
        {
            transform.Rotate(0, rotateSpeed * -1, 0);
            yield return 0;
        }
        
        finishedLooking = true;
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
            state = ActionState.PATROLLING;
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
        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(restDelay);
        
        // Sets the cat's destination to its bed.
        agent.SetDestination(bed.transform.position);
        
        // Waits for the cat to get to the bed and "rest".
        yield return new WaitUntil(() => Vector3.Distance(agent.transform.position, bed.transform.position) 
                                         <= agent.stoppingDistance);
        
        // Rotates the cat 180 degrees to the left.
        state = ActionState.LOOKING;
        for (int i = 0; i < 180; i++)
        {
            transform.Rotate(0, (rotateSpeed * 2) * -1, 0);
            yield return 0;
        }
        
        yield return wait;

        // After the cat finishes resting, this lets Rest() know that Resting() was successful.
        finishedRest = true;
        
        // Plays an audio cue to let the player know the cat has finished its rest.
        catAudioSource.clip = actionCue;
        catAudioSource.PlayOneShot(actionCue);
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
            // state = ActionState.WORKING;
            state = ActionState.RESTING;
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

    private IEnumerator DetectionSound()
    {
        // Delay between each run of this coroutine to prevent Unity crashing.
        WaitForSeconds wait = new WaitForSeconds(0.2f);
        
        // Infinite loop to ensure this is constantly running.
        while (!gameOver)
        {
            yield return wait;
            
            // If the player is spotted and the audio clip hasn't played, it will play.
            if (playerSpotted && !clipHasPlayed)
            {
                catAudioSource.clip = detectionSound;
                catAudioSource.PlayOneShot(detectionSound);
                clipHasPlayed = true;
                
                // Ensures the clip doesn't repeatedly play and overlap with itself.
                yield return new WaitUntil(() => !catAudioSource.isPlaying);
            }
        }
    }

    private IEnumerator LookAfterHunting()
    {
        // The delay until the cat moves on from hunting.
        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(huntCooldown);

        // Ensures cat will only start looking around after arriving at its destination.
        yield return new WaitUntil(() => Vector3.Distance(agent.transform.position, agent.destination) 
                                         <= agent.stoppingDistance);
        StartCoroutine(LookAround());
        yield return new WaitUntil(() => finishedLooking);
        
        // Once the cat finishes looking around, finishedLooking is once again false and the cat can be idle.
        finishedLooking = false;
        yield return wait;
        state = ActionState.IDLE;
    }

    private IEnumerator Animate()
    {
        // Delay between each run of this coroutine to prevent Unity crashing.
        WaitForSeconds wait = new WaitForSeconds(0.2f);
        
        while (!gameOver)
        {
            yield return wait;
            
            if (state == ActionState.IDLE || state == ActionState.LOOKING)
            {
                // Plays the cat's Idle animation.
                bool isRunning = catAnimator.GetBool("isRunning");
                bool isWalking = catAnimator.GetBool("isWalking");
                catAnimator.SetBool("isRunning", false);
                catAnimator.SetBool("isWalking", false);
            }
            else if (state == ActionState.HUNTING)
            {
                // Plays the cat's Running animation.
                bool isRunning = catAnimator.GetBool("isRunning");
                bool isWalking = catAnimator.GetBool("isWalking");
                catAnimator.SetBool("isRunning", true);
                catAnimator.SetBool("isWalking", false);
            }
            else if (state == ActionState.PATROLLING || state == ActionState.RESTING)
            {
                // Plays the cat's Walking animation.
                bool isRunning = catAnimator.GetBool("isRunning");
                bool isWalking = catAnimator.GetBool("isWalking");
                catAnimator.SetBool("isRunning", false);
                catAnimator.SetBool("isWalking", true);
            }
        }

        yield return null;
    }

    public void endgameHunt()
    {
        // If the player crosses the collider in the room 2 doorway, this will trigger.
        isEndgame = true;
        
        // All running coroutines for possible current behaviors will stop.
        StopCoroutine(LookAround());
        StopCoroutine(Resting());
        StopCoroutine(Patrolling());
        
        // The cat will prioritize hunting down the player and will not stop chasing them.
        state = ActionState.HUNTING;
        playerSpotted = true; 
        eyes.sprite = spottedSprite;
        agent.speed = playerSpeed + catSpeedOffset;
        agent.acceleration = agent.speed;
        
        agent.SetDestination(player.transform.position);
    }
}