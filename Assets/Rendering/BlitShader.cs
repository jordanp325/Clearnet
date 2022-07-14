using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(BlitShaderRenderer), PostProcessEvent.AfterStack, "BlitShader")]
public sealed class BlitShader : PostProcessEffectSettings
{
    // [Range(0f, 1f), Tooltip("Grayscale effect intensity.")]
    public ColorParameter colorAdded = new ColorParameter();
}

public sealed class BlitShaderRenderer : PostProcessEffectRenderer<BlitShader>
{
    public override void Init(){
        
    }
    public override void Render(PostProcessRenderContext context)
    {
        var sheet = context.propertySheets.Get(Shader.Find("Shader Graphs/SquareShiftShader"));
        sheet.properties.SetColor("Add_color", settings.colorAdded);
        // Texture t = new Texture2D(context.camera.pixelWidth, context.camera.pixelHeight);
        // context.command.Blit(context.source, t, );
        // context.command.Blit(t, context.destination, ));
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}