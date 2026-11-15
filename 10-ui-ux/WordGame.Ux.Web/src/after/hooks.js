import { useEffect, useState } from 'react'

// Seconds left until the deadline, updated every second.
// The server sends a duration, not a time of day: the clocks of the server and
// of the player's device do not have to agree.
export function useSecondsLeft(deadline) {
  const [now, setNow] = useState(Date.now)

  useEffect(() => {
    if (deadline === null) return
    setNow(Date.now())
    const timer = setInterval(() => setNow(Date.now()), 250)
    return () => clearInterval(timer)
  }, [deadline])

  return deadline === null ? null : Math.max(0, Math.ceil((deadline - now) / 1000))
}

// True when `busy` has lasted longer than a second.
// Under 0.1 s a response feels instant, under 1 s the flow of thought is kept;
// after that the user needs to see that the system is still working.
export function useIsSlow(busy, thresholdMs = 1000) {
  const [slow, setSlow] = useState(false)

  useEffect(() => {
    if (!busy) {
      setSlow(false)
      return
    }
    const timer = setTimeout(() => setSlow(true), thresholdMs)
    return () => clearTimeout(timer)
  }, [busy, thresholdMs])

  return slow
}

// The email is remembered on the device: recognition instead of recall.
// Storage can be unavailable (private mode, embedded pages), the game works without it.
const emailKey = 'wordgame-email'

export function loadEmail() {
  try {
    return localStorage.getItem(emailKey) ?? ''
  } catch {
    return ''
  }
}

export function saveEmail(email) {
  try {
    localStorage.setItem(emailKey, email)
  } catch {
    // Not remembered, the player is asked again next time
  }
}

export function formatTime(seconds) {
  return `${Math.floor(seconds / 60)}:${String(seconds % 60).padStart(2, '0')}`
}
