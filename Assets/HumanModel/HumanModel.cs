using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;

[CustomEditor(typeof(HumanModel))]
public class HumanModelEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = new();

        InspectorElement.FillDefaultInspector(root, serializedObject, this);

        Button destroyRBsButton = new(() =>
        {
            var target = this.target as MonoBehaviour;
            List<Component> toDestroy = new();
            toDestroy.AddRange(target.GetComponentsInChildren<Joint>());
            toDestroy.AddRange(target.GetComponentsInChildren<Rigidbody>());
            toDestroy.AddRange(target.GetComponentsInChildren<Collider>());
            foreach (var c in toDestroy)
            {
                Undo.DestroyObjectImmediate(c);
            }
        });
        destroyRBsButton.Add(new Label("Destroy All Rigidbodies"));

        Button toggleRBKinematic = new(() =>
        {
            var target = this.target as MonoBehaviour;
            var rbs = target.GetComponentsInChildren<Rigidbody>();
            foreach (var rb in rbs)
            {
                Undo.RecordObject(rb, "Toggled RigidBody kinematic");
                rb.isKinematic = !rb.isKinematic;
            }
        });
        toggleRBKinematic.Add(new Label("Toggle RB Kinematic"));


        Button ragdoll = new(() =>
        {
            var model = target as HumanModel;
            model.Ragdoll();
        });
        ragdoll.Add(new Label("Ragdoll"));

        Button assignCollidersHitBoxes = new(() =>
        {
            var model = target as HumanModel;
            foreach (var entityHB in model.GetComponentsInChildren<EntityHitBox>())
            {
                Undo.DestroyObjectImmediate(entityHB);
            }
            var colliders = model.GetComponentsInChildren<Collider>();
            foreach (var collider in colliders)
            {
                var hb = Undo.AddComponent<EntityHitBox>(collider.gameObject);
                hb.Model = model;
                hb.Reset();
            }
            Undo.RecordObjects(colliders, "Set layers of colliders");
            foreach (var collider in colliders)
            {
                collider.gameObject.layer = LayerMask.NameToLayer("Entity");
            }

        });
        assignCollidersHitBoxes.Add(new Label("Assign Hitboxes"));

        root.Add(destroyRBsButton);
        root.Add(toggleRBKinematic);
        root.Add(ragdoll);
        root.Add(assignCollidersHitBoxes);

        return root;
    }
}
#endif

[System.Serializable]
public class HumanHealth
{
    static readonly BodyPart[] bodyParts = new BodyPart[]
    {
        BodyPart.Head,
        BodyPart.Chest,
        BodyPart.LeftArm,
        BodyPart.RightArm,
        BodyPart.LeftLeg,
        BodyPart.RightLeg,
    };

    public bool Invincible = false;

    public float Head = 15;
    public float Chest = 100;
    public float LeftArm = 15;
    public float RightArm = 15;
    public float LeftLeg = 30;
    public float RightLeg = 30;

    public int NDeadParts = 0;

    public bool IsPartDead(BodyPart part) => this[part] <= 0;
    public bool IsDead => this[BodyPart.Head] <= 0 || this[BodyPart.Chest] <= 0;

    public float this[BodyPart part]
    {
        get => part switch
        {
            BodyPart.Head => Head,
            BodyPart.Chest => Chest,
            BodyPart.LeftArm => LeftArm,
            BodyPart.RightArm => RightArm,
            BodyPart.LeftLeg => LeftLeg,
            BodyPart.RightLeg => RightLeg,
            _ => throw new System.NotImplementedException()
        };
        set
        {
            switch (part)
            {
                case BodyPart.Head:
                    Head = value;
                    break;
                case BodyPart.Chest:
                    Chest = value;
                    break;
                case BodyPart.LeftArm:
                    LeftArm = value;
                    break;
                case BodyPart.RightArm:
                    RightArm = value;
                    break;
                case BodyPart.LeftLeg:
                    LeftLeg = value;
                    break;
                case BodyPart.RightLeg:
                    RightLeg = value;
                    break;
                default:
                    throw new System.NotImplementedException();
            }
        }
    }

    public void Damage(BodyPart part, float damage)
    {
        if(Invincible) return;
        if (IsPartDead(part)) {
            DamageNonDead(damage);
        }
        else
        {
            this[part] -= damage;
            if (this[part] <= 0)
            {
                this[part] = 0;
                NDeadParts++;
            }
        }
    }

    void DamageNonDead(float damage)
    {
        float distributed = damage / (bodyParts.Length - NDeadParts);
        foreach(var bodyPart in bodyParts)
        {
            if(!IsPartDead(bodyPart))
            {
                Damage(bodyPart, distributed);
            }
        }
    }

    public override string ToString()
    {
        return $"<Health H={Head} C={Chest} LA={LeftArm} RA={RightArm} LL={LeftLeg} RL={RightLeg}/>";
    }
}

public class HumanModel : MonoBehaviour
{
    [SerializeField] public Entity Entity;
    [SerializeField] public HumanHealth Health;
    [SerializeField] public List<Collider> Colliders;
    [SerializeField] public List<Rigidbody> Rigidbodies;

    [SerializeField] public UnityEvent DeathEvent;

    [System.NonSerialized] public Animator Animator;
    [System.NonSerialized] public HumanIK HumanIK;


    void Reset()
    {
        Rigidbodies = new(GetComponentsInChildren<Rigidbody>());
        Colliders = new(GetComponentsInChildren<Collider>());
    }

    void Awake()
    {
        Animator = GetComponent<Animator>();
        HumanIK = GetComponent<HumanIK>();

        foreach(var rb in Rigidbodies)
        {
            rb.isKinematic = true;
        }
    }

    public void Ragdoll()
    {
        Animator.enabled = false;
        HumanIK.enabled = false;
        foreach (var collider in Colliders)
        {
            collider.enabled = false;
            if(collider.GetComponent<EntityHitBox>() is var hb)
            {
                hb.enabled = false;
            }
        }
        foreach (var rb in Rigidbodies)
        {
            rb.isKinematic = false;
            rb.GetComponent<Collider>().enabled = true;
        }
    }

    public void OnHit(BodyPart bodyPart, float damage)
    {
        Health.Damage(bodyPart, damage);
        if (Health.IsDead)
        {
            Ragdoll();
            DeathEvent.Invoke();
        }
    }
}