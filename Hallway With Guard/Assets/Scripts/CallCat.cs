using System;
using UnityEngine;
using UnityEngine.AI;

public class CallCat : MonoBehaviour
{
    public GameObject cat;
    private CatBehavior catBehavior;

    void Start()
    {
        catBehavior = cat.GetComponent<CatBehavior>();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            catBehavior.endgameHunt();
        }
    }
}
