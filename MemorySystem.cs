using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class MemorySystem : UdonSharpBehaviour
{
    [Header("Memory Settings")]
    [SerializeField] private int maxMemoryEntries = 20;
    [SerializeField] private int maxUserMemories = 5;
    
    [System.Serializable]
    public class MemoryEntry
    {
        public string topic;
        public string content;
        public float importance;
    }
    
    // Arrays to store general memories
    private string[] memoryTopics;
    private string[] memoryContents;
    private float[] memoryImportance;
    private int memoryCount = 0;
    
    // Arrays to store user-specific memories
    private string[] userIds;
    private string[][] userMemoryContents;
    private int[] userMemoryCount;
    private int userCount = 0;
    
    void Start()
    {
        InitializeMemoryArrays();
    }
    
    private void InitializeMemoryArrays()
    {
        // Initialize general memory arrays
        memoryTopics = new string[maxMemoryEntries];
        memoryContents = new string[maxMemoryEntries];
        memoryImportance = new float[maxMemoryEntries];
        
        // Initialize user memory arrays
        userIds = new string[maxMemoryEntries];
        userMemoryContents = new string[maxMemoryEntries][];
        userMemoryCount = new int[maxMemoryEntries];
        
        // Initialize each user's memory array
        for (int i = 0; i < maxMemoryEntries; i++)
        {
            userMemoryContents[i] = new string[maxUserMemories];
        }
    }
    
    // Store a new general memory
    public void StoreMemory(string topic, string content, float importance)
    {
        if (string.IsNullOrEmpty(topic) || string.IsNullOrEmpty(content))
            return;
            
        // Check if we already have this topic
        int existingIndex = FindMemoryIndex(topic);
        
        if (existingIndex >= 0)
        {
            // Update existing memory
            memoryContents[existingIndex] = content;
            memoryImportance[existingIndex] = importance;
        }
        else if (memoryCount < maxMemoryEntries)
        {
            // Add new memory
            memoryTopics[memoryCount] = topic;
            memoryContents[memoryCount] = content;
            memoryImportance[memoryCount] = importance;
            memoryCount++;
        }
        else
        {
            // Replace least important memory
            int leastImportantIndex = FindLeastImportantMemoryIndex();
            memoryTopics[leastImportantIndex] = topic;
            memoryContents[leastImportantIndex] = content;
            memoryImportance[leastImportantIndex] = importance;
        }
    }
    
    // Find memory by topic
    public string RecallMemory(string topic)
    {
        int index = FindMemoryIndex(topic);
        if (index >= 0)
        {
            return memoryContents[index];
        }
        return null;
    }
    
    // Store user-specific memory
    public void StoreUserMemory(string userId, string content)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(content))
            return;
            
        // Find user index or create new
        int userIndex = FindUserIndex(userId);
        
        if (userIndex < 0 && userCount < maxMemoryEntries)
        {
            // New user
            userIndex = userCount;
            userIds[userCount] = userId;
            userMemoryCount[userCount] = 0;
            userCount++;
        }
        else if (userIndex < 0)
        {
            // Too many users, can't store
            return;
        }
        
        // Add memory for this user
        if (userMemoryCount[userIndex] < maxUserMemories)
        {
            // Add new memory
            userMemoryContents[userIndex][userMemoryCount[userIndex]] = content;
            userMemoryCount[userIndex]++;
        }
        else
        {
            // Shift memories to make room for new one
            for (int i = 0; i < maxUserMemories - 1; i++)
            {
                userMemoryContents[userIndex][i] = userMemoryContents[userIndex][i + 1];
            }
            
            // Add new memory at the end
            userMemoryContents[userIndex][maxUserMemories - 1] = content;
        }
    }
    
    // Get all memories for a user
    public string[] GetUserMemories(string userId)
    {
        int userIndex = FindUserIndex(userId);
        if (userIndex < 0)
            return new string[0];
            
        // Create result array with the correct size
        string[] result = new string[userMemoryCount[userIndex]];
        
        // Copy memories
        for (int i = 0; i < userMemoryCount[userIndex]; i++)
        {
            result[i] = userMemoryContents[userIndex][i];
        }
        
        return result;
    }
    
    // Helper methods
    private int FindMemoryIndex(string topic)
    {
        for (int i = 0; i < memoryCount; i++)
        {
            if (memoryTopics[i] == topic)
            {
                return i;
            }
        }
        return -1;
    }
    
    private int FindLeastImportantMemoryIndex()
    {
        if (memoryCount <= 0)
            return 0;
            
        int leastImportantIndex = 0;
        float lowestImportance = memoryImportance[0];
        
        for (int i = 1; i < memoryCount; i++)
        {
            if (memoryImportance[i] < lowestImportance)
            {
                lowestImportance = memoryImportance[i];
                leastImportantIndex = i;
            }
        }
        
        return leastImportantIndex;
    }
    
    private int FindUserIndex(string userId)
    {
        for (int i = 0; i < userCount; i++)
        {
            if (userIds[i] == userId)
            {
                return i;
            }
        }
        return -1;
    }
    
    // Get most important memories (limited by count)
    public string[] GetImportantMemories(int count)
    {
        if (memoryCount <= 0)
            return new string[0];
            
        int resultSize = Mathf.Min(count, memoryCount);
        string[] result = new string[resultSize];
        
        // Create temporary arrays for sorting
        int[] indices = new int[memoryCount];
        for (int i = 0; i < memoryCount; i++)
        {
            indices[i] = i;
        }
        
        // Simple bubble sort by importance (descending)
        for (int i = 0; i < memoryCount - 1; i++)
        {
            for (int j = 0; j < memoryCount - i - 1; j++)
            {
                if (memoryImportance[indices[j]] < memoryImportance[indices[j + 1]])
                {
                    // Swap
                    int temp = indices[j];
                    indices[j] = indices[j + 1];
                    indices[j + 1] = temp;
                }
            }
        }
        
        // Copy top memories to result
        for (int i = 0; i < resultSize; i++)
        {
            int index = indices[i];
            result[i] = memoryTopics[index] + ": " + memoryContents[index];
        }
        
        return result;
    }
}
