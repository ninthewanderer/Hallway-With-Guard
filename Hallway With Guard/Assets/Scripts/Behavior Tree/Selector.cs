using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selector : Node
{
    public Selector(string n)
    {
        name = n;
    }

    public override Status Process()
    {
        // Starts by checking the status of the current child.
        Status childStatus = children[currentChild].Process();
        
        // If it is running, it continues to run.
        if (childStatus == Status.RUNNING)
        {
            return Status.RUNNING;
        }
        
        // If the child succeeded, stop there and pick that child.
        if (childStatus == Status.SUCCESS)
        {
            currentChild = 0;
            return Status.SUCCESS;
        }

        // If it failed, move onto the next child.
        currentChild++;

        /* If the currentChild count is higher than the amount of children this node has, the count is reset and the
         selector was unsuccessful. */
        if (currentChild >= children.Count)
        {
            currentChild = 0;
            return Status.FAILURE;
        }

        // If nothing else, the process returned will be "Running".
        return Status.RUNNING;
    }
}
