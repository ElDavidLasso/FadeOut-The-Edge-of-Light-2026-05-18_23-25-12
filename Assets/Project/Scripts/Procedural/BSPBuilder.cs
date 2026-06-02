using UnityEngine;

public class BSPBuilder
{
    private int minNodeSize;
    private int minRoomSize;

    public BSPBuilder(int minNodeSize, int minRoomSize)
    {
        this.minNodeSize = minNodeSize;
        this.minRoomSize = minRoomSize;
    }

    public void SplitNode(NodeBSP node)
    {
        
        if (node.bounds.width < minNodeSize * 2 && node.bounds.height < minNodeSize * 2)
            return;

        
        bool splitHorizontally = Random.value > 0.5f;

        if (node.bounds.width > node.bounds.height && (float)node.bounds.width / node.bounds.height >= 1.25f)
            splitHorizontally = false; 
        else if (node.bounds.height > node.bounds.width && (float)node.bounds.height / node.bounds.width >= 1.25f)
            splitHorizontally = true;  

        int maxSlice = (splitHorizontally ? node.bounds.height : node.bounds.width) - minNodeSize;
        if (maxSlice <= minNodeSize) return;

        int splitPoint = Random.Range(minNodeSize, maxSlice);

        
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

        
        SplitNode(node.leftChild);
        SplitNode(node.rightChild);
    }

    public void GenerateStructures(NodeBSP node)
    {
        if (node == null) return;

        if (node.IsLeaf)
        {
            
            int roomWidth = Random.Range(minRoomSize, node.bounds.width - 1);
            int roomHeight = Random.Range(minRoomSize, node.bounds.height - 1);

            int roomX = node.bounds.x + Random.Range(1, node.bounds.width - roomWidth);
            int roomY = node.bounds.y + Random.Range(1, node.bounds.height - roomHeight);

            node.roomBounds = new RectInt(roomX, roomY, roomWidth, roomHeight);
        }
        else
        {
            
            GenerateStructures(node.leftChild);
            GenerateStructures(node.rightChild);

            
            ConnectRooms(node);
        }
    }

    private void ConnectRooms(NodeBSP node)
    {
        Vector2 leftCenter = GetRoomCenter(node.leftChild);
        Vector2 rightCenter = GetRoomCenter(node.rightChild);

        int x1 = (int)leftCenter.x;
        int y1 = (int)leftCenter.y;
        int x2 = (int)rightCenter.x;
        int y2 = (int)rightCenter.y;

        
        int distanceX = Mathf.Abs(x1 - x2);
        int distanceY = Mathf.Abs(y1 - y2);
        int totalDistance = distanceX + distanceY;

        
        int thickness = (totalDistance > 4) ? 1 : 3;

        int minX = Mathf.Min(x1, x2);
        int maxX = Mathf.Max(x1, x2);
        node.Corridors.Add(new RectInt(minX, y1, (maxX - minX) + thickness, thickness));

        int minY = Mathf.Min(y1, y2);
        int maxY = Mathf.Max(y1, y2);
        node.Corridors.Add(new RectInt(x2, minY, thickness, (maxY - minY) + thickness));
    }

    private Vector2 GetRoomCenter(NodeBSP node)
    {
        
        if (node.IsLeaf) return node.roomBounds.center;
        return GetRoomCenter(node.leftChild ?? node.rightChild);
    }
}