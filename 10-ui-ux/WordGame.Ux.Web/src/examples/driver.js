// Brings an example to the moment worth showing (a started game, typed answers),
// by using the page like a player would: the same steps work for both versions.

const sleep = (ms) => new Promise((resolve) => setTimeout(resolve, ms))

async function find(lookup) {
  for (let attempt = 0; attempt < 100; attempt++) {
    const element = lookup()
    if (element) return element
    await sleep(30)
  }
  return null
}

const labelled = (text) => () =>
  [...document.querySelectorAll('label')].find((label) => label.textContent.trim() === text)?.control

const button = (text) => () =>
  [...document.querySelectorAll('button')].find((b) => b.textContent.trim() === text && !b.disabled)

// React listens to `input` events and ignores a value that was only assigned
function type(input, value) {
  Object.getOwnPropertyDescriptor(HTMLInputElement.prototype, 'value').set.call(input, value)
  input.dispatchEvent(new Event('input', { bubbles: true }))
}

export async function run(steps) {
  for (const [action, target, value] of steps) {
    if (action === 'fill') {
      const input = await find(labelled(target))
      if (input) type(input, value)
    } else if (action === 'click') {
      const element = await find(button(target))
      element?.click()
    }
  }
  await sleep(50)
}
