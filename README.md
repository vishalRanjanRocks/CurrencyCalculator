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

    ##Clone the repository
    1. git clone https://github.com/vishalRanjanRocks/CurrencyCalculator
    2. cd CurrencyExchange

    ##Install .NET 8 SDK
    1. Download from Microsoft .NET.

    ##Restore dependencies
    1. Restore dependencies

    ##Run the API
    1. dotnet run --project CurrencyExchange

    ## Run Tests and View Coverage
    1. dotnet test --collect:"XPlat Code Coverage"
    2. reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:Html
    3. start coverage-report/index.html

### 3. 🚀 Deployment Strategy
1. Environment-Specific Configuration
   
2.  1. Build & Release Pipeline (CI/CD)
    2. Build and restore dependencies
    3. Run unit & integration tests
    4. Collect and publish test coverage
  
3. Deploy to Azure App Service (Linux)
| Environment | Platform             | Strategy                         |
| ----------- | -------------------- | -------------------------------- |
| **Dev**     | Local / Azure (B1)   | Debug build, local cache         |
| **Test**    | Azure App Service    | Rate limits & full JWT/RBAC      |
| **Prod**    | Azure / Docker / K8s | Auto-scale, observability, Redis |

4. Horizontal Scaling
Designed to be stateless, supporting multiple instances.
Rate limiting is IP-based and globally applied.
Use Redis distributed cache (future enhancement) for shared state.
Can be deployed to:
Azure App Service Plan (scale-out)
Azure Kubernetes Service (AKS)
Docker Swarm / ECS

5. Observability
Logs (Serilog) are structured and correlate with:
Correlation ID
JWT Client ID
Distributed tracing with OpenTelemetry
Supports Seq, ELK, or Azure Monitor integration

6. Health & Monitoring
Expose /ping or /health endpoint (add if missing)
Azure App Insights / Prometheus support via OpenTelemetry


###  Assumptions Made ###
1. Small Application : In case of small application.It is created with monolithic architecture
2. Assuming user is already present in Database.For login just to pass role either as "Admin" or "User" and Password as "password"
3. Redis or distributed cache is not used; in-memory cache is sufficient for now.
4. Rate limiting is applied per IP address globally.
5. Roles used are "Admin" and "User" only.
6. Frankfurter API is considered stable and reliable.

### Possible Future Enchancements ###
1. Add support for Redis distributed cache for production scaling
2.  Multi-provider currency rates (e.g., ExchangeRatesAPI, Fixer.io)
3.  Dockerize and deploy to Azure/Kubernetes
4.  Replace static password with Identity provider (Azure AD, IdentityServer)
5.  Add file-based or cloud-based logging (Serilog + Seq/ELK)

### Imprortant Note ###
1. Missed to implement factory design Pattern to accomodate dynamic selection of currency exchange
2. Facing some issue  with token generation so as of now all the api's are open
3. Test coverage report is in repository.Please find report in CurrencyExchange.Tests\coverage-report\index.html
   ![image](https://github.com/user-attachments/assets/92603c40-22d6-493a-8ba0-d9ffc187b103)
