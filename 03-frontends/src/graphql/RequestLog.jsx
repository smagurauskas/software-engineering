function clockTime(value) {
  const stamp = new Date(value)
  const milliseconds = String(stamp.getMilliseconds()).padStart(3, '0')

  return `${stamp.toLocaleTimeString([], { hour12: false })}.${milliseconds}`
}

export default function RequestLog({ requests, onReset }) {
  const bytes = requests.reduce((total, request) => total + request.bytes, 0)

  return (
    <section className="log">
      <h2>
        HTTP requests: <strong>{requests.length}</strong>
        <span className="total"> {bytes} bytes received</span>
        <button type="button" onClick={onReset}>
          Reset
        </button>
      </h2>

      <ol>
        {requests.map((request, index) => (
          <li key={index}>
            <time className="stamp">{clockTime(request.at)}</time>
            <code>{request.label}</code>
            <span className="meta">
              {request.status} · {request.bytes} B · {request.ms} ms
            </span>
          </li>
        ))}
      </ol>
    </section>
  )
}
