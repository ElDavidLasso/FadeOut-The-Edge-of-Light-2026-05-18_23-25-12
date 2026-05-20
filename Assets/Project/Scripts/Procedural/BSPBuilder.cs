using UnityEngine;

public class BSPBuilder
{
    private int minRoomSize;

    public BSPBuilder(int minRoomSize)
    {
        this.minRoomSize = minRoomSize;
    }

    public void SplitNode(NodeBSP node)
    {
        // Validación de parada: Si el nodo es muy pequeño para dividirse, lo dejamos como hoja.
        if (node.bounds.width < minRoomSize * 2 || node.bounds.height < minRoomSize * 2)
            return;

        // Decidimos la dirección del corte (horizontal o vertical)
        bool splitHorizontally = Random.value > 0.5f;

        // Forzamos un corte en la otra dirección si las proporciones son extremas
        if (node.bounds.width > node.bounds.height && node.bounds.width / node.bounds.height >= 1.25f)
            splitHorizontally = false;
        else if (node.bounds.height > node.bounds.width && node.bounds.height / node.bounds.width >= 1.25f)
            splitHorizontally = true;

        // Calculamos el punto de corte respetando el tamaño mínimo
        int max = (splitHorizontally ? node.bounds.height : node.bounds.width) - minRoomSize;
        if (max <= minRoomSize) return; // Fallback de seguridad

        int splitPoint = Random.Range(minRoomSize, max);

        // Instanciamos los hijos
        if (splitHorizontally)
        {
            node.leftChild = new NodeBSP(new RectInt(node.bounds.x, node.bounds.y, node.bounds.width, splitPoint));
            node.rightChild = new NodeBSP(new RectInt(node.bounds.x, node.bounds.y + splitPoint, node.bounds.width, node.bounds.height - splitPoint));
        }
        else
        {
            node.leftChild = new NodeBSP(new RectInt(node.bounds.x, node.bounds.y, splitPoint, node.bounds.height));
            node.rightChild = new NodeBSP(new RectInt(node.bounds.x + splitPoint, node.bounds.y, node.bounds.width - splitPoint, node.bounds.height));
        }

        // Recursividad
        SplitNode(node.leftChild);
        SplitNode(node.rightChild);
    }

        public void GenerateRoomsAndCorridors(NodeBSP node)
    {
        if (node.IsLeaf)
        {
            // Generar una habitación con padding dentro de los Bounds
            int roomWidth = Random.Range(minRoomSize, node.bounds.width - 1);
            int roomHeight = Random.Range(minRoomSize, node.bounds.height - 1);
            int roomX = node.bounds.x + Random.Range(1, node.bounds.width - roomWidth);
            int roomY = node.bounds.y + Random.Range(1, node.bounds.height - roomHeight);

            node.roomBounds = new RectInt(roomX, roomY, roomWidth, roomHeight);
        }
        else
        {
            // Recorrido Post-Orden: Ir hasta abajo primero
            GenerateRoomsAndCorridors(node.leftChild);
            GenerateRoomsAndCorridors(node.rightChild);

            // Al volver, conectar los hijos
            node.roomBounds = ConnectNodes(node);
        }
    }

    private RectInt ConnectNodes(NodeBSP node)
    {
        Vector2 leftCenter = node.leftChild.roomBounds.center;
        Vector2 rightCenter = node.rightChild.roomBounds.center;

        int minX = (int)Mathf.Min(leftCenter.x, rightCenter.x);
        int maxX = (int)Mathf.Max(leftCenter.x, rightCenter.x);
        int minY = (int)Mathf.Min(leftCenter.y, rightCenter.y);
        int maxY = (int)Mathf.Max(leftCenter.y, rightCenter.y);

        int corridorThickness = 2; // Grosor del pasillo

        // Retornamos un RectInt que encapsula los centros de ambas habitaciones
        // Para simplificar, en 2D el corredor se dibuja como una superposición de rectángulos
        if (Mathf.Abs(leftCenter.x - rightCenter.x) > Mathf.Abs(leftCenter.y - rightCenter.y))
        {
            // Conexión Horizontal
            node.Corridor = new RectInt(minX, minY, maxX - minX, corridorThickness);
        }
        else
        {
            // Conexión Vertical
            node.Corridor = new RectInt(minX, minY, corridorThickness, maxY - minY);
        }

        // Para la recursión, el nodo padre hereda una aproximación del área conectada
        return node.Corridor;
    }
}

