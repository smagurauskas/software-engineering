import { useState } from 'react'

async function post(url, body) {
  const response = await fetch(url, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  })
  return response.json()
}

export default function App() {
  const [email, setEmail] = useState('')
  const [words, setWords] = useState([])
  const [translations, setTranslations] = useState({})
  const [result, setResult] = useState(null)
  const [experience, setExperience] = useState(null)

  async function loadExperience() {
    const response = await fetch(`/api/users/${encodeURIComponent(email)}`)
    setExperience(response.ok ? (await response.json()).experience : 0)
  }

  async function startGame(event) {
    event.preventDefault()
    const game = await post('/api/games', { playerEmail: email })
    setWords(game.words)
    setTranslations({})
    setResult(null)
    await loadExperience()
  }

  async function submit(event) {
    event.preventDefault()
    const response = await post('/api/games/complete', {
      playerEmail: email,
      translations: words.map((word) => translations[word] ?? ''),
    })
    setResult(response)
    if (response.success) {
      setWords([])
    }
    await loadExperience()
  }

  return (
    <main>
      <h1>Word game</h1>
      <p>Translate the English words to German.</p>

      <form onSubmit={startGame}>
        <label>
          Email
          <input type="email" required value={email} onChange={(e) => setEmail(e.target.value)} />
        </label>
        <button type="submit">Start game</button>
      </form>

      {words.length > 0 && (
        <form onSubmit={submit}>
          {words.map((word) => (
            <label key={word}>
              {word}
              <input
                value={translations[word] ?? ''}
                onChange={(e) => setTranslations({ ...translations, [word]: e.target.value })}
              />
            </label>
          ))}
          <button type="submit">Submit</button>
        </form>
      )}

      {result && (
        <p role="status">
          {result.success
            ? `Correct! +${result.experienceGained} XP`
            : 'Some translations are wrong, try again'}
        </p>
      )}

      {experience !== null && <p>Total experience: {experience} XP</p>}
    </main>
  )
}
