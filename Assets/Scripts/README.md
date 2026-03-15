# Game单例和资源加载系统使用指南

## 概述

本系统提供了完整的游戏单例、资源预加载和资源管理功能，包括：

1. **Game单例** - 全局游戏状态管理
2. **资源预加载** - 游戏启动时异步加载所有必要资源
3. **资源加载管理器** - 统一管理游戏资源的加载和缓存
4. **加载场景控制器** - 显示加载进度和状态

## 核心类说明

### Game.cs

游戏主单例类，负责：

- 单例模式实现
- 资源预加载管理
- 场景异步加载
- 资源缓存管理
- 加载进度事件广播

### ResourceManager.cs

静态资源管理辅助类，提供：

- 简化的静态方法访问资源
- 预制体、精灵、音频等资源的快捷加载
- 内存管理和资源释放

### LoadingController.cs

加载场景UI控制器，用于：

- 显示加载进度条
- 显示加载状态信息
- 自动跳转到下一个场景

## 使用方法

### 1. 设置Game单例

在Unity中：

1. 创建一个空GameObject，命名为"Game"
2. 添加`Game.cs`脚本组件
3. 在Inspector中配置：
    - `Dont Destroy On Load`: 是否跨场景保留（建议开启）
    - `Preload On Awake`: 是否在Awake时自动预加载资源
    - `Preload Scenes`: 需要预加载的场景列表

### 2. 使用ResourceManager加载资源

```csharp
// 加载预制体
GameObject prefab = ResourceManager.LoadPrefab("Player");
GameObject player = ResourceManager.InstantiatePrefab("Player", Vector3.zero, Quaternion.identity);

// 加载精灵（带父对象）
GameObject buttonPrefab = ResourceManager.InstantiatePrefab("Button", canvasTransform);

// 加载精灵图片
Sprite sprite = ResourceManager.LoadSprite("Background");
if (sprite != null)
{
    imageComponent.sprite = sprite;
}

// 加载音频
AudioClip bgm = ResourceManager.LoadAudio("BackgroundMusic");
if (bgm != null)
{
    audioSource.clip = bgm;
    audioSource.Play();
}

// 通用资源加载
ScriptableObject config = ResourceManager.LoadResource<ScriptableObject>("Configs/GameConfig");

// 从图集加载精灵
Sprite icon = ResourceManager.LoadSpriteFromAtlas("Icons/IconAtlas", "sword_icon");
```

### 3. 使用Game实例直接访问

```csharp
// 检查是否正在加载
if (Game.Instance.IsLoading)
{
    Debug.Log("加载中...");
}

// 获取加载进度
float progress = Game.Instance.LoadProgress;

// 获取当前加载信息
string info = Game.Instance.CurrentLoadInfo;

// 订阅加载事件
Game.Instance.OnLoadProgress += (progress) => {
    Debug.Log($"加载进度: {progress * 100}%");
};

Game.Instance.OnLoadInfoChanged += (info) => {
    Debug.Log($"加载信息: {info}");
};

Game.Instance.OnPreloadComplete += () => {
    Debug.Log("预加载完成！");
};
```

### 4. 场景加载

```csharp
// 使用ResourceManager加载场景
ResourceManager.LoadSceneAsync("Game", () => {
    Debug.Log("游戏场景加载完成");
});

// 或者使用Game实例
StartCoroutine(Game.Instance.LoadSceneAsync("Game"));

// 预加载场景（不激活）
StartCoroutine(Game.Instance.PreloadSceneAsync("NextLevel"));
```

### 5. 内存管理

```csharp
// 清除资源缓存
ResourceManager.ClearCache();

// 卸载未使用的资源
ResourceManager.UnloadUnusedAssets();

// 强制垃圾回收
ResourceManager.ForceGC();

// 完整释放内存
ResourceManager.ReleaseMemory();
```

### 6. 设置加载场景

1. 创建加载场景UI：
    - Slider组件用于显示进度条
    - TextMeshProUGUI用于显示百分比
    - TextMeshProUGUI用于显示加载信息
    - TextMeshProUGUI用于显示版本号

2. 添加`LoadingController.cs`脚本：
    - 拖拽UI元素到对应的字段
    - 设置`Auto Load Next Scene`为true
    - 设置`Next Scene Name`为要跳转的场景名称

## 资源文件夹结构建议

```
Assets/Resources/
├── Prefabs/
│   ├── UI/
│   │   ├── Button.prefab
│   │   └── Panel.prefab
│   └── Game/
│       ├── Player.prefab
│       └── Enemy.prefab
├── Images/
│   ├── Background.png
│   ├── Icon.png
│   └── Atlas.png (多图集)
├── Audio/
│   ├── BGM/
│   │   ├── MainTheme.mp3
│   │   └── BattleTheme.mp3
│   └── SFX/
│       ├── Click.wav
│       └── Explosion.wav
└── Configs/
    ├── GameConfig.asset
    └── LevelConfig.asset
```

## 事件系统

Game类提供了三个主要事件：

```csharp
// 加载进度更新事件
Game.Instance.OnLoadProgress += (float progress) => {
    // 更新进度条UI
};

// 加载信息更新事件
Game.Instance.OnLoadInfoChanged += (string info) => {
    // 更新信息文本UI
};

// 预加载完成事件
Game.Instance.OnPreloadComplete += () => {
    // 执行加载完成后的操作
};
```

## 最佳实践

1. **资源预加载**：游戏启动时预加载所有必要的UI预制体和常用资源
2. **按需加载**：关卡资源在进入关卡时加载，离开时卸载
3. **内存管理**：定期调用`ReleaseMemory()`释放未使用的资源
4. **缓存管理**：利用Game类内置的缓存系统，避免重复加载
5. **异步加载**：对于大资源或场景，使用异步加载方法

## 注意事项

1. 确保Game单例在场景加载顺序中最早创建
2. Resources文件夹中的资源会被打包到游戏中，注意控制资源大小
3. 对于大量小资源，建议使用图集（Sprite Atlas）管理
4. 音频资源建议使用压缩格式（如MP3、OGG）
5. 定期测试内存使用情况，避免内存泄漏

## 故障排查

**问题：Game.Instance为null**

- 解决：确保场景中有GameObject挂载了Game脚本

**问题：资源加载失败**

- 解决：检查资源路径是否正确，确保资源在Resources文件夹中

**问题：内存占用过高**

- 解决：调用`ResourceManager.ReleaseMemory()`释放未使用的资源

**问题：加载进度不更新**

- 解决：确保正确订阅了`OnLoadProgress`事件
