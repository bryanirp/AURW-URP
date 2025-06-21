using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering;

namespace aurw
{
    public class AURW_SceneColorFeature : ScriptableRendererFeature
    {
        [SerializeField]
        private RenderPassEvent renderEvent = RenderPassEvent.AfterRenderingSkybox;

        [SerializeField, Range(0, 4)]
        private int qualityLevel = 0; // 0 = full res, 1 = half, 2 = quarter...

        class RefractionSceneColorPass : ScriptableRenderPass
        {
            private RTHandle _sceneColor;
            private string _globalTexName = "_AURW_RefractionSceneColor";
            private RenderTextureDescriptor _descriptor;
            private int _qualityLevel;

            public RefractionSceneColorPass(RenderPassEvent passEvent, int qualityLevel)
            {
                renderPassEvent = passEvent;
                _qualityLevel = qualityLevel;
            }

            public void Setup(RenderTextureDescriptor descriptor)
            {
                _descriptor = descriptor;
                _descriptor.depthBufferBits = 0;

                // Apply quality reduction
                int div = Mathf.Clamp(_qualityLevel, 0, 4);
                _descriptor.width = Mathf.Max(1, _descriptor.width >> div);
                _descriptor.height = Mathf.Max(1, _descriptor.height >> div);
            }

            [System.Obsolete]
            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                ConfigureInput(ScriptableRenderPassInput.Color);
                _sceneColor = RTHandles.Alloc(_descriptor, name: _globalTexName);
            }

            [System.Obsolete]
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (renderingData.cameraData.isSceneViewCamera)
                    return;

                CommandBuffer cmd = CommandBufferPool.Get("AURW_RefractionSceneColor");

                Blitter.BlitCameraTexture(
                    cmd,
                    renderingData.cameraData.renderer.cameraColorTargetHandle,
                    _sceneColor
                );

                cmd.SetGlobalTexture(_globalTexName, _sceneColor);

                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }

            public override void OnCameraCleanup(CommandBuffer cmd)
            {
                if (_sceneColor != null)
                {
                    RTHandles.Release(_sceneColor);
                    _sceneColor = null;
                }
            }
        }

        private RefractionSceneColorPass _pass;

        public override void Create()
        {
            _pass = new RefractionSceneColorPass(renderEvent, qualityLevel);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            _pass.Setup(desc);
            renderer.EnqueuePass(_pass);
        }
    }
}