# ☸️ Monitoramento Kubernetes - FIAP Tech Challenge Fase 4

## 📋 Visão Geral

Este diretório contém os manifestos Kubernetes para implementar o stack completo de **Logs e Monitoramento** conforme requisitos da Fase 4.

## 🎯 Componentes Implementados

### 1. **Prometheus** (Coleta de Métricas)
- **Arquivo**: `prometheus-deployment.yaml`
- **Namespace**: `monitoring`
- **Porta**: 9090
- **Função**: Coletar métricas de CPU, memória, requisições HTTP dos microsserviços

### 2. **Grafana** (Visualização)
- **Arquivo**: `grafana-deployment.yaml`
- **Namespace**: `monitoring`
- **Porta**: 3000
- **Credenciais**: admin/admin (MUDAR EM PRODUÇÃO!)
- **Função**: Dashboards para visualizar métricas e status do cluster

### 3. **Jaeger** (APM - Distributed Tracing)
- **Arquivo**: `jaeger-deployment.yaml`
- **Namespace**: `monitoring`
- **Porta**: 16686 (UI), 14268 (Collector)
- **Função**: Rastreamento de requisições entre microsserviços

### 4. **Metrics Server** (Para HPA)
- **Instalação direta** (não é arquivo custom)
- **Função**: Fornecer métricas de CPU/memória para o HPA funcionar

## 🚀 Como Instalar

### Pré-requisitos
```powershell
# 1. Ter cluster Kubernetes rodando
kubectl cluster-info

# 2. Ter namespace de produção criado
kubectl create namespace fiap-games-prod
```

### Instalação Step-by-Step

#### 1. Instalar Metrics Server (Necessário para HPA)
```powershell
kubectl apply -f https://github.com/kubernetes-sigs/metrics-server/releases/latest/download/components.yaml

# Verificar instalação
kubectl get deployment metrics-server -n kube-system
kubectl top nodes
```

#### 2. Criar Namespace de Monitoramento
```powershell
kubectl create namespace monitoring
```

#### 3. Deploy Prometheus
```powershell
kubectl apply -f prometheus-deployment.yaml

# Verificar pods
kubectl get pods -n monitoring -l app=prometheus

# Verificar serviço
kubectl get svc -n monitoring prometheus-service
```

#### 4. Deploy Grafana
```powershell
kubectl apply -f grafana-deployment.yaml

# Verificar pods
kubectl get pods -n monitoring -l app=grafana

# Obter IP externo (LoadBalancer)
kubectl get svc -n monitoring grafana-service
```

#### 5. Deploy Jaeger
```powershell
kubectl apply -f jaeger-deployment.yaml

# Verificar pods
kubectl get pods -n monitoring -l app=jaeger

# Obter IP externo
kubectl get svc -n monitoring jaeger-service
```

## 🔍 Acessando as Ferramentas

### Obter IPs Externos
```powershell
# Ver todos os serviços de monitoramento
kubectl get svc -n monitoring

# Exemplo de saída:
# NAME                  TYPE           EXTERNAL-IP      PORT(S)
# prometheus-service    LoadBalancer   20.1.2.3        9090:30001/TCP
# grafana-service       LoadBalancer   20.1.2.4        3000:30002/TCP
# jaeger-service        LoadBalancer   20.1.2.5        16686:30003/TCP
```

### URLs de Acesso
- **Prometheus**: `http://<EXTERNAL-IP>:9090`
- **Grafana**: `http://<EXTERNAL-IP>:3000` (admin/admin)
- **Jaeger**: `http://<EXTERNAL-IP>:16686`

## 📊 Configurando Dashboards no Grafana

### 1. Acessar Grafana
```
URL: http://<GRAFANA-IP>:3000
User: admin
Password: admin
```

### 2. Importar Dashboards Pré-Configurados

#### Dashboard: Kubernetes Cluster Monitoring
1. Ir em **+ (Create)** → **Import**
2. Digite o ID: **315**
3. Selecionar datasource: **Prometheus**
4. Click **Import**

#### Dashboard: .NET Application Monitoring
1. Ir em **+ (Create)** → **Import**
2. Digite o ID: **10915**
3. Selecionar datasource: **Prometheus**
4. Click **Import**

#### Dashboard: RabbitMQ Monitoring
1. Ir em **+ (Create)** → **Import**
2. Digite o ID: **10991**
3. Selecionar datasource: **Prometheus**
4. Click **Import**

### 3. Dashboards Customizados Importantes

#### Métricas para Monitorar (HPA):
```
- CPU Usage por Pod: rate(container_cpu_usage_seconds_total[5m])
- Memory Usage por Pod: container_memory_working_set_bytes
- HTTP Request Rate: rate(http_requests_total[5m])
- HTTP Request Duration: histogram_quantile(0.95, http_request_duration_seconds_bucket)
- Número de Pods ativos: kube_deployment_status_replicas_available
```

## 🎯 Validando Funcionamento

### 1. Verificar Prometheus Coletando Métricas
```powershell
# Port-forward para testar localmente (opcional)
kubectl port-forward -n monitoring svc/prometheus-service 9090:9090
```

Acessar: `http://localhost:9090`
- Ir em **Status** → **Targets**
- Verificar se os jobs estão **UP** (verde)

### 2. Verificar Grafana
```powershell
kubectl port-forward -n monitoring svc/grafana-service 3000:3000
```

Acessar: `http://localhost:3000`
- Login com admin/admin
- Ir em **Explore**
- Executar query: `up`
- Deve mostrar métricas

### 3. Verificar Jaeger
```powershell
kubectl port-forward -n monitoring svc/jaeger-service 16686:16686
```

Acessar: `http://localhost:16686`
- Selecionar serviço: **Games.Api**
- Click **Find Traces**
- Deve mostrar traces das requisições

### 4. Verificar Metrics Server (HPA)
```powershell
# Ver uso de CPU/Memória dos nodes
kubectl top nodes

# Ver uso de CPU/Memória dos pods
kubectl top pods -n fiap-games-prod

# Ver status do HPA
kubectl get hpa -n fiap-games-prod
```

## 📈 Testando HPA com Monitoramento

### 1. Gerar Carga nas APIs
```powershell
# Instalar Apache Bench
choco install apache-httpd

# Teste de carga no Games API
ab -n 10000 -c 100 http://<GAMES-API-IP>/api/v1/search/games
```

### 2. Observar em Tempo Real

**Terminal 1 - HPA**:
```powershell
kubectl get hpa -n fiap-games-prod -w
```

**Terminal 2 - Pods**:
```powershell
kubectl get pods -n fiap-games-prod -w
```

**Terminal 3 - Métricas**:
```powershell
kubectl top pods -n fiap-games-prod -w
```

**Grafana**: Abrir dashboard e ver:
- CPU usage aumentando
- Pods sendo criados
- Request rate crescendo

## 🔧 Troubleshooting

### Prometheus não está coletando métricas
```powershell
# Ver logs do Prometheus
kubectl logs -n monitoring -l app=prometheus

# Verificar configuração
kubectl get configmap -n monitoring prometheus-config -o yaml

# Verificar se apps estão com anotações corretas
kubectl describe pod -n fiap-games-prod <pod-name> | grep -i annotations
```

### Grafana não conecta ao Prometheus
```powershell
# Verificar se Prometheus service existe
kubectl get svc -n monitoring prometheus-service

# Testar conectividade dentro do cluster
kubectl run -it --rm debug --image=busybox --restart=Never -- wget -O- http://prometheus-service.monitoring:9090/-/healthy
```

### Metrics Server não funciona
```powershell
# Ver logs
kubectl logs -n kube-system -l k8s-app=metrics-server

# Comum em ambientes de teste: adicionar flag --kubelet-insecure-tls
kubectl edit deployment metrics-server -n kube-system
# Adicionar em args:
#   - --kubelet-insecure-tls
```

### Jaeger não recebe traces
```powershell
# Verificar se apps estão configurados com OpenTelemetry
kubectl logs -n fiap-games-prod <pod-name> | grep -i "jaeger\|telemetry"

# Verificar variáveis de ambiente
kubectl describe pod -n fiap-games-prod <pod-name> | grep -i "JAEGER"
```

## 📹 Para o Vídeo (15 minutos)

### Demonstração de Monitoramento (Sua Parte - 5 min):

1. **Mostrar Dashboards Grafana** (2 min):
   - Dashboard de Kubernetes cluster
   - Dashboard de aplicações .NET
   - Métricas em tempo real

2. **Demonstrar APM com Jaeger** (1 min):
   - Mostrar trace de uma requisição
   - Explicar distributed tracing entre microsserviços

3. **Demonstrar HPA funcionando** (2 min):
   - Iniciar teste de carga
   - Mostrar no Grafana: CPU subindo
   - Mostrar pods sendo criados automaticamente
   - Explicar como métricas alimentam o HPA

## ✅ Checklist de Entrega

- [ ] Prometheus instalado e coletando métricas
- [ ] Grafana instalado com dashboards configurados
- [ ] Jaeger instalado e recebendo traces
- [ ] Metrics Server instalado (HPA funcionando)
- [ ] Teste de carga demonstrando HPA + métricas
- [ ] Screenshots/gravação mostrando funcionamento
- [ ] Documentação completa (este README)

## 🤝 Integração com Colega

### O que você precisa dele:
- ✅ Cluster Kubernetes criado
- ✅ Acesso kubectl configurado
- ✅ Namespace `fiap-games-prod` criado
- ✅ Aplicações deployadas (para coletar métricas reais)

### O que ele precisa de você:
- ✅ Confirmação de que monitoramento está funcionando
- ✅ URL do Grafana para incluir no vídeo
- ✅ Evidências de que HPA está usando métricas corretamente

## 📚 Referências

- [Prometheus Kubernetes](https://prometheus.io/docs/prometheus/latest/configuration/configuration/#kubernetes_sd_config)
- [Grafana Dashboards](https://grafana.com/grafana/dashboards/)
- [Jaeger OpenTelemetry](https://www.jaegertracing.io/docs/latest/opentelemetry/)
- [Kubernetes Metrics Server](https://github.com/kubernetes-sigs/metrics-server)
- [HPA Walkthrough](https://kubernetes.io/docs/tasks/run-application/horizontal-pod-autoscale-walkthrough/)

---

**Status**: ✅ Pronto para deploy assim que cluster estiver disponível
**Responsável**: [Seu Nome]
**Data**: Novembro 2025
