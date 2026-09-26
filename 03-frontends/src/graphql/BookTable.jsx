export default function BookTable({ books, selectedBookId, onSelect, onDelete }) {
  if (books.length === 0) {
    return <p className="empty">No books here yet.</p>
  }

  return (
    <table>
      <thead>
        <tr>
          <th>Title</th>
          <th>Author</th>
          <th>Year</th>
          <th>Editions</th>
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
            <td>{book.author?.name ?? 'unknown'}</td>
            <td>{book.year}</td>
            <td>{book.editions.length}</td>
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
