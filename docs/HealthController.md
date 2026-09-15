# HealthController

`HealthController` exposes the application's aggregate health check over HTTP. It delegates the check to `HealthCheckService` and is registered through the application's controller routing.

## Endpoint

### `GET /Health`

Runs all health checks registered with `HealthCheckService`. The endpoint takes no route, query, or request-body parameters.

| Result | Status | Content type | Body |
| --- | --- | --- | --- |
| Healthy | `200 OK` | `application/json` | An object containing `version`, `uptime`, and the aggregate `result` |
| Unhealthy or unrecognized result | `503 Service Unavailable` | `application/problem+json` | ASP.NET Core `ProblemDetails` |

## Response shape

The successful response is shaped as follows:

```json
{
  "version": null,
  "uptime": null,
  "result": {
    "status": 0,
    "timestamp": "2026-09-15T12:00:00Z",
    "checkedAt": "2026-09-15T12:00:00Z",
    "entries": [
      {
        "name": "storage",
        "status": 0,
        "description": "Available",
        "duration": "00:00:00.0120000",
        "data": {}
      }
    ]
  }
}
```

`status` values use the numeric `HealthStatus` enum representation: `0` is `Healthy`, `1` is `Degraded`, and `2` is `Unhealthy`. The top-level `version` and `uptime` fields are populated only when the returned result type exposes properties with those names; the current `HealthCheckResult` does not, so both serialize as `null`.

The failure response is:

```json
{
  "title": "Service Unavailable",
  "status": 503,
  "detail": "Health check failed."
}
```

> **Current behavior:** `HealthController` looks for an `IsHealthy` property on the service result. The current `HealthCheckResult` exposes `Status` instead, so `IsHealthy` is not found and the endpoint returns the `503` response for every request, including when all registered checks report `Healthy`.

## Example request

Replace the base URL with the address printed when the application starts. For a local HTTPS profile:

```bash
curl --include --insecure https://localhost:5001/Health
```

The current implementation responds with `503 Service Unavailable` and the problem-details body shown above.
