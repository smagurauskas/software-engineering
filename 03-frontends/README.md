# React client

## Run

Three terminals:

```bash
dotnet run --project 02-http-api/01-rest --urls http://localhost:5080
```

```bash
dotnet run --project 02-http-api/02-graphql --urls http://localhost:5090
```

```bash
npm install && npm run dev
```

Open <http://localhost:5173>.

## Exercises

1. Make the GraphQL path lazy: a small list query, then a second query for the selected book only.
   Compare requests and bytes against both current versions.
2. Give the REST path an `include=author,editions` query parameter and implement it in the API. You
   have now invented a worse GraphQL, which is the usual road to it.
3. Delete a book in one tab, then open it in the other. Explain what each path does.
4. Point the GraphQL path at `booksNPlusOne` instead of `books` and read the server console.
