using System;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityMcpBridge.Editor.Helpers;

namespace UnityMcpBridge.Editor.Tools
{
    /// <summary>
    /// Captures screenshots of the Game or Scene view and returns them as base64 strings.
    /// </summary>
    public static class CaptureView
    {
        /// <summary>
        /// Main handler for view capture actions.
        /// </summary>
        public static object HandleCommand(JObject @params)
        {
            string action = @params["action"]?.ToString().ToLower() ?? "capture";
            string target = @params["target"]?.ToString()?.ToLower() ?? "game";

            switch (action)
            {
                case "capture":
                    return Capture(target);
                default:
                    return Response.Error($"Unknown action: '{action}'. Valid action is 'capture'.");
            }
        }

        private static object Capture(string target)
        {
            try
            {
                Texture2D screenshot = null;

                if (target == "scene")
                {
                    SceneView sceneView = SceneView.lastActiveSceneView;
                    if (sceneView == null)
                    {
                        return Response.Error("No active Scene view found.");
                    }

                    Camera cam = sceneView.camera;
                    int width = Mathf.RoundToInt(sceneView.position.width);
                    int height = Mathf.RoundToInt(sceneView.position.height);
                    RenderTexture rt = new RenderTexture(width, height, 24);
                    cam.targetTexture = rt;
                    cam.Render();
                    RenderTexture.active = rt;
                    screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);
                    screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                    screenshot.Apply();
                    cam.targetTexture = null;
                    RenderTexture.active = null;
                    UnityEngine.Object.DestroyImmediate(rt);
                }
                else // default to game view
                {
                    screenshot = ScreenCapture.CaptureScreenshotAsTexture();
                }

                if (screenshot == null)
                {
                    return Response.Error("Failed to capture screenshot.");
                }

                byte[] pngData = screenshot.EncodeToPNG();
                string base64 = Convert.ToBase64String(pngData);
                int widthOut = screenshot.width;
                int heightOut = screenshot.height;
                UnityEngine.Object.DestroyImmediate(screenshot);

                return Response.Success(
                    "Captured screenshot.",
                    new { base64 = base64, width = widthOut, height = heightOut }
                );
            }
            catch (Exception e)
            {
                return Response.Error($"Capture failed: {e.Message}");
            }
        }
    }
}
