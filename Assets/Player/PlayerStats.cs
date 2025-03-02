using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] EntityMovement movement;

    public enum StatType
    {
        None,
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

    public UnityEvent<StatType, float> OnStatChanged = new();

    public float Darkness
    {
        get => darkness; set
        {
            if (value < 0) darkness = 0;
            else if (value > 10) darkness = 10;
            else darkness = value;

            OnStatChanged.Invoke(StatType.Darkness, darkness);
        }
    }
    public float Nightmare
    {
        get => nightmare; set
        {
            if (value < 0) nightmare = 0;
            else if (value > 10) nightmare = 10;
            else nightmare = value;

            OnStatChanged.Invoke(StatType.Nightmare, nightmare);
        }
    }
    public float Stimulation
    {
        get => stimulation; set
        {
            if (value < 0) stimulation = 0;
            else if (value > 10) stimulation = 10;
            else stimulation = value;

            OnStatChanged.Invoke(StatType.Stimulation, stimulation);
        }
    }
    public float Mania
    {
        get => mania; set
        {
            if (value < 0) mania = 0;
            else if (value > 10) mania = 10;
            else mania = value;

            OnStatChanged.Invoke(StatType.Mania, mania);
        }
    }
    public float Dissociation
    {
        get => dissociation; set
        {
            if (value < 0) dissociation = 0;
            else if (value > 10) dissociation = 10;
            else dissociation = value;

            OnStatChanged.Invoke(StatType.Dissociation, dissociation);
        }
    }

    public float PainTolerance
    {
        get => painTolerance; set
        {
            if (value < 0) painTolerance = 0;
            else if (value > 10) painTolerance = 10;
            else painTolerance = value;

            OnStatChanged.Invoke(StatType.PainTolerance, painTolerance);
        }
    }
    public float Strength
    {
        get => strength; set
        {
            if (value < 0) strength = 0;
            else if (value > 10) strength = 10;
            else strength = value;

            OnStatChanged.Invoke(StatType.Strength, strength);
        }
    }
    public float Shakiness
    {
        get => shakiness; set
        {
            if (value < 0) shakiness = 0;
            else if (value > 10) shakiness = 10;
            else shakiness = value;

            OnStatChanged.Invoke(StatType.Shakiness, shakiness);
        }
    }
    public float Speed
    {
        get => speed; set
        {
            if (value < 0) speed = 0;
            else if (value > 10) speed = 10;
            else speed = value;

            movement.MovementSpeed = speed + 1;
            OnStatChanged.Invoke(StatType.Speed, speed);
        }
    }
    public float Reaction
    {
        get => reaction; set
        {
            if (value < 0) reaction = 0;
            else if (value > 10) reaction = 10;
            else reaction = value;

            OnStatChanged.Invoke(StatType.Reaction, reaction);
        }
    }

    float darkness;
    float nightmare;
    float stimulation;
    float mania;
    float dissociation;
    float painTolerance;
    float strength;
    float shakiness;
    float speed;
    float reaction;


    [SerializeField] Slider[] statSliders;

    void Start()
    {
        darkness = 0;
        nightmare = 0;
        stimulation = 0;
        mania = 0;
        dissociation = 0;
        painTolerance = 0;
        strength = 0;
        shakiness = 0;
        Speed = 5;
        reaction = 0;

    }

    void Update()
    {
        statSliders[0].value = darkness;
        statSliders[1].value = nightmare;
        statSliders[2].value = stimulation;
        statSliders[3].value = mania;
        statSliders[4].value = dissociation;
    }

    public void ChangeStat(StatType stat, float value)
    {
        switch (stat)
        {
            case StatType.Darkness:
                Darkness += value;
                break;
            case StatType.Nightmare:
                Nightmare += value;
                break;
            case StatType.Stimulation:
                Stimulation += value;
                break;
            case StatType.Mania:
                Mania += value;
                break;
            case StatType.Dissociation:
                Dissociation += value;
                break;
            case StatType.PainTolerance:
                PainTolerance += value;
                break;
            case StatType.Strength:
                Strength += value;
                break;
            case StatType.Shakiness:
                Shakiness += value;
                break;
            case StatType.Speed:
                Speed += value;
                break;
            case StatType.Reaction:
                Reaction += value;
                break;
        }
    }

}
