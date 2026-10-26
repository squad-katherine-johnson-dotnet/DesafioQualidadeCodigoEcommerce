# 🛒 Refatoração E-Commerce 

## 🎯 Visão Geral e Objetivo

Refatorar uma Web API monolítica em .NET que continha graves problemas de arquitetura (como *God Controller*, ausência de camadas, acoplamento direto com banco de dados, falhas em transações e violações do SOLID), transformando-a em uma aplicação limpa, manutenível, testável e bem estruturada.

Projeto desenvolvido em squad como parte do desafio de **Qualidade de Código e Arquitetura** do Bootcamp Back-End .NET (WoMakersCode).

---

## 🏗️ Arquitetura e Estrutura do Esqueleto (Etapa 1)

O projeto foi reestruturado seguindo o padrão de Arquitetura em Camadas (Layered Architecture) para separar adequadamente as responsabilidades:

```text
DesafioQualidadeCodigoEcommerce/
├── src/
│   ├── Ecommerce.Domain/         # Entidades, Enums e Interfaces de Repositório/Serviço
│   ├── Ecommerce.Application/    # Casos de Uso, DTOs (Request/Response) e Regras de Negócio
│   ├── Ecommerce.Infrastructure/ # DbContext, Configuração de Banco de Dados e Repositórios
│   └── Ecommerce.API/            # Controllers (HTTP), Middlewares e Injeção de Dependência
└── tests/
    └── Ecommerce.Tests/          # Projeto de Testes Unitários (xUnit)
```

---

## 👥 Divisão de Responsabilidades e Features (Etapa 2)

| Responsável | Área / Feature | Descrição e Escopo Técnico |
| --- | --- | --- |
| **Rosana Lima** | **Setup Base & Produtos** | Criação da arquitetura em camadas e DTOs, CRUD de Produtos e validações. |
| **Yasmin Bezerra** | **Estoque** | Consultas, movimentações (entrada/saída), regras de reabastecimento e validação contra quantidades inválidas. |
| **Mariana Brombilla** | **Pedidos** | Cálculo de total e regras de desconto, validação de itens com estoque e correção da atomicidade/estorno em falhas de pagamento. |
| **Bianca Fernandes** | **Pagamento & Logística** | Desacoplamento de serviços, processamento assíncrono, controle de status (Enums), lógica de reprocessamento/reenvio e implementação de testes automatizados nas camadas de domínio, aplicação e infraestrutura. |

---

## ⚙️ Como Executar o Projeto

### 1. Clonar o repositório

```bash
git clone https://github.com/squad-katherine-johnson-dotnet/DesafioQualidadeCodigoEcommerce.git
cd DesafioQualidadeCodigoEcommerce
```

### 2. Compilar a solução

```bash
dotnet build
```

### 3. Configurar o ambiente

**Banco de dados:** o projeto utiliza SQLite para persistência dos dados. A connection string padrão está definida em `appsettings.json`.

**Mocks:** as configurações de pagamento e logística estão no arquivo `appsettings.Development.json`.

| Configuração | Valor padrão | Comportamento |
|---|---|---|
| `PagamentoMock:AprovarPagamento` | `true` | Simula a aprovação do pagamento. |
| `LogisticaMock:SimularFalha` | `false` | Permite testar o fluxo normal de logística. |

Para simular uma falha no processamento logístico, altere temporariamente `LogisticaMock:SimularFalha` para `true`. Após o teste, restaure o valor para `false`.

### 4. Executar a API

```bash
dotnet run --project src/Ecommerce.API/Ecommerce.API.csproj
```

### 5. Acessar o Swagger

Com a API em execução, acesse uma das URLs:

- **HTTPS:** https://localhost:7290/swagger
- **HTTP:** http://localhost:5032/swagger

---

## 📌 Endpoints da API

A API disponibiliza endpoints para gerenciamento de produtos, controle de estoque e operações de pedidos. Todos podem ser consultados e testados pelo Swagger.

### 🏷️ Produtos

| Método | Endpoint | Descrição |
|---|---|---|
| `GET` | `/api/Produtos` | Lista os produtos cadastrados. |
| `GET` | `/api/Produtos/{id}` | Consulta um produto pelo ID. |
| `POST` | `/api/Produtos` | Cadastra um produto. |
| `PUT` | `/api/Produtos/{id}` | Atualiza os dados de um produto. |
| `DELETE` | `/api/Produtos/{id}` | Remove um produto. |

### 📦 Estoque

| Método | Endpoint | Descrição |
|---|---|---|
| `GET` | `/api/Estoque` | Consulta o estoque dos produtos. |
| `GET` | `/api/Estoque/{id}` | Consulta o estoque de um produto pelo ID. |
| `GET` | `/api/Estoque/abaixo-do-minimo` | Lista produtos abaixo da quantidade mínima configurada. O parâmetro `quantidadeMinima` é opcional e tem valor padrão `5`. |
| `POST` | `/api/Estoque/{id}/entradas` | Registra uma entrada de estoque para um produto. |
| `POST` | `/api/Estoque/{id}/saidas` | Registra uma saída de estoque para um produto. |
| `POST` | `/api/Estoque/saidas-em-lote` | Registra saídas de estoque para vários produtos em uma operação. |

### 🛒 Pedidos

| Método | Endpoint | Descrição |
|---|---|---|
| `GET` | `/api/Pedidos` | Lista os pedidos cadastrados. |
| `GET` | `/api/Pedidos/{id}` | Consulta um pedido pelo ID. |
| `POST` | `/api/Pedidos` | Cria um pedido. |
| `POST` | `/api/Pedidos/{id}/pagamento/reprocessar` | Solicita o reprocessamento do pagamento de um pedido. |
| `POST` | `/api/Pedidos/{id}/logistica/reenviar` | Solicita o reenvio do processamento logístico de um pedido. |

### 🧪 Testar os endpoints

Com a API em execução, acesse o Swagger para consultar os parâmetros, os modelos de requisição e as respostas de cada endpoint. Consulte as URLs na seção [Como Executar o Projeto](#-como-executar-o-projeto).

---

## 🧪 Testes Automatizados

Para executar os testes automatizados da solução, utilize:

```bash
dotnet test
```

Na última execução validada, foram aprovados **154 testes**, sem falhas ou testes ignorados.

---

## 🔄 Fluxo de Trabalho e Git da Squad

1. **Atualizar a branch principal antes de iniciar:**

```bash

git switch main
git pull origin main

```

2. **Criar a sua branch a partir da `main`:**

```bash

git switch -c feature/nome-da-sua-area

```

3. **Enviar as alterações e abrir Pull Request (PR) para a `main`:**

```bash

git add .
git commit -m "feat: implementa camada de estoque"
git push -u origin feature/nome-da-sua-area

```
