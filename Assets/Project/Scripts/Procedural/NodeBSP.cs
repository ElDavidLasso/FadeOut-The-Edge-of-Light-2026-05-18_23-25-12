using System.Collections.Generic;
using UnityEngine;

public class NodeBSP
{
    public RectInt bounds;       
    public RectInt roomBounds;   

    public NodeBSP leftChild;
    public NodeBSP rightChild;

    
    public List<RectInt> Corridors { get; set; } = new List<RectInt>();

    public NodeBSP(RectInt bounds)
    {
        this.bounds = bounds;
    }

    public bool IsLeaf => leftChild == null && rightChild == null;
}
