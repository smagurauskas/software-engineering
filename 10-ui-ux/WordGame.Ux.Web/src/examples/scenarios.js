// One entry per example in the lecture: how the fake server behaves, and the steps
// that bring each version of the game to the moment worth showing.
//
//   server  options of the fake server from the start
//   then    options applied after the steps (so the steps themselves stay fast)

const startBefore = [
  ['fill', 'Email', 'ada@example.com'],
  ['click', 'Start game'],
]
const startAfter = [['click', 'Play']]

const answers = (hello, world, game) =>
  [
    ['fill', 'hello', hello],
    ['fill', 'world', world],
    ['fill', 'game', game],
  ].filter(([, , value]) => value !== '')

const both = (steps) => ({ before: [...startBefore, ...steps], after: [...startAfter, ...steps] })

export const scenarios = {
  // Nothing done yet: what a new player sees
  'first-screen': { before: [], after: [] },

  // A started game, nothing typed
  started: both([]),

  // One typo, ready to submit
  'wrong-word': both(answers('hallo', 'wlet', 'spiel')),

  // All answers are correct, but the server has lost the game (it keeps games in memory)
  'lost-game': { server: { loseGames: true }, ...both(answers('hallo', 'welt', 'spiel')) },

  // 20 seconds instead of 5 minutes, so the limit can be watched running out
  'time-limit': { server: { timeLimitSeconds: 20 }, ...both(answers('hallo', 'welt', '')) },

  // Two answers typed: what happens when a new game is asked for?
  restart: both(answers('hallo', 'welt', '')),

  // The server takes 2.5 s to answer, and the first submit fails
  'slow-network': { then: { latencyMs: 2500, failSubmits: 1 }, ...both(answers('hallo', 'welt', 'spiel')) },

  // All correct, ready to submit: how does a game end?
  ending: both(answers('hallo', 'welt', 'spiel')),

  // A Saturday: experience is doubled. Who knows?
  weekend: { server: { weekend: true }, before: startBefore, after: [] },

  // Play with the keyboard only
  keyboard: { before: [['fill', 'Email', 'ada@example.com']], after: [] },
}
