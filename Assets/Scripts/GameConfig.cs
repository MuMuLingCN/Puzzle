using UnityEngine;
using System;

/// <summary>
/// 游戏配置项枚举
/// </summary>
public enum ConfigKey
{
    MusicEnabled,
    SoundEnabled,
    VibrationEnabled
}

/// <summary>
/// 游戏配置管理类，负责存储和管理游戏设置
/// </summary>
public class GameConfig : MonoBehaviour
{
    private static GameConfig _instance;
    public static GameConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject configObj = new GameObject("GameConfig");
                _instance = configObj.AddComponent<GameConfig>();
            }
            return _instance;
        }
    }

    // 配置字典
    private System.Collections.Generic.Dictionary<ConfigKey, bool> configData = new System.Collections.Generic.Dictionary<ConfigKey, bool>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        // 加载配置
        LoadConfig();

        Debug.Log("GameConfig实例已创建");
    }

    /// <summary>
    /// 获取配置值
    /// </summary>
    public bool GetConfig(ConfigKey key)
    {
        if (configData.ContainsKey(key))
        {
            return configData[key];
        }

        // 默认值：所有配置项默认为 true
        return true;
    }

    /// <summary>
    /// 设置配置值
    /// </summary>
    public void SetConfig(ConfigKey key, bool value)
    {
        if (!configData.ContainsKey(key) || configData[key] != value)
        {
            configData[key] = value;
            SaveSingleConfig(key, value);
            ApplyConfig(key);
        }
    }

    /// <summary>
    /// 从本地存储加载配置
    /// </summary>
    private void LoadConfig()
    {
        // 加载所有配置项
        foreach (ConfigKey key in System.Enum.GetValues(typeof(ConfigKey)))
        {
            bool defaultValue = true;
            configData[key] = PlayerPrefs.GetInt(key.ToString(), defaultValue ? 1 : 0) == 1;
            ApplyConfig(key);
        }

    }

    /// <summary>
    /// 保存单个配置到本地存储
    /// </summary>
    private void SaveSingleConfig(ConfigKey key, bool value)
    {
        PlayerPrefs.SetInt(key.ToString(), value ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log($"配置已保存 - {key}:{value}");
    }

    /// <summary>
    /// 应用配置到系统
    /// </summary>
    private void ApplyConfig(ConfigKey key)
    {
        // 应用音频设置到AudioManager

        switch (key)
        {
            case ConfigKey.MusicEnabled:
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.MusicEnabled = configData[key];
                }
                break;
            case ConfigKey.SoundEnabled:
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.SoundEnabled = configData[key];
                }
                break;
            case ConfigKey.VibrationEnabled:
                // 振动设置不需要立即应用，但可以在这里添加相关逻辑
                Vibrate();
                break;
            default:
                Debug.LogWarning($"未知配置项: {key}");
                break;
        }
    }

    /// <summary>
    /// 触发振动（如果启用）
    /// </summary>
    /// <param name="milliseconds">振动持续时间（毫秒）</param>
    public void Vibrate(long milliseconds = 100)
    {

#if UNITY_ANDROID && !UNITY_EDITOR
        Handheld.Vibrate();
#elif UNITY_IOS && !UNITY_EDITOR
        iOS.Vibrate.vibrateiOS((int)milliseconds);
#endif
    }


    /// <summary>
    /// 清除所有本地存储的配置
    /// </summary>
    public void ClearAllConfig()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("所有配置已清除");
    }



    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}

#if UNITY_IOS
/// <summary>
/// iOS振动辅助类
/// </summary>
public static class iOS
{
    [DllImport("__Internal")]
    private static extern void _vibrateiOS(int duration);

    public static void vibrateiOS(int duration)
    {
        if (SystemInfo.supportsVibration)
        {
            _vibrateiOS(duration);
        }
    }
}
#endif
