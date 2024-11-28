#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(RoomTypeConnection))]
class RoomTypeConnectionDrawer : PropertyDrawer {
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var root = new VisualElement();

        var aProp = property.FindPropertyRelative("A");
        var bProp = property.FindPropertyRelative("B");

        var horizontal = new VisualElement();
        horizontal.style.flexDirection = FlexDirection.Row;

		var aField = new PropertyField(aProp, "");
		aField.style.flexGrow = 1;
		var bField = new PropertyField(aProp, "");
		bField.style.flexGrow = 1;
        horizontal.Add(aField);
        horizontal.Add(bField);

        root.Add(horizontal);

        return root;
    }
}
#endif

[System.Serializable]
struct RoomTypeConnection : System.IEquatable<RoomTypeConnection> {
    public RoomTypeSO A;
    public RoomTypeSO B;

    public override bool Equals(object obj)
    {
        return Equals((RoomTypeConnection)obj);
    }

    public bool Equals(RoomTypeConnection other)
    {
        return (A == other.A && B == other.B) || (A == other.B && B == other.A);
    }

    public static bool operator ==(RoomTypeConnection a, RoomTypeConnection b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(RoomTypeConnection a, RoomTypeConnection b)
    {
        return !a.Equals(b);
    }

    public override int GetHashCode()
    {
        return A.GetInstanceID() < B.GetInstanceID()
            ? new RoomTypeConnection(){
                A = B,
                B = A,
            }.GetHashCode()
            : base.GetHashCode();
    }
}