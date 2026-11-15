import { useEffect, useRef, useState } from 'react'
import { formatTime } from './hooks.js'

const experiencePerLevel = 100

// Progress towards the next level: the closer a goal looks, the more effort
// people put into reaching it, so the distance left is what is shown.
function Level({ total }) {
  const level = Math.floor(total / experiencePerLevel) + 1
  const inLevel = total % experiencePerLevel

  return (
    <div className="level">
      <div className="level-text">
        <span>Level {level}</span>
        <span>
          {experiencePerLevel - inLevel} XP to level {level + 1}
        </span>
      </div>
      <div className="bar" role="progressbar" aria-valuemin={0} aria-valuemax={experiencePerLevel} aria-valuenow={inLevel}>
        <div style={{ width: `${inLevel}%` }} />
      </div>
    </div>
  )
}

function SaveExperience({ onSave }) {
  const [email, setEmail] = useState('')
  const [busy, setBusy] = useState(false)
  const [failed, setFailed] = useState(false)

  async function submit(event) {
    event.preventDefault()
    setBusy(true)
    setFailed(false)
    try {
      await onSave(email)
    } catch {
      setFailed(true)
      setBusy(false)
    }
  }

  return (
    <form className="save" onSubmit={submit}>
      <label htmlFor="email">Email, to keep this experience</label>
      <div className="save-row">
        <input id="email" type="email" required autoComplete="email" value={email} onChange={(e) => setEmail(e.target.value)} />
        <button type="submit" className="secondary" disabled={busy}>
          {busy ? 'Saving…' : 'Save'}
        </button>
      </div>
      {failed && (
        <p className="alert" role="alert">
          Could not save. The experience is still here: try again.
        </p>
      )}
    </form>
  )
}

// The last screen of a game is what the player remembers. It sums up the game,
// shows what was earned and puts the next game one key press away.
export default function Result({ result, rules, total, saved, onSave, children }) {
  const section = useRef(null)

  // Saving removes the email form, and the focus with it: it is put on the next step
  useEffect(() => {
    if (saved) section.current.querySelector('button.primary')?.focus()
  }, [saved])

  return (
    <section className="card result" ref={section}>
      <div role="status">
        <h2>All {result.results.length} correct!</h2>
      </div>
      <p className="gain">
        +{result.experienceGained} XP
        {rules.bonusReason && <span className="bonus">{rules.bonusReason}</span>}
      </p>
      <p className="hint">
        {formatTime(result.secondsTaken)} · {result.attempts === 1 ? 'first try' : `${result.attempts} tries`}
      </p>
      <Level total={total} />
      {children}
      {!saved && <SaveExperience onSave={onSave} />}
    </section>
  )
}
