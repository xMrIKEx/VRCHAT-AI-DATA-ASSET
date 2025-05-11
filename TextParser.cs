using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class TextParser : UdonSharpBehaviour
{
    [Header("Common Words")]
    [SerializeField] private string[] stopWords;
    
    [Header("Sentiment Analysis")]
    [SerializeField] private string[] positiveWords;
    [SerializeField] private string[] negativeWords;
    
    // Parse the input text and extract key information
    public string[] ExtractKeywords(string input, int maxKeywords)
    {
        if (string.IsNullOrEmpty(input))
            return new string[0];
            
        input = input.ToLower();
        
        // Split into words
        string[] words = SplitIntoWords(input);
        
        // Filter out stop words and create a frequency array
        string[] filteredWords = new string[words.Length];
        int[] wordFrequency = new int[words.Length];
        int uniqueWordCount = 0;
        
        for (int i = 0; i < words.Length; i++)
        {
            string word = words[i];
            
            // Skip empty words
            if (string.IsNullOrEmpty(word))
                continue;
                
            // Skip stop words
            if (IsStopWord(word))
                continue;
                
            // Check if we've seen this word before
            bool foundMatch = false;
            for (int j = 0; j < uniqueWordCount; j++)
            {
                if (filteredWords[j] == word)
                {
                    wordFrequency[j]++;
                    foundMatch = true;
                    break;
                }
            }
            
            // If this is a new word, add it
            if (!foundMatch && uniqueWordCount < filteredWords.Length)
            {
                filteredWords[uniqueWordCount] = word;
                wordFrequency[uniqueWordCount] = 1;
                uniqueWordCount++;
            }
        }
        
        // Create result array limited to maxKeywords
        int resultSize = Mathf.Min(maxKeywords, uniqueWordCount);
        string[] result = new string[resultSize];
        
        // Find the most common words
        for (int i = 0; i < resultSize; i++)
        {
            int maxFrequencyIndex = 0;
            int maxFrequency = 0;
            
            for (int j = 0; j < uniqueWordCount; j++)
            {
                if (wordFrequency[j] > maxFrequency)
                {
                    maxFrequency = wordFrequency[j];
                    maxFrequencyIndex = j;
                }
            }
            
            // Add the most frequent word to the result
            result[i] = filteredWords[maxFrequencyIndex];
            
            // Mark this word as processed
            wordFrequency[maxFrequencyIndex] = -1;
        }
        
        return result;
    }
    
    // Analyze sentiment of input text (positive, negative, neutral)
    public int AnalyzeSentiment(string input)
    {
        if (string.IsNullOrEmpty(input))
            return 0;
            
        input = input.ToLower();
        string[] words = SplitIntoWords(input);
        
        int positiveScore = 0;
        int negativeScore = 0;
        
        for (int i = 0; i < words.Length; i++)
        {
            string word = words[i];
            
            // Check positive words
            for (int j = 0; j < positiveWords.Length; j++)
            {
                if (word == positiveWords[j])
                {
                    positiveScore++;
                    break;
                }
            }
            
            // Check negative words
            for (int j = 0; j < negativeWords.Length; j++)
            {
                if (word == negativeWords[j])
                {
                    negativeScore++;
                    break;
                }
            }
        }
        
        // Calculate overall sentiment
        if (positiveScore > negativeScore)
            return 1;  // Positive
        else if (negativeScore > positiveScore)
            return -1; // Negative
        else
            return 0;  // Neutral
    }
    
    // Check if a word is a stop word
    private bool IsStopWord(string word)
    {
        for (int i = 0; i < stopWords.Length; i++)
        {
            if (word == stopWords[i])
                return true;
        }
        return false;
    }
    
    // Split text into individual words
    private string[] SplitIntoWords(string text)
    {
        // Replace punctuation with spaces
        text = text.Replace('.', ' ');
        text = text.Replace(',', ' ');
        text = text.Replace('!', ' ');
        text = text.Replace('?', ' ');
        text = text.Replace(';', ' ');
        text = text.Replace(':', ' ');
        text = text.Replace('-', ' ');
        text = text.Replace('(', ' ');
        text = text.Replace(')', ' ');
        
        // Split by spaces
        string[] words = text.Split(' ');
        
        // Further clean up words
        for (int i = 0; i < words.Length; i++)
        {
            words[i] = words[i].Trim();
        }
        
        return words;
    }
    
    // Check if input contains any of the specified keywords
    public bool ContainsAnyKeyword(string input, string[] keywords)
    {
        if (string.IsNullOrEmpty(input) || keywords == null || keywords.Length == 0)
            return false;
            
        input = input.ToLower();
        
        for (int i = 0; i < keywords.Length; i++)
        {
            string keyword = keywords[i].ToLower();
            if (!string.IsNullOrEmpty(keyword) && input.Contains(keyword))
            {
                return true;
            }
        }
        
        return false;
    }
}
