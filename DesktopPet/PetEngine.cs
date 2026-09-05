using System.Drawing;

namespace DesktopPet;

/// <summary>
/// 宠物行为状态机：管理 Walking / Idle 状态切换、位置计算、闲置动画
/// </summary>
public class PetEngine
{
    public enum PetState { Walking, Idle }

    private readonly PetSettings _settings;
    private readonly Random _rng = new();
    private readonly Rectangle _screenBounds;

    private PetState _state = PetState.Idle;
    private PointF _position;
    private PointF _target;
    private int _idleDuration;
    private double _idleElapsed;
    private bool _facingLeft;

    public PetState State => _state;
    public PointF Position => _position;
    public bool FacingLeft => _facingLeft;

    public PetEngine(PetSettings settings, PointF startPos)
    {
        _settings = settings;
        _position = startPos;
        _screenBounds = Screen.PrimaryScreen!.WorkingArea;
        _idleDuration = NextIdleDuration();
    }

    /// <summary>每帧调用：更新位置和状态</summary>
    public void Update()
    {
        switch (_state)
        {
            case PetState.Walking:
                UpdateWalking();
                break;
            case PetState.Idle:
                UpdateIdle();
                break;
        }
    }

    private void UpdateWalking()
    {
        float dx = _target.X - _position.X;
        float dy = _target.Y - _position.Y;
        float dist = (float)Math.Sqrt(dx * dx + dy * dy);

        if (dist < _settings.Speed)
        {
            _position = _target;
            SwitchToIdle();
            return;
        }

        _position.X += dx / dist * _settings.Speed;
        _position.Y += dy / dist * _settings.Speed;
        _facingLeft = dx < 0;
    }

    private void UpdateIdle()
    {
        _idleElapsed += 33.33; // ~30fps
        if (_idleElapsed >= _idleDuration)
        {
            SwitchToWalking();
        }
    }

    /// <summary>获取当前帧的 Y 轴偏移（闲置弹跳动画）</summary>
    public int GetIdleOffsetY()
    {
        if (_state != PetState.Idle) return 0;

        double progress = _idleElapsed / _settings.IdlePeriod;
        double wave = Math.Sin(progress * 2 * Math.PI);
        return (int)(wave * _settings.IdleAmplitude);
    }

    private void SwitchToIdle()
    {
        _state = PetState.Idle;
        _idleElapsed = 0;
        _idleDuration = NextIdleDuration();
    }

    private void SwitchToWalking()
    {
        _state = PetState.Walking;
        _idleElapsed = 0;

        int margin = _settings.PetSize + 20;
        _target = new PointF(
            _rng.Next(margin, _screenBounds.Width - margin),
            _rng.Next(margin, _screenBounds.Height - margin)
        );
    }

    private int NextIdleDuration() => _rng.Next(3000, 8000);
}
