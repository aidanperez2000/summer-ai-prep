import { useState } from 'react'
import axios from 'axios'
import './App.css'

type Signal = {
  name: string;
  strength: number;
  explanation: string;
}

type Interpretation = {
  interpretationText: string;
  confidenceScore: number;
  reasoning: string;
}

type AnalyzeToneResponse = {
  signals: Signal[];
  interpretations: Interpretation[];
  ambiguities?: string[];
}

function App() {
  const [text, setText] = useState('')
  const [conversationContext, setConversationContext] = useState('')
  const [relationshipType, setRelationshipType] = useState('')
  const [result, setResult] = useState<AnalyzeToneResponse | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  async function analyzeTone() {
    setLoading(true)
    setError(null)
    try {
      const analyzeUrl = import.meta.env.VITE_TONELENS_ANALYZE_URL ?? 'http://localhost:5136/api/ToneAnalysis/analyze'
      const response = await axios.post<AnalyzeToneResponse>(analyzeUrl, {
        text,
        conversationContext,
        relationshipType
      })
      setResult(response.data)
    } catch (err) {
      setError('Failed to analyze tone. Please try again.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <main className="page">
      <section className='card'>
        <h1>ToneLens</h1>
        <p>
          Analyze the tone of your messages and get insights to improve your communication. Enter your message, provide some context about the conversation and relationship, and let ToneLens help you understand the underlying signals and interpretations.
        </p>
        <label>
          Message:
          <textarea value={text} onChange={e => setText(e.target.value)} />
        </label>
        <label>
          Conversation Context:
          <textarea value={conversationContext} onChange={e => setConversationContext(e.target.value)} />
        </label>
        <label>
          Relationship Type:
          <input type="text" value={relationshipType} onChange={e => setRelationshipType(e.target.value)} />
        </label>
        <button onClick={analyzeTone} disabled={loading || text.trim().length === 0}>{loading ? 'Analyzing...' : 'Analyze Tone'}</button>
      </section>
      {error && <div className="error">{error}</div>}
      {result && (
        <section className='card'>
          <h2>Analysis Results</h2>
          <h3>Signals Detected:</h3>
          <ul>
            {result.signals.map((signal, index) => (
              <li key={index}>
                <strong>{signal.name}</strong> (Strength: {signal.strength}): {signal.explanation}
              </li>
            ))}
          </ul>
          <h3>Interpretations:</h3>
          <ul>
            {result.interpretations.map((interpretation, index) => (
              <li key={index}>
                <strong>Confidence Score: {interpretation.confidenceScore}</strong>: {interpretation.interpretationText}
                <br />
                <em>Reasoning: {interpretation.reasoning}</em>
              </li>
            ))}
          </ul>
          {result.ambiguities && result.ambiguities.length > 0 && (
            <>
              <h3>Ambiguities Detected:</h3>
              <ul>
                {result.ambiguities.map((ambiguity, index) => (
                  <li key={index}>{ambiguity}</li>
                ))}
              </ul>
            </>
          )}
        </section>
      )}
    </main>
  )
}

export default App
