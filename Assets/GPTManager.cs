using System;
using System.Collections.Generic;
using Anaglyph.DisplayCapture;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Models;
using UnityEngine;

public class GptManager : MonoBehaviour {

    private DisplayCaptureManager displayCaptureManager;
    
    
    private void Start() {
        GameObject displayCaptureManagerObject = GameObject.Find("Screen Capture Texture Manager");
        displayCaptureManager = displayCaptureManagerObject.GetComponent<DisplayCaptureManager>();
    }

    private String fetchBase64String() {
        String base64Image = displayCaptureManager.convertCapturetoBase64();
        return base64Image;
    }

    public async void sendPhotoToGPt(String prompt) {
        var api = new OpenAIClient("");
        var base64Image = fetchBase64String();
        Debug.Log($"This is the prompt your sending meow:" + prompt);
        var messages = new List<Message>
        {
            new Message(Role.System, "You are a helpful assistant."),
            new Message(Role.User, new List<Content>
            {
                prompt,
                new ImageUrl($"data:image/jpeg;base64,{base64Image}")
            })
        };
        var chatRequest = new ChatRequest(messages, model: Model.GPT4o);
        var response = await api.ChatEndpoint.GetCompletionAsync(chatRequest);
        Debug.Log($"{response.FirstChoice.Message.Role}: {response.FirstChoice.Message.Content} | Finish Reason: {response.FirstChoice.FinishDetails}");
        
    
    
    
    }
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    // Update is called once per frame
    private void Update() {
    }
}