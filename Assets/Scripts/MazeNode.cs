/**
 * This code is based on the Maze Generation Algorithm implementation for Unity found at:
 * https://github.com/EvanatorM/Maze-Generation-Algorithm-Unity
 * 
 * Original Author: EvanatorM
 * Accessed on: 02.12.2023
 * 
 * Modifications made:
 * - Extended the `NodeState` enum to include `Start`, `Goal`, and `Highlighted` states for enhanced maze node state representation.
 * - Added a `neighbors` list to each maze node to keep track of adjacent nodes, facilitating pathfinding and neighbor management.
 * - Added a private `state` field of type `NodeState` to store the current state of the node.
 * - Added a `State` property with a getter and setter that automatically updates the node's color when the state changes.
 * - Added the `UpdateColor` method to handle the coloring of the new node states (`Start`, `Goal`, and `Highlighted`) in addition to the original states and desired theme.
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NodeState
{
    Available,
    Current,
    Completed,
    Start,  
    Goal,
    Highlighted
}

public class MazeNode : MonoBehaviour
{
    [SerializeField] GameObject[] walls;
    [SerializeField] MeshRenderer floor;

    public List<MazeNode> neighbors = new List<MazeNode>();

    public void RemoveWall(int wallToRemove)
    {
        walls[wallToRemove].gameObject.SetActive(false);
    }

    private NodeState state;

    public NodeState State
    {
        get { return state; }
        set
        {
            state = value;
            UpdateColor();
        }
    }

    private void UpdateColor()
    {

        MazeGenerator generator2 = FindObjectOfType<MazeGenerator>(); // Find the MazeGenerator in the scene.
        if (generator2 != null)
        {
            if (generator2.mazeTheme == 0)
            {
                switch (state)
                {
                    case NodeState.Available:
                        floor.material.color = Color.white;
                        break;
                    case NodeState.Current:
                        floor.material.color = new Color(0f, 0.10f, 0f, 1f);
                        break;
                    case NodeState.Completed:
                        floor.material.color = new Color(0f, 0.30f, 0f, 1f);
                        break;
                    case NodeState.Start:
                        floor.material.color = new Color(0f, 0.30f, 0f, 1f);
                        break;
                    case NodeState.Goal:
                        floor.material.color = Color.green;
                        break;
                    case NodeState.Highlighted:
                        floor.material.color = Color.red;
                        break;
                }
            }

            else if (generator2.mazeTheme == 1)
            {
                switch (state)
                {
                    case NodeState.Available:
                        floor.material.color = Color.white;
                        break;
                    case NodeState.Current:
                        floor.material.color = new Color(1.0f, 0.4314f, 0.7804f, 1f);
                        break;
                    case NodeState.Completed:
                        floor.material.color = Color.cyan;
                        break;
                    case NodeState.Start:
                        floor.material.color = Color.cyan;
                        break;
                    case NodeState.Goal:
                        floor.material.color = Color.green;
                        break;
                    case NodeState.Highlighted:
                        floor.material.color = new Color(1.0f, 0.4314f, 0.7804f, 1f);
                        break;
                }
            }
        }
    }

    public void SetState(NodeState newState)
    {
        State = newState;
    }
}
