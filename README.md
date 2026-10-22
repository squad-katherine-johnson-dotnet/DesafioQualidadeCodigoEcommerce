# 🛒 Desafio Refatoração E-Commerce - Squad Katherine Johnson

Projeto desenvolvido em squad como parte do desafio de **Qualidade de Código e Arquitetura** do Bootcamp Back-End .NET (WoMakersCode).

---

## 🎯 Objetivo do Desafio

Refatorar uma Web API monolítica em .NET que continha graves problemas de arquitetura (como *God Controller*, ausência de camadas, acoplamento direto com banco de dados, falhas em transações e violações do SOLID), transformando-a em uma aplicação limpa, manutenível, testável e bem estruturada.

---

## 🏗️ Arquitetura e Estrutura do Esqueleto (Etapa 1)

O projeto foi reestruturado no padrão de **Arquitetura em Camadas (Clean / Layered Architecture)** para separar adequadamente as responsabilidades:

```text
EcommerceRefatorado/
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
| <nobr>**Rosana Lima**</nobr> | **Setup Base & Produtos** | Criação da arquitetura em camadas e DTOs, CRUD de Produtos, validações e testes unitários do catálogo. |
| <nobr>**Yasmin Bezerra**</nobr> | **Estoque** | Consultas, movimentações (entrada/saída), regras de reabastecimento e validação contra quantidades inválidas. |
| <nobr>**Mariana Brombilla**</nobr> | **Pedidos** | Cálculo de total e regras de desconto, validação de itens com estoque e correção da atomicidade/estorno em falhas de pagamento. |
| <nobr>**Bianca Fernandes**</nobr> | **Pagamento & Logística** | Desacoplamento de serviços, processamento assíncrono, controle de status (Enum) e lógica de reprocessamento/reenvio. |

---

## 🚀 Como Executar o Projeto

1. **Clonar o repositório:**

```bash
git clone [https://github.com/squad-katherine-johnson-dotnet/DesafioQualidadeCodigoEcommerce.git](https://github.com/squad-katherine-johnson-dotnet/DesafioQualidadeCodigoEcommerce.git)
cd DesafioQualidadeCodigoEcommerce

```

2. **Compilar a solução:**

```bash

dotnet build

```

3. **Executar a API:**

```bash

dotnet run --project src/Ecommerce.API/Ecommerce.API.csproj

```

4. **Acessar o Swagger:**

Abra o navegador em `http://localhost:5000/swagger/index.html`.

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
