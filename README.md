# MiniPay – Distributed Banking System

A distributed microservices-based banking system for simulating basic financial operations using modern software engineering practices.

## Table of Contents

* [Features](#features)
* [Architecture](#architecture)
* [Getting Started](#getting-started)
* [Usage](#usage)
* [Contributing](#contributing)
* [License](#license)

## Features

* Modular microservices architecture
* Asynchronous messaging with **RabbitMQ**
* Centralized API routing with **Ocelot**
* Structured logging with **Serilog**
* User authentication and email confirmation flow
* PostgreSQL integration with **Entity Framework Core**
* Transactional email simulation
* Caching support with **Redis**
* Automated unit testing with **xUnit**
* Dockerized services for easy deployment
* CI/CD with **GitHub Actions**
* Cloud-ready (AWS compatible)

## Architecture

The system consists of the following microservices:

1. **Authentication Service**: Manages user registration, authentication, and JWT token issuance.
2. **User Service**: Handles user profile management and email confirmation (excluding password).
3. **Wallet Service**: Manages user wallets and balances (only updated via events).
4. **Transaction Service**: Handles creation and listing of financial transactions (e.g., deposits, transfers).
5. **Notification Service**: Simulates sending transactional emails (e.g., email confirmation).
6. **Verification Service**: Generates and manages verification codes for user actions (e.g., email confirmation).
7. **Shared Library**: Contains common utilities and models used across services.
8. **API Gateway**: Routes external HTTP requests to the appropriate services and handles JWT-based authentication and rate limiting.

> All services are connected through a Docker network and share a centralized Redis instance for caching.
> Only the API Gateway is exposed externally. The **Verification** and **Notification** services are internal-only.

### User Created Flow

```text
Event: user-created                 → `auth service` emits event                          [x]
  → queue: new-user                 → `user service` creates profile                      [x]
  → queue: new-wallet               → `wallet service` creates wallet                     [ ]
  → queue: new-email-verification   → `verification service` generates verification code  [x]
    → queue: new-user-notification  → `notification service` sends verification email     [x]
```

* The `user-created` event is published after signup.
* Each consumer handles part of the process:

  * `user service`: stores user profile (excluding password)
  * `wallet service`: creates a wallet
  * `verification service`: generates verification code
  * `notification service`: sends verification email

### Email Confirmation Flow

```text
Event: email-confirmed          → `verification service` emits event
  → queue: email-confirmed      → `user service` updates user profile
```

* The `email-confirmed` event is published after the user confirms their email.
* The `user service` consumes the event to update the user's profile as confirmed.

### Transaction Flow

```text
Event: transaction-created       → `transaction service` emits event
  → queue: update-wallet         → wallet service
```

* After a transaction is created, an event is emitted.
* The `wallet service` consumes the event to update the balance and invalidate cached wallet data in Redis.

> **Wallet balances are never updated via API — only through events.**

## Getting Started

### Prerequisites

Make sure you have the following installed:

* [.NET 8 SDK](https://dotnet.microsoft.com/download)
* [Docker](https://www.docker.com/)
* [Docker Compose](https://docs.docker.com/compose/)

### 1. Run Docker Infrastructure

```bash
docker compose up -d
```

This will start the **RabbitMQ** broker, **PostgreSQL** databases used by the services and **Redis** for caching.

### 2. Run the Microservices

Each service has its own solution and follows a layered architecture. You can run them individually:

```bash
dotnet build
dotnet run --project Authentication/Authentication.Api    # port 5010
dotnet run --project User/User.Api                        # port 5020
dotnet run --project Wallet/Wallet.Api                    # port 5030
dotnet run --project Transaction/Transaction.Api          # port 5040
dotnet run --project Verification/Verification.Api        # port 5050
dotnet run --project Notification/Notification.Api        # port 5060
dotnet run --project ApiGateway/ApiGateway.Api            # port 5000
```

## Usage

You can interact with the services using the Swagger UI or any HTTP client through the API Gateway:

### Auth
#### Sign Up

`POST /auth/signup`

Registers a new user.

* Content-Type: `application/json`
* Body:

```json
{
  "name": "user",
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

Returns: `201 Created` on success with user details and token

#### Sign In

`POST /auth/signin`

Authenticates a user and returns a JWT token.

* Content-Type: `application/json`
* Body:

```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

Returns: `200 OK` on success with user details and token

### User
#### Details

`GET /user`

Retrieves the authenticated user's details.

* Headers:
  - `Authorization: Bearer <token>`
  - `Content-Type: application/json`

Returns: `200 OK` on success or `404 Not Found` if the user does not exist

#### Update

`PUT /user`

Updates the authenticated user's details.

* Headers:
  - `Authorization: Bearer <token>`
  - `Content-Type: application/json`

* Body:

```json
{
  "name": "new name",               // optional
  "email": "newemail@example.com"   // optional
}
```

Returns: `200 OK` on success or `404 Not Found` if the user does not exist

#### Confirm Email

`GET /user/email/confirm/{code}`

Confirms a user's email address using the confirmation code.

* Headers:
  - `Authorization: Bearer <token>`
  - `Content-Type: application/json`

Returns: `200 OK` on success or `404 Not Found` if the user does not exist

### Wallet 

#### Details

`GET /wallet`

Retrieves the user's wallet details including balance and currency.

* Headers:
  - `Authorization: Bearer <token>`
  - `Content-Type: application/json`

Returns: `200 OK` with wallet information

#### Update

`PUT /wallet`

Updates wallet information such as currency.

* Headers:
  - `Authorization: Bearer <token>`
  - `Content-Type: application/json`

* Body:

```json
{
  "currency": "USD",      // optional, e.g., USD, EUR
  "country": "US",        // optional, e.g., US, GB
  "status": "active",     // optional, e.g., active, inactive
  "type": "personal"      // optional, e.g., personal, business
}
```

### Transactions

#### Create

`POST /transactions`

Creates a financial transaction for the authenticated user.

* Headers:
  - `Authorization: Bearer <token>`
  - `Content-Type: application/json`

* Body:

```json
{
  "type": "DEPOSIT",                    // DEPOSIT | WITHDRAWAL | TRANSFER | PAYMENT | RECEIPT | REFUND
  "amount": 100.00,
  "description": "Initial deposit",
  "target_wallet_code": "23a0md3x"      // optional, required for transfer, payment, receipt, refund
}
```

Returns: `201 Created` with updated wallet balance

#### List

`GET /transactions`

Retrieves a list of transactions for the authenticated user's wallet.

* Headers:
  - `Authorization: Bearer <token>`
  - `Content-Type: application/json`

Supports pagination with query parameters:
- `page`: Page number (default: 1)
- `size`: Number of items per page (default: 10)
- `type`: Filter by transaction type (optional)
- `start_date`: Filter by start date (optional, format: YYYY-MM-DD)
- `end_date`: Filter by end date (optional, format: YYYY-MM-DD)

Returns: `200 OK` with an array of transaction details

## Contributing

Contributions are welcome!
Feel free to fork this repository and submit a pull request with a clear explanation of your changes.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.