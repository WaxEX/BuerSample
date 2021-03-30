using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FrostGlass : VolumeComponent, IPostProcessComponent
{
    [Range(0.0f, 1.0f), Tooltip("エフェクト強度")]
    public FloatParameter ratio = new FloatParameter(0.0f);

    public bool IsActive() => ratio.value > Mathf.Epsilon;
    public bool IsTileCompatible() => false;
}