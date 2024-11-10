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
				DestroyImmediate(c);
			}
		});
		destroyRBsButton.Add(new Label("Destroy All Rigidbodies"));

		root.Add(destroyRBsButton);

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
}