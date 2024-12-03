using System;
using Meta.WitAi.CallbackHandlers;
using Oculus.Voice;
using UnityEngine;
using UnityEngine.Events;
using System.Reflection;
using TMPro;

public class VoiceManager : MonoBehaviour {

    [SerializeField] private AppVoiceExperience appVoiceExperience;
    [SerializeField] private WitResponseMatcher witResponseMatcher;
    [SerializeField] private TextMeshProUGUI transcriptionText;
    
    // Add a property for a reference of the DispalyCaptureMaanger 
    
    
    [SerializeField] private UnityEvent wakeWordDetected;
    [SerializeField] private UnityEvent<String> completeTranscription;

    private bool voiceCommandReady;


    void Awake() {
        Debug.Log("Wit has been awoken");
        appVoiceExperience.VoiceEvents.OnRequestCompleted.AddListener(ReactivateVoice);
        appVoiceExperience.VoiceEvents.OnPartialTranscription.AddListener(onPartialTranscription);
        appVoiceExperience.VoiceEvents.OnFullTranscription.AddListener(OnFullTranscription);
        
        var eventField = typeof(WitResponseMatcher).GetField("onMultiValueEvent", BindingFlags.NonPublic | BindingFlags.Instance);
        if (eventField != null && eventField.GetValue(witResponseMatcher) is MultiValueEvent onMultiValueEvent) {
            onMultiValueEvent.AddListener(WakeWordDetected);
            Debug.Log("Wit has been onMultiValueEvent");
        }
        
        appVoiceExperience.Activate();
        Debug.Log("Wit has been Activated");
    }

    private void OnDestroy() {
        appVoiceExperience.VoiceEvents.OnRequestCompleted.RemoveListener(ReactivateVoice);
        appVoiceExperience.VoiceEvents.OnFullTranscription.RemoveListener(OnFullTranscription);
        
        var eventField = typeof(WitResponseMatcher).GetField("onMultiValueEvent", BindingFlags.NonPublic | BindingFlags.Instance);
        if (eventField != null && eventField.GetValue(witResponseMatcher) is MultiValueEvent onMultiValueEvent) {
            onMultiValueEvent.RemoveListener(WakeWordDetected);
        }
        
        appVoiceExperience.Activate();
    }


    private void ReactivateVoice() {
        appVoiceExperience.Activate();
    }

    private void WakeWordDetected(string[] values) {
        Debug.Log("Wit has been Detected");
        voiceCommandReady = true;
        wakeWordDetected?.Invoke();
    }

    private void onPartialTranscription(string transcription) {
        if (!voiceCommandReady) return;
        transcriptionText.text = transcription;
    }

    private void OnFullTranscription(string transcription) {
        if (!voiceCommandReady) return;
        voiceCommandReady = false;
        Debug.Log(transcription);
        completeTranscription?.Invoke(transcription);
    }

    
    
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Wit has been STARTED");
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
