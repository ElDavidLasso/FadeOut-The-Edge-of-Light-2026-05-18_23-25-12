using System.Collections.Generic;
using UnityEngine;

public class NodeBSP
{
    public RectInt bounds;       // El contenedor exterior (Sub-tarea 1.2)
    public RectInt roomBounds;   // El espacio jugable real (Sub-tarea 1.3)

    public NodeBSP leftChild;
    public NodeBSP rightChild;

    // Lista para soportar los tramos de pasillos en forma de "L"
    public List<RectInt> Corridors { get; set; } = new List<RectInt>();

    public NodeBSP(RectInt bounds)
    {
        this.bounds = bounds;
    }

    public bool IsLeaf => leftChild == null && rightChild == null;
}
