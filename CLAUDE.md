# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

A multi-store point-of-sale system: two ASP.NET Core (net10.0) APIs behind an Ocelot gateway, a Next.js 16 web app, SQL Server, and RabbitMQ. `docs/REQUIREMENTS.md` is the spec — 57 numbered requirements, each with its unhappy paths and status codes. **Read the relevant requirement before changing behaviour, and add a requirement there before building something new.** The tests are written against those numbered rules.

## Commands

```bash
dotnet build PosSystem.sln
dotnet test                                    # both test projects
dotnet test tests/Pos.Application.Tests        # unit tests only (no database needed)
dotnet test --filter "FullyQualifiedName~OrderServiceTests"
dotnet test --filter "FullyQualifiedName~A_cashiers_cash_sale_is_completed_at_once_and_the_receipt_shows_the_change"
dotnet format                                  # run on the files a change touched, before committing
```

Web app (`src/web`):

```bash
npm install && npm run dev      # http://localhost:3000, needs GATEWAY_URL in .env.local
npm run lint
npm run format                  # prettier --write .
npm run build
```

Whole system:

```bash
cp .env.example .env && docker compose up --build
```

Web on `:3000`, gateway `:5000`, POS API `:5024`, Notification API `:5026`, RabbitMQ management `:15672`. `docker compose down` keeps the named volumes `pos-db-data` and `pos-rabbitmq-data`; `-v` deletes them.

EF Core migrations (both APIs call `MigrateAsync()` at startup, so there is normally no `database update` step):

```bash
dotnet ef migrations add <Name> --project src/Pos.Infrastructure --startup-project src/Pos.Api
dotnet ef migrations add <Name> --project src/Notification.Api --startup-project src/Notification.Api
```

### Running locally without Docker

Nothing secret is in committed files. Locally the values come from user-secrets, in Docker from the environment variables in `docker-compose.yml`, and when deployed from Azure Key Vault if `KeyVault:Url` is set. Both APIs throw at startup if a value is missing or too short.

```bash
dotnet user-secrets --project src/Pos.Api set "ConnectionStrings:PosDatabase" "Server=(localdb)\MSSQLLocalDB;Database=PosDb;Trusted_Connection=True;TrustServerCertificate=True"
dotnet user-secrets --project src/Pos.Api set "Jwt:Secret" "<at least 32 characters>"
dotnet user-secrets --project src/Pos.Api set "Seed:AdminPassword" "<at least 8 characters>"
dotnet user-secrets --project src/Pos.Api set "Seed:CashierPassword" "<at least 8 characters>"
dotnet user-secrets --project src/Notification.Api set "ConnectionStrings:NotificationDatabase" "Server=(localdb)\MSSQLLocalDB;Database=NotificationDb;Trusted_Connection=True;TrustServerCertificate=True"
dotnet user-secrets --project src/Notification.Api set "Jwt:Secret" "<the same secret as the POS API>"
```

`src/web/.env.local` needs `GATEWAY_URL=http://localhost:5000`.

The POS API runs fine without RabbitMQ (publishing failures are logged and swallowed). The Notification API deliberately **stops** when it cannot reach RabbitMQ, relying on Docker's restart policy — expect it to exit if you run it locally with no broker.

### Test gotchas

- `Pos.Api.Tests` boots the real API in memory via `PosApiFactory`, which sets environment variables (they beat user-secrets) pointing at `(localdb)\MSSQLLocalDB;Database=PosDbTest`. It needs SQL Server LocalDB, and it migrates and seeds that database on first run.
- Test parallelization is disabled assembly-wide (`CollectionBehaviorSettings.cs`) because the classes share that one database.
- Integration tests log in as the seeded staff (`admin@riversidemart.test` / `cashier@riversidemart.test`) with the passwords `PosApiFactory` puts in the environment (`Admin#Test2026`, `Cashier#Test2026`).
- The two checkout tests stock a product as the admin before they sell it, because that database is seeded once and kept: a test that simply bought the first product would drain the shelf run by run and eventually fail with 409.
- `Pos.Application.Tests` mocks the repository interfaces with Moq — no database, no broker.

## Architecture

### Request path

Browser → Next.js server (`src/web`) → gateway (`:5000`) → POS API or Notification API → SQL Server. The browser never calls the gateway directly: a page calls `callWebApi`/`readFromApi`, which attach the token from localStorage as a Bearer header and call the web app's own route handlers under `src/web/app/api/*`; those read the header with `readBearerToken(request)` and forward the call with `callPosApi`. A page's own reads go through one proxy, `app/api/gateway/[...path]/route.ts` (GET only, prefixes `pos` and `notifications`); the handful of route handlers that existed before it, such as `app/api/notifications/route.ts` for the 30-second poll, still read through `callPosApi` themselves. `lib/callPosApi.ts` is the only file that knows `GATEWAY_URL`.

Ocelot maps `/pos/{everything}` → POS API `/api/{everything}` and `/notifications/{everything}` → Notification API `/api/{everything}`, passing `Authorization` through untouched. `ocelot.json` is used locally, `ocelot.Docker.json` when `ASPNETCORE_ENVIRONMENT=Docker` — one whole file is selected on purpose (`Program.cs`), never merged.

### POS API layering (strict, one direction)

`Pos.Api` (controllers, middleware) → `Pos.Application` (services, DTOs, interfaces, exceptions) → `Pos.Domain` (entities, enums). `Pos.Infrastructure` implements the Application interfaces (EF repositories, bcrypt hasher, JWT creator, RabbitMQ publisher, seed loader) and is referenced only by `Pos.Api` for DI registration in `Program.cs`. Application never references Infrastructure or ASP.NET Core.

Controllers do three things: read claims, call one service method, return a status code. All business rules live in the services; all data access lives behind an `I*Repository`. The Notification API is deliberately flat — its controller uses `NotificationDbContext` directly, because two queries do not justify a service and a repository.

### Store scoping (multi-tenant)

Every user, category, product, and order belongs to one store. `storeId` **always** comes from the token's `storeId` claim via `CurrentUserClaims.GetStoreId(User)` in the controller, never from a body or query string, and is passed to the service as a plain parameter. Repositories filter by it, so another store's row returns 404, not 403. Any new tenant-owned query must carry `storeId` the same way.

### Errors

Services throw `ValidationException` / `UnauthorizedException` / `ForbiddenException` / `NotFoundException` / `ConflictException`; `ErrorHandlingMiddleware` turns each into its status code with a `{ "message": "..." }` body, and anything else into a logged 500 with "Something went wrong". Bare 401/403 written by `[Authorize]` get the same body. DataAnnotations failures are reshaped by `ValidationErrorResponseFactory`. **Every error the API returns has that one shape** — do not return `ProblemDetails` or a bare string. Middleware order is fixed: request logging outermost, then error handling.

### Checkout (the one place with real ordering rules)

`OrderService.PlaceOrderAsync` validates and prices every line, checks the cash covers the total, reduces stock on the tracked `Product` entities, then does a single `SaveChangesAsync` in `OrderRepository.SaveNewOrderAsync` so order and stock are written together or not at all. Order items copy the product's name and price at the time of sale, and the total is computed by the API, never taken from the request. Messages are published only after the save returns. A cashier's or admin's order is a walk-in sale: status `Completed` immediately, no `order.placed` message (the buyer has no account), and a cash sale requires `AmountTendered`.

### Messaging

One durable queue, `notifications`. `RabbitMqNotificationMessagePublisher` (singleton, one connection/channel for the app's life) publishes persistent JSON for `order.placed` and `stock.low`; a publish failure is logged and never undoes the order. `NotificationMessagesConsumer` (a `BackgroundService` in the Notification API) saves one row per message and acknowledges only after the save, with `prefetchCount: 1` and `autoAck: false`.

### One token, two services

The POS API issues an HS256 JWT (id, email, role, `storeId`; 24 hours). Both APIs validate issuer, audience, lifetime, key, and pin `ValidAlgorithms` to HS256. The `Jwt:Secret` must be identical across both — that is why compose passes one `JWT_SECRET` to both.

### Web app

Next.js App Router. Every page that needs the token is a client component, because the token lives in the browser's localStorage under `pos_token` (the JWT) and `pos_user` (name and role for the navbar) - see `lib/loginTokenStorage.ts`, the only file that touches those keys. Such a page calls `useRequireLogin([...roles])` at the top and renders nothing until `isReady`; that is convenience only, the API enforces roles regardless. `useRequireLogin` and `NavBar` read storage with `useSyncExternalStore`, so logging in or out - in this tab or another - updates them without a reload. `/register` is the one page still rendered on the server (`export const dynamic = "force-dynamic"`, because it fetches the store list). After a write, a component calls the prop the page gave it - `onProductChanged`, `onCategoryChanged`, `onStatusChanged`, or `onSaved` for the product form - and the page fetches again; `router.refresh()` is no use now that the data is fetched in the browser. Redux Toolkit holds only the cart, in the browser, created once per tab by `StoreProvider`. Tailwind v4 for styling. `next.config.ts` uses `output: "standalone"` for the Docker image.

## Conventions

The code is written in a deliberately plain, spelled-out style. Match it:

- Explicit types everywhere in C#, never `var`. Explicit `if (x == false)` instead of `!x`. Named intermediate booleans (`bool hasEnoughStock = ...`) instead of conditions inline. No expression-bodied methods, no LINQ query chains where a `foreach` reads more plainly.
- File-scoped namespaces; `System` usings first, import groups separated by a blank line (enforced by `.editorconfig`).
- Comments explain *why this design*, not what the line does. Non-obvious classes open with a short numbered "How X works here" block, and cite the docs page they follow (`// Learned from: https://learn.microsoft.com/...`).
- TypeScript mirrors this: named `function` declarations, `props` destructured on the first line of the body, `=== false` comparisons, one type per file under `lib/types/`.
- Forms: a visible label per input, the API's `message` shown as plain text under the form, submit disabled while the request is in flight.
- Commits are one small change with an imperative sentence describing the behaviour ("Require the amount tendered on a walk-in cash sale"), and features go test-first: a commit "Add a failing test for ..." immediately before the commit that makes it pass, then `dotnet format` on the touched files.
