using UnityEngine;

public class CutIn : MonoBehaviour
{
    public GameObject cutInPanel; // SkillCutInPanel をアサイン
    public float displayDuration = 0.5f; // 表示時間（秒）

    public void ShowCutIn()
    {
        cutInPanel.SetActive(true);
        Invoke("HideCutIn", displayDuration);
    }

    void HideCutIn()
    {
        cutInPanel.SetActive(false);
    }
}
