# ToneLens

 ToneLens is a tool designed to analyze the tone of a given text. It allows users to input text, provide conversation context, and relationship information to get a comprehensive analysis of the tone used in the text. The tool can be used for various applications, including improving communication skills, understanding emotional undertones, and enhancing writing style.

## Features

- **Text Input**: Users can input any text they want to analyze.
- **Conversation Context**: Users can provide additional context about the conversation to get a more accurate tone analysis.
- **Relationship Information**: Users can specify the relationship between the parties involved in the conversation to tailor the tone analysis accordingly.
- **Tone Analysis**: The tool analyzes the tone of the text and provides insights into the emotional undertones, sentiment, and overall tone used in the communication.
- **Interpretation**: The tool offers interpretations of the tone analysis, helping users understand the implications of their communication style.
- **Ambiguity Detection**: The tool can identify ambiguous language and provide suggestions for clearer communication.
- **Suggested Rewrites**: Based on the tone analysis, the tool can suggest rewrites to improve the tone of the text.

## How to Run

To run ToneLens, follow these steps:

1. Clone the repository:

   ```bash
   git clone https://github.com/aidanperez2000/summer-ai-prep.git
   ```

2. Navigate to the ToneLens directory:

   ```bash
   cd summer-ai-prep/mini_projects/tonelens
   ```

3. Run Ollama server and pull the model used by the API (`qwen3`):

   ```bash
   ollama pull qwen3
   ``` 

   Then start the Ollama server:

   ```bash
   ollama serve
   ```

4. Open up a new terminal.

5. cd into the ToneLens.Api directory:

   ```bash
   cd summer-ai-prep/mini_projects/tonelens/backend/ToneLens.Api
   ```

6. Run the API:

   ```bash
   dotnet run
   ```

7. Open up a new terminal.

8. cd into the ToneLens.Frontend directory:

   ```bash
   cd summer-ai-prep/mini_projects/tonelens/frontend/tonelens-frontend
   ```

9. Install dependencies:

   ```bash
   npm install
   ```

10. Run the frontend:

    ```bash
    npm run dev
    ```
