const history = []

export class GraphQlError extends Error {
  constructor(errors) {
    super(errors.map((error) => error.message).join(' '))

    this.errors = errors
  }
}

export function getHistory() {
  return [...history]
}

export function clearHistory() {
  history.length = 0
}

async function send(label, query, variables = {}) {
  const sentAt = new Date()
  const startedAt = performance.now()

  const response = await fetch('/graphql', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ query, variables }),
  })

  const text = await response.text()

  history.push({
    label: `POST /graphql (${label})`,
    status: response.status,
    bytes: new Blob([text]).size,
    ms: Math.round(performance.now() - startedAt),
    at: sentAt,
  })

  const payload = text ? JSON.parse(text) : null

  if (payload?.errors) {
    throw new GraphQlError(payload.errors)
  }

  return payload.data
}

const catalogQuery = `
  query Catalog {
    authors {
      id
      name
      country
    }
    books {
      id
      title
      year
      author {
        id
        name
        country
      }
      editions {
        id
        format
        pages
        year
      }
    }
  }
`

const addBookMutation = `
  mutation AddBook($title: String!, $authorId: UUID!, $year: Int!) {
    addBook(title: $title, authorId: $authorId, year: $year) {
      id
    }
  }
`

const deleteBookMutation = `
  mutation DeleteBook($id: UUID!) {
    deleteBook(id: $id)
  }
`

export const loadCatalog = () => send('catalog', catalogQuery)

export const addBook = (book) => send('addBook', addBookMutation, book)

export const removeBook = (id) => send('deleteBook', deleteBookMutation, { id })
