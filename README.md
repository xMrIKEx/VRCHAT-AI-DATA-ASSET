# VRCHAT-AI-DATA-ASSET

# 🧠 UdonSharp AI Bot (Fixed & Optimized)

A fully restructured and VRChat-compatible version of the Open Source AI Bot using **UdonSharp**.

This bot simulates AI interactions using local logic, keyword analysis, sentiment estimation, and in-world memory — without relying on external APIs or unsupported C# features.

---

## ✅ Why This Version?

The original AI Bot was **incompatible with UdonSharp** due to the use of:
- HTTP requests to external services like OpenAI
- Unsupported C# features:
  - Dictionaries
  - LINQ
  - Async/await
  - JSON
  - Lambda expressions

This rewritten version works **natively in VRChat** using local logic and **UdonSharp 1.x or 2.x**, and includes memory, parsing, and rule-based response features.

---

## 🛠️ Implementation Breakdown

### 🔹 `AIBotUdon.cs`
- Main AI Controller Script
- Handles:
  - User input
  - Response display
  - Scroll view history
  - Submits to `TextParser` and `ResponseManager`

### 🔹 `ResponseManager.cs`
- Manages keyword-based response logic
- Stores keyword-response mappings
- Selects appropriate fallback responses if no match
- Supports basic sentiment-based responses

### 🔹 `TextParser.cs`
- Lightweight text analysis tool
- Splits player input into:
  - Words
  - Keywords
  - Sentiment indicators
- Filters out stop words and detects basic input types

### 🔹 `MemorySystem.cs`
- Stores in-world memory and player-specific memory
- Two types:
  - **General memory** (topic, content, importance)
  - **User memory** (rotating slots per user)
- Can be adapted for basic **persistence using synced variables**
- Includes logic to:
  - Retrieve topic-based memories
  - Replace low-importance entries
  - Maintain overflow via FIFO policy

---

## 🔧 Setup Instructions

### 📦 Requirements
- Unity 2019 or later
- VRChat SDK 3 (UDON World)
- UdonSharp (latest version)

### 🛠 Steps
1. Create a new Unity world project
2. Import:
   - VRChat SDK 3 (UDON)
   - UdonSharp package
3. Add the following scripts:
   - `AIBotUdon.cs`
   - `ResponseManager.cs`
   - `TextParser.cs`
   - `MemorySystem.cs`
4. Create UI Elements in a Canvas:
   - InputField (user input)
   - Button (submit)
   - Text area or Scroll View (chat history)
5. Link all components in the Inspector:
   - Assign references inside `AIBotUdon.cs`
   - Add keyword groups to `ResponseManager`
6. Test in VRChat!

---

## 🧠 Persistence (Optional)

To enable **basic persistent memory across instances**, modify `MemorySystem.cs` to use `[UdonSynced]` variables and sync data over the network.

Example:
```csharp
[UdonSynced(UdonSyncMode.None)] private string[] syncedMemoryTopics;
