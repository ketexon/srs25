using System.Collections;
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class ChatScript : MonoBehaviour
{
    public Transform chatContainer;
    public GameObject messageText;
    public List<string> users;
    public List<string> messages;
    public List<string> colors = new List<string> { "red", "blue", "purple", "green" };
    private int i = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(SpawnMessages()); 
    }

    IEnumerator SpawnMessages()
    {
        while (chatContainer.childCount < 20)
        {
            SpawnMessage();
            yield return new WaitForSeconds(.3f); 
        }
    }

    public void SpawnMessage()
    {
        GameObject newMessage = Instantiate(messageText, chatContainer);
        MessageScript message = newMessage.GetComponent<MessageScript>();
        message.chatParent = gameObject;
        TMP_Text textComponent = newMessage.GetComponent<TMP_Text>();
        if (textComponent != null)
        {
            int randomColor = Random.Range(0, colors.Count);
            textComponent.text = $"<color=\"{colors[randomColor]}\">{users[i]}</color>: {messages[i]}";
            // for this to work users must be the same length as messages
            i = (i + 1) % messages.Count;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
