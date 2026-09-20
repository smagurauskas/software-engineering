const history = []

export class RestError extends Error {
  constructor(status, problem) {
    super(problem?.title ?? `The request failed with status ${status}.`)

    this.status = status
    this.problem = problem
  }
}

export function getHistory() {
  return [...history]
}

export function clearHistory() {
  history.length = 0
}

async function request(path, { method = 'GET', body, contentType = 'application/json' } = {}) {
  const sentAt = new Date()
  const startedAt = performance.now()

  const response = await fetch(`/api${path}`, {
    method,
    headers: body === undefined ? undefined : { 'Content-Type': contentType },
    body: body === undefined ? undefined : JSON.stringify(body),
  })

  const text = await response.text()

  history.push({
    label: `${method} /api${path}`,
    status: response.status,
    bytes: new Blob([text]).size,
    ms: Math.round(performance.now() - startedAt),
    at: sentAt,
  })

  if (!response.ok) {
    throw new RestError(response.status, text ? JSON.parse(text) : null)
  }

  return text ? JSON.parse(text) : null
}

export const getAuthors = () => request('/authors')

export const getBooks = () => request('/books?sort=-year')

export const getBooksOfAuthor = (authorId) => request(`/authors/${authorId}/books`)

export const getBook = (id) => request(`/books/${id}`)

export const getAuthor = (id) => request(`/authors/${id}`)

export const getEditions = (bookId) => request(`/books/${bookId}/editions`)

export const createBook = (book) => request('/books', { method: 'POST', body: book })

export const deleteBook = (id) => request(`/books/${id}`, { method: 'DELETE' })
