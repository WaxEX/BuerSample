using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FrostGlassPostProcessRendererFeature : ScriptableRendererFeature
{
    private FrostGlassPostProcess scriptablePass;

    public Shader BlurShader;
    public Shader WhiteShader;

    public override void Create()
    {
        this.name = "FrostGlass PostProcess";
        scriptablePass = new FrostGlassPostProcess(RenderPassEvent.BeforeRenderingPostProcessing, BlurShader, WhiteShader);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        scriptablePass.Setup(renderer.cameraColorTarget);
        renderer.EnqueuePass(scriptablePass);
    }

    class FrostGlassPostProcess : ScriptableRenderPass
    {
        public Material matBlurX;
        public Material matBlurY;
        public Material matWhite;

        private RenderTargetIdentifier currentTarget;

        private RenderTargetHandle tmpTargetA;
        private RenderTargetHandle tmpTargetB;

        public FrostGlassPostProcess(RenderPassEvent evt, Shader blurShader, Shader whiteShader)
        {
            matBlurX = CoreUtils.CreateEngineMaterial(blurShader);
            matBlurX.SetVector("_Dirction", new Vector2(1, 0));

            matBlurY = CoreUtils.CreateEngineMaterial(blurShader);
            matBlurY.SetVector("_Dirction", new Vector2(0, 1));

            matWhite = CoreUtils.CreateEngineMaterial(whiteShader);
            matWhite.SetColor("_Color", Color.white);

            tmpTargetA.Init("_tmpRenderTargetA");
            tmpTargetB.Init("_tmpRenderTargetB");

            renderPassEvent = evt;
        }


        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var frost = VolumeManager.instance.stack.GetComponent<FrostGlass>();
            if (frost == null || !frost.IsActive()) return;

            var cmd = CommandBufferPool.Get("_FrostGlassPostProcess");
            Render(cmd, ref renderingData, frost);
            context.ExecuteCommandBuffer(cmd);
            context.Submit();
            CommandBufferPool.Release(cmd);
        }

        public void Setup(RenderTargetIdentifier currentTarget)
        {
            this.currentTarget = currentTarget;
        }

        void Render(CommandBuffer cmd, ref RenderingData renderingData, FrostGlass frost)
        {
            ref var cameraData = ref renderingData.cameraData;
            var ratio = frost.ratio.value;

            var targetA = tmpTargetA.id;
            var targetB = tmpTargetB.id;

            // 途中描画用のtexture用意 ブラーかけるし解像度低くていいよね
            var w = cameraData.camera.scaledPixelWidth / 2;
            var h = cameraData.camera.scaledPixelHeight / 2;
            cmd.GetTemporaryRT(targetA, w, h, 0, FilterMode.Point, RenderTextureFormat.Default);
            cmd.GetTemporaryRT(targetB, w, h, 0, FilterMode.Point, RenderTextureFormat.Default);

            cmd.Blit(currentTarget, targetA);

            // 横にブラー
            matBlurX.SetFloat("_Ratio", ratio);
            cmd.Blit(targetA, targetB, matBlurX);

            // 縦にブラー
            matBlurY.SetFloat("_Ratio", ratio);
            cmd.Blit(targetB, targetA, matBlurY);

            // 白くする
            matWhite.SetFloat("_Ratio", ratio);
            cmd.Blit(targetA, currentTarget, matWhite);
        }
    }
}


