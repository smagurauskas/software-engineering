import { createRoot } from 'react-dom/client'
import BeforeApp from '../../../../07-testing/WordGame.Web/src/App.jsx'
import beforeStyles from '../../../../07-testing/WordGame.Web/src/styles.css?inline'
import AfterApp from '../after/App.jsx'
import afterStyles from '../after/styles.css?inline'
import { run } from './driver.js'
import { installFakeServer } from './fakeServer.js'
import { scenarios } from './scenarios.js'

// Runs inside an iframe of a slide. assets/ux-example.lua sets `window.uxExample`
// to the example to show: { name: 'wrong-word', variant: 'before' | 'after' }.
const { name, variant } = window.uxExample
const scenario = scenarios[name]
const App = variant === 'before' ? BeforeApp : AfterApp

const style = document.createElement('style')
style.textContent = `${variant === 'before' ? beforeStyles : afterStyles}
  /* The frame in a slide is small: both versions are scaled down by the same factor */
  html { zoom: 0.78; }
  main { margin-top: 1rem; }
  .example-reset {
    all: unset; position: fixed; top: 4px; right: 6px; z-index: 10; cursor: pointer;
    font: 16px system-ui; color: #55606b; padding: 2px 6px; border-radius: 4px;
  }
  .example-reset:hover { background: #dfe3e8; }`
document.head.append(style)

const container = document.createElement('div')
const reset = document.createElement('button')
reset.className = 'example-reset'
reset.textContent = '↻'
reset.title = 'Start this example again'
reset.type = 'button'
document.body.append(reset, container)

let root

async function start() {
  root?.unmount()
  if (!scenario) {
    container.textContent = `Unknown example: ${name}`
    return
  }
  const server = installFakeServer(scenario.server)
  root = createRoot(container)
  root.render(<App />)
  await run(scenario[variant])
  Object.assign(server, scenario.then)
  // The slide deck keeps the keyboard until the presenter clicks into the example
  document.activeElement?.blur()
  window.parent.postMessage({ uxExample: 'ready' }, '*')
}

reset.addEventListener('click', start)

// Keys that move between slides are always passed to the deck, even while a text
// field is focused: the presenter must never have to click out of an example to go
// on. (The cursor cannot be moved with the arrows inside a field - not needed here.)
// Escape gives the keyboard back to the deck without changing the slide.
const deckKeys = ['ArrowRight', 'ArrowLeft', 'ArrowDown', 'ArrowUp', 'PageDown', 'PageUp', 'Escape']

window.addEventListener('keydown', (event) => {
  if (deckKeys.includes(event.key)) {
    event.preventDefault()
    window.parent.postMessage({ uxExample: 'key', key: event.key }, '*')
  }
})

start()
