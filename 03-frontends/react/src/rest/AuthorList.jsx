export default function AuthorList({ authors, selectedAuthorId, onSelect }) {
  return (
    <ul className="authors">
      <li>
        <button
          type="button"
          className={selectedAuthorId === null ? 'selected' : ''}
          onClick={() => onSelect(null)}
        >
          All books
        </button>
      </li>

      {authors.map((author) => (
        <li key={author.id}>
          <button
            type="button"
            className={selectedAuthorId === author.id ? 'selected' : ''}
            onClick={() => onSelect(author.id)}
          >
            {author.name}
            <span className="country">{author.country}</span>
          </button>
        </li>
      ))}
    </ul>
  )
}
