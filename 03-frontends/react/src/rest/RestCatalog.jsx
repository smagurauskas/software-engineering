import { useEffect, useState } from 'react'

import AuthorList from './AuthorList.jsx'
import BookDetails from './BookDetails.jsx'
import BookTable from './BookTable.jsx'
import NewBookForm from './NewBookForm.jsx'
import RequestLog from './RequestLog.jsx'
import {
  RestError,
  clearHistory,
  createBook,
  deleteBook,
  getAuthor,
  getAuthors,
  getBook,
  getBooks,
  getBooksOfAuthor,
  getEditions,
  getHistory,
} from './restClient.js'

const unreachable = 'The REST API did not answer. Is it running on http://localhost:5080?'

function describe(reason) {
  if (reason instanceof RestError && reason.status < 502) {
    return `${reason.status} ${reason.message}`
  }

  return unreachable
}

export default function RestCatalog() {
  const [authors, setAuthors] = useState([])
  const [books, setBooks] = useState([])
  const [selectedAuthorId, setSelectedAuthorId] = useState(null)
  const [details, setDetails] = useState(null)
  const [requests, setRequests] = useState([])
  const [error, setError] = useState(null)
  const [version, setVersion] = useState(0)

  useEffect(() => {
    let ignore = false

    Promise.all([
      getAuthors(),
      selectedAuthorId === null ? getBooks() : getBooksOfAuthor(selectedAuthorId),
    ])
      .then(([loadedAuthors, loadedBooks]) => {
        if (ignore) {
          return
        }

        setAuthors(loadedAuthors)
        setBooks(loadedBooks)
        setError(null)
      })
      .catch((reason) => {
        if (!ignore) {
          setError(describe(reason))
        }
      })
      .finally(() => {
        if (!ignore) {
          setRequests(getHistory())
        }
      })

    return () => {
      ignore = true
    }
  }, [selectedAuthorId, version])

  async function showDetails(id) {
    setDetails(null)

    try {
      const book = await getBook(id)
      const author = await getAuthor(book.authorId)
      const editions = await getEditions(id)

      setDetails({ book, author, editions })
      setError(null)
    } catch (reason) {
      setError(describe(reason))
    } finally {
      setRequests(getHistory())
    }
  }

  async function handleCreate(book) {
    try {
      await createBook(book)
    } finally {
      setRequests(getHistory())
    }

    setVersion((current) => current + 1)
  }

  async function handleDelete(id) {
    try {
      await deleteBook(id)

      setDetails(null)
      setVersion((current) => current + 1)
    } catch (reason) {
      setError(describe(reason))
    } finally {
      setRequests(getHistory())
    }
  }

  function reset() {
    clearHistory()
    setDetails(null)
    setRequests([])
    setVersion((current) => current + 1)
  }

  return (
    <div>
      <p className="lead">
        Every screen needs its own endpoint. A book carries <code>authorId</code>, not the author, so
        the details panel has to walk the graph one request at a time.
      </p>

      {error && <p className="error">{error}</p>}

      <div className="columns">
        <section>
          <h2>Authors</h2>
          <AuthorList
            authors={authors}
            selectedAuthorId={selectedAuthorId}
            onSelect={(id) => {
              setDetails(null)
              setSelectedAuthorId(id)
            }}
          />
        </section>

        <section>
          <h2>Books</h2>
          <BookTable
            books={books}
            authors={authors}
            selectedBookId={details?.book.id ?? null}
            onSelect={showDetails}
            onDelete={handleDelete}
          />

          <BookDetails details={details} />

          <h2>Add a book</h2>
          <NewBookForm authors={authors} onCreate={handleCreate} />
        </section>
      </div>

      <RequestLog requests={requests} onReset={reset} />
    </div>
  )
}
