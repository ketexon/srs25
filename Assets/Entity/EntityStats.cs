using UnityEngine;
using UnityEngine.Events;

public class EntityStats : MonoBehaviour
{
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


    [Range(1, 10)]
    public float startingDarkness, startingNightmare, startingStimulation, startingMania, startingDissociation, 
        startingPainTolerance, startingStrength, startingShakiness, startingSpeed, startingReaction;

    public UnityEvent<StatType, float> OnStatChanged = new();

    public float Darkness
    {
        get => darkness; set
        {
            if (value < 1) darkness = 0;
            else if (value > 10) darkness = 10;
            else darkness = value;

            OnStatChanged.Invoke(StatType.Darkness, darkness);
        }
    }
    public float Nightmare
    {
        get => nightmare; set
        {
            if (value < 1) nightmare = 0;
            else if (value > 10) nightmare = 10;
            else nightmare = value;

            OnStatChanged.Invoke(StatType.Nightmare, nightmare);
        }
    }
    public float Stimulation
    {
        get => stimulation; set
        {
            if (value < 1) stimulation = 0;
            else if (value > 10) stimulation = 10;
            else stimulation = value;

            OnStatChanged.Invoke(StatType.Stimulation, stimulation);
        }
    }
    public float Mania
    {
        get => mania; set
        {
            if (value < 1) mania = 0;
            else if (value > 10) mania = 10;
            else mania = value;

            OnStatChanged.Invoke(StatType.Mania, mania);
        }
    }
    public float Dissociation
    {
        get => dissociation; set
        {
            if (value < 1) dissociation = 0;
            else if (value > 10) dissociation = 10;
            else dissociation = value;

            OnStatChanged.Invoke(StatType.Dissociation, dissociation);
        }
    }

    public float PainTolerance
    {
        get => painTolerance; set
        {
            if (value < 1) painTolerance = 0;
            else if (value > 10) painTolerance = 10;
            else painTolerance = value;

            OnStatChanged.Invoke(StatType.PainTolerance, painTolerance);
        }
    }
    public float Strength
    {
        get => strength; set
        {
            if (value < 1) strength = 0;
            else if (value > 10) strength = 10;
            else strength = value;

            OnStatChanged.Invoke(StatType.Strength, strength);
        }
    }
    public float Shakiness
    {
        get => shakiness; set
        {
            if (value < 1) shakiness = 0;
            else if (value > 10) shakiness = 10;
            else shakiness = value;

            OnStatChanged.Invoke(StatType.Shakiness, shakiness);
        }
    }
    public float Speed
    {
        get => speed; set
        {
            if (value < 1) speed = 1;
            else if (value > 10) speed = 10;
            else speed = value;

            OnStatChanged.Invoke(StatType.Speed, speed);
        }
    }
    public float Reaction
    {
        get => reaction; set
        {
            if (value < 1) reaction = 0;
            else if (value > 10) reaction = 10;
            else reaction = value;

            OnStatChanged.Invoke(StatType.Reaction, reaction);
        }
    }

    float darkness, nightmare, stimulation, mania, dissociation, painTolerance, strength, shakiness, speed, reaction;


    void Start()
    {
        Darkness = startingDarkness;
        Nightmare = startingNightmare;
        Stimulation = startingStimulation;
        Mania = startingMania;
        Dissociation = startingDissociation;
        PainTolerance = startingPainTolerance;
        Strength = startingStrength;
        Shakiness = startingShakiness;
        Speed = startingSpeed;
        Reaction = startingReaction;
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
