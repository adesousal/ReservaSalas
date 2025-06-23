## ReservaSalas

Sistema para gerenciamento de reservas de salas de reunião em um coworking, com aplicação ASP.NET Core MVC consumindo uma Web API.

---

### 📚 Tecnologias

- **ASP.NET Core 9.0** (MVC e Web API)
- **Entity Framework Core** (Npgsql provider para PostgreSQL)
- **Clean Architecture / DDD**
  - Camadas: Domain, Application, Infrastructure, WebApp
  - Padrões: Repository, Unit of Work
- **Injeção de Dependência** (Microsoft DI)
- **xUnit + Moq** para testes unitários
- **Coverlet** para cobertura de código via XPlat Collector
- **Bootstrap 5 + Bootstrap Icons** no front-end
- **jQuery Validation Unobtrusive** para validação client-side
- **Swagger / Swashbuckle** para documentação da API

> **Por que essas escolhas?**
>
> - .NET 9: performance, recursos modernos e suporte LTS.
> - EF Core + PostgreSQL: compatível, extensível e performático.
> - Clean Architecture e DDD: separação clara de responsabilidades e fácil manutenção.
> - xUnit/Moq: testes isolados, mocks leves e integração com CI.
> - Coverlet XPlat: coleta de cobertura sem dependências externas.
> - Bootstrap: UI responsiva, rápida de adotar sem esforço de design.
> - Swagger: facilita descoberta e testes da API.

---

### 🎯 Regras de Negócio

1. **Conflito de horário**
   - Cada reserva dura 1 hora.
   - Não é possível reservar sala A das 10h às 11h se já existir outra reserva sobrepondo esse intervalo.
2. **Cancelamento**
   - Só pode ser realizado até 24 horas antes do horário agendado.
3. **Status**
   - `Confirmada` ou `Cancelada`.
4. **Envio de e-mail**
   - Confirmação e cancelamento disparam notificações por e-mail.
   - Falha no envio não reverte a operação, mas informa o cliente.
5. **Validação de usuário**
   - Deve ser um e-mail válido (server & client).

---

### ⚙️ Pré-requisitos

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL 12+](https://www.postgresql.org/download/)
- Opcional: [Mailtrap](https://mailtrap.io/) ou SMTP real para testes de e-mail
- Node/npm (para gerenciar pacotes front-end, se personalizado)

---

### 🚀 Passo a passo de configuração e execução

1. **Clone o repositório**

   ```bash
   git clone https://github.com/adesousal/ReservaSalas.git
   cd ReservaSalas
   ```

2. **Configurar **``** (API)**\
   Adicione as strings de conexão e configurações de e-mail:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=ReservaSalas;Username=usuario;Password=senha"
     },
     "Email": {
       "Remetente": "seu@remetente.com",
       "Smtp": {
         "Host": "smtp.seuprovedor.com",
         "Port": 587,
         "Usuario": "seu@usuario",
         "Senha": "sua-senha"
       }
     }
   }
   ```

3. **Criar banco e extensão **``\
   Execute no psql:

   ```sql
   CREATE DATABASE "ReservaSalas";
   \c "ReservaSalas";
   CREATE EXTENSION IF NOT EXISTS unaccent;
   ```

4. **Aplicar migrations e gerar banco**

   ```bash
   cd ReservaSalas.API
   dotnet ef database update
   ```

5. **Rodar a API com Swagger**

   ```bash
   dotnet run
   # Acesse https://localhost:5001/swagger
   ```

6. **Rodar a aplicação MVC (front-end)**

   ```bash
   cd ../ReservaSalas.WebApp
   dotnet run --urls "https://localhost:5101"
   # Acesse https://localhost:5101
   ```

---

### 🧪 Testes Unitários

- **Executar todos os testes**

  ```bash
  cd ReservaSalas.Tests
  dotnet test
  ```

- **Cobertura de código**\
  Tem duas maneiras de gerar e visualizar o relatório de cobertura:

  1. **Script PowerShell** (recomendado):

     ```powershell
     # Na raiz do repositório
     Set-ExecutionPolicy Bypass -Scope Process -Force
     .\generate_coverage.ps1
     ```

     Ele irá:

     - Executar testes com coleta de cobertura (XPlat Collector).
     - Gerar relatório HTML em `coverage/report`.
     - Abrir automaticamente `coverage/report/index.htm` no navegador.

  2. **Manual** (sem script):

     ```bash
     # Executar testes e coletar cobertura
     dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput=./coverage/coverage.opencover.xml

     # Gerar relatório HTML (ReportGenerator instalado)
     reportgenerator \
       -reports:coverage/coverage.opencover.xml \
       -targetdir:coverage/report \
       -reporttypes:Html

     # Abrir relatório
     start coverage/report/index.htm   # Windows
     open coverage/report/index.htm    # macOS
     xdg-open coverage/report/index.htm # Linux
     ```

---

### 🔍 Estrutura de pastas

```
/ReservaSalas
 ├─ ReservaSalas.API           # Web API (handlers, controllers, DTOs)
 ├─ Domain                     # Entidades, Enums, Filters, Repositórios
 ├─ Application                # Casos de uso, DTOs, serviços de aplicação
 ├─ Infrastructure             # EF Core, implementações de repositório e e-mail
 ├─ WebApp                     # ASP.NET Core MVC front-end
 └─ ReservaSalas.Tests         # Testes unitários xUnit + Moq
```

---

### 📖 Considerações Finais

Este projeto adota boas práticas de Clean Code, SOLID e DDD, garantindo escalabilidade e facilidade de manutenção. O envio de e-mail está desacoplado e tratado com resiliência, assegurando que falhas na comunicação não quebrem a operação principal.

Sinta-se à vontade para adaptar provedores de e-mail, estilizar o front-end ou evoluir os padrões de design conforme a necessidade do seu time!

