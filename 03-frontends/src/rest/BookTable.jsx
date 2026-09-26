export default function BookTable({ books, authors, selectedBookId, onSelect, onDelete }) {
  if (books.length === 0) {
    return <p className="empty">No books here yet.</p>
  }

  const nameById = new Map(authors.map((author) => [author.id, author.name]))

  return (
    <table>
      <thead>
        <tr>
          <th>Title</th>
          <th>Author</th>
          <th>Year</th>
          <th>ISBN</th>
          <th></th>
        </tr>
      </thead>
      <tbody>
        {books.map((book) => (
          <tr key={book.id} className={book.id === selectedBookId ? 'selected' : ''}>
            <td>
              <button type="button" className="link" onClick={() => onSelect(book.id)}>
                {book.title}
              </button>
            </td>
            <td>{nameById.get(book.authorId) ?? 'unknown'}</td>
            <td>{book.year}</td>
            <td className="isbn">{book.isbn}</td>
            <td>
              <button type="button" className="danger" onClick={() => onDelete(book.id)}>
                Delete
              </button>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  )
}
