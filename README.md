# Water Tracker

A water consumption tracking API built with .NET 10, OpenIddict (OAuth 2.0 password grant), and Docker SQL Server.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker](https://www.docker.com/) (for SQL Server)

## Setup

### 1. Start SQL Server

```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=WaterTracker123!" \
  -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
```

### 2. Apply migrations

```bash
dotnet ef database update --project WaterTracker.Api
```

This creates the database tables (Identity, OpenIddict, WaterIntakes).

### 3. Run the API

```bash
dotnet run --project WaterTracker.Api --launch-profile https
```

The API starts at `https://localhost:7184`. Swagger UI is at `https://localhost:7184/swagger`.

## Testing the endpoints

Swagger lets you test most endpoints directly. For the token endpoint, use the form fields rendered in the UI.

### 1. Register a user

**POST** `/api/auth/register`

```json
{
  "email": "test@example.com",
  "password": "Password1",
  "forename": "Test",
  "surname": "User"
}
```

### 2. Get a token

**POST** `/connect/token`

Set the form fields:
- `grant_type`: `password`
- `username`: `test@example.com`
- `password`: `Password1`
- `client_id`: `watertracker-blazor`

Returns an `access_token`, `token_type: Bearer`, and `expires_in`.

### 3. Authorize in Swagger

Click the **Authorize** button at the top of Swagger and paste:

```
Bearer <your-access-token>
```

Now you can call the authenticated endpoints:

### 4. Get profile

**GET** `/api/auth/me`

Returns the authenticated user's email, forename, surname, and userId.

### 5. Manage water intakes

All endpoints require authentication.

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/waterintake` | List all intakes for the logged-in user |
| GET | `/api/waterintake/{id}` | Get a specific intake |
| POST | `/api/waterintake` | Create an intake (send `amount` and `date`) |
| PUT | `/api/waterintake/{id}` | Update an intake |
| DELETE | `/api/waterintake/{id}` | Soft-delete an intake |

### Testing with curl

```bash
# Register
curl -k -X POST https://localhost:7184/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Password1","forename":"Test","surname":"User"}'

# Get token
curl -k -X POST https://localhost:7184/connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password&username=test@example.com&password=Password1&client_id=watertracker-blazor"

# Use token (replace TOKEN with the access_token value)
curl -k https://localhost:7184/api/auth/me \
  -H "Authorization: Bearer TOKEN"

# Create an intake
curl -k -X POST https://localhost:7184/api/waterintake \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"amount":500,"date":"2026-05-18T00:00:00Z"}'
```

## Project structure

```
WaterTracker.Api/    Web API (controllers, services, models)
WaterTracker/        Blazor WebAssembly client (in progress)
WaterTracker.Tests/  xUnit tests (in progress)
```
