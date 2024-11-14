using UnityEngine;

[System.Serializable]
public enum BodyPart
{
    Head = 0,
    Chest,
    LeftArm,
    RightArm,
    LeftLeg,
    RightLeg,
}

public class EntityHitBox : BulletHitBox
{
    [SerializeField] public HumanModel Model;
    [SerializeField] public BodyPart BodyPart;
    [SerializeField] float damageMult = 1.0f;

    public void Reset()
    {
        var s = gameObject.name.ToLower();
        var rightSide = s.EndsWith(".r") || s.Contains("right");

        BodyPart = (
            s.Contains("pelvis") || s.Contains("spine") || s.Contains("chest") || s.Contains("neck")
            ? BodyPart.Chest
            : s.Contains("arm") || s.Contains("shoulder")
            ? (rightSide ? BodyPart.RightArm : BodyPart.LeftArm)
            : s.Contains("thigh") || s.Contains("shin")
            ? (rightSide ? BodyPart.RightLeg : BodyPart.LeftLeg)
            : BodyPart.Head
        );

        switch(BodyPart)
        {
            case BodyPart.Head: 
                EnergyPenalty = 2;
                damageMult = 1.5f;
                break;
            case BodyPart.Chest:
                EnergyPenalty = 4;
                damageMult = 1.0f;
                break;
            case (BodyPart.LeftArm or BodyPart.RightArm): 
                EnergyPenalty = 1;
                damageMult = 0.3f;
                break;
            case (BodyPart.LeftLeg or BodyPart.RightLeg):
                EnergyPenalty = 1.5f;
                damageMult = 0.6f;
                break;
        }

        Model = GetComponentInParent<HumanModel>();
    }

    public override void OnHit(float damage)
    {
        Model.OnHit(BodyPart, damage * damageMult);
    }
}
