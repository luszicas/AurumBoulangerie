# 🥐 Aurum Boulangerie

### Sistema Phygital de Autoatendimento & Kitchen Display (KDS)

> **Solution:** xFood
> **Status:** 🚧 Em desenvolvimento ativo
> **Stack:** .NET 9 | ASP.NET Core MVC | EF Core | WinForms

---

## 🔗 Demonstração Visual

📸 Acesse fotos do projeto, capturas de tela e o portfólio completo em:  
👉 **https://lucashuerdo.com.br**

## 📌 Visão Geral

A **Aurum Boulangerie** é um sistema integrado desenvolvido sobre a solution **xFood**, projetado para digitalizar e organizar a operação de padarias e cafés premium.

O projeto conecta:

* 🌐 Salão (Aplicação Web MVC)
* 👨‍🍳 Cozinha (KDS Desktop)
* 🗄️ Banco de dados centralizado

Tudo compartilhando as mesmas regras de negócio através de **Clean Architecture**, garantindo consistência, reuso e escalabilidade.

O conceito é **Phygital (Physical + Digital)**: eliminar filas, reduzir erros e tornar o fluxo de produção inteligente.

---

# 🏛️ Arquitetura da Solution (xFood)

A solution foi construída seguindo **Clean Architecture + N-Layer**, com separação explícita de responsabilidades.

```
xFood.sln
│
├── xFood.Domain
├── xFood.Infrastructure
├── xFood.Web
└── xFood.KDS
```

---

## 🔹 xFood.Domain

Camada central e independente.

Contém:

* Entidades principais:

  * `Product`
  * `Category`
  * `Order`
  * `OrderItem`
  * `User`
* Enums de Status
* Interfaces de Repositório
* Regras de negócio

**Não depende de nenhuma outra camada.**

---

## 🔹 xFood.Infrastructure

Responsável por implementar o acesso a dados.

* `xFoodDbContext`
* Entity Framework Core 9
* Configurações Fluent API
* Migrations
* Seed inteligente executado no startup
* Implementação concreta dos Repositórios

---

## 🔹 xFood.Web (ASP.NET Core MVC)

Interface do cliente e do administrador.

### Funcionalidades:

#### 🥐 Módulo Cliente

* Vitrine digital estilo **Dark Luxury**
* Filtro por categoria
* Live Search com AJAX
* Carrinho de compras
* Checkout nominal (pedido por nome)
* Controle automático de estoque
* Geração automática de número do pedido

#### 📊 Módulo Administrativo

* Dashboard com KPIs diários
* CRUD completo (Produtos, Categorias, Usuários)
* Controle de estoque
* RBAC (Administrador / Gerente)
* Sessão autenticada via Session

---

## 🔹 xFood.Desktop (Windows Forms)

Kitchen Display System executado em ambiente de cozinha.

### Características:

* Polling automático a cada 5 segundos
* Exibição de pedidos em formato de ticket
* Interface de alto contraste
* Fluxo de status:

```
Pendente → Em Preparo → Pronto → Entregue
```

* Compartilha Domain e Infrastructure com a Web
* Atualização centralizada via banco SQL Server

---

# 🚀 Stack Tecnológica

| Camada          | Tecnologia                    |
| --------------- | ----------------------------- |
| Backend         | .NET 9                        |
| Web             | ASP.NET Core MVC              |
| Desktop         | Windows Forms                 |
| ORM             | Entity Framework Core 9       |
| Banco           | SQL Server (LocalDB)          |
| Front-End       | Bootstrap 5 + CSS customizado |
| Comunicação KDS | Polling Timer                 |
| Arquitetura     | Clean Architecture            |

---

# 🔄 Fluxo Operacional

1. Cliente realiza pedido na Web
2. Pedido é persistido no SQL Server
3. KDS detecta automaticamente via polling
4. Chef atualiza status
5. Administração acompanha em tempo real

---

# 🧠 Decisões Técnicas Relevantes

* Separação total de regras de negócio
* Reuso de DLLs entre Web e Desktop
* Banco único compartilhado
* Seed automático configurado no `Program.cs`
* Migrations versionadas
* Arquitetura preparada para futura migração para:

  * SignalR (tempo real real)
  * API REST
  * SaaS multi-tenant

---

# 🛠️ Como Executar

## Pré-requisitos

* .NET 9 SDK
* SQL Server LocalDB
* Visual Studio 2022+

---

## 1️⃣ Clonar

```bash
git clone https://github.com/lucas.hlima8/AurumBoulangerie.git
cd AurumBoulangerie
```

---

## 2️⃣ Restaurar pacotes

```bash
dotnet restore
```

---

## 3️⃣ Aplicar migrations

```bash
dotnet ef database update --startup-project xFood.Web
```

---

## 4️⃣ Executar aplicação Web

Definir `xFood.Web` como Startup Project e executar.

---

## 5️⃣ Executar KDS

Iniciar `xFood.desktop` como nova instância.

---

# 📈 Roadmap

* [ ] Integração com pagamento online (Pix / Cartão)
* [ ] Login de clientes
* [ ] Histórico de pedidos
* [ ] Sistema de fidelidade (Lista VIP)
* [ ] Substituição de polling por SignalR
* [ ] Adaptação para Totem físico

---

# 🎯 Objetivo do Projeto

Este projeto demonstra:

* Arquitetura limpa aplicada na prática
* Reuso entre aplicações Web e Desktop
* Modelagem de domínio consistente
* Estrutura pronta para produto comercial
* Capacidade de construção full-stack .NET

---

# 📄 Licença

Projeto proprietário.
Desenvolvido por Lucas Huerdo.

🌐 **Meu Portfólio:** https://lucashuerdo.com.br  

⭐ Projeto desenvolvido com foco em qualidade, organização e aprendizado contínuo.

