# Blazor WebAssembly client

## Run

Two terminals, API first:

```bash
dotnet run --project 02-http-api/01-rest --urls http://localhost:5080
```

```bash
dotnet run --project 03-frontends/blazor
```

Open <http://localhost:5200>.

## Exercises

1. Move `Pages/Catalog.razor` into two components, a list and a details panel, passing data with
   `[Parameter]`.
2. Replace the hand written form with `EditForm` plus `DataAnnotationsValidator` and decide whether
   client side validation should replace or complement the 400 from the API.
3. Publish it with `dotnet publish` and look at the size of `wwwroot/_framework`. Compare with the
   React bundle and discuss what that means for a first visit.
4. Point `HttpClient` at the GraphQL API and try to make this screen work with one request. Notice
   how much of the model layer you have to hand write, then look at what `03-frontends/react` does instead.
