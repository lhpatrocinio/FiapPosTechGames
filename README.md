# FiapPosTechGames

Sistema de catálogo de jogos com integração completa ao **Elasticsearch** para busca avançada e analytics.

## 🎯 **Projeto FIAP - Fase 3 - Elasticsearch**

Este projeto implementa um microserviço de jogos com funcionalidades avançadas de busca usando **Elasticsearch 8.10.4**, seguindo os requisitos do TechChallenge FIAP Fase 3.

### **Arquitetura**
- **.NET 8** com Clean Architecture
- **Entity Framework Core 9.0.4** para persistência
- **SQL Server** como banco principal
- **Elasticsearch 8.10.4** para busca e analytics
- **NEST Client** para integração .NET ↔ Elasticsearch
- **Docker** para infraestrutura

## 🚀 **Como Iniciar**

### **1. Pré-requisitos**
```bash
# Ferramentas necessárias
- Docker Desktop
- .NET 8 SDK
- Git
```

### **2. Infraestrutura (Docker)**
```bash
# 1. Navegar para o diretório de infraestrutura
cd ../FiapPostTechDocker

# 2. Subir SQL Server + Elasticsearch
docker-compose up -d sqlserver elasticsearch

# 3. Verificar containers rodando
docker ps
# Deve mostrar: sqlserver (1433) + elasticsearch (9200)

# 4. Testar Elasticsearch
curl http://localhost:9200
# Response: {"cluster_name": "docker-cluster", "version": {"number": "8.10.4"}}
```

### **3. Aplicação (.NET)**
```bash
# 1. Navegar para a API
cd src/Games.Api

# 2. Restaurar pacotes
dotnet restore

# 3. Executar aplicação
dotnet run
# Aplicação disponível em: http://localhost:80
# Swagger disponível em: http://localhost/swagger
```

### **4. Inicialização Automática**
A aplicação faz automaticamente:
- ✅ **Criação do índice**: Índice "games" criado no Elasticsearch
- ✅ **Aplicação de migrations**: DatabaseStructure + InitialSeedData
- ✅ **Indexação inicial**: 5 jogos + relacionamentos indexados
- ✅ **Health checks**: Monitoramento de SQL Server + Elasticsearch

## 🔍 **Elasticsearch - Funcionalidades**

### **Endpoints Disponíveis**

#### **1. Busca Avançada de Jogos**
```http
GET /api/v1/search/games
```

**Parâmetros suportados:**
- `query`: Busca textual (título, descrição, desenvolvedor)
- `genres`: Array de gêneros para filtrar
- `minPrice` / `maxPrice`: Range de preços
- `minRating`: Rating mínimo
- `developer`: Busca por desenvolvedor específico
- `from`: Paginação (offset)
- `size`: Tamanho da página (máx 100)

**Exemplos:**
```bash
# Busca simples
curl "http://localhost/api/v1/search/games?query=witcher"

# Busca com filtros
curl "http://localhost/api/v1/search/games?query=adventure&genres=RPG&minPrice=0&maxPrice=100&minRating=4.0"

# Paginação
curl "http://localhost/api/v1/search/games?query=game&from=0&size=10"
```

#### **2. Reindexação Manual**
```http
POST /api/v1/search/reindex
```

**Uso:**
```bash
curl -X POST "http://localhost/api/v1/search/reindex"
# Response: {"message": "Reindex concluído com sucesso", "gamesIndexed": 5}
```

### **Funcionalidades de Busca**

#### **🎯 Busca Full-Text**
- **Multi-match**: Busca em título, descrição e desenvolvedor
- **Fuzzy search**: Tolerância a erros de digitação
- **Boost**: Título tem prioridade 2x sobre outros campos
- **Analyzer**: Análise padrão para texto em português

#### **🎛️ Filtros Avançados**
- **Gêneros**: Filtro exato por múltiplos gêneros
- **Range de preços**: MinPrice ↔ MaxPrice
- **Rating mínimo**: Jogos acima de uma nota específica
- **Desenvolvedor**: Match parcial no nome do desenvolvedor

#### **📊 Características Técnicas**
- **Índice**: `games` com mapeamentos otimizados
- **Tipos de dados**: Text, Keyword, Number (ScaledFloat), Date
- **Performance**: Timeout 30s, 3 retries, bulk indexing
- **Monitoramento**: Health checks integrados

## 🗄️ **Dados de Exemplo**

### **Jogos Indexados (5 jogos)**
```json
[
  {
    "title": "The Witcher 3: Wild Hunt",
    "genres": ["RPG", "Adventure", "Open World"],
    "price": 59.99,
    "rating": 4.9,
    "developer": "CD Projekt Red"
  },
  {
    "title": "Minecraft",
    "genres": ["Sandbox", "Survival", "Adventure"],
    "price": 26.95,
    "rating": 4.8,
    "developer": "Mojang Studios"
  },
  {
    "title": "Counter-Strike 2",
    "genres": ["Shooter", "Action", "Competitive"],
    "price": 0.00,
    "rating": 4.2,
    "developer": "Valve Corporation"
  },
  {
    "title": "FIFA 24",
    "genres": ["Sports", "Simulation"],
    "price": 299.90,
    "rating": 4.1,
    "developer": "EA Sports"
  },
  {
    "title": "Forza Horizon 5",
    "genres": ["Racing", "Action", "Open World"],
    "price": 249.90,
    "rating": 4.7,
    "developer": "Playground Games"
  }
]
```

### **Gêneros Disponíveis (11 gêneros)**
- RPG, Adventure, Sandbox, Survival, Shooter
- Action, Competitive, Sports, Simulation, Racing, Open World

## 🔧 **Configuração**

### **Connection Strings**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=sqlserver;Database=pos_tech_games;User=sa;Password=huaHhbSyjn9bttt;TrustServerCertificate=true;MultipleActiveResultSets=true",
    "Elasticsearch": "http://localhost:9200"
  }
}
```

### **Docker Compose (Elasticsearch)**
```yaml
elasticsearch:
  image: docker.elastic.co/elasticsearch/elasticsearch:8.10.4
  container_name: elasticsearch
  ports:
    - "9200:9200"
    - "9300:9300"
  environment:
    - discovery.type=single-node
    - xpack.security.enabled=false
    - "ES_JAVA_OPTS=-Xms512m -Xmx512m"
  networks:
    - postech-network
  restart: unless-stopped
```

## 🧪 **Testando a Integração**

### **1. Verificar Status**
```bash
# SQL Server
curl "http://localhost/health"

# Elasticsearch Server  
curl "http://localhost:9200"

# Elasticsearch Index
curl "http://localhost:9200/games"

# Contagem de documentos
curl "http://localhost:9200/games/_count"
# Expected: {"count": 5}
```

### **2. Exemplos de Busca**

#### **Busca por texto:**
```bash
curl "http://localhost/api/v1/search/games?query=witcher"
# Encontra: The Witcher 3 (boost no título)
```

#### **Filtro por gênero:**
```bash
curl "http://localhost/api/v1/search/games?genres=RPG"
# Encontra: The Witcher 3
```

#### **Range de preço:**
```bash
curl "http://localhost/api/v1/search/games?minPrice=0&maxPrice=50"
# Encontra: Minecraft, Counter-Strike 2
```

#### **Busca complexa:**
```bash
curl "http://localhost/api/v1/search/games?query=action&genres=Adventure&genres=Open%20World&minRating=4.5"
# Encontra: The Witcher 3, Forza Horizon 5
```

### **3. Swagger UI**
Acesse: **http://localhost/swagger**
- Teste todos os endpoints interativamente
- Veja exemplos de request/response
- Valide schemas dos DTOs

## 📈 **Monitoramento**

### **Health Checks**
```bash
# Health check geral
curl "http://localhost/health"

# Health check específico do Elasticsearch
curl "http://localhost/health/ready"
```

### **Logs da Aplicação**
```bash
# Logs de indexação
[INF] Elasticsearch index 'games' created successfully
[INF] Bulk indexed 5 games successfully

# Logs de busca
[INF] Search executed: Query='witcher', Results=1
```

### **Kibana (Opcional)**
Se disponível em http://localhost:5601:
- Visualizar dados indexados
- Criar dashboards de analytics
- Monitorar performance das queries

## 🚨 **Troubleshooting**

### **Elasticsearch não conecta**
```bash
# 1. Verificar container
docker ps | grep elasticsearch

# 2. Verificar logs
docker logs elasticsearch

# 3. Testar conectividade
curl http://localhost:9200/_cluster/health

# 4. Recriar índice se necessário  
curl -X DELETE "http://localhost:9200/games"
curl -X POST "http://localhost/api/v1/search/reindex"
```

### **Dados não indexados**
```bash
# 1. Verificar migrations aplicadas
dotnet ef migrations list --project ../Games.Infrastructure

# 2. Forçar reindex
curl -X POST "http://localhost/api/v1/search/reindex"

# 3. Verificar contagem
curl "http://localhost:9200/games/_count"
```

### **Performance lenta**
- Verificar memória do Elasticsearch (ES_JAVA_OPTS)
- Otimizar queries de busca
- Implementar cache para buscas frequentes

## 🎓 **Requisitos FIAP Atendidos**

### **✅ Aula 1 - Busca de Texto**
- Multi-match queries com fuzzy search
- Boost em campos prioritários

### **✅ Aula 2 - Configuração**
- Docker setup completo
- Índices e mapeamentos otimizados

### **✅ Aula 3 - .NET Integration**
- NEST client configurado
- Services e interfaces implementados

### **✅ Aula 4 - Indexação**
- Bulk indexing de dados existentes
- Sincronização automática SQL → Elasticsearch

### **✅ Aula 5 - Consultas Avançadas**
- Filtros combinados (Bool queries)
- Range queries para preços e ratings
- Paginação e ordenação

## 👥 **Contribuição**

Este projeto faz parte do ecossistema FIAP Post Tech:
- **FiapPosTechGames**: Este projeto (Catalog + Search)
- **FiapPostTechUsers**: Microserviço de usuários
- **FiapPostTechPayments**: Microserviço de pagamentos
- **FiapPostTechDocker**: Infraestrutura centralizada

## 📄 **Documentação Adicional**

- **Swagger API**: http://localhost/swagger (quando rodando)
- **Copilot Instructions**: `.github/copilot-instructions.md`
- **Context Tests**: `CONTEXT_MIGRATION_TESTS.md`
- **Docker Infrastructure**: `../FiapPostTechDocker/docker-compose.yml`