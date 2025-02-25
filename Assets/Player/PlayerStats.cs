using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public float Darkness
    {
        get => darkness; set
        {
            if (value < 0) darkness = 0;
            else if (value > 10) darkness = 10;
            else darkness = value;
        }
    }
    public float Nightmare
    {
        get => nightmare; set
        {
            if (value < 0) nightmare = 0;
            else if (value > 10) nightmare = 10;
            else nightmare = value;
        }
    }
    public float Stimulation
    {
        get => stimulation; set
        {
            if (value < 0) stimulation = 0;
            else if (value > 10) stimulation = 10;
            else stimulation = value;
        }
    }
    public float Mania
    {
        get => mania; set
        {
            if (value < 0) mania = 0;
            else if (value > 10) mania = 10;
            else mania = value;
        }
    }
    public float Dissociation
    {
        get => dissociation; set
        {
            if (value < 0) dissociation = 0;
            else if (value > 10) dissociation = 10;
            else dissociation = value;
        }
    }

    float darkness;
    float nightmare;
    float stimulation;
    float mania;
    float dissociation;

    void Start()
    {
        darkness = 0;
        nightmare = 0;
        stimulation = 0;
        mania = 0;
        dissociation = 0;
    }

}
