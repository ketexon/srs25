using UnityEngine;

public class DebugInfo : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text statsTMP;
    [SerializeField] private Entity player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (player == null)
        {
            Debug.LogError("No player found");
            return;
        }

        player.GetComponent<EntityStats>().OnStatChanged.AddListener(OnStatsChanged);
        OnStatsChanged(EntityStats.StatType.Darkness, 0);
    }

    void OnStatsChanged(EntityStats.StatType statType, float value)
    {
        statsTMP.text = "";
        foreach (var stat in EntityStats.StatTypes)
        {
            statsTMP.text += $"{stat}: {player.GetComponent<EntityStats>().GetStat(stat)}\n";
        }
    }
}
