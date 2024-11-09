using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class Settings
{
    public static float Sensitivity {
        get => PlayerPrefs.GetFloat("sensitivity", 0.3f);
        set => PlayerPrefs.SetFloat("sensitivity", value);
    }
}