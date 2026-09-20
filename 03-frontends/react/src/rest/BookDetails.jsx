export default function BookDetails({ details }) {
  if (details === null) {
    return <p className="empty">Pick a title to load its author and editions.</p>
  }

  const { book, author, editions } = details

  return (
    <div className="details">
      <h3>{book.title}</h3>
      <p>
        {author.name}, {author.country}, {book.year}
      </p>

      <ul>
        {editions.map((edition) => (
          <li key={edition.id}>
            {edition.format}, {edition.pages} pages, {edition.year}
          </li>
        ))}
      </ul>

      {editions.length === 0 && <p className="empty">No editions on record.</p>}
    </div>
  )
}
