namespace DesktopPet;

/// <summary>
/// 宠物全局配置：缩放、速度、动画参数
/// </summary>
public class PetSettings
{
    // 缩放
    public float Scale { get; set; } = 0.25f;
    public const float MinScale = 0.1f;
    public const float MaxScale = 2.0f;
    public const float ScaleStep = 0.1f;

    // 速度等级
    public enum SpeedLevel { Slow, Normal, Fast }
    public SpeedLevel SpeedLevelValue { get; set; } = SpeedLevel.Normal;

    private static readonly Dictionary<SpeedLevel, float> SpeedMap = new()
    {
        [SpeedLevel.Slow] = 1.0f,
        [SpeedLevel.Normal] = 3.0f,
        [SpeedLevel.Fast] = 6.0f,
    };

    public float Speed => SpeedMap[SpeedLevelValue];

    // 原始图片尺寸（缩小一倍）
    public int OriginalSize { get; set; } = 64;

    // 当前绘制尺寸
    public int PetSize => (int)(OriginalSize * Scale);

    // 闲置动画参数
    public double IdlePeriod { get; set; } = 1200;   // 弹跳周期 ms
    public double IdleAmplitude { get; set; } = 5;   // 弹跳幅度 px

    public void ScaleUp() => Scale = Math.Min(MaxScale, Scale + ScaleStep);
    public void ScaleDown() => Scale = Math.Max(MinScale, Scale - ScaleStep);
}
