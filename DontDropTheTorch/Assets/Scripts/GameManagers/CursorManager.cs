using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public Texture2D cursorTexture;

    private Vector2 cursorHotspot;

    void Start()
    {
        cursorHotspot = new Vector2(cursorTexture.width / 2, cursorTexture.height / 2);
        Cursor.SetCursor(cursorTexture, cursorHotspot, CursorMode.Auto);
    }

    public void UpdateCursorScaleAccordingToPlayersAccuracy(float accuracy)
    {
        // The worse player's accuracy is, wider the cursor
        float multiplyBy = 2 - accuracy; // 0 <= accuracy <= 1
        Cursor.SetCursor(cursorTexture, cursorHotspot * multiplyBy, CursorMode.Auto);
    }
}
