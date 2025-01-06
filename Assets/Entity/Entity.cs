using UnityEngine;

public class Entity : MonoBehaviour
{
    [SerializeField] public HumanModel HumanModel;
    [SerializeField] public Gun Gun;
    [SerializeField] public TeamSO Team;
    [SerializeField] public EntityMovement Movement;
    [SerializeField] public EntityItemController ItemController;

    void Awake()
    {
        HumanModel.DeathEvent.AddListener(OnDeath);
    }

    public void OnDeath()
    {
        Gun.Drop();
    }
}
