using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sequence : Node
{
    public Sequence(string n)
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

        // If it failed, the sequence has failed automatically.
        if (childStatus == Status.FAILURE)
        {
            return Status.FAILURE;
        }

        // If the current child is successful, move onto evaluating the next child in the sequence.
        currentChild++;

        /* If the currentChild count is higher than the amount of children this node has, the count is reset and the
         sequence was successful. */
        if (currentChild >= children.Count)
        {
            currentChild = 0;
            return Status.SUCCESS;
        }

        // If nothing else, the process returned will be "Running".
        return Status.RUNNING;
    }
}
