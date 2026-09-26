export default function BookDetails({ book }) {
  if (book === null) {
    return <p className="empty">Pick a title. No request will be sent.</p>
  }

  return (
    <div className="details">
      <h3>{book.title}</h3>
      <p>
        {book.author?.name ?? 'unknown'}, {book.author?.country ?? 'unknown'}, {book.year}
      </p>

      <ul>
        {book.editions.map((edition) => (
          <li key={edition.id}>
            {edition.format}, {edition.pages} pages, {edition.year}
          </li>
        ))}
      </ul>

      {book.editions.length === 0 && <p className="empty">No editions on record.</p>}
    </div>
  )
}
