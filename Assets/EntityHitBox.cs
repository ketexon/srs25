using UnityEngine;

[System.Serializable]
public enum BodyPart
{
    Head,
    Chest,
    Arm,
    Leg,
}

public class EntityHitBox : MonoBehaviour
{
    [SerializeField] public BodyPart BodyPart;

    void Reset()
    {
        var s = gameObject.name.ToLower();
        BodyPart = (
            s.Contains("chest") || s.Contains("stomach")
            ? BodyPart.Chest
            : s.Contains("arm")
            ? BodyPart.Arm
            : s.Contains("thigh") || s.Contains("shin")
            ? BodyPart.Leg
            : BodyPart.Head
        );
    }
}
