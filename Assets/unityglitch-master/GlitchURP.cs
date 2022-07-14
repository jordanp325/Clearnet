using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GlitchURP : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {
        public RenderTargetIdentifier source;
        public GlitchURP parent;
        private float _glitchup;
        private float _glitchdown;
        private float flicker;
        private float _glitchupTime = 0.05f;
        private float _glitchdownTime = 0.05f;
        private float _flickerTime = 0.5f;
        private Material _material;

        float intensity;

        float colorIntensity;

        float flipIntensity;
        RenderTargetHandle temp;
        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in an performance manner.
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            temp.Init("_temp");
            _material = new Material(parent.Shader);
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cb = CommandBufferPool.Get();
            cb.GetTemporaryRT(temp.id, renderingData.cameraData.cameraTargetDescriptor);
            
            //all the fun stuff
            {
                intensity = ConsoleManager.glitchIntensity;
                flipIntensity = ConsoleManager.flipIntensity;
                colorIntensity = ConsoleManager.colorIntensity;

                _material.SetFloat("_Intensity", intensity);
                _material.SetFloat("_ColorIntensity", colorIntensity);
                _material.SetTexture("_DispTex", parent.displacementMap);

                flicker += Time.deltaTime * colorIntensity;
                if (flicker > _flickerTime)
                {
                    _material.SetFloat("filterRadius", Random.Range(-3f, 3f) * colorIntensity);
                    _material.SetVector("direction", Quaternion.AngleAxis(Random.Range(0, 360) * colorIntensity, Vector3.forward) * Vector4.one);
                    flicker = 0;
                    _flickerTime = Random.value;
                }

                if (colorIntensity == 0)
                    _material.SetFloat("filterRadius", 0);

                _glitchup += Time.deltaTime * flipIntensity;
                if (_glitchup > _glitchupTime)
                {
                    if (Random.value < 0.1f * flipIntensity)
                        _material.SetFloat("flip_up", Random.Range(0, 1f) * flipIntensity);
                    else
                        _material.SetFloat("flip_up", 0);

                    _glitchup = 0;
                    _glitchupTime = Random.value / 10f;
                }

                if (flipIntensity == 0)
                    _material.SetFloat("flip_up", 0);

                _glitchdown += Time.deltaTime * flipIntensity;
                if (_glitchdown > _glitchdownTime)
                {
                    if (Random.value < 0.1f * flipIntensity)
                        _material.SetFloat("flip_down", 1 - Random.Range(0, 1f) * flipIntensity);
                    else
                        _material.SetFloat("flip_down", 1);

                    _glitchdown = 0;
                    _glitchdownTime = Random.value / 10f;
                }

                if (flipIntensity == 0)
                    _material.SetFloat("flip_down", 1);

                if (Random.value < 0.05 * intensity)
                {
                    _material.SetFloat("displace", Random.value * intensity);
                    _material.SetFloat("scale", 1 - Random.value * intensity);
                }
                else
                    _material.SetFloat("displace", 0);
            }

            Blit(cb, source, temp.Identifier(), _material);
            Blit(cb, temp.Identifier(), source);

            context.ExecuteCommandBuffer(cb);
            cb.Release();
        }

        /// Cleanup any allocated resources that were created during the execution of this render pass.
        public override void FrameCleanup(CommandBuffer cmd)
        {
        }
    }

    public Texture2D displacementMap;
	public Shader Shader;
	[Header("Glitch Intensity")]

	// [Range(0, 1)]
	// public float intensity;

	// [Range(0, 1)]
	// public float flipIntensity;

	// [Range(0, 1)]
	// public float colorIntensity;
    CustomRenderPass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass();

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        m_ScriptablePass.parent = this;
        m_ScriptablePass.source = renderer.cameraColorTarget;
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


