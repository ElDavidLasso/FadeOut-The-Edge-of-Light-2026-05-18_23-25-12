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

        int x1 = (int)leftCenter.x;
        int y1 = (int)leftCenter.y;
        int x2 = (int)rightCenter.x;
        int y2 = (int)rightCenter.y;

        int corridorThickness = 2; // Grosor del pasillo

        // 1. Trazar Pasillo Horizontal (desde el Centro 1 hasta la X del Centro 2)
        int minX = Mathf.Min(x1, x2);
        int maxX = Mathf.Max(x1, x2);
        node.Corridors.Add(new RectInt(minX, y1, (maxX - minX) + corridorThickness, corridorThickness));

        // 2. Trazar Pasillo Vertical (desde la Y del Centro 1 hasta el Centro 2)
        int minY = Mathf.Min(y1, y2);
        int maxY = Mathf.Max(y1, y2);
        node.Corridors.Add(new RectInt(x2, minY, corridorThickness, (maxY - minY) + corridorThickness));

        // Fix: Return a RectInt, not a List<RectInt>. 
        // Here, return the bounding rectangle that contains all corridors.
        int corridorXMin = Mathf.Min(node.Corridors[0].xMin, node.Corridors[1].xMin);
        int corridorYMin = Mathf.Min(node.Corridors[0].yMin, node.Corridors[1].yMin);
        int corridorXMax = Mathf.Max(node.Corridors[0].xMax, node.Corridors[1].xMax);
        int corridorYMax = Mathf.Max(node.Corridors[0].yMax, node.Corridors[1].yMax);
        return new RectInt(
            corridorXMin,
            corridorYMin,
            corridorXMax - corridorXMin,
            corridorYMax - corridorYMin
        );
    }
}

