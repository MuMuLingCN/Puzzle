using UnityEngine;
using UnityEngine.UI;
using UI;
using System.Collections.Generic;

/// <summary>
/// 配置项与Switch的映射关系
/// </summary>
[System.Serializable]
public class ConfigSwitchMapping
{
    [Tooltip("配置键")]
    public ConfigKey configKey;

    [Tooltip("对应的Switch组件")]
    public Switch switchComponent;

    [Tooltip("配置项名称（用于调试显示）")]
    public string configName;
}

/// <summary>
/// 设置界面控制器
/// </summary>
public class SettingController : MonoBehaviour, ISwitchListener
{
    [Header("配置项列表")]
    [Tooltip("配置键与Switch的映射关系列表")]
    public List<ConfigSwitchMapping> configMappings = new List<ConfigSwitchMapping>();

    private void Start()
    {
        // 初始化Switch状态（从GameConfig加载配置）
        InitializeSwitchStates();

        // 添加监听器
        AddListeners();

        Debug.Log($"SettingController已初始化 - 共{configMappings.Count}个配置项");
    }

    /// <summary>
    /// 初始化Switch状态（从GameConfig加载配置）
    /// </summary>
    private void InitializeSwitchStates()
    {
        foreach (var mapping in configMappings)
        {
            if (mapping.switchComponent != null)
            {
                bool configValue = GetConfigValue(mapping.configKey);
                mapping.switchComponent.IsOn = configValue;

                string displayName = string.IsNullOrEmpty(mapping.configName)
                    ? mapping.configKey.ToString()
                    : mapping.configName;
                Debug.Log($"{displayName} Switch初始化: {configValue}");
            }
        }
    }

    /// <summary>
    /// 添加监听器
    /// </summary>
    private void AddListeners()
    {
        foreach (var mapping in configMappings)
        {
            if (mapping.switchComponent != null)
            {
                mapping.switchComponent.AddListener(this);
            }
        }
    }

    /// <summary>
    /// 移除监听器
    /// </summary>
    private void RemoveListeners()
    {
        foreach (var mapping in configMappings)
        {
            if (mapping.switchComponent != null)
            {
                mapping.switchComponent.RemoveListener(this);
            }
        }
    }

    /// <summary>
    /// 获取配置值
    /// </summary>
    private bool GetConfigValue(ConfigKey key)
    {

        return GameConfig.Instance.GetConfig(key);
    }


    /// <summary>
    /// Switch状态改变事件（ISwitchListener接口实现）
    /// </summary>
    public void OnSwitchStateChanged(Switch switchComponent, bool isOn)
    {
        var mapping = configMappings.Find(m => m.switchComponent == switchComponent);
        if (mapping != null)
        {
            string displayName = string.IsNullOrEmpty(mapping.configName)
                ? mapping.configKey.ToString()
                : mapping.configName;

            Debug.Log($"{displayName}开关: {(isOn ? "开启" : "关闭")}");

            // 更新配置
            GameConfig.Instance.SetConfig(mapping.configKey, isOn);

        }
    }



    /// <summary>
    /// 解绑事件
    /// </summary>
    private void OnDestroy()
    {
        // 移除监听器
        RemoveListeners();

    }
}
