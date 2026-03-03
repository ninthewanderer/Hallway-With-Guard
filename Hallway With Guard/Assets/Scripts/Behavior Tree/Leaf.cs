using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leaf : Node
{
    // Stores the method to be executed by the leaf node in Tick & stores a pointer to Tick with ProcessMethod.
    // Tick will essentially be one "frame" or "loop" within the behavior tree.
    public delegate Status Tick();
    public Tick ProcessMethod;
    
    // Leaf constructors.
    public Leaf() { }
    public Leaf(string name, Tick pm)
    {
        this.name = name;
        ProcessMethod = pm;
    }
    
    // "Processes" the leaf node and returns the status of its processed method.
    public override Status Process()
    {
        if (ProcessMethod != null)
        {
            return ProcessMethod();
        }
        else
        {
            return Status.FAILURE;
        }
    }
}
