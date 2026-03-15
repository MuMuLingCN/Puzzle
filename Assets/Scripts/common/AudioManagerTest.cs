using UnityEngine;

/// <summary>
/// 声音管理器测试脚本
/// 演示如何使用AudioManager播放音乐和音效，以及控制声音开关
/// </summary>
public class AudioManagerTest : MonoBehaviour
{
    [Header("测试设置")]
    [SerializeField] private string testMusicName = "BackgroundMusic";
    [SerializeField] private string testSoundName = "ClickSound";

    void Update()
    {
        // 按下M键切换背景音乐
        if (Input.GetKeyDown(KeyCode.M))
        {
            AudioManager.Instance.ToggleMusic();
        }

        // 按下S键切换音效
        if (Input.GetKeyDown(KeyCode.S))
        {
            AudioManager.Instance.ToggleSound();
        }

        // 按下1键播放背景音乐
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            AudioManager.Instance.PlayMusic(testMusicName);
            Debug.Log($"播放背景音乐: {testMusicName}");
        }

        // 按下2键播放音效
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            AudioManager.Instance.PlaySound(testSoundName);
            Debug.Log($"播放音效: {testSoundName}");
        }

        // 按下3键停止背景音乐
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            AudioManager.Instance.StopMusic();
            Debug.Log("停止背景音乐");
        }

        // 按下P键暂停所有音频
        if (Input.GetKeyDown(KeyCode.P))
        {
            AudioManager.Instance.PauseAll();
            Debug.Log("暂停所有音频");
        }

        // 按下R键恢复所有音频
        if (Input.GetKeyDown(KeyCode.R))
        {
            AudioManager.Instance.ResumeAll();
            Debug.Log("恢复所有音频");
        }

        // 按下Up/Down键调整背景音乐音量
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            AudioManager.Instance.MusicVolume = Mathf.Clamp01(AudioManager.Instance.MusicVolume + 0.1f);
            Debug.Log($"背景音乐音量: {AudioManager.Instance.MusicVolume:F2}");
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            AudioManager.Instance.MusicVolume = Mathf.Clamp01(AudioManager.Instance.MusicVolume - 0.1f);
            Debug.Log($"背景音乐音量: {AudioManager.Instance.MusicVolume:F2}");
        }

        // 按下Left/Right键调整音效音量
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            AudioManager.Instance.SoundVolume = Mathf.Clamp01(AudioManager.Instance.SoundVolume + 0.1f);
            Debug.Log($"音效音量: {AudioManager.Instance.SoundVolume:F2}");
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            AudioManager.Instance.SoundVolume = Mathf.Clamp01(AudioManager.Instance.SoundVolume - 0.1f);
            Debug.Log($"音效音量: {AudioManager.Instance.SoundVolume:F2}");
        }
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 400));
        GUILayout.Label("=== 声音管理器测试 ===");
        GUILayout.Label($"背景音乐: {(AudioManager.Instance.MusicEnabled ? "开启" : "关闭")}");
        GUILayout.Label($"音效: {(AudioManager.Instance.SoundEnabled ? "开启" : "关闭")}");
        GUILayout.Label($"背景音乐音量: {AudioManager.Instance.MusicVolume:F2}");
        GUILayout.Label($"音效音量: {AudioManager.Instance.SoundVolume:F2}");
        GUILayout.Space(10);
        GUILayout.Label("控制说明:");
        GUILayout.Label("M - 切换背景音乐开关");
        GUILayout.Label("S - 切换音效开关");
        GUILayout.Label("1 - 播放背景音乐");
        GUILayout.Label("2 - 播放音效");
        GUILayout.Label("3 - 停止背景音乐");
        GUILayout.Label("P - 暂停所有音频");
        GUILayout.Label("R - 恢复所有音频");
        GUILayout.Label("↑/↓ - 调整背景音乐音量");
        GUILayout.Label("←/→ - 调整音效音量");
        GUILayout.EndArea();
    }
}
