using UnityEngine;

public class Entity : MonoBehaviour
{
    [SerializeField] public HumanModel HumanModel;
    [SerializeField] public Gun Gun;
    [SerializeField] public TeamSO Team;
    [SerializeField] public EntityMovement Movement;
    [SerializeField] public EntityItemController ItemController;
    [SerializeField] public EntityStats Stats;

    void Awake()
    {
        HumanModel.DeathEvent.AddListener(OnDeath);
        Movement.SwitchGuns.AddListener(GunSwitch);
    }

    public void OnDeath()
    {
        Gun.Drop();
    }
    public void GunSwitch(Gun newGun)
    {
        ItemController.Items[EntityItemController.GunIndex] = newGun;
    }
}
