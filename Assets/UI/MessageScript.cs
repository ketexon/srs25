using System.Collections;
using UnityEngine;

public class MessageScript : MonoBehaviour
{
    public GameObject chatParent;
    private RectTransform RectTransform => (RectTransform)transform;
    private RectTransform ChatParentRectTransform => (RectTransform)chatParent.transform;
    private float parentTop => ChatParentRectTransform.rect.yMax + ChatParentRectTransform.position.y;
    private float messageBottom => RectTransform.rect.yMin + RectTransform.position.y;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(CheckIfMessageIsOffScreen());
    }

    IEnumerator CheckIfMessageIsOffScreen()
    {
        while (true)
        {
            // Debug.Log($"parent {parentTop} child: {messageBottom}");

            if (messageBottom > parentTop)
            {
                Destroy(gameObject);
            }

            // this could b changed to update depending on ChatScript chatSpawnInterval val for potential small performance optimization. but honestly i think this is ok
            yield return new WaitForSeconds(0.3f);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
