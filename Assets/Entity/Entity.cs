using UnityEngine;

public class Entity : MonoBehaviour
{
    [SerializeField] public HumanModel HumanModel;
    [SerializeField] public Gun Gun;
    [SerializeField] public TeamSO Team;

    void Awake()
    {
        HumanModel.DeathEvent.AddListener(OnDeath);
    }

    public void OnDeath()
    {
        Gun.Drop();
    }
}
