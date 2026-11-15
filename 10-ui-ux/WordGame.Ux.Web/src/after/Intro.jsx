import { formatTime } from './hooks.js'

// The rules are shown before the player commits to a game, in the words of the
// player ("10 XP per word"), not of the code (ExperiencePerWord = 10).
export function Rules({ rules }) {
  if (!rules) return null

  return (
    <ul className="rules">
      <li>{rules.wordsPerGame} words</li>
      <li>{rules.experiencePerWord * rules.experienceMultiplier} XP per word</li>
      <li>{formatTime(rules.timeLimitSeconds)} to answer</li>
      {rules.bonusReason && <li className="bonus">{rules.bonusReason}</li>}
    </ul>
  )
}

export default function Intro({ rules, children }) {
  return (
    <section className="card">
      <p className="lead">Translate English words to German.</p>
      <Rules rules={rules} />
      {children}
    </section>
  )
}
