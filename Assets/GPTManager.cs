using System.Collections.Generic;
using OpenAI;
using UnityEngine;

public class GptManager : MonoBehaviour {

    private OpenAIApi openAI = new OpenAIApi();
    private List<ChatMessage> messages = new List<ChatMessage>();


    public async void GptTest() {
        ChatMessage newMessage = new ChatMessage();
        newMessage.Content = "Who are you";
        newMessage.Role = "user";
        
        messages.Add(newMessage);
        
        CreateChatCompletionRequest request = new CreateChatCompletionRequest();
        request.Messages = messages;
        request.Model = "gpt-3.5-turbo";
        
        var response = await openAI.CreateChatCompletion(request);

        if (response.Choices != null && response.Choices.Count > 0) {
            var chatMessage = response.Choices[0].Message;
            messages.Add(chatMessage);
            Debug.Log("GPT response: " + chatMessage.Content); 
            
        }
        
        
    }
    
    
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start() {
    }

    // Update is called once per frame
    private void Update() {
    }
}