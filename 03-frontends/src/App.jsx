import { useState } from 'react'

import GraphQlCatalog from './graphql/GraphQlCatalog.jsx'
import RestCatalog from './rest/RestCatalog.jsx'

export default function App() {
  const [path, setPath] = useState('rest')

  return (
    <main>
      <header>
        <h1>Catalog</h1>
        <p>
          The same screen twice. <code>src/rest</code> talks to the REST API from 02-http-api,{' '}
          <code>src/graphql</code> talks to the GraphQL one. The two folders share no code.
        </p>

        <nav>
          <button
            type="button"
            className={path === 'rest' ? 'selected' : ''}
            onClick={() => setPath('rest')}
          >
            REST
          </button>
          <button
            type="button"
            className={path === 'graphql' ? 'selected' : ''}
            onClick={() => setPath('graphql')}
          >
            GraphQL
          </button>
        </nav>
      </header>

      {path === 'rest' ? <RestCatalog /> : <GraphQlCatalog />}
    </main>
  )
}
