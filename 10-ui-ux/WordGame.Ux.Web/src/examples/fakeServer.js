// The examples in the slides have no backend: `fetch` is replaced by this in-memory
// server. It answers the routes of the testing lecture's API (for the "before" game)
// and the /api/v2 routes the upgraded game needs. The same idea as the test doubles of the
// testing lecture, one level up: the whole server is the double.
//
// A scenario can make the server misbehave on purpose: answer slowly, fail,
// forget its games or use a short time limit.

const german = { hello: 'hallo', world: 'welt', game: 'spiel' }
const experiencePerWord = 10

const defaults = {
  latencyMs: 120,
  timeLimitSeconds: 300,
  weekend: false,
  loseGames: false, // the server "restarts" right after a game is started
  failSubmits: 0, // this many submits fail as if the network was down
}

const json = (status, data) =>
  new Response(JSON.stringify(data ?? {}), { status, headers: { 'Content-Type': 'application/json' } })

const isCorrect = (word, answer) => german[word] === (answer ?? '').trim().toLowerCase()

export function installFakeServer(options) {
  const config = { ...defaults, ...options }
  const users = new Map()
  const gamesByEmail = new Map() // lecture 7: one game per player
  const gamesById = new Map() // v2: any number of games, no email needed
  let nextId = 1

  const multiplier = () => (config.weekend ? 2 : 1)
  const expired = (game) => Date.now() - game.startedAt > config.timeLimitSeconds * 1000
  const addExperience = (email, amount) => {
    users.set(email, (users.get(email) ?? 0) + amount)
    return users.get(email)
  }
  const rules = () => ({
    wordsPerGame: 3,
    experiencePerWord,
    timeLimitSeconds: config.timeLimitSeconds,
    experienceMultiplier: multiplier(),
    bonusReason: config.weekend ? 'Weekend: double XP' : null,
  })

  function handle(method, path, body) {
    const words = Object.keys(german)
    let match

    // The routes of the testing lecture
    if (method === 'POST' && path === '/api/games') {
      if (!config.loseGames) gamesByEmail.set(body.playerEmail, { words, startedAt: Date.now() })
      return json(200, { words })
    }
    if (method === 'POST' && path === '/api/games/complete') {
      const game = gamesByEmail.get(body.playerEmail)
      const success = !!game && !expired(game) && game.words.every((word, i) => isCorrect(word, body.translations[i]))
      if (!success) return json(200, { success: false, experienceGained: 0 })
      gamesByEmail.delete(body.playerEmail)
      const gained = words.length * experiencePerWord * multiplier()
      addExperience(body.playerEmail, gained)
      return json(200, { success: true, experienceGained: gained })
    }
    if (method === 'GET' && (match = path.match(/^\/api\/users\/(.+)$/))) {
      const email = decodeURIComponent(match[1])
      return users.has(email) ? json(200, { email, experience: users.get(email) }) : json(404)
    }

    // The routes of the upgraded game
    if (method === 'GET' && path === '/api/v2/rules') {
      return json(200, rules())
    }
    if (method === 'POST' && path === '/api/v2/games') {
      const gameId = `game-${nextId++}`
      if (!config.loseGames) gamesById.set(gameId, { words, startedAt: Date.now(), attempts: 0, won: null })
      return json(200, { gameId, words, rules: rules() })
    }
    if (method === 'POST' && (match = path.match(/^\/api\/v2\/games\/([^/]+)\/answers$/))) {
      const game = gamesById.get(match[1])
      if (!game || game.won) return json(404)
      if (expired(game)) return json(410)
      game.attempts++
      const results = game.words.map((word, i) => isCorrect(word, body.translations[i]))
      const allCorrect = results.every(Boolean)
      let experienceGained = 0
      let totalExperience = null
      if (allCorrect) {
        experienceGained = words.length * experiencePerWord * multiplier()
        game.won = { experience: experienceGained, claimed: !!body.playerEmail }
        if (body.playerEmail) totalExperience = addExperience(body.playerEmail, experienceGained)
      }
      return json(200, {
        allCorrect,
        results,
        experienceGained,
        totalExperience,
        attempts: game.attempts,
        secondsTaken: Math.round((Date.now() - game.startedAt) / 1000),
      })
    }
    if (method === 'POST' && (match = path.match(/^\/api\/v2\/games\/([^/]+)\/claim$/))) {
      const game = gamesById.get(match[1])
      if (!game?.won || game.won.claimed) return json(404)
      game.won.claimed = true
      return json(200, { totalExperience: addExperience(body.playerEmail, game.won.experience) })
    }
    return json(404)
  }

  window.fetch = async (url, init = {}) => {
    const method = init.method ?? 'GET'
    const isSubmit = /\/(complete|answers)$/.test(url)
    await new Promise((resolve) => setTimeout(resolve, config.latencyMs))
    if (isSubmit && config.failSubmits > 0) {
      config.failSubmits--
      throw new TypeError('Failed to fetch')
    }
    return handle(method, url, init.body ? JSON.parse(init.body) : {})
  }

  return config
}
