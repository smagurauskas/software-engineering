import { useEffect, useRef, useState } from 'react'
import { api } from './api.js'
import { formatTime, useIsSlow, useSecondsLeft } from './hooks.js'
import { Rules } from './Intro.jsx'

const problems = {
  network: 'Could not reach the server. Your answers are kept: check the connection and try again.',
  server: 'Something went wrong on our side. Your answers are kept: try again in a moment.',
}

const marks = {
  correct: '✓ Correct',
  wrong: '✗ Not quite',
  missing: 'Missing',
}

export default function Game({ game, deadline, email, onWon, onEnded, onNewWords }) {
  const { words, rules } = game
  const [answers, setAnswers] = useState(() => words.map(() => ''))
  const [states, setStates] = useState(() => words.map(() => null)) // correct | wrong | missing | null
  const [busy, setBusy] = useState(false)
  const [problem, setProblem] = useState(null)
  const [confirming, setConfirming] = useState(false)
  const inputs = useRef([])
  const secondsLeft = useSecondsLeft(deadline)
  const slow = useIsSlow(busy)

  // The first field is ready for typing: no click is needed to begin
  useEffect(() => inputs.current[0]?.focus(), [])

  // The player is told the moment the time is over, not after typing and submitting
  useEffect(() => {
    if (secondsLeft === 0) onEnded('expired')
  }, [secondsLeft])

  function change(index, value) {
    setAnswers(answers.with(index, value))
    setStates(states.with(index, null))
  }

  function focusFirst(found) {
    inputs.current[found.findIndex(Boolean)]?.focus()
  }

  async function submit(event) {
    event.preventDefault()
    if (busy) return

    // An empty field cannot be correct: it is caught here, without a request
    const missing = answers.map((answer) => answer.trim() === '')
    if (missing.some(Boolean)) {
      setStates(states.map((state, index) => (missing[index] ? 'missing' : state)))
      focusFirst(missing)
      return
    }

    setBusy(true)
    setProblem(null)
    try {
      const response = await api.submitAnswers(game.gameId, answers, email || null)
      if (response.allCorrect) {
        onWon(response)
        return
      }
      // Every word gets its own result and the cursor is moved to the first one to fix
      setStates(response.results.map((correct) => (correct ? 'correct' : 'wrong')))
      focusFirst(response.results.map((correct) => !correct))
    } catch (error) {
      if (error.kind === 'expired' || error.kind === 'gone') {
        onEnded(error.kind)
      } else {
        setProblem(error.kind) // the typed answers stay on the screen
      }
    } finally {
      setBusy(false)
    }
  }

  // Enter moves to the next word and submits only on the last one,
  // so the whole game can be played without leaving the keyboard
  function keyDown(event, index) {
    if (event.key !== 'Enter') return
    const next = states.findIndex((state, i) => i > index && state !== 'correct')
    if (next !== -1) {
      event.preventDefault()
      inputs.current[next].focus()
    }
  }

  // Typed answers are never thrown away silently: the player is asked first,
  // but only when there is something to lose
  function newWords() {
    if (answers.some((answer) => answer.trim() !== '')) {
      setConfirming(true)
    } else {
      onNewWords()
    }
  }

  const correct = states.filter((state) => state === 'correct').length
  const wrong = states.filter((state) => state === 'wrong').length
  const low = secondsLeft <= rules.timeLimitSeconds * 0.2

  return (
    <form className="card" onSubmit={submit} noValidate>
      <Rules rules={rules} />

      <div className={low ? 'timer low' : 'timer'}>
        <span role="timer" aria-label="Time left">
          {formatTime(secondsLeft)} left
        </span>
        <div className="bar">
          <div style={{ width: `${(secondsLeft / rules.timeLimitSeconds) * 100}%` }} />
        </div>
      </div>

      {words.map((word, index) => (
        <div className={`word ${states[index] ?? ''}`} key={word}>
          <label htmlFor={`word-${index}`}>{word}</label>
          <input
            id={`word-${index}`}
            ref={(element) => (inputs.current[index] = element)}
            value={answers[index]}
            onChange={(event) => change(index, event.target.value)}
            onKeyDown={(event) => keyDown(event, index)}
            readOnly={states[index] === 'correct'}
            aria-invalid={states[index] === 'wrong' || states[index] === 'missing'}
            aria-describedby={`mark-${index}`}
            lang="de"
            autoComplete="off"
            autoCapitalize="off"
            spellCheck={false}
            enterKeyHint={index === words.length - 1 ? 'done' : 'next'}
          />
          {/* Never colour alone: a symbol and a word say the same */}
          <span className="mark" id={`mark-${index}`}>
            {marks[states[index]]}
          </span>
        </div>
      ))}

      <p className="status" role="status">
        {wrong > 0 && `${correct} of ${words.length} correct. Fix the marked ${wrong === 1 ? 'word' : 'words'} and check again.`}
        {slow && 'Still checking…'}
      </p>

      {problem && (
        <p className="alert" role="alert">
          {problems[problem]}
        </p>
      )}

      {confirming ? (
        <div className="confirm" role="alertdialog" aria-label="Discard answers">
          <p>The typed answers will be lost.</p>
          <button type="button" className="secondary" onClick={onNewWords} autoFocus>
            Discard and get new words
          </button>
          <button type="button" className="primary" onClick={() => setConfirming(false)}>
            Keep playing
          </button>
        </div>
      ) : (
        <div className="actions">
          <button type="submit" className="primary" disabled={busy}>
            {busy ? 'Checking…' : problem ? 'Try again' : 'Check answers'}
          </button>
          <button type="button" className="link" onClick={newWords}>
            New words
          </button>
        </div>
      )}
    </form>
  )
}
