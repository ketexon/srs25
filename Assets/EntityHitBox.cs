using UnityEngine;

[System.Serializable]
public enum BodyPart
{
    Head,
    Chest,
    Arm,
    Leg,
}

public class EntityHitBox : BulletHitBox
{
    [SerializeField] public HumanModel Model;
    [SerializeField] public BodyPart BodyPart;

    public void Reset()
    {
        var s = gameObject.name.ToLower();
        BodyPart = (
            s.Contains("pelvis") || s.Contains("spine") || s.Contains("chest") || s.Contains("neck")
            ? BodyPart.Chest
            : s.Contains("arm") || s.Contains("shoulder")
            ? BodyPart.Arm
            : s.Contains("thigh") || s.Contains("shin")
            ? BodyPart.Leg
            : BodyPart.Head
        );

        switch(BodyPart)
        {
            case BodyPart.Head: EnergyPenalty = 2; break;
            case BodyPart.Chest: EnergyPenalty = 4; break;
            case BodyPart.Arm: EnergyPenalty = 1; break;
            case BodyPart.Leg: EnergyPenalty = 1.5f; break;
        }

        Model = GetComponentInParent<HumanModel>();
    }

    public override void OnHit(float damage)
    {
        Debug.Log($"ONHIT: {gameObject.name} {damage}");
    }
}
