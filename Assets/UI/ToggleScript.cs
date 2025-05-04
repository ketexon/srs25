using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ToggleScript : MonoBehaviour
{

    [SerializeField] EntityStats entityStats;
    [SerializeField] private int defaultStatValue = 1; 
    [SerializeField] private GameObject defaultHuman; 
    [System.Serializable]
    public class HumansWithStats
    {
        public EntityStats.StatType StatType;
        public GameObject Human;
    }
    public List<HumansWithStats> HumansandStats;

    void OnEnable()
    {
        entityStats.StatChangedEvent.AddListener(OnStatsChanged);
    }

    void OnDisable()
    {
        entityStats.StatChangedEvent.RemoveListener(OnStatsChanged);
    }

    void OnStatsChanged(EntityStats.StatType stat, float value)
    {
        ToggleHuman(); 
    }

    public void ToggleHuman()
    {
        // switch to model corresponding to the highest stat
        var highestStat = entityStats.Stats.Where(kvp => EntityStats.PsychologicalStats.Contains(kvp.Key)).OrderByDescending(kvp => kvp.Value).FirstOrDefault(); 
        Debug.Log($"Highest stat: {highestStat.Key} with value: {highestStat.Value}");

        // if the highest stat is too low, 
            // activate the default human model
            // and deactive all others
        // else 
            // deactive the default human model
            // and activate the human model corresponding to the highest stat

        if (highestStat.Value <= defaultStatValue)
        {
            defaultHuman.SetActive(true); 
            foreach (var human in HumansandStats)
            {
                human.Human.SetActive(false); 
            }
        }
        else 
        {
            defaultHuman.SetActive(false);
            foreach (var human in HumansandStats)
            {
                if (human.StatType == highestStat.Key)
                {
                    human.Human.SetActive(true); 
                    Debug.Log(human.Human.name + " is active now.");
                }
                else
                {
                    human.Human.SetActive(false);
                }
            }
        }
    }
}
