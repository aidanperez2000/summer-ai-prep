import { useEffect, useRef, useState } from 'react'
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

type SuggestedRewrite = {
  tone: string;
  rewrittenText: string;
  explanation: string;
}

type AnalyzeToneResponse = {
  signals: Signal[];
  interpretations: Interpretation[];
  ambiguities?: string[];
  suggestedRewrites: SuggestedRewrite[];
}

function App() {
  const loadingStages = [
    { atMs: 0, message: 'Sending your request…' },
    { atMs: 1500, message: 'Reading conversation context…' },
    { atMs: 3500, message: 'Interpreting tone signals…' },
    { atMs: 6000, message: 'Building nuanced interpretations…' },
    { atMs: 9000, message: 'Finalizing response…' }
  ]

  const [text, setText] = useState('')
  const [conversationContext, setConversationContext] = useState('')
  const [relationshipType, setRelationshipType] = useState('')
  const [result, setResult] = useState<AnalyzeToneResponse | null>(null)
  const [loading, setLoading] = useState(false)
  const [progress, setProgress] = useState(0)
  const [loadingMessage, setLoadingMessage] = useState(loadingStages[0].message)
  const [error, setError] = useState<string | null>(null)
  const progressIntervalRef = useRef<number | null>(null)

  function clearProgressInterval() {
    if (progressIntervalRef.current !== null) {
      window.clearInterval(progressIntervalRef.current)
      progressIntervalRef.current = null
    }
  }

  function startProgressSimulation() {
    const startedAt = Date.now()
    setProgress(8)
    setLoadingMessage(loadingStages[0].message)

    clearProgressInterval()
    const stagesDescending = [...loadingStages].reverse()
    progressIntervalRef.current = window.setInterval(() => {
      const elapsedMs = Date.now() - startedAt
      const computedProgress = Math.min(92, 10 + elapsedMs / 120)
      const nextStage = stagesDescending.find(stage => elapsedMs >= stage.atMs)
      setProgress(computedProgress)
      if (nextStage) {
        setLoadingMessage(nextStage.message)
      }
    }, 200)
  }

  useEffect(() => {
    return () => {
      clearProgressInterval()
    }
  }, [])

  async function analyzeTone() {
    setLoading(true)
    setProgress(0)
    startProgressSimulation()
    setError(null)
    setResult(null)
    try {
      const analyzeUrl = import.meta.env.VITE_TONELENS_ANALYZE_URL ?? 'http://localhost:5136/api/ToneAnalysis/analyze'
      const response = await axios.post<AnalyzeToneResponse>(analyzeUrl, {
        text,
        conversationContext,
        relationshipType
      })
      clearProgressInterval()
      setProgress(100)
      setLoadingMessage('Done!')
      setResult(response.data)
    } catch (err) {
      setError('Failed to analyze tone. Please try again.')
    } finally {
      clearProgressInterval()
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
        {loading && (
          <div className="loading-panel" aria-live="polite" aria-busy="true">
            <div className="progress-meta">
              <span>{loadingMessage}</span>
              <span>{Math.round(progress)}%</span>
            </div>
            <div 
              className="progress-track"
              role="progressbar"
              aria-label="Analysis progress"
              aria-valuemin={0}
              aria-valuemax={100}
              aria-valuenow={Math.round(progress)}
              aria-valuetext={loadingMessage}>
              <div className="progress-fill" style={{ width: `${progress}%` }} />
            </div>
            <p className="loading-hint">Tip: Longer messages and context can take a few extra seconds.</p>
          </div>
        )}
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
          {result.suggestedRewrites && result.suggestedRewrites.length > 0 && (
            <>
              <h3>Suggested Rewrites:</h3>
              <ul>
                {result.suggestedRewrites.map((rewrite, index) => (
                  <li key={index}>
                    <strong>{rewrite.tone} Tone:</strong> {rewrite.explanation}
                    <br />
                    <em>Rewritten Text: {rewrite.rewrittenText}</em>
                  </li>
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
