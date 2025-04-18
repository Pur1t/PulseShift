using UnityEngine;
using System.Collections;

public class PrefabToSprite : MonoBehaviour
{
    public int imageSize = 512; // Resolution of the output sprite

    public Sprite CapturePrefabAsSprite(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("Prefab is null! Cannot capture sprite.");
            return null;
        }

        // Create a temporary camera
        GameObject camObj = new GameObject("TempCamera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.clear; // Transparent background
        cam.orthographic = true;
        cam.orthographicSize = 5;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 100f;

        // Create a RenderTexture to render the prefab
        RenderTexture renderTex = new RenderTexture(imageSize, imageSize, 24);
        cam.targetTexture = renderTex;

        // Instantiate the prefab in the scene
        GameObject instance = Instantiate(prefab, Vector3.zero, Quaternion.identity);

        // Calculate the prefab bounds and adjust camera position
        Bounds bounds = CalculateBounds(instance);
        cam.transform.position = bounds.center + new Vector3(0, 0, -10);
        cam.orthographicSize = bounds.extents.y * 1.5f; // Adjust size to fit the object

        // Render the prefab to texture
        cam.Render();

        // Convert the RenderTexture to Texture2D
        RenderTexture.active = renderTex;
        Texture2D texture = new Texture2D(imageSize, imageSize, TextureFormat.ARGB32, false);
        texture.ReadPixels(new Rect(0, 0, imageSize, imageSize), 0, 0);
        texture.Apply();

        // Cleanup
        RenderTexture.active = null;
        cam.targetTexture = null;
        Destroy(camObj);
        Destroy(instance);

        // Convert Texture2D to Sprite
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    private Bounds CalculateBounds(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            return new Bounds(obj.transform.position, Vector3.zero);
        }

        Bounds bounds = renderers[0].bounds;
        foreach (Renderer renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }
        return bounds;
    }
}