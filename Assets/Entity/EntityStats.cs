using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.UIElements;

[CustomEditor(typeof(EntityStats))]
public class EntityStatsEditor : Editor
{
    private System.Action cleanup = null;
    
    public override VisualElement CreateInspectorGUI()
    {
        cleanup?.Invoke();
        cleanup = null;
        
        var root = new VisualElement();
        InspectorElement.FillDefaultInspector(root, serializedObject, this);

        if (!EditorApplication.isPlaying && !EditorApplication.isPaused)
            return root;
        
        var stats = (EntityStats)target;

        if (!stats.didAwake) return root;
            
        Dictionary<EntityStats.StatType, FloatField> fields = new();
        foreach (var statType in EntityStats.StatTypes)
        {
            var field = new FloatField
            {
                label = statType.ToString(),
                value = stats[statType]
            };
            field.RegisterValueChangedCallback(evt =>
            {
                stats.SetStat(statType, evt.newValue);
            });
            fields[statType] = field;
            root.Add(field);
        }

        stats.OnStatChanged.AddListener(OnStatChanged);
        cleanup = () =>
        {
            stats.OnStatChanged.RemoveListener(OnStatChanged);
        };

        return root;

        void OnStatChanged(EntityStats.StatType stat, float value)
        {
            fields[stat].value = value;
        }
    }
}

#endif

public class EntityStats : MonoBehaviour
{
    public enum StatType
    {
        Darkness,
        Nightmare,
        Stimulation,
        Mania,
        Dissociation,
        PainTolerance,
        Strength,
        Shakiness,
        Speed,
        Reaction
    }
    
    public static Dictionary<StatType, float> DefaultStats = new()
    {
        { StatType.Darkness, 1 },
        { StatType.Nightmare, 1 },
        { StatType.Stimulation, 1 },
        { StatType.Mania, 1 },
        { StatType.Dissociation, 1 },
        { StatType.PainTolerance, 1 },
        { StatType.Strength, 1 },
        { StatType.Shakiness, 1 },
        { StatType.Speed, 1 },
        { StatType.Reaction, 1 }
    };

    public static List<StatType> StatTypes = new()
    {
        StatType.Darkness,
        StatType.Nightmare,
        StatType.Stimulation,
        StatType.Mania,
        StatType.Dissociation,
        StatType.PainTolerance,
        StatType.Strength,
        StatType.Shakiness,
        StatType.Speed,
        StatType.Reaction,
    };

    [System.Serializable]
    struct StartStat
    {
        public StatType StatType;
        [Range(1, 10)] public float Value;
    }

    [SerializeField] private List<StartStat> startStatOverrides = new();

    public UnityEvent<StatType, float> OnStatChanged = new();

    Dictionary<StatType, float> stats = new();

    void Awake()
    {
        foreach (var statType in StatTypes)
        {
            stats[statType] = DefaultStats[statType];
        }

        foreach (var startStat in startStatOverrides)
        {
            stats[startStat.StatType] = startStat.Value;
        }
        
        Debug.Log(stats[StatType.Darkness]);
    }

    public float GetStat(StatType stat)
    {
        return this[stat];
    }

    public void SetStat(StatType stat, float value)
    {
        this[stat] = value;
    }

    public void ChangeStat(StatType stat, float delta)
    {
        this[stat] += delta;
    }
    
    public float this[StatType stat]
    {
        get => stats[stat];
        set
        {
            var old = stats[stat];
            if (old == value) return;
            stats[stat] = value;
            OnStatChanged.Invoke(stat, value);
        }
    }
}