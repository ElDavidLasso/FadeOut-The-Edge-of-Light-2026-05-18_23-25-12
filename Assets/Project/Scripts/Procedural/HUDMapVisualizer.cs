using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(RawImage))]
public class HUDMapVisualizer : MonoBehaviour
{
    [Header("Configuración Visual (Basado en tus referencias)")]
    public Color backgroundColor = new Color(0, 0, 0, 0.7f); 
    public Color roomColor = new Color(0, 1, 0, 0.3f);      
    public Color corridorColor = new Color(0, 1, 1, 0.4f);  
    public Color wallColor = Color.white;                  
    public Color playerColor = Color.red;                  

    [Header("Ajustes Técnicos")]
    [Tooltip("Resolución de la textura del minimapa (no de la UI)")]
    [SerializeField] private int textureResolution = 256;
    [SerializeField] private int wallThickness = 1;

    
    private RawImage hudImage;
    private Texture2D mapTexture;
    private Color[] mapPixels;

    private int mapGridWidth;
    private int mapGridHeight;
    private float scaleFactor;

    
    private List<RectInt> roomBoundsOnMap = new List<RectInt>();

    private void Awake()
    {
        hudImage = GetComponent<RawImage>();
    }

    
    public void Initialize(int bspMapWidth, int bspMapHeight)
    {
        this.mapGridWidth = bspMapWidth;
        this.mapGridHeight = bspMapHeight;

        
        scaleFactor = (float)textureResolution / Mathf.Max(bspMapWidth, bspMapHeight);

        
        mapTexture = new Texture2D(textureResolution, textureResolution, TextureFormat.RGBA32, false);
        mapTexture.filterMode = FilterMode.Point; 
        mapTexture.wrapMode = TextureWrapMode.Clamp;

        mapPixels = new Color[textureResolution * textureResolution];
        hudImage.texture = mapTexture;

        ClearMap();
    }

    private void ClearMap()
    {
        for (int i = 0; i < mapPixels.Length; i++) mapPixels[i] = backgroundColor;
        roomBoundsOnMap.Clear();
    }

    
    public void DrawEntireMap(NodeBSP rootNode)
    {
        if (mapTexture == null) return;
        ClearMap();

        RecurseAndDraw(rootNode);

        
        mapTexture.SetPixels(mapPixels);
        mapTexture.Apply();
    }

    
    private void RecurseAndDraw(NodeBSP node)
    {
        if (node == null) return;

        
        RectInt mapRect = ScaleRect(node.roomBounds);

        if (node.IsLeaf)
        {
            
            mapPixels.DrawFilledRectangle(mapRect, roomColor, textureResolution, textureResolution);

            
            mapPixels.DrawOutlineRectangle(mapRect, wallColor, wallThickness, textureResolution, textureResolution);

            
            roomBoundsOnMap.Add(node.roomBounds);
        }
        else
        {
            
            if (node.Corridors != null)
            {
                foreach (RectInt corridor in node.Corridors)
                {
                    RectInt scaledCorridor = ScaleRect(corridor);
                    mapPixels.DrawFilledRectangle(scaledCorridor, corridorColor, textureResolution, textureResolution);
                }
            }

            
            RecurseAndDraw(node.leftChild);
            RecurseAndDraw(node.rightChild);
        }
    }

    
    public void UpdatePlayerPosition(Vector3 playerWorldPos, float bspTileSize)
    {
        if (mapGridWidth == 0) return;

        
        int playerGridX = Mathf.FloorToInt(playerWorldPos.x / bspTileSize);
        int playerGridY = Mathf.FloorToInt(playerWorldPos.z / bspTileSize);

       
        int texX = Mathf.FloorToInt(playerGridX * scaleFactor);
        int texY = Mathf.FloorToInt(playerGridY * scaleFactor);

        
        if (playerGridX >= 0 && playerGridX < mapGridWidth && playerGridY >= 0 && playerGridY < mapGridHeight)
        {
            MovePlayerUIElement(playerGridX, playerGridY);
        }
    }

    private GameObject playerUIIndicator;
    private void MovePlayerUIElement(int gridX, int gridY)
    {
        if (playerUIIndicator == null)
        {
            playerUIIndicator = new GameObject("PlayerIndicator", typeof(Image));
            playerUIIndicator.transform.SetParent(this.transform, false);
            playerUIIndicator.GetComponent<Image>().color = playerColor;
            RectTransform rt = playerUIIndicator.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.zero;
            rt.sizeDelta = new Vector2(4f, 4f);
        }

        
        float normX = (float)gridX / mapGridWidth;
        float normY = (float)gridY / mapGridHeight;

        
        RectTransform myRT = GetComponent<RectTransform>();
        float finalX = normX * myRT.rect.width;
        float finalY = normY * myRT.rect.height;

        playerUIIndicator.GetComponent<RectTransform>().anchoredPosition = new Vector2(finalX, finalY);
    }

    private RectInt ScaleRect(RectInt logicRect)
    {
        return new RectInt(
            Mathf.FloorToInt(logicRect.x * scaleFactor),
            Mathf.FloorToInt(logicRect.y * scaleFactor),
            Mathf.FloorToInt(logicRect.width * scaleFactor),
            Mathf.FloorToInt(logicRect.height * scaleFactor)
        );
    }
}
