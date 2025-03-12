using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class ToggleScript : MonoBehaviour
{

    public List<GameObject> humans;
    public List<float> stats;

    private void Update()
    {

        for (int i = 0; i < humans.Count; i++)
        {
            KeyCode key = (KeyCode)(KeyCode.Alpha0 + i);

            if (Input.GetKeyDown(key))
            {
                int index = (int)key - (int)KeyCode.Alpha0;
                ToggleState(index);
            }
        }
    }

    public void ToggleState(int index)
    {
        for (int i = 0; i < humans.Count; i++)
        {
            humans[i].SetActive(false);
        }

        humans[index].SetActive(true);
    }
}
