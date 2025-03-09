using UnityEngine;

public class MessageScript : MonoBehaviour
{
    public GameObject chatParent;
    private RectTransform RectTransform => (RectTransform)transform;
    private RectTransform ChatParentRectTransform => (RectTransform)chatParent.transform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float parentTop = ChatParentRectTransform.rect.yMax + ChatParentRectTransform.position.y;
        float messageBottom = RectTransform.rect.yMin + RectTransform.position.y;
        Debug.Log($"parent {parentTop} child: {messageBottom}");

        if (messageBottom > parentTop)
        {
            Destroy(gameObject);
        }
    }
}
