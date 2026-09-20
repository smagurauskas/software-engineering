import { useState } from 'react'

import { GraphQlError } from './graphqlClient.js'

const emptyDraft = { title: '', authorId: '', year: '' }

export default function NewBookForm({ authors, onCreate }) {
  const [draft, setDraft] = useState(emptyDraft)
  const [message, setMessage] = useState(null)
  const [isSaving, setIsSaving] = useState(false)

  function update(field, value) {
    setDraft((current) => ({ ...current, [field]: value }))
  }

  async function handleSubmit(event) {
    event.preventDefault()

    setIsSaving(true)
    setMessage(null)

    try {
      await onCreate({
        title: draft.title,
        authorId: draft.authorId,
        year: Number(draft.year),
      })

      setDraft(emptyDraft)
      setMessage('Created.')
    } catch (reason) {
      setMessage(reason instanceof GraphQlError ? reason.message : 'The GraphQL API did not answer.')
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <form onSubmit={handleSubmit}>
      <label>
        Title
        <input
          required
          value={draft.title}
          onChange={(event) => update('title', event.target.value)}
        />
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
      </label>

      <label>
        Year
        <input
          required
          type="number"
          value={draft.year}
          onChange={(event) => update('year', event.target.value)}
        />
      </label>

      <button type="submit" disabled={isSaving}>
        {isSaving ? 'Saving' : 'Create book'}
      </button>

      {message && <p className="message">{message}</p>}
    </form>
  )
}
