using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviorTree : Node
{
    // Constructors for the tree.
    public BehaviorTree()
    {
        // The root node for the tree will have a distinct name to distinguish it by default.
        this.name = "Root";
    }

    public BehaviorTree(string name)
    {
        this.name = name;
    }
    
    // Processes the child node of the tree by calling its method and obtaining its status (successful, failure, etc).
    public override Status Process()
    {
        return children[currentChild].Process();
    }
    
    // A struct to store the level within the tree of the given node.
    struct NodeLevel
    {
        public int level;
        public Node node;
    }
    
    // Prints out the tree.
    public void PrintTree()
    {
        // treePrintout will hold the final string that will be printed.
        string treePrintout = "";
        
        // Stores a stack of all nodes, starting with the root of the tree.
        Stack<NodeLevel> nodeStack = new Stack<NodeLevel>();
        Node rootNode = this;
        nodeStack.Push(new NodeLevel { level = 0, node = rootNode } );
        
        // Loops through the stack while there is more than 1 node (which there always will be with the root).
        while (nodeStack.Count != 0)
        {
            // Stores the topmost node in the stack and adds it to the treePrintout string.
            NodeLevel currentNode = nodeStack.Pop();
            treePrintout += new string ('-', currentNode.level) + "> " + currentNode.node.name + "\n";

            // Loops through the current node's children (if any) starting from the rightmost child to the leftmost child.
            for (int i = currentNode.node.children.Count - 1; i >= 0; i--)
            {
                // Adds the child node to the node stack.
                nodeStack.Push(new NodeLevel { level = currentNode.level + 1, node = currentNode.node.children[i] } );
            }
        }
        
        // Prints out all nodes in the tree.
        Debug.Log(treePrintout);
    }
}
