using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DesktopPet;

public class PetForm : Form
{
    // ===== 字段 =====
    private readonly PetSettings _settings = new();
    private PetEngine? _engine;
    private PhraseTooltip? _tooltip;
    private NotifyIcon? _notifyIcon;
    private ContextMenuStrip? _contextMenu;

    private Image? _petRun1;
    private Image? _petRun2;
    private Image? _petIdle;
    private int _runFrame;
    private double _runFrameTimer;
    private bool _facingLeft;
    private bool _isMinimized;
    private bool _isClosing;

    private bool _dragging;
    private Point _dragStartPos;
    private Point _dragOffset;
    private System.Windows.Forms.Timer? _phraseTimer;

    public PetForm()
    {
        var exeDir = AppDomain.CurrentDomain.BaseDirectory;
        
        // 设置窗口属性
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        TopMost = true;
        StartPosition = FormStartPosition.Manual;
        TransparencyKey = Color.Gray; // 使用灰色作为透明键
        BackColor = Color.Gray;
        DoubleBuffered = true; // 启用双缓冲，减少闪烁
        
        try
        {
            LoadImages();
            
            // 验证图片是否加载成功
            if (_petIdle == null || _petRun1 == null || _petRun2 == null)
            {
                MessageBox.Show("图片加载失败！请确保 Resources/img 目录存在", 
                    "桌面宠物 - 错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw new Exception("图片加载失败");
            }

            _settings.OriginalSize = _petIdle.Width;
            
            // 设置窗口大小
            ClientSize = new Size(_settings.PetSize, _settings.PetSize);

            var screen = Screen.PrimaryScreen!.WorkingArea;
            _engine = new PetEngine(_settings, new PointF(screen.Width / 2f, screen.Height / 2f));
            _facingLeft = _engine.FacingLeft;

            _notifyIcon = new NotifyIcon { Text = "桌面宠物", Visible = false };
            using var bmp16 = new Bitmap(16, 16);
            using var g16 = Graphics.FromImage(bmp16);
            g16.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g16.DrawImage(_petIdle, 0, 0, 16, 16);
            _notifyIcon.Icon = Icon.FromHandle(bmp16.GetHicon());
            _notifyIcon.DoubleClick += (_, _) => RestoreFromTray();

            _contextMenu = CreateContextMenu();

            var timer = new System.Windows.Forms.Timer { Interval = 33 };
            timer.Tick += TimerTick;
            timer.Start();

            _phraseTimer = new System.Windows.Forms.Timer { Interval = 3000 };
            _phraseTimer.Tick += PhraseTimerTick;
            _phraseTimer.Start();

            Location = new Point((int)_engine.Position.X, (int)_engine.Position.Y);

            MouseDown += PetForm_MouseDown;
            MouseMove += PetForm_MouseMove;
            MouseUp += PetForm_MouseUp;
            Paint += PetForm_Paint;
            FormClosing += PetForm_FormClosing;

            Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show("初始化失败:\n" + ex.Message,
                "桌面宠物 - 错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            throw;
        }
    }

    // ===== 图片加载 =====

    private void LoadImages()
    {
        var exeDir = AppDomain.CurrentDomain.BaseDirectory;
        var imgDir = Path.Combine(exeDir, "Resources", "img");
        var logFile = Path.Combine(exeDir, "debug.log");
        
        File.AppendAllText(logFile, $"\n=== LoadImages ===\n");
        File.AppendAllText(logFile, $"imgDir: {imgDir}\n");
        File.AppendAllText(logFile, $"exists: {Directory.Exists(imgDir)}\n");

        try
        {
            if (Directory.Exists(imgDir))
            {
                var idlePath = Path.Combine(imgDir, "正面.png");
                var run1Path = Path.Combine(imgDir, "奔跑1.png");
                var run2Path = Path.Combine(imgDir, "奔跑2.png");
                
                File.AppendAllText(logFile, $"Files: 正面={File.Exists(idlePath)} 奔跑1={File.Exists(run1Path)} 奔跑2={File.Exists(run2Path)}\n");
                
                if (File.Exists(idlePath)) 
                {
                    _petIdle = Image.FromFile(idlePath);
                    File.AppendAllText(logFile, $"Loaded 正面.png: {_petIdle.Width}x{_petIdle.Height}\n");
                }
                if (File.Exists(run1Path)) 
                {
                    _petRun1 = Image.FromFile(run1Path);
                    File.AppendAllText(logFile, $"Loaded 奔跑1.png: {_petRun1.Width}x{_petRun1.Height}\n");
                }
                if (File.Exists(run2Path)) 
                {
                    _petRun2 = Image.FromFile(run2Path);
                    File.AppendAllText(logFile, $"Loaded 奔跑2.png: {_petRun2.Width}x{_petRun2.Height}\n");
                }
            }
        }
        catch (Exception ex)
        {
            File.AppendAllText(logFile, $"Error: {ex.Message}\n");
        }

        // 回退到 pet.png
        if (_petIdle == null)
        {
            var localPath = Path.Combine(exeDir, "Resources", "pet.png");
            if (File.Exists(localPath))
            {
                _petIdle = Image.FromFile(localPath);
                _petRun1 = _petIdle;
                _petRun2 = _petIdle;
                File.AppendAllText(logFile, $"Using pet.png: {_petIdle.Width}x{_petIdle.Height}\n");
            }
        }
        
        File.AppendAllText(logFile, $"Final: idle={_petIdle != null} run1={_petRun1 != null} run2={_petRun2 != null}\n");
    }

    // ===== 渲染 =====

    private void PetForm_Paint(object? sender, PaintEventArgs e)
    {
        if (_engine == null) return;

        e.Graphics.Clear(Color.Gray); // 使用灰色，会被透明键隐藏
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

        int size = _settings.PetSize;
        int offsetY = _engine.GetIdleOffsetY();

        Image? displayImage = GetDisplayImage();
        if (displayImage == null) return;

        // 镜像：图片朝左，向右走时翻转
        if (!_facingLeft)
        {
            e.Graphics.TranslateTransform(size, 0);
            e.Graphics.ScaleTransform(-1, 1);
        }

        e.Graphics.DrawImage(displayImage, 0, offsetY, size, size);

        if (!_facingLeft)
            e.Graphics.ResetTransform();
    }

    private Image? GetDisplayImage()
    {
        if (_engine == null) return _petIdle;

        if (_engine.State == PetEngine.PetState.Walking)
        {
            _runFrameTimer += 33.33;
            if (_runFrameTimer >= 100)
            {
                _runFrameTimer = 0;
                _runFrame = (_runFrame + 1) % 2;
            }
            return _runFrame == 0 ? _petRun1 : _petRun2;
        }
        else
        {
            return _petIdle;
        }
    }

    // ===== 动画循环 =====

    private void TimerTick(object? sender, EventArgs e)
    {
        if (_isMinimized || _engine == null) return;

        _engine.Update();
        _facingLeft = _engine.FacingLeft;

        var pos = _engine.Position;
        Location = new Point((int)pos.X, (int)pos.Y);

        Invalidate(); // 触发重绘
    }

    // ===== 定时提示语 =====

    private void PhraseTimerTick(object? sender, EventArgs e)
    {
        if (_isMinimized || _engine == null) return;
        ShowPhraseTooltip();
    }

    // ===== 交互 =====

    private void PetForm_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            _dragStartPos = e.Location;
            _dragging = false;
        }
        else if (e.Button == MouseButtons.Right)
        {
            _contextMenu?.Show(this, e.Location);
        }
    }

    private void PetForm_MouseMove(object? sender, MouseEventArgs e)
    {
        if (_dragging)
        {
            var newLoc = PointToScreen(new Point(e.X - _dragOffset.X, e.Y - _dragOffset.Y));
            Location = newLoc;

            if (_engine != null)
                _engine = new PetEngine(_settings, new PointF(Location.X, Location.Y));
        }
        else if (_dragStartPos != Point.Empty)
        {
            var dx = e.X - _dragStartPos.X;
            var dy = e.Y - _dragStartPos.Y;
            if (dx * dx + dy * dy > 25)
            {
                _dragging = true;
                _dragOffset = _dragStartPos;
            }
        }
    }

    private void PetForm_MouseUp(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            if (!_dragging)
            {
                ShowPhraseTooltip();
            }
            _dragging = false;
            _dragStartPos = Point.Empty;
        }
    }

    private void ShowPhraseTooltip()
    {
        _tooltip?.Close();
        _tooltip = new PhraseTooltip(
            PhraseManager.GetRandom(),
            PointToScreen(Point.Empty),
            _settings.PetSize
        );
        _tooltip.Show(this);
    }

    // ===== 右键菜单 =====

    private ContextMenuStrip CreateContextMenu()
    {
        var menu = new ContextMenuStrip();

        menu.Items.Add(new ToolStripMenuItem("放大", null, (_, _) => { _settings.ScaleUp(); ApplyScale(); }));
        menu.Items.Add(new ToolStripMenuItem("缩小", null, (_, _) => { _settings.ScaleDown(); ApplyScale(); }));
        menu.Items.Add(new ToolStripSeparator());

        var speedMenu = new ToolStripMenuItem("速度");
        var slowItem = new ToolStripMenuItem("慢") { Checked = _settings.SpeedLevelValue == PetSettings.SpeedLevel.Slow };
        var normalItem = new ToolStripMenuItem("正常") { Checked = _settings.SpeedLevelValue == PetSettings.SpeedLevel.Normal };
        var fastItem = new ToolStripMenuItem("快") { Checked = _settings.SpeedLevelValue == PetSettings.SpeedLevel.Fast };

        slowItem.Click += (_, _) => SetSpeed(PetSettings.SpeedLevel.Slow, slowItem, normalItem, fastItem);
        normalItem.Click += (_, _) => SetSpeed(PetSettings.SpeedLevel.Normal, slowItem, normalItem, fastItem);
        fastItem.Click += (_, _) => SetSpeed(PetSettings.SpeedLevel.Fast, slowItem, normalItem, fastItem);

        speedMenu.DropDownItems.Add(slowItem);
        speedMenu.DropDownItems.Add(normalItem);
        speedMenu.DropDownItems.Add(fastItem);
        menu.Items.Add(speedMenu);

        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(new ToolStripMenuItem("最小化到托盘", null, (_, _) => MinimizeToTray()));
        menu.Items.Add(new ToolStripMenuItem("退出", null, (_, _) => ForceClose()));

        return menu;
    }

    private void SetSpeed(PetSettings.SpeedLevel level, ToolStripMenuItem slow, ToolStripMenuItem normal, ToolStripMenuItem fast)
    {
        _settings.SpeedLevelValue = level;
        slow.Checked = level == PetSettings.SpeedLevel.Slow;
        normal.Checked = level == PetSettings.SpeedLevel.Normal;
        fast.Checked = level == PetSettings.SpeedLevel.Fast;
    }

    private void ApplyScale() => ClientSize = new Size(_settings.PetSize, _settings.PetSize);

    // ===== 托盘 =====

    private void MinimizeToTray()
    {
        _isMinimized = true;
        Visible = false;
        if (_notifyIcon != null) _notifyIcon.Visible = true;
    }

    private void RestoreFromTray()
    {
        _isMinimized = false;
        if (_notifyIcon != null) _notifyIcon.Visible = false;
        Visible = true;
        BringToFront();
    }

    // ===== 生命周期 =====

    private void PetForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (!_isClosing) { e.Cancel = true; MinimizeToTray(); return; }

        _phraseTimer?.Stop();
        _phraseTimer?.Dispose();
        _notifyIcon?.Dispose();
        _tooltip?.Dispose();
        _petRun1?.Dispose();
        _petRun2?.Dispose();
        _petIdle?.Dispose();
    }

    private void ForceClose() { _isClosing = true; Close(); }
}
