import { useState } from 'react'

import { RestError } from './restClient.js'

const emptyDraft = { title: '', authorId: '', year: '', isbn: '' }

export default function NewBookForm({ authors, onCreate }) {
  const [draft, setDraft] = useState(emptyDraft)
  const [fieldErrors, setFieldErrors] = useState({})
  const [message, setMessage] = useState(null)
  const [isSaving, setIsSaving] = useState(false)

  function update(field, value) {
    setDraft((current) => ({ ...current, [field]: value }))
  }

  async function handleSubmit(event) {
    event.preventDefault()

    setIsSaving(true)
    setFieldErrors({})
    setMessage(null)

    try {
      await onCreate({
        title: draft.title,
        authorId: draft.authorId,
        year: Number(draft.year),
        isbn: draft.isbn,
      })

      setDraft(emptyDraft)
      setMessage('Created.')
    } catch (reason) {
      if (reason instanceof RestError && reason.problem?.errors) {
        setFieldErrors(reason.problem.errors)
      } else if (reason instanceof RestError) {
        setMessage(`${reason.status} ${reason.problem?.detail ?? reason.message}`)
      } else {
        setMessage('The REST API did not answer.')
      }
    } finally {
      setIsSaving(false)
    }
  }

  function errorsFor(field) {
    const messages = fieldErrors[field]

    return messages ? <span className="field-error">{messages.join(' ')}</span> : null
  }

  return (
    <form onSubmit={handleSubmit}>
      <label>
        Title
        <input value={draft.title} onChange={(event) => update('title', event.target.value)} />
        {errorsFor('Title')}
      </label>

      <label>
        Author
        <select
          required
          value={draft.authorId}
          onChange={(event) => update('authorId', event.target.value)}
        >
          <option value="">Pick an author</option>
          {authors.map((author) => (
            <option key={author.id} value={author.id}>
              {author.name}
            </option>
          ))}
        </select>
        {errorsFor('AuthorId')}
      </label>

      <label>
        Year
        <input
          type="number"
          value={draft.year}
          onChange={(event) => update('year', event.target.value)}
        />
        {errorsFor('Year')}
      </label>

      <label>
        ISBN
        <input value={draft.isbn} onChange={(event) => update('isbn', event.target.value)} />
        {errorsFor('Isbn')}
      </label>

      <button type="submit" disabled={isSaving}>
        {isSaving ? 'Saving' : 'Create book'}
      </button>

      {message && <p className="message">{message}</p>}
    </form>
  )
}
