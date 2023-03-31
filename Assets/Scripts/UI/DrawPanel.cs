using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DrawPanel : MonoBehaviour {

    public Color brushColor = Color.black;
    public float brushSize = 10f;

    private bool isDrawing = false;
    private Vector2 lastPos = Vector2.zero;
    private List<Vector2> positions = new List<Vector2>();
    private Texture2D texture;

    void Start () {
        Image image = GetComponent<Image>();
        texture = new Texture2D(
            (int)image.rectTransform.rect.width,
            (int)image.rectTransform.rect.height,
            TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        image.sprite = Sprite.Create(
            texture, new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f));
        Clear();
    }

    void Update () {
        if (Input.GetMouseButtonDown(0)) {
            isDrawing = true;
            lastPos = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(0)) {
            isDrawing = false;
            lastPos = Vector2.zero;
            positions.Clear();
        }
        if (isDrawing) {
            Vector2 currentPos = Input.mousePosition;
            if (Vector2.Distance(currentPos, lastPos) > brushSize / 10f) {
                positions.Add(currentPos);
                lastPos = currentPos;
                DrawBrush();
            }
        }
    }

    void DrawBrush () {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            GetComponent<RectTransform>(), lastPos, null, out Vector2 localPoint);
        Color[] pixels = texture.GetPixels();
        int width = texture.width;
        int height = texture.height;
        int centerX = Mathf.RoundToInt(localPoint.x + width / 2);
        int centerY = Mathf.RoundToInt(localPoint.y + height / 2);
        for (int x = centerX - Mathf.RoundToInt(brushSize / 2f);
             x < centerX + Mathf.RoundToInt(brushSize / 2f); x++) {
            for (int y = centerY - Mathf.RoundToInt(brushSize / 2f);
                 y < centerY + Mathf.RoundToInt(brushSize / 2f); y++) {
                if (x >= 0 && x < width && y >= 0 && y < height) {
                    int index = y * width + x;
                    float distance = Vector2.Distance(
                        new Vector2(x, y),
                        new Vector2(centerX, centerY)) / (brushSize / 2f);
                    Color pixelColor = pixels[index];
                    Color blendColor = Color.Lerp(pixelColor, brushColor, distance);
                    pixels[index] = blendColor;
                }
            }
        }
        texture.SetPixels(pixels);
        texture.Apply();
    }

    public void Clear () {
        Color[] pixels = texture.GetPixels();
        for (int i = 0; i < pixels.Length; i++) {
            pixels[i] = Color.white;
        }
        texture.SetPixels(pixels);
        texture.Apply();
    }

}