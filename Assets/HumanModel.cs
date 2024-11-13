using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;

[CustomEditor(typeof(HumanModel))]
public class HumanModelEditor : Editor {
    public override VisualElement CreateInspectorGUI()
    {
		VisualElement root = new();

		InspectorElement.FillDefaultInspector(root, serializedObject, this);

		Button destroyRBsButton = new(() => {
			var target = this.target as MonoBehaviour;
			List<Component> toDestroy = new();
			toDestroy.AddRange(target.GetComponentsInChildren<Joint>());
			toDestroy.AddRange(target.GetComponentsInChildren<Rigidbody>());
			toDestroy.AddRange(target.GetComponentsInChildren<Collider>());
			foreach(var c in toDestroy){
				Undo.DestroyObjectImmediate(c);
			}
		});
		destroyRBsButton.Add(new Label("Destroy All Rigidbodies"));

        Button toggleRBKinematic = new(() => {
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
			foreach(var entityHB in model.GetComponentsInChildren<EntityHitBox>())
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
			foreach(var collider in colliders)
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
public class HumanHealth {
	public float HeadHealth = 15;
	public float ChestHealth = 100;
	public float LeftArmHealth = 15;
	public float RightArmHealth = 15;
	public float LeftLegHealth = 30;
	public float RightLegHealth = 30;
}

public class HumanModel : MonoBehaviour {
	[SerializeField] public Entity Entity;
	[SerializeField] public HumanHealth Health;
	[SerializeField] public List<Collider> Colliders;
	[SerializeField] public List<Rigidbody> Rigidbodies;

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
    }

	public void Ragdoll()
	{
		Animator.enabled = false;
		HumanIK.enabled = false;
		foreach(var collider in Colliders)
		{
			collider.enabled = false;
		}
		foreach(var rb in Rigidbodies)
		{
			rb.isKinematic = false;
			rb.GetComponent<Collider>().enabled = true;
		}
	}
}