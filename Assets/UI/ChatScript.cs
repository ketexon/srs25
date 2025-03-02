using System.Collections;
using UnityEngine;

public class ChatScript : MonoBehaviour
{
    public Transform chatContainer;
    public GameObject messageContainer; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(SpawnMessages()); 
    }

    IEnumerator SpawnMessages()
    {
        while (chatContainer.childCount < 10)
        {
            SpawnMessage();
            yield return new WaitForSeconds(2f); 
        }
    }

    public void SpawnMessage()
    {
        GameObject newMessage = Instantiate(messageContainer, chatContainer);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
