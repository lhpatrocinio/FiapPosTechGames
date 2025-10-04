# 🎮 FIAP Tech Challenge Phase 3 - AI Agent Instructions# FiapPosTechGames - AI Agent Instructions



## 🏗️ Architecture Overview## Tech Challenge Status Overview

**PROJETO FIAP FASE 3 - ELASTICSEARCH STATUS:**

This is a **microservices ecosystem** implementing FIAP Phase 3 requirements with Clean Architecture, Event-Driven patterns, and Elasticsearch integration.

### ✅ IMPLEMENTADO (Requirement Check)

### Microservices Structure- ✅ **Indexar dados dos jogos**: ElasticsearchService com GameDocument mapping completo

- **🎮 FiapPosTechGames** (port 7000): Game catalog with Elasticsearch search/analytics/recommendations- ✅ **Busca avançada**: Multi-match com fuzzy search, filtros por gênero, preço, rating, desenvolvedor

- **👤 FiapPosTechUsers** (port 5000): User management with Identity Framework + user behavior tracking- ✅ **Health checks**: Monitoramento ES via `/health` endpoints

- **💳 FiapPosTechPayments** (port 9200): Payment processing- ✅ **Sincronização automática**: ElasticsearchSyncService indexa dados no startup

- **🐳 FiapPostTechDocker**: Shared infrastructure (SQL Server, Elasticsearch, RabbitMQ, Prometheus)- ✅ **Infraestrutura Docker**: Container pronto para produção

- ✅ **Monitoramento**: Prometheus metrics e logs estruturados

### Network Architecture

All services use `postech-network` external Docker network. Critical dependency: start infrastructure first via `../FiapPostTechDocker/docker-compose up -d sqlserver elasticsearch rabbitmq`.### ✅ FASES COMPLETAS - ELASTICSEARCH

- ✅ **FASE 1 (COMPLETA)**: Agregações ES para jogos populares, stats por gênero, price analytics

## 🔍 Core Patterns- ✅ **FASE 2 (COMPLETA)**: Recomendações por similaridade (gêneros, rating, desenvolvedor) 

- 🚀 **FASE 3 (PRÓXIMA)**: Tracking de buscas e métricas de negócio avançadas

### Clean Architecture Layers (All Services)

```### ✅ IMPLEMENTADO COMPLETO - FASES 1 & 2

src/- ✅ **ElasticsearchAnalyticsService**: Agregações para jogos populares/top-rated

├── {Service}.Api/          # Controllers, Middleware, Extensions- ✅ **AnalyticsController**: Endpoints `/analytics/popular-games`, `/analytics/genres-stats`, `/analytics/price-analytics`, `/analytics/top-rated`, `/analytics/catalog-overview`

├── {Service}.Application/  # Services, DTOs, Consumers, Bootstrapper  - ✅ **Sistema de Recomendações**: 5 algoritmos baseados em similaridade (game-based, similar-games, genre-based, personalized, developer-based)

├── {Service}.Domain/       # Entities, Events, Enums- ✅ **RecommendationController**: 6 endpoints funcionando `/recommendation/game-based/{id}`, `/recommendation/similar-games`, `/recommendation/genre-based`, `/recommendation/personalized`, `/recommendation/developer-based`, `/recommendation/health`

└── {Service}.Infrastructure/ # Repositories, External Services, Bootstrapper- ✅ **AutoMapper Integration**: Mapeamentos completos para Analytics e Recommendations

```- ✅ **Authentication**: [AllowAnonymous] aplicado para testes Docker

- ✅ **Sistema Testado**: Todos endpoints funcionando em produção Docker

### Dependency Injection Pattern

Every service uses Bootstrapper pattern in `Program.cs`:## Architecture Overview

```csharpThis is a **Clean Architecture .NET 8 microservice** implementing a games catalog with **Elasticsearch search integration**. Core components:

#region [DI]

ApplicationBootstrapper.Register(builder.Services);- **Games.Api**: Controllers, extensions, DTOs (entry point on port 80)

InfraBootstrapper.Register(builder.Services, builder.Configuration);- **Games.Application**: Services, DTOs, repository interfaces  

#endregion- **Games.Infrastructure**: Data access, Elasticsearch, monitoring

```- **Games.Domain**: Entities (Game, GameGenre, Library, etc.)



### Event-Driven Architecture## Key Implementation Patterns

- **Publisher**: Users service publishes `UserCreatedEvent` via RabbitMQ (`user-created-queue`)

- **Consumer**: Games service consumes events with `UserCreatedConsumer : BackgroundService`### Elasticsearch Integration (PRODUCTION READY)

- **Pattern**: JSON serialization, manual ACK, persistent queues (`durable: true`)- **Index name**: Always `"games"` (hardcoded constant)  

- **Service**: `ElasticsearchService` handles all ES operations

## 🔐 Authentication & Authorization- **Background sync**: `ElasticsearchSyncService` runs automatic indexing on startup

- **Health checks**: ES connectivity monitored at `/health` endpoints

### Custom Role Authorization- **Document mapping**: `GameDocument.FromGame()` transforms entities

Uses `RoleAuthorizationMiddleware` before ASP.NET Core authorization:- **Search features**: Multi-match with fuzzy search, genre filtering, price ranges

```csharp

app.UseAuthentication();                        // 1°: popula HttpContext.User### Current Search Capabilities (PRODUCTION READY)

app.UseMiddleware<RoleAuthorizationMiddleware>(); // 2°: custom middleware```csharp

app.UseAuthorization();                         // 3°: aplica [Authorize]// Implemented in ElasticsearchService.BuildQuery()

```- Multi-match: Title (boost 2x), Description, Developer  

- Fuzzy search: Tolerance for typos

### Controller Patterns- Genre filtering: Exact keyword matching

- API versioning: `[Route("api/v{version:apiVersion}/[controller]")]`- Price/rating ranges: Decimal precision queries

- Role-based: `[Authorize(Roles = "Admin,Usuario")]`- Pagination: from/size parameters

- Anonymous endpoints for testing: `[AllowAnonymous]````



## 🔍 Elasticsearch Integration (Games Service)### Analytics Capabilities (PRODUCTION READY)

```csharp

### Critical Services// Implemented in ElasticsearchAnalyticsService

- **ElasticsearchService**: CRUD operations on `games` index- Popular games with search counts and ratings

- **ElasticsearchAnalyticsService**: Complex aggregations for metrics- Genre statistics with price/rating aggregations

- **ElasticsearchRecommendationService**: ML-style recommendations- Price range distribution analysis

- **ElasticsearchSyncService**: Background sync SQL Server → Elasticsearch- Top-rated games ranking

- Catalog overview with comprehensive metrics

### Configuration Pattern```

```csharp

// In Extensions/ElasticsearchExtensions.cs### Recommendation Engine (PRODUCTION READY)

services.AddElasticsearch(configuration);```csharp

// Registers: IElasticClient, IElasticsearchService, ElasticsearchSyncService, Health Checks// Implemented in ElasticsearchRecommendationService - 5 Algorithms:

```- Game-based: Recommendations based on specific game similarity

- Similar games: Advanced similarity search with multiple criteria

### Health Check Strategy- Genre-based: Recommendations by preferred genres

- `/health`: General application health- Personalized: User profile-based recommendations

- `/health/ready`: Elasticsearch connectivity (for container readiness)- Developer-based: Recommendations by game developer

- Custom `ElasticsearchHealthCheck` with cluster status details- Similarity scoring: Weighted algorithm (Genres 40%, Rating 25%, Developer 20%, Price 15%)

```

## 🐳 Docker & Infrastructure

## Critical Development Workflows

### Development Workflow

```bash### Local Development Setup

# 1. Start infrastructure```bash

cd ../FiapPostTechDocker && docker-compose up -d sqlserver elasticsearch rabbitmq# 1. Start infrastructure (requires external FiapPostTechDocker repo)

cd ../FiapPostTechDocker

# 2. Start individual service  docker-compose up -d sqlserver elasticsearch

dotnet run  # Auto-applies migrations, seeds data, syncs Elasticsearch

```# 2. Run application

cd src/Games.Api

### Container Health Checksdotnet run  # Available on http://localhost:80

All services use `/metrics` endpoint for health monitoring with 30s intervals, 40s start period.```



### Database Migrations### Current Production Endpoints (ALL TESTED & WORKING)

Auto-applied on startup via `app.ExecuteMigrations()` extension. Games service includes seed data for 13 games + genres.

#### Search Endpoints

## 📊 Monitoring & Observability- **Search**: `GET /api/v1/search/games` ✅ PRODUCTION READY

- **Reindex**: `POST /api/v1/search/reindex` ✅ MAINTENANCE

### Prometheus Integration

- **Metrics**: `/metrics` endpoint on all services#### Analytics Endpoints (FASE 1 COMPLETA)

- **Runtime metrics**: .NET runtime stats collection- **Popular Games**: `GET /api/v1/analytics/popular-games` ✅ PRODUCTION READY

- **Health metrics**: Forwarded to Prometheus via `ForwardToPrometheus()`- **Genre Stats**: `GET /api/v1/analytics/genres-stats` ✅ PRODUCTION READY

- **Price Analytics**: `GET /api/v1/analytics/price-analytics` ✅ PRODUCTION READY

### Structured Logging (Serilog)- **Top Rated**: `GET /api/v1/analytics/top-rated` ✅ PRODUCTION READY

- **Pattern**: Enriched with machine name, correlation ID, exception details- **Catalog Overview**: `GET /api/v1/analytics/catalog-overview` ✅ PRODUCTION READY

- **Custom middleware**: `LogRequestActionFilter` for request/response logging

- **Template**: `"HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms"`#### Recommendation Endpoints (FASE 2 COMPLETA)

- **Game-based**: `GET /api/v1/recommendation/game-based/{gameId}` ✅ PRODUCTION READY

## 🛠️ Development Commands- **Similar Games**: `POST /api/v1/recommendation/similar-games` ✅ PRODUCTION READY

- **Genre-based**: `POST /api/v1/recommendation/genre-based` ✅ PRODUCTION READY

### Essential Builds- **Personalized**: `POST /api/v1/recommendation/personalized` ✅ PRODUCTION READY

```bash- **Developer-based**: `POST /api/v1/recommendation/developer-based` ✅ PRODUCTION READY

dotnet build {Service}.sln          # Build entire solution- **Health Check**: `GET /api/v1/recommendation/health` ✅ MONITORING

dotnet ef migrations add {Name}     # Add migration

dotnet ef database update          # Apply migrations manually#### Health & Monitoring

```- **Health**: `GET /health` ✅ MONITORING

- **Ready**: `GET /health/ready` ✅ MONITORING

### Elasticsearch Operations- **Live**: `GET /health/live` ✅ MONITORING

```bash- **Metrics**: `GET /metrics` ✅ PROMETHEUS

# Test connectivity

curl http://localhost:9200/_cluster/health### API Patterns

- **Versioned APIs**: All controllers use `[ApiVersion("1.0")]` and route `api/v{version:apiVersion}/[controller]`

# Force reindex- **Authorization**: Controllers require `[Authorize(Roles = "Games")]`

POST /api/v1/search/reindex- **AutoMapper**: DTOs mapped with `MapperProfile` - always use `_mapper.Map<T>()`

- **Standard responses**: Use ProducesResponseType attributes for OpenAPI

# Check indexed count

curl http://localhost:9200/games/_count### Database Patterns  

```- **Table naming**: All tables prefixed with `GMS_` (e.g., `GMS_Games`, `GMS_GenreTypes`)

- **Migrations**: Auto-executed in `Program.cs` via `app.ExecuteMigrations()`

### Container Management- **Seeded data**: 5 games with fixed GUIDs, 11 genre types predefined

```bash- **Connection string**: Uses `DefaultConnection` pointing to SQL Server

docker-compose up -d                # Start infrastructure

docker ps | grep elasticsearch     # Check container status## Production Monitoring (READY)

docker logs {container}           # Debug container issues- **Container name**: `FiapGames` on port 7000 → 80

```- **External network**: `postech-network` (must exist)

- **Health endpoints**: `/health`, `/health/ready`, `/health/live`  

## 🚨 Critical Gotchas- **Prometheus**: Metrics exposed at `/metrics`

- **Background services**: `ElasticsearchSyncService`, `UserCreatedConsumer`

### Microservices Boundaries

- **Games** service should NOT handle user-specific data directly## Extension Methods Pattern

- **Users** service owns all user behavior tracking (`UserBehavior`, `UserPreferences` entities)All cross-cutting concerns use extension methods in `Games.Api/Extensions/`:

- Cross-service communication via events or orchestration at frontend/gateway level- `AddElasticsearch()`, `AddVersioning()`, `AddSwaggerDocumentation()`

- Follow this pattern for new infrastructure concerns

### Connection Strings

All services expect SQL Server at `sqlserver` hostname with password `huaHhbSyjn9bttt`. Elasticsearch at `http://elasticsearch:9200`.**STATUS ATUAL**: ✅ Fases 1 e 2 COMPLETAS - Sistema em produção com 13 jogos indexados, 5 algoritmos de recomendação ativos, 5 endpoints de analytics funcionando.



### Entity Framework Patterns**PRÓXIMA FASE 3**: Implementar tracking avançado de buscas, métricas de comportamento do usuário, machine learning para recomendações dinâmicas, e cache Redis para performance.

- Uses `ApplicationDbContext` in all services

- Identity Framework in Users service with custom entities (`UsersEntitie`, `Roles`)**TESTADO EM PRODUÇÃO**: Todos os endpoints validados via Docker com Elasticsearch + SQL Server funcionando perfeitamente.

- Precision configuration for decimals: `HasPrecision(18, 2)`

## 🎯 FIAP Requirements Compliance

### Elasticsearch Requirements
1. **Index game data efficiently**: ✅ GameDocument with optimized mappings
2. **Advanced user history queries**: ✅ User behavior tracking in Users service  
3. **Aggregations for metrics**: ✅ AnalyticsService with complex aggregations

### Architecture Requirements
- **Clean Architecture**: ✅ 4-layer separation across all services
- **Microservices**: ✅ Service boundaries with event communication
- **Docker**: ✅ Full containerization with shared network
- **Monitoring**: ✅ Prometheus metrics + health checks

## 💡 AI Agent Tips

- Always check infrastructure containers are running before debugging API issues
- Use Swagger UI (`http://localhost:{port}/swagger`) for endpoint testing
- Monitor logs for Elasticsearch sync status during startup
- Event-driven features require RabbitMQ container to be healthy
- Build warnings are mostly XML documentation - safe to ignore during development