// Every way a request can fail gets a name, so the user interface can say what
// happened and what to do next - instead of one message for everything.
export class ApiError extends Error {
  constructor(kind) {
    super(kind)
    this.kind = kind // 'network' | 'expired' | 'gone' | 'server'
  }
}

async function request(url, options) {
  let response
  try {
    response = await fetch(url, options)
  } catch {
    throw new ApiError('network') // offline, DNS, the server is down
  }
  if (response.status === 410) throw new ApiError('expired')
  if (response.status === 404) throw new ApiError('gone')
  if (!response.ok) throw new ApiError('server')
  return response.json()
}

function post(url, body) {
  return request(url, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body ?? {}),
  })
}

export const api = {
  rules: () => request('/api/v2/rules'),

  startGame: () => post('/api/v2/games'),

  submitAnswers: (gameId, translations, playerEmail) =>
    post(`/api/v2/games/${gameId}/answers`, { translations, playerEmail }),

  claim: (gameId, playerEmail) => post(`/api/v2/games/${gameId}/claim`, { playerEmail }),

  // A player who has not finished a game yet does not exist: that is 0 XP, not an error
  async experience(playerEmail) {
    try {
      return (await request(`/api/users/${encodeURIComponent(playerEmail)}`)).experience
    } catch (error) {
      if (error.kind === 'gone') return 0
      throw error
    }
  },
}
