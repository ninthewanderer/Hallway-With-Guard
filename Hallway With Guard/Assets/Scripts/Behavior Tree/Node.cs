using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    // Used to track the status of the given node.
    public enum Status { SUCCESS, RUNNING, FAILURE };
    public Status status;
    
    // Used to track the children of the given node.
    public List<Node> children = new List<Node>();
    public int currentChild = 0;
    
    // For debugging purposes, the name every node will have.
    public string name;

    // Node constructors.
    public Node() { }
    public Node(string name)
    {
        this.name = name;
    }
    
    // Returns the status of the node after processing the child's method.
    public virtual Status Process()
    {
        return children[currentChild].Process();
    }
    
    // Method for adding in a child.
    public void AddChild(Node node)
    {
        children.Add(node);
    }
}
