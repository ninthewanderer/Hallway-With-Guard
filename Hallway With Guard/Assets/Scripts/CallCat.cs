using System;
using UnityEngine;
using UnityEngine.AI;

public class CallCat : MonoBehaviour
{
    public GameObject cat;
    private CatBehavior catBehavior;

    void Start()
    {
        // Gets the script for the cat so that it can call endgameHunt().
        catBehavior = cat.GetComponent<CatBehavior>();
    }
    
    // If the player enters this collider, the endgameHunt() method will trigger.
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            catBehavior.endgameHunt();
        }
    }
}
