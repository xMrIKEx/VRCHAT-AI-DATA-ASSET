using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class ResponseManager : UdonSharpBehaviour
{
    [Header("Response Categories")]
    [SerializeField] private ResponseCategory[] responseCategories;
    
    [Header("Advanced Settings")]
    [SerializeField] private bool useFallbackResponses = true;
    [SerializeField] private string[] fallbackResponses;
    
    // This class is used for Unity Inspector organization only
    [System.Serializable]
    public class ResponseCategory
    {
        public string categoryName;
        [TextArea(2, 5)]
        public string[] triggerKeywords;
        [TextArea(3, 10)]
        public string[] responses;
    }
    
    // Returns a response based on input text and category matching
    public string GetResponse(string input)
    {
        if (string.IsNullOrEmpty(input))
            return GetFallbackResponse();
            
        input = input.ToLower();
        
        // Check each category for keyword matches
        for (int i = 0; i < responseCategories.Length; i++)
        {
            ResponseCategory category = responseCategories[i];
            
            // Skip empty categories
            if (category.responses == null || category.responses.Length == 0)
                continue;
                
            // Check keywords
            string[] keywords = category.triggerKeywords;
            if (keywords != null)
            {
                for (int j = 0; j < keywords.Length; j++)
                {
                    string keyword = keywords[j].ToLower();
                    if (!string.IsNullOrEmpty(keyword) && input.Contains(keyword))
                    {
                        // Found a matching keyword in this category
                        return GetRandomResponse(category.responses);
                    }
                }
            }
        }
        
        // No category matched, return fallback
        return GetFallbackResponse();
    }
    
    private string GetRandomResponse(string[] responses)
    {
        if (responses == null || responses.Length == 0)
            return GetFallbackResponse();
            
        int randomIndex = Random.Range(0, responses.Length);
        return responses[randomIndex];
    }
    
    private string GetFallbackResponse()
    {
        if (!useFallbackResponses || fallbackResponses == null || fallbackResponses.Length == 0)
            return "I'm not sure how to respond to that.";
            
        int randomIndex = Random.Range(0, fallbackResponses.Length);
        return fallbackResponses[randomIndex];
    }
    
    // Utility method for finding response categories by name
    public string[] GetCategoryKeywords(string categoryName)
    {
        if (string.IsNullOrEmpty(categoryName))
            return null;
            
        for (int i = 0; i < responseCategories.Length; i++)
        {
            if (responseCategories[i].categoryName == categoryName)
            {
                return responseCategories[i].triggerKeywords;
            }
        }
        
        return null;
    }
    
    // Utility method for finding responses by category name
    public string[] GetCategoryResponses(string categoryName)
    {
        if (string.IsNullOrEmpty(categoryName))
            return null;
            
        for (int i = 0; i < responseCategories.Length; i++)
        {
            if (responseCategories[i].categoryName == categoryName)
            {
                return responseCategories[i].responses;
            }
        }
        
        return null;
    }
}
