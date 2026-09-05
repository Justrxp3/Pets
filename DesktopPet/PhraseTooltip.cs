using System.Drawing;
using System.Drawing.Drawing2D;

namespace DesktopPet;

/// <summary>
/// 短句气泡窗口：在宠物上方显示随机短句，自动淡出
/// </summary>
public class PhraseTooltip : Form
{
    private readonly string _phrase;
    private float _opacity = 0.95f;
    private int _fadeTicks;
    private readonly System.Windows.Forms.Timer _fadeTimer;

    public PhraseTooltip(string phrase, Point screenLocation, int petWidth)
    {
        _phrase = phrase;

        // 测量文字尺寸
        using var font = new Font("Microsoft YaHei UI", 11f, FontStyle.Bold);
        var textSize = TextRenderer.MeasureText(phrase, font);
        int padding = 24;
        Width = Math.Max(textSize.Width + padding, 120);
        Height = textSize.Height + padding;

        // 窗口样式
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        TopMost = true;
        Opacity = _opacity;
        BackColor = Color.White;
        TransparencyKey = Color.White;

        // 定位：居中于宠物上方
        int x = screenLocation.X + (petWidth - Width) / 2;
        int y = screenLocation.Y - Height - 8;

        // 边界修正
        var screen = Screen.FromPoint(screenLocation).WorkingArea;
        x = Math.Max(screen.Left, Math.Min(x, screen.Right - Width));
        y = Math.Max(screen.Top, Math.Min(y, screen.Bottom - Height));

        Location = new Point(x, y);

        // 淡出定时器
        _fadeTimer = new System.Windows.Forms.Timer { Interval = 50 };
        _fadeTimer.Tick += FadeTick;

        Shown += (_, _) =>
        {
            _fadeTimer.Start();
        };
    }

    private void FadeTick(object? sender, EventArgs e)
    {
        _fadeTicks++;
        _opacity -= 0.04f;

        if (_opacity <= 0)
        {
            _fadeTimer.Stop();
            Close();
            return;
        }

        Opacity = Math.Max(0, _opacity);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        int r = 12;
        var rect = new Rectangle(0, 0, Width - 1, Height - 1);

        // 圆角背景
        using var path = CreateRoundedRect(rect, r);
        using var brush = new SolidBrush(Color.FromArgb(240, 255, 240));
        g.FillPath(brush, path);

        // 边框
        using var pen = new Pen(Color.FromArgb(180, 220, 180), 1.5f);
        g.DrawPath(pen, path);

        // 文字
        using var font = new Font("Microsoft YaHei UI", 11f, FontStyle.Bold);
        using var textBrush = new SolidBrush(Color.FromArgb(80, 80, 80));
        var textSize = TextRenderer.MeasureText(_phrase, font);
        var textPos = new Point(
            (Width - textSize.Width) / 2,
            (Height - textSize.Height) / 2
        );
        TextRenderer.DrawText(g, _phrase, font, textPos, Color.FromArgb(80, 80, 80));
    }

    private static GraphicsPath CreateRoundedRect(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        int d = radius * 2;
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _fadeTimer.Stop();
        _fadeTimer.Dispose();
        base.OnFormClosing(e);
    }
}
