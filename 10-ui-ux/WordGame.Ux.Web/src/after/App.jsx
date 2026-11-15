import { useEffect, useState } from 'react'
import { api } from './api.js'
import Game from './Game.jsx'
import { loadEmail, saveEmail, useIsSlow } from './hooks.js'
import Intro from './Intro.jsx'
import Result from './Result.jsx'

const problems = {
  network: 'Could not reach the server. Check the connection and try again.',
  server: 'Something went wrong on our side. Try again in a moment.',
}

export default function App() {
  const [email, setEmail] = useState(loadEmail)
  const [experience, setExperience] = useState(null) // saved on the server, known once there is an email
  const [unsaved, setUnsaved] = useState([]) // games won before an email was given
  const [rules, setRules] = useState(null)
  const [phase, setPhase] = useState('intro') // intro | playing | won | expired | gone
  const [game, setGame] = useState(null)
  const [deadline, setDeadline] = useState(null)
  const [result, setResult] = useState(null)
  const [starting, setStarting] = useState(false)
  const [problem, setProblem] = useState(null)
  const slow = useIsSlow(starting)

  useEffect(() => {
    // The rules are nice to have: the game is playable without them
    api.rules().then(setRules, () => {})
  }, [])

  useEffect(() => {
    if (email) api.experience(email).then(setExperience, () => {})
  }, [email])

  // Nothing is asked before the first game: the player sees what the game is
  // before being asked for anything.
  async function start() {
    setStarting(true)
    setProblem(null)
    try {
      const started = await api.startGame()
      setGame(started)
      setRules(started.rules)
      setDeadline(Date.now() + started.rules.timeLimitSeconds * 1000)
      setPhase('playing')
    } catch (error) {
      setProblem(error.kind)
    } finally {
      setStarting(false)
    }
  }

  function won(response) {
    setResult(response)
    if (response.totalExperience === null) {
      setUnsaved([...unsaved, { gameId: game.gameId, experience: response.experienceGained }])
    } else {
      setExperience(response.totalExperience)
    }
    setPhase('won')
  }

  // The email is asked when it has a visible purpose: there is experience to keep
  async function save(newEmail) {
    let total = 0
    for (const { gameId } of unsaved) {
      total = (await api.claim(gameId, newEmail)).totalExperience
    }
    saveEmail(newEmail)
    setEmail(newEmail)
    setExperience(total)
    setUnsaved([])
  }

  function forget() {
    saveEmail('')
    setEmail('')
    setExperience(null)
  }

  const unsavedExperience = unsaved.reduce((sum, won) => sum + won.experience, 0)
  const total = (experience ?? 0) + unsavedExperience
  const startButton = (label) => (
    <button className="primary" onClick={start} disabled={starting} autoFocus>
      {starting ? 'Starting…' : label}
    </button>
  )
  const startProblem = (
    <>
      {slow && <p className="hint">Still working…</p>}
      {problem && (
        <p className="alert" role="alert">
          {problems[problem]}
        </p>
      )}
    </>
  )

  return (
    <main>
      <header>
        <h1>Word game</h1>
        {email && (
          <p className="player">
            {email} · {experience ?? '…'} XP ·{' '}
            <button className="link" onClick={forget}>
              Not you?
            </button>
          </p>
        )}
      </header>

      {phase === 'intro' && (
        <Intro rules={rules}>
          {startButton(problem ? 'Try again' : 'Play')}
          {startProblem}
        </Intro>
      )}

      {phase === 'playing' && (
        <Game
          key={game.gameId}
          game={game}
          deadline={deadline}
          email={email}
          onWon={won}
          onEnded={setPhase}
          onNewWords={start}
        />
      )}
      {phase === 'playing' && startProblem}

      {phase === 'won' && (
        <Result result={result} rules={rules} total={total} saved={unsaved.length === 0} onSave={save}>
          {startButton(problem ? 'Try again' : 'Play again')}
          {startProblem}
        </Result>
      )}

      {phase === 'expired' && (
        <section className="card">
          <h2>Time ran out</h2>
          <p>These words can no longer be checked. No experience was lost.</p>
          {startButton('New game')}
          {startProblem}
        </section>
      )}

      {phase === 'gone' && (
        <section className="card">
          <h2>This game is no longer available</h2>
          <p>The server does not know this game any more, it may have been restarted. Saved experience is safe.</p>
          {startButton('New game')}
          {startProblem}
        </section>
      )}
    </main>
  )
}
