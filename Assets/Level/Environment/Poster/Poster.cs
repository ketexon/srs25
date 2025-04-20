using System;
using Kutie.Extensions;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;

[CustomEditor(typeof(Poster))]
class PosterEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = new VisualElement();

        InspectorElement.FillDefaultInspector(root, serializedObject, this);

        var pickNewPosterButton =
            new Button(() => ((Poster)target).PickNewPoster(shared: true))
            {
                text = "Pick New Poster"
            };
        root.Add(pickNewPosterButton);

        return root;
    }
}
#endif

public class Poster : MonoBehaviour
{
    [SerializeField] private new Renderer renderer;
    [SerializeField] private PosterSet posterSet;

    private void Start()
    {
        PickNewPoster();
    }

    // note: the option for shared is because
    // settings materials in the editor is bad
    // however, this is *shared*, so all posters will
    // have this material (immediately changed when the scene is loaded)
    public void PickNewPoster(bool shared = false)
    {
        if (!renderer || !posterSet)
        {
            Debug.LogWarning("Renderer or PosterSet not set.");
            return;
        }
        var mat = shared ? renderer.sharedMaterial : renderer.material;
        mat.mainTexture = posterSet.Posters.Sample();
    }
}