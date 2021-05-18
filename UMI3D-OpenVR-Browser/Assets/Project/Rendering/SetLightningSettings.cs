using UnityEngine;

public class SetLightningSettings : MonoBehaviour
{
    private static SetLightningSettings instance;

    public Material defaultSkyboxMat;

    public Material connectionSkyboxMat;
    [ColorUsage(true, true)]
    public Color connectionAmbientColor;
    void Start()
    {
        instance = this;
        SetConnectionSceneSettings();
    }


    /// <summary>
    /// Resets environements which were added for the connection scene to match with the default server desktop scene.
    /// </summary>
    public static void ResetLightningSettings()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
        RenderSettings.ambientIntensity = 1;
        RenderSettings.fog = false;
        RenderSettings.skybox = instance?.defaultSkyboxMat;
    }

    private void SetConnectionSceneSettings()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = connectionAmbientColor;
        RenderSettings.fog = true;
        RenderSettings.skybox = instance?.connectionSkyboxMat;
    }
}
