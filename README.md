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
2. **User Service**: Handles user profile management and email confirmation.
3. **Wallet Service**: Manages user wallets and balances.
4. **Transaction Service**: Handles financial transactions such as deposits, withdrawals, transfers, and payments.
5. **Notification Service**: Simulates sending transactional emails (e.g., email confirmations).
6. **API Gateway**: Routes requests to the appropriate microservices and handles authentication and rate limiting.

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
dotnet run --project Authentication/Authentication.Api
dotnet run --project User/User.Api
dotnet run --project Wallet/Wallet.Api
dotnet run --project Transaction/Transaction.Api
dotnet run --project Notification/Notification.Api
dotnet run --project ApiGateway/ApiGateway.Api
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