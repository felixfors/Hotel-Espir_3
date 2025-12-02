using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class ShadowTest : MonoBehaviour
{
    public CustomPassVolume volume;
    private LuminanceProbeDebugPass probePass;

    void Start()
    {
        if (volume)
            probePass = volume.customPasses.OfType<LuminanceProbeDebugPass>().FirstOrDefault();
    }

    void Update()
    {
        if (probePass != null)
        {
            Debug.Log("Scene Luminance: " + probePass.luminance);
        }
    }
}
