# 💱 Currency Converter API

A robust, secure, and scalable currency conversion API built with ASP.NET Core (.NET 8). This API integrates with the Frankfurter public API to fetch exchange rates, supports JWT-based authentication, rate limiting, caching, structured logging, distributed tracing, and more.

--- 

## 🚀 Features

- ✅ Latest, historical, and conversion endpoints
- 🔒 JWT Authentication with Role-Based Access Control (RBAC)
- 📈 Distributed tracing (OpenTelemetry)
- 🔁 Polly-based retry and circuit-breaker policies
- 📊 In-memory caching for performance
- 🚦 API rate limiting to prevent abuse
- 🧪 90%+ Unit Test Coverage with Integration Tests
- 🔀 API versioning and environment-specific configurations

---

## 🛠️ Setup Instructions

### 1. 🔧 Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [Visual Studio 2022+](https://visualstudio.microsoft.com) or VS Code
- Optional: [Docker](https://www.docker.com/), [Redis](https://redis.io)

### 2. ⚙️ Configuration

1.	Clone the repository
           a. git clone <your-repo-url>
           b. cd CurrencyExchange
2.	Install .NET 8 SDK
          a. Download from Microsoft .NET.
3.	Restore dependencies
          a. dotnet restore
4.	Configure settings
          •Edit CurrencyExchange/appsettings.json and appsettings.Development.json for API keys, provider URLs, and JWT settings as needed.
5.	Run the API
          a. dotnet run --project CurrencyExchange
6.    Run Tests and View Coverage
          a. dotnet test --collect:"XPlat Code Coverage"
          b. reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:Html
          c. start coverage-report/index.html

###  Assumptions Made ###
1. Small Application : In case of small application.It is created with monolithic architecture
2. Assuming user is already present in Database.For login just to pass role either as "Admin" or "User" and Password as "password"
3. Redis or distributed cache is not used; in-memory cache is sufficient for now.
4.Rate limiting is applied per IP address globally.
5.Roles used are "Admin" and "User" only.
6.Frankfurter API is considered stable and reliable.

### Possible Future Enchancements ###
✅ Add support for Redis distributed cache for production scaling
🧾 Swagger-based client generation for SDKs
🌐 Multi-provider currency rates (e.g., ExchangeRatesAPI, Fixer.io)
📉 Historical chart data visualization (via frontend)
📦 Dockerize and deploy to Azure/Kubernetes
👨‍💼 Replace static password with Identity provider (Azure AD, IdentityServer)
📊 Store conversion logs and analytics in database
📂 Add file-based or cloud-based logging (Serilog + Seq/ELK)

### Imprortant Note ###
1. Missed to implement factory design Pattern to accomodate dynamic selection of currency exchange
2. Facing some issue  with token generation so as of now all the api's are open
3. Test coverage report is in repository.Please find report in CurrencyExchange.Tests\coverage-report\index.html
   ![image](https://github.com/user-attachments/assets/92603c40-22d6-493a-8ba0-d9ffc187b103)
