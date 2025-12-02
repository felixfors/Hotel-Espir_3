using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Experimental.Rendering;

public class LuminanceProbeDebugPass : CustomPass
{
    [Header("Downsample Settings")]
    public int size = 32;

    private RTHandle downsampleRT;

    [Header("Results (read-only)")]
    [Range(0, 1)]
    public float luminance;

    private AsyncGPUReadbackRequest readbackRequest;
    private bool pendingReadback = false;

    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        downsampleRT = RTHandles.Alloc(
            size, size,
            dimension: TextureDimension.Tex2D,
            colorFormat: GraphicsFormat.R16G16B16A16_SFloat,
            useDynamicScale: false,
            name: "LuminanceProbeRT"
        );
    }

    protected override void Execute(CustomPassContext ctx)
    {
        // Blit kamerans buffert till liten RT
        HDUtils.BlitCameraTexture(ctx.cmd, ctx.cameraColorBuffer, downsampleRT);

        // AsyncGPUReadback
        if (!pendingReadback)
        {
            pendingReadback = true;
            readbackRequest = AsyncGPUReadback.Request(
                downsampleRT.rt, 0, TextureFormat.RGBAFloat, OnCompleteReadback
            );
        }

        // Debug: skriv RT:n direkt till kamerans färgbuffert
        HDUtils.BlitCameraTexture(ctx.cmd, downsampleRT, ctx.cameraColorBuffer);
    }

    private void OnCompleteReadback(AsyncGPUReadbackRequest req)
    {
        pendingReadback = false;
        if (req.hasError || !req.done) return;

        var data = req.GetData<Color>();
        if (data.Length == 0) return;

        double sum = 0.0;
        for (int i = 0; i < data.Length; i++)
        {
            Color c = data[i];
            sum += 0.2126 * c.r + 0.7152 * c.g + 0.0722 * c.b;
        }

        luminance = (float)(sum / data.Length);
    }

    protected override void Cleanup()
    {
        RTHandles.Release(downsampleRT);
    }
}
