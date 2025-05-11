using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class AIBotUdon : UdonSharpBehaviour
{
    [Header("UI Components")]
    public InputField userInputField;
    public Text outputText;
    public ScrollRect scrollRect;
    public Button submitButton;
    public GameObject loadingIndicator;

    [Header("Configuration")]
    [SerializeField] private string botName = "VRChat AI";
    [SerializeField] private int maxResponsesStored = 10;
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private int maxPromptLength = 100;
    
    [Header("Response Datasets")]
    [TextArea(3, 10)]
    [SerializeField] private string[] greetingResponses;
    [TextArea(3, 10)]
    [SerializeField] private string[] generalResponses;
    [TextArea(3, 10)]
    [SerializeField] private string[] questionResponses;
    [TextArea(3, 10)]
    [SerializeField] private string[] helpResponses;
    [TextArea(3, 10)]
    [SerializeField] private string[] reactionResponses;
    
    [Header("Trigger Keywords")]
    [SerializeField] private string[] greetingKeywords;
    [SerializeField] private string[] questionKeywords;
    [SerializeField] private string[] helpKeywords;
    
    // Network sync variables
    [UdonSynced] private string syncedUserInput = "";
    [UdonSynced] private string syncedResponse = "";
    [UdonSynced] private int syncedResponseType = 0;
    
    // Private variables
    private string[] conversationHistory;
    private int historyIndex = 0;
    private bool isTypingResponse = false;
    private string currentFullResponse = "";
    private string currentDisplayedResponse = "";
    private int charIndex = 0;
    
    void Start()
    {
        // Initialize conversation history
        conversationHistory = new string[maxResponsesStored];
        
        // Set up UI elements
        if (submitButton != null)
        {
            submitButton.onClick.AddListener(new System.Action(this.OnSubmitButtonPressed));
        }
    }
    
    public void OnSubmitButtonPressed()
    {
        if (!Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
        
        ProcessUserInput();
    }
    
    public void ProcessUserInput()
    {
        if (userInputField == null || string.IsNullOrEmpty(userInputField.text))
            return;
            
        // Get user input and validate
        string userInput = userInputField.text.Trim();
        if (string.IsNullOrEmpty(userInput))
            return;
            
        // Truncate if too long
        if (userInput.Length > maxPromptLength)
        {
            userInput = userInput.Substring(0, maxPromptLength);
        }
        
        // Clear input field
        userInputField.text = "";
        
        // Update UI with user's input
        AppendToOutput("You: " + userInput);
        
        // Set synced variables
        syncedUserInput = userInput;
        
        // Generate response based on input
        GenerateResponse(userInput);
        
        // Request serialization to sync with other clients
        RequestSerialization();
        
        // Start typing animation
        StartTypingAnimation();
    }
    
    public void GenerateResponse(string userInput)
    {
        userInput = userInput.ToLower();
        
        // Determine response type based on keywords
        int responseType = DetermineResponseType(userInput);
        syncedResponseType = responseType;
        
        // Generate appropriate response
        switch (responseType)
        {
            case 0: // Greeting
                syncedResponse = SelectRandomResponse(greetingResponses);
                break;
            case 1: // Question
                syncedResponse = SelectRandomResponse(questionResponses);
                break;
            case 2: // Help
                syncedResponse = SelectRandomResponse(helpResponses);
                break;
            case 3: // Reaction
                syncedResponse = SelectRandomResponse(reactionResponses);
                break;
            default: // General
                syncedResponse = SelectRandomResponse(generalResponses);
                break;
        }
        
        // Store in conversation history
        StoreInHistory(userInput, syncedResponse);
    }
    
    private int DetermineResponseType(string input)
    {
        // Check for greeting keywords
        for (int i = 0; i < greetingKeywords.Length; i++)
        {
            if (input.Contains(greetingKeywords[i]))
                return 0;
        }
        
        // Check for question keywords
        for (int i = 0; i < questionKeywords.Length; i++)
        {
            if (input.Contains(questionKeywords[i]))
                return 1;
        }
        
        // Check for help keywords
        for (int i = 0; i < helpKeywords.Length; i++)
        {
            if (input.Contains(helpKeywords[i]))
                return 2;
        }
        
        // Check if it's likely a reaction needed
        if (input.Length < 5)
            return 3;
            
        // Default to general response
        return 4;
    }
    
    private string SelectRandomResponse(string[] responseArray)
    {
        if (responseArray == null || responseArray.Length == 0)
            return "I don't know what to say.";
            
        int randomIndex = Random.Range(0, responseArray.Length);
        return responseArray[randomIndex];
    }
    
    private void StoreInHistory(string userInput, string botResponse)
    {
        string historyEntry = "User: " + userInput + "\nBot: " + botResponse;
        conversationHistory[historyIndex] = historyEntry;
        historyIndex = (historyIndex + 1) % maxResponsesStored;
    }
    
    public override void OnDeserialization()
    {
        // When data is received, show the response if we're not the one who sent it
        if (!Networking.IsOwner(gameObject) && !string.IsNullOrEmpty(syncedResponse))
        {
            // Update UI with the received response
            currentFullResponse = botName + ": " + syncedResponse;
            StartTypingAnimation();
        }
    }
    
    private void StartTypingAnimation()
    {
        if (loadingIndicator != null)
            loadingIndicator.SetActive(true);
            
        isTypingResponse = true;
        currentDisplayedResponse = "";
        charIndex = 0;
        
        // Use SendCustomEventDelayedFrames for the typing animation
        SendCustomEventDelayedFrames(nameof(TypeNextCharacter), 5);
    }
    
    public void TypeNextCharacter()
    {
        if (!isTypingResponse || string.IsNullOrEmpty(currentFullResponse))
            return;
            
        // Type the next character
        if (charIndex < currentFullResponse.Length)
        {
            currentDisplayedResponse += currentFullResponse[charIndex];
            outputText.text = currentDisplayedResponse;
            charIndex++;
            
            // Scroll to bottom
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
            
            // Schedule the next character
            SendCustomEventDelayedSeconds(nameof(TypeNextCharacter), typingSpeed);
        }
        else
        {
            // Typing complete
            FinishTypingAnimation();
        }
    }
    
    private void FinishTypingAnimation()
    {
        isTypingResponse = false;
        if (loadingIndicator != null)
            loadingIndicator.SetActive(false);
            
        // Add the full response to the output
        AppendToOutput(currentFullResponse);
    }
    
    private void AppendToOutput(string text)
    {
        if (outputText != null)
        {
            // Add new line if there's already text
            if (!string.IsNullOrEmpty(outputText.text))
            {
                outputText.text += "\n\n" + text;
            }
            else
            {
                outputText.text = text;
            }
            
            // Scroll to bottom
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
    
    public void ClearConversation()
    {
        if (outputText != null)
        {
            outputText.text = "";
        }
        
        // Reset history
        for (int i = 0; i < maxResponsesStored; i++)
        {
            conversationHistory[i] = "";
        }
        historyIndex = 0;
    }
    
    // Public methods that can be triggered by UI buttons
    
    public void SayHello()
    {
        if (!isTypingResponse)
        {
            currentFullResponse = botName + ": " + SelectRandomResponse(greetingResponses);
            StartTypingAnimation();
        }
    }
    
    public void GiveHelp()
    {
        if (!isTypingResponse)
        {
            currentFullResponse = botName + ": " + SelectRandomResponse(helpResponses);
            StartTypingAnimation();
        }
    }
}
