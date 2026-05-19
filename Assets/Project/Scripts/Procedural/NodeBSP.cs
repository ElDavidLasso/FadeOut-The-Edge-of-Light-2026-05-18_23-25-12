using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeBSP 
{
    public RectInt bounds; //Espacio total que ocupa este nodo
    public RectInt roomBounds; //Espacio real de la habitación, que será más pequeño que el bounds para dejar espacio a los pasillos
    public NodeBSP leftChild, rightChild; //Referencias de divisiones
    public NodeBSP(RectInt bounds) { //Constructor
        this.bounds = bounds; 
    }
    // Patrón de conveniencia: ¿Es un nodo final (cuarto) o uno de división?
    // Vital para la etapa de instanciación y creación de pasillos.
    public bool IsLeaf => leftChild == null && rightChild == null;
    public bool Split(int minRoomSize)
    {
        if (!IsLeaf) return false;

        bool splitH = UnityEngine.Random.value > 0.5f;
        if (bounds.width > bounds.height && (float)bounds.width / bounds.height >= 1.25f) splitH = false;
        else if (bounds.height > bounds.width && (float)bounds.height / bounds.width >= 1.25f) splitH = true;

        int max = (splitH ? bounds.height : bounds.width) - minRoomSize;
        if (max <= minRoomSize) return false;

        int split = UnityEngine.Random.Range(minRoomSize, max);

        if (splitH)
        {
            leftChild = new NodeBSP(new RectInt(bounds.x, bounds.y, bounds.width, split));
            rightChild = new NodeBSP(new RectInt(bounds.x, bounds.y + split, bounds.width, bounds.height - split));
        }
        else
        {
            leftChild = new NodeBSP(new RectInt(bounds.x, bounds.y, split, bounds.height));
            rightChild = new NodeBSP(new RectInt(bounds.x + split, bounds.y, bounds.width - split, bounds.height));
        }
        return true;
    }
}
