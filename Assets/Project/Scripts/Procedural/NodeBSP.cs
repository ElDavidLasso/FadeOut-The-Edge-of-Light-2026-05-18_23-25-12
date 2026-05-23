using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeBSP 
{
    public RectInt bounds; //Espacio total que ocupa este nodo
    public RectInt roomBounds; //Espacio real de la habitación,(siempre más pequeña o igual a Bounds)
    public NodeBSP leftChild, rightChild; //Hijos del árbol binario
    public System.Collections.Generic.List<RectInt> Corridors { get; set; } = new System.Collections.Generic.List<RectInt>();
    public NodeBSP(RectInt bounds) { //Constructor
        this.bounds = bounds; 
    }
    // Patrón de conveniencia: ¿Es un nodo final (cuarto) o uno de división?
    // Vital para la etapa de instanciación y creación de pasillos.
    public bool IsLeaf => leftChild == null && rightChild == null;// Un nodo es una hoja si no tiene hijos

}
