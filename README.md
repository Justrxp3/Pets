# 桌面宠物 (Desktop Pet)

一个可爱的 Windows 桌面宠物应用，支持自定义素材、奔跑动画、拖拽交互等功能。

![桌面宠物](https://img.shields.io/badge/平台-Windows%2010/11-blue) ![.NET](https://img.shields.io/badge/.NET-8.0-purple)

## ✨ 功能特性

- 🐾 **桌面宠物**：在桌面上自由移动的可爱宠物
- 🎨 **自定义素材**：支持替换正面、奔跑动画图片
- 🔄 **奔跑动画**：两帧交替显示，模拟奔跑效果
- 🪞 **镜像翻转**：向左/向右移动时自动翻转图片方向
- 💬 **随机短句**：定时显示可爱短句，点击宠物也会显示
- 📏 **自由缩放**：右键菜单实时调整宠物大小（10%~200%）
- 🎯 **拖拽移动**：可以拖拽宠物到桌面任意位置
- 🗂️ **最小化托盘**：不使用时最小化到系统托盘
- 🎭 **透明背景**：完全透明的窗口，无背景色干扰
- ⚡ **单文件发布**：打包为单个 EXE，便于分发

## 📋 系统要求

### 运行环境
- **操作系统**：Windows 10 / Windows 11
- **内存**：至少 2GB RAM
- **磁盘空间**：至少 200MB（发布后）
- **.NET 运行时**：不需要（已打包在 EXE 中）

### 开发环境（如需修改代码）
- **.NET 8 SDK**：[下载地址](https://dotnet.microsoft.com/download/dotnet/8.0)
- **IDE**：Visual Studio 2022 或 VS Code（可选）
- **Git**：[下载地址](https://git-scm.com/)

## 🚀 快速开始

### 方式一：直接使用已发布版本

1. 下载 `publish` 文件夹（或从 Release 页面下载）
2. 解压到任意位置（如桌面、D盘等）
3. 双击 `DesktopPet.exe` 即可运行

### 方式二：从源码编译发布

#### 1. 安装 .NET 8 SDK

```powershell
# 检查是否已安装
dotnet --version

# 如果未安装，从官网下载并安装：
# https://dotnet.microsoft.com/download/dotnet/8.0
```

#### 2. 克隆或下载项目

```powershell
# 如果使用 Git
git clone <仓库地址>
cd DesktopPet

# 或直接下载 ZIP 并解压
```

#### 3. 安装依赖（如有）

```powershell
# 进入项目目录
cd DesktopPet

# 还原 NuGet 包（通常自动完成）
dotnet restore
```

#### 4. 运行开发版本

```powershell
# 启动开发模式（带调试信息）
dotnet run
```

#### 5. 发布为独立 EXE

```powershell
# 发布为单文件应用（包含 .NET 运行时）
dotnet publish DesktopPet.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o .\publish

# 发布完成后，EXE 位于：.\publish\DesktopPet.exe
```

**发布参数说明**：
- `-c Release`：使用 Release 配置（优化性能）
- `-r win-x64`：目标平台为 Windows 64位
- `--self-contained true`：包含 .NET 运行时（用户无需安装）
- `-p:PublishSingleFile=true`：打包为单个 EXE 文件
- `-p:IncludeNativeLibrariesForSelfExtract=true`：包含本机库
- `-o .\publish`：输出到 `publish` 目录

## 📁 项目结构

```
DesktopPet/
├── DesktopPet.csproj          # 项目配置文件
├── Program.cs                 # 程序入口
├── PetForm.cs                 # 主窗口（宠物渲染、交互）
├── PetEngine.cs               # 宠物行为引擎（移动、状态）
├── PetSettings.cs             # 配置参数（缩放、速度等）
├── PhraseManager.cs           # 短句管理
├── PhraseTooltip.cs           # 短句提示框
└── Resources/                 # 资源文件
    ├── img/
    │   ├── 正面.png           # 静止时显示
    │   ├── 奔跑1.png          # 奔跑动画帧1
    │   └── 奔跑2.png          # 奔跑动画帧2
    └── pet.png                # 备用图片
```

## 🎨 自定义素材

### 替换宠物图片

1. 准备三张 PNG 图片（支持透明通道）：
   - `正面.png` - 静止时显示
   - `奔跑1.png` - 奔跑动画帧1
   - `奔跑2.png` - 奔跑动画帧2

2. 替换图片文件：
   - 覆盖 `Resources\img\` 目录下的对应文件

3. 重新发布：
   ```powershell
   # 先关闭正在运行的桌面宠物
   taskkill /F /IM DesktopPet.exe
   
   # 重新发布
   dotnet publish DesktopPet.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o .\publish
   ```

### 更换 EXE 图标

1. 将图片转换为 `.ico` 格式（推荐在线工具：https://convertico.com/）
2. 将图标文件命名为 `icon.ico`，放到 `Resources\` 目录
3. 修改 `DesktopPet.csproj`，确保包含：
   ```xml
   <ApplicationIcon>Resources\icon.ico</ApplicationIcon>
   ```
4. 重新发布

## 📦 分发说明

### 打包发布

将 `publish` 文件夹压缩为 ZIP，发送给别人即可：

```
publish.zip
└── publish/
    ├── DesktopPet.exe          # 主程序（约150MB）
    └── Resources/
        └── img/
            ├── 正面.png
            ├── 奔跑1.png
            └── 奔跑2.png
```

### 注意事项

✅ **必须保持目录结构**：`DesktopPet.exe` 和 `Resources` 文件夹必须在同一目录  
✅ **相对路径加载**：程序会自动在 EXE 同级目录查找 `Resources\img\`  
❌ **不要单独移动 EXE**：缺少素材文件会导致程序报错

## ⚙️ 配置参数

编辑 `PetSettings.cs` 可调整：

| 参数 | 说明 | 默认值 |
|------|------|--------|
| `Scale` | 初始缩放倍率 | 0.25 |
| `MinScale` | 最小缩放 | 0.1 |
| `MaxScale` | 最大缩放 | 2.0 |
| `OriginalSize` | 基础尺寸(px) | 64 |
| `SpeedLevelValue` | 速度等级 | Normal |
| `IdlePeriod` | 闲置弹跳周期(ms) | 1200 |
| `IdleAmplitude` | 闲置弹跳幅度(px) | 5 |

## 🎮 使用说明

### 基本操作

- **拖动宠物**：鼠标左键按住拖动
- **显示短句**：单击宠物或等待自动显示
- **右键菜单**：
  - 放大/缩小
  - 调整速度（慢/正常/快）
  - 最小化到托盘
  - 退出程序

### 托盘操作

- **双击托盘图标**：恢复显示
- **右键托盘图标**：菜单操作

## 🔒 安全说明

### 隐私保护

✅ **无网络请求**：程序完全离线运行，不会连接互联网  
✅ **无数据收集**：不会收集、上传任何用户数据  
✅ **无后台服务**：不安装后台服务或启动项  
✅ **本地存储**：所有数据（配置、日志）保存在本地  

### 代码安全

✅ **开源透明**：所有代码开源，可自行审查  
✅ **无混淆**：代码未混淆，易于审计  
✅ **无外部依赖**：除 .NET 框架外无第三方依赖  
✅ **无敏感信息**：代码中不包含 API 密钥、密码等敏感信息  

### 系统权限

程序运行时需要的权限：
- ✅ **窗口创建**：创建透明窗口显示宠物
- ✅ **鼠标输入**：响应拖拽、点击操作
- ✅ **文件读取**：读取 `Resources\img\` 目录下的图片文件
- ❌ **不需要管理员权限**
- ❌ **不需要网络访问**
- ❌ **不需要注册表修改**

## 🐛 常见问题

### Q: 宠物有灰色/白色背景方块？
A: 素材 PNG 的透明区域不是真正的透明（Alpha=0），而是填充了纯色。请用图片编辑软件清除背景，确保透明区域 Alpha=0。

### Q: 宠物移动有残影？
A: 程序使用双缓冲技术，正常情况下不会有残影。如果出现，可能是显卡驱动问题，尝试更新驱动。

### Q: 宠物太大/太小？
A: 右键菜单可以实时缩放（10%~200%），也可以修改 `PetSettings.cs` 中的 `OriginalSize` 调整基础尺寸。

### Q: 如何开机自启？
A: 将 `DesktopPet.exe` 的快捷方式放到 `shell:startup` 目录（`Win+R` 输入 `shell:startup` 回车）。

### Q: 发布后运行报错"找不到图片"？
A: 确保 `DesktopPet.exe` 和 `Resources` 文件夹在同一目录下。

### Q: EXE 文件很大（约150MB）？
A: 因为包含了完整的 .NET 运行时，这是正常的。可以通过裁剪运行时减小体积（需要高级配置）。

## 📝 开发说明

### 代码结构

- `PetForm.cs`：主窗口，负责渲染、交互、图片加载
- `PetEngine.cs`：宠物行为引擎，控制移动、状态切换
- `PetSettings.cs`：全局配置参数
- `PhraseManager.cs`：短句池管理
- `PhraseTooltip.cs`：短句提示框窗口

### 调试运行

```powershell
# 开发模式运行
dotnet run

# 带调试信息
dotnet run --configuration Debug
```

### 编译检查

```powershell
# 检查代码是否有错误
dotnet build

# Release 模式检查
dotnet build -c Release
```

## 📄 许可证

本项目采用 MIT 许可证 - 详见 [LICENSE](LICENSE) 文件

## 🤝 贡献

欢迎提交 Issue 和 Pull Request！

## 📮 联系方式

如有问题或建议，请提交 Issue。

## 🙏 致谢

- .NET 团队提供优秀的开发框架
- 所有贡献者和使用者

---

**享受你的桌面宠物！** 🐾✨
