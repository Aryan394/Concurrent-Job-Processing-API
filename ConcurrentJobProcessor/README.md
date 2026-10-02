# Concurrent Job Processor

An ASP.NET Core API that accepts jobs over HTTP, stores them in PostgreSQL, and processes them with a fixed pool of background workers. Submitting a job returns immediately. The work runs afterward, and clients check status by job id.

## How a job moves through the system

1. `POST /api/jobs` creates a `Job` with a new id, status `Queued`, and the current UTC time. `JobStore` writes that row to the `Jobs` table.
2. The same request then places the job on an in-memory queue (`Channel<Job>`). The HTTP call returns the saved job.
3. Three workers, started with the application, wait on that queue. `Constants.WorkerCount` sets the pool size. A free worker takes the next job.
4. The worker sets the status to `Processing`, records `StartedAt`, and saves the row.
5. Processing is a stand-in for real work: the worker waits five seconds. The payload is stored and returned with the job; it is not executed.
6. On success the worker sets status `Completed`, records `CompletedAt` and `LastUpdatedAt`, and saves the row. On an exception it sets status `Failed`, stores `ErrorMessage`, and saves the row.
7. `GET /api/jobs/{id}` reads the current row so a client can see whether the job is still queued, running, finished, or failed.

Workers share one queue and run at the same time, so up to three jobs are in `Processing` together. Further jobs stay in the channel until a worker is free.

```text
POST /api/jobs
      |
      v
  Jobs table (Queued) ----> in-memory channel
                                  |
                    +-------------+-------------+
                    v             v             v
                Worker 1      Worker 2      Worker 3
                    |             |             |
                    +-------------+-------------+
                                  |
                                  v
              Jobs table (Processing, then Completed or Failed)
```

## What happens on startup

Before the API accepts traffic, `JobRecoveryService` loads every row still marked `Queued` and puts those jobs back on the channel. That covers a process that stopped after the row was saved and before a worker took the job.

Jobs already marked `Processing`, `Completed`, or `Failed` are left as they are. A job that was running when the process stopped stays `Processing` and is not picked up again.

The queue itself is only in memory. Anything waiting in the channel, and not yet saved as `Queued`, is gone after a restart. The normal create path saves the row before enqueueing, so a crash between those two steps is what recovery is for.

## Status values

| Status | Meaning |
| --- | --- |
| `Queued` | Saved and waiting for a worker, or waiting to be recovered on the next startup. |
| `Processing` | A worker has taken the job and the five-second wait is in progress. |
| `Completed` | The worker finished without an error. |
| `Failed` | The worker threw. `ErrorMessage` holds the exception message. Failed jobs are not retried. |

## API

In Development, Swagger UI is at `/swagger`.

| Method | Path | Behavior |
| --- | --- | --- |
| `POST` | `/api/jobs` | Body: `{ "name": "...", "payload": "..." }`. Both fields are optional. Returns the created job. |
| `GET` | `/api/jobs/{id}` | Returns the job, or 404 if the id is unknown. |

The `http` launch profile listens on `http://localhost:5053`. The `https` profile also listens on `https://localhost:7230`.

```bash
curl -X POST http://localhost:5053/api/jobs \
  -H "Content-Type: application/json" \
  -d '{"name":"example","payload":"hello"}'
```

Poll `GET /api/jobs/{id}` until `status` is `Completed` or `Failed`. A successful job takes about five seconds after a worker starts it.

## Main pieces

| Piece | Role |
| --- | --- |
| `JobsController` | Creates jobs and reads them by id. |
| `JobStore` | Entity Framework access to the `Jobs` table. |
| `JobQueue` | Unbounded in-memory channel shared by the controller and the workers. |
| `JobWorker` | Hosted service that runs the worker pool. |
| `JobRecoveryService` | Re-queues rows left in `Queued` when the process starts. |
| `JobDbContext` | EF Core context for PostgreSQL. |

`JobStore` is scoped to a request or to a worker scope. `JobQueue` is a singleton so every worker reads the same channel. Each worker opens its own scope before it touches the database.

## Configuration

The PostgreSQL connection string comes from a `.env` file, loaded before the host is built. ASP.NET Core then reads it as `DefaultConnection`.

```text
ConnectionStrings__DefaultConnection="Host=...;Port=5432;Database=postgres;Username=postgres;Password=...;SSL Mode=Require;Trust Server Certificate=true"
```

Copy `.env.example` to `.env` and set the real connection string. The application throws on startup if that value is missing. `.env` is gitignored.

The database schema is in the EF Core migrations under `Migrations/`. The app does not apply them on startup. For a new database:

```bash
dotnet ef database update
```

## Run

Requires the .NET 10 SDK.

```bash
dotnet run
```

The Development environment opens Swagger. Worker activity is written to the console (`Worker N started Job ...`, `completed`, `failed`, and `Recovered Job ...`).
