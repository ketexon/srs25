using UnityEngine;

public class EntityItem : MonoBehaviour
{
    [SerializeField] protected Entity entity;
    [SerializeField] public HandIKTarget HandIKTargets;

    [System.NonSerialized]
    public int CycleDir = 1;

    public virtual bool CanEquip => true;

    /// <summary>
    /// Cycles the item. If the item can
    /// no longer cycle, return false.
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public virtual bool Cycle(int dir, bool wrap = false) => false;

    public virtual void Use(bool start = true){

    }
}
