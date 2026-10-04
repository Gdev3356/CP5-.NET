# Contra-Filé API

API REST para gerenciamento de restaurante: mesas, pedidos, cardápio, usuários e avaliações.

##  Integrantes

| Nome | RM |
|---|---|
| Gustavo Keiji Okada | RM563428 |


##  Arquitetura

Clean Architecture em 4 projetos:

- **`Contra-Filé.Domain`** — entidades (`Mesa`, `Pedido`, `Prato`, `User`, `UserConfig`, `Avaliacao`), regras de negócio e `DomainException`. Não depende de mais nenhum projeto.
- **`Contra-Filé.Application`** — casos de uso (`IMesaService`, `IPedidoService`, `IPratoService`), DTOs de request/response e o contrato `IRepository<T>`.
- **`Contra-Filé.Infrastructure`** — `ContraFiléContext` (EF Core), `Configurations` de mapeamento e a implementação genérica `Repository<T>`.
- **`Contra-Filé.API`** — controllers, Swagger, `GlobalExceptionHandler`, health checks e composição da aplicação (`Program.cs`).

##  Domínio e SGBD

- **SGBD:** MySQL (via `Pomelo.EntityFrameworkCore.MySql`)
- **Connection string:** `ConnectionStrings:ContraFiléMySQL` no `appsettings.json` (local) — **não commitar credenciais reais**.
- **Entidades principais:** `Mesa` (número, ocupação), `Pedido` (mesa, usuário, valor total), `Prato` (nome, preço, tempo de preparo, descrição), `User` (nome, e-mail, senha com hash bcrypt, data de nascimento), `Avaliacao` (usuário, nota de 1 a 5, descrição).

##  Como subir a API

```bash
# Na raiz da solution
dotnet restore
dotnet run --project Contra-Filé.API
```

A porta exata aparece no console ao subir (ex.: `http://localhost:5074`). URLs principais:

| Recurso | URL |
|---|---|
| Swagger UI | `http://localhost:PORTA/` (raiz) |
| Health check | `http://localhost:PORTA/health` |
| API (exemplo) | `http://localhost:PORTA/api/pedido` |

> `/health` **não aparece** na lista do Swagger — é mapeado fora do MVC, propositalmente.

##  Health Checks (`GET /health`)

Endpoint único (`GET /health`) reportando 3 checks:

| Check | O que valida |
|---|---|
| `self` | Processo da API está no ar (sempre `Healthy` se a API respondeu). |
| `mysql-database` | Conectividade com o MySQL, via `AddDbContextCheck<ContraFiléContext>()` sobre o mesmo `DbContext` do CP2. |
| `fiap-site` | Disponibilidade do site `https://www.fiap.com.br` (check externo recomendado pelo enunciado). |

Status HTTP: `Healthy`/`Degraded` → **200**, `Unhealthy` → **503**.

### Exemplo — tudo saudável (200 OK)

```json
{
  "status": "Healthy",
  "totalDurationMs": 45.2,
  "checks": [
    { "name": "self", "status": "Healthy", "durationMs": 0.1, "description": "API no ar.", "exception": null },
    { "name": "mysql-database", "status": "Healthy", "durationMs": 12.3, "description": null, "exception": null },
    { "name": "fiap-site", "status": "Healthy", "durationMs": 30.8, "description": "Site da FIAP respondendo normalmente.", "exception": null }
  ]
}
```

### Exemplo — banco fora do ar (503 Service Unavailable)

```json
{
  "status": "Unhealthy",
  "totalDurationMs": 5021.7,
  "checks": [
    { "name": "self", "status": "Healthy", "durationMs": 0.1, "description": "API no ar.", "exception": null },
    { "name": "mysql-database", "status": "Unhealthy", "durationMs": 5000.4, "description": null, "exception": "Unable to connect to any of the specified MySQL hosts." },
    { "name": "fiap-site", "status": "Healthy", "durationMs": 21.2, "description": "Site da FIAP respondendo normalmente.", "exception": null }
  ]
}
```

> O campo `exception` só aparece com detalhe em ambiente **Development**. Em produção, o valor vem `null` mesmo com o check `Unhealthy`.

> Se o `fiap-site` cair (rede instável, bloqueio de firewall), ele também vira `Unhealthy` e derruba o `/health` inteiro para 503, mesmo com o banco ok — é intencional, evidencia como uma dependência externa pode impactar o status agregado.

## Logs estruturados e correlação (`traceId`)

Todo log usa `ILogger<T>` com propriedades nomeadas (nunca string concatenada) e inclui o `TraceId` da requisição (`HttpContext.TraceIdentifier`), permitindo correlacionar todas as linhas de uma mesma chamada.

### Exemplo — abertura de pedido com sucesso

```
info: Contra_Filé.Controllers.PedidoController[0]
      Abrindo pedido para Mesa 3fa85f64-5717-4562-b3fc-2c963f66afa6 e Usuario 9c1b1e2a-... . TraceId: 0HN9K2L8QF3A1:00000003
info: Contra_Filé.Controllers.PedidoController[0]
      Pedido 7d2e4f10-... aberto com sucesso (Mesa 3fa85f64-..., Usuario 9c1b1e2a-...). TraceId: 0HN9K2L8QF3A1:00000003
```

### Exemplo — erro tratado pelo `GlobalExceptionHandler`

```
fail: Contra_Filé.API.Exceptions.GlobalExceptionHandler[0]
      Exceção não tratada. TraceId: 0HN9K2L8QF3A1:00000005 - Mensagem: Mesa 'a1b2c3d4-...' não encontrada.
      System.Collections.Generic.KeyNotFoundException: Mesa 'a1b2c3d4-...' não encontrada.
         at Contra_Filé.Application.Services.PedidoService.Create(PedidoRequest request)
```

Em **Development**, o mesmo `traceId` também vem no corpo da resposta (`ProblemDetails.Extensions["traceId"]`); em **Production**, a resposta nunca inclui stack trace — o detalhe fica só no log.

**[ESPAÇO PARA PRINT REAL]** — cole aqui o print do console com o log de vocês.

## Mapeamento de exceções (`GlobalExceptionHandler`)

| Exceção | Status HTTP | Título |
|---|---|---|
| `ArgumentNullException` | 400 | Requisição inválida |
| `ArgumentException` | 400 | Requisição inválida |
| `DomainException` | 400 | Regra de negócio violada |
| `InvalidOperationException` | 400 | Operação inválida |
| `KeyNotFoundException` | 404 | Recurso não encontrado |
| `UnauthorizedAccessException` | 401 | Não autorizado |
| Qualquer outra | 500 | Erro interno do servidor |

`KeyNotFoundException` é usada, por exemplo, quando `POST /api/pedido` referencia uma `Mesa` ou `User` que não existe.

## Testes automatizados

Dois projetos de teste na solution:

- **`Contra-Filé.Domain.Tests`** — referencia só o `Domain`, sem mocks. Cobre a regra de nota (1 a 5) e descrição (12-100 caracteres) da entidade `Avaliacao`.
- **`Contra-Filé.Application.Tests`** — usa `Moq` para simular `IRepository<T>`. Cobre `PedidoService.Create`: Mesa/Usuário inexistente lança `KeyNotFoundException` e **não** persiste (`Times.Never`); caminho feliz persiste uma vez (`Times.Once`).

```bash
# Na raiz da solution
dotnet test
```

### Exemplo de saída esperada

```
Passed!  - Failed: 0, Passed: 7, Skipped: 0, Total: 7, Duration: 42 ms - Contra-Filé.Domain.Tests.dll
Passed!  - Failed: 0, Passed: 3, Skipped: 0, Total: 3, Duration: 118 ms - Contra-Filé.Application.Tests.dll
```

**[ESPAÇO PARA PRINT REAL]** — cole aqui a saída de `dotnet test` (ou print do Test Explorer) de vocês.

## `/docs/`

Pasta com as evidências desta entrega (prints/JSON):

- [ ] `health-healthy.png` ou `.json` — resposta de `/health` com tudo no ar
- [ ] `health-unhealthy.png` ou `.json` — resposta de `/health` com o banco fora do ar (503)
- [ ] `log-traceid.png` — console mostrando as duas linhas de log com o mesmo `TraceId`
- [ ] `dotnet-test.png` — saída do `dotnet test` com os testes passando# CP5-.NET
