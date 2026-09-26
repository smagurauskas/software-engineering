import { useEffect, useState } from "react";

import AuthorList from "./AuthorList.jsx";
import BookDetails from "./BookDetails.jsx";
import BookTable from "./BookTable.jsx";
import NewBookForm from "./NewBookForm.jsx";
import RequestLog from "./RequestLog.jsx";
import {
  GraphQlError,
  addBook,
  clearHistory,
  getHistory,
  loadCatalog,
  removeBook,
} from "./graphqlClient.js";

const unreachable =
  "The GraphQL API did not answer. Is it running on http://localhost:5090?";

function describe(reason) {
  if (reason instanceof GraphQlError) {
    return reason.message;
  }

  return unreachable;
}

export default function GraphQlCatalog() {
  const [authors, setAuthors] = useState([]);
  const [books, setBooks] = useState([]);
  const [selectedAuthorId, setSelectedAuthorId] = useState(null);
  const [selectedBookId, setSelectedBookId] = useState(null);
  const [requests, setRequests] = useState([]);
  const [error, setError] = useState(null);
  const [version, setVersion] = useState(0);

  useEffect(() => {
    let ignore = false;

    loadCatalog()
      .then((data) => {
        if (ignore) {
          return;
        }

        setAuthors(data.authors);
        setBooks(data.books);
        setError(null);
      })
      .catch((reason) => {
        if (!ignore) {
          setError(describe(reason));
        }
      })
      .finally(() => {
        if (!ignore) {
          setRequests(getHistory());
        }
      });

    return () => {
      ignore = true;
    };
  }, [version]);

  const visibleBooks =
    selectedAuthorId === null
      ? books
      : books.filter((book) => book.author.id === selectedAuthorId);

  const selectedBook = books.find((book) => book.id === selectedBookId) ?? null;

  async function handleCreate(book) {
    try {
      await addBook(book);
    } finally {
      setRequests(getHistory());
    }

    setVersion((current) => current + 1);
  }

  async function handleDelete(id) {
    try {
      await removeBook(id);

      setSelectedBookId(null);
      setVersion((current) => current + 1);
    } catch (reason) {
      setError(describe(reason));
    } finally {
      setRequests(getHistory());
    }
  }

  function reset() {
    clearHistory();
    setSelectedBookId(null);
    setRequests([]);
    setVersion((current) => current + 1);
  }

  return (
    <div>
      <p className="lead">
        One query asks for the whole graph: books, their author and their
        editions. Filtering by author and opening a title work on data that is
        already in memory, so the counter below stops moving.
      </p>

      {error && <p className="error">{error}</p>}

      <div className="columns">
        <section>
          <h2>Authors</h2>
          <AuthorList
            authors={authors}
            selectedAuthorId={selectedAuthorId}
            onSelect={(id) => {
              setSelectedBookId(null);
              setSelectedAuthorId(id);
            }}
          />
        </section>

        <section>
          <h2>Books</h2>
          <BookTable
            books={visibleBooks}
            selectedBookId={selectedBookId}
            onSelect={setSelectedBookId}
            onDelete={handleDelete}
          />

          <BookDetails book={selectedBook} />

          <h2>Add a book</h2>
          <NewBookForm authors={authors} onCreate={handleCreate} />
        </section>
      </div>

      <RequestLog requests={requests} onReset={reset} />
    </div>
  );
}
