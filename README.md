# Contra-Filé API

API REST para gerenciamento de restaurante: mesas, pedidos, cardápio, usuários e avaliações.

## Integrantes

| Nome | RM |
|---|---|
| Gustavo Keiji Okada | RM563428 |

## Arquitetura

Clean Architecture em 4 projetos:

- **`Contra-Filé.Domain`** — entidades (`Mesa`, `Pedido`, `Prato`, `User`, `UserConfig`, `Avaliacao`), regras de negócio e `DomainException`. Não depende de mais nenhum projeto.
- **`Contra-Filé.Application`** — casos de uso (`IMesaService`, `IPedidoService`, `IPratoService`), DTOs de request/response e o contrato `IRepository<T>`.
- **`Contra-Filé.Infrastructure`** — `ContraFiléContext` (EF Core), `Configurations` de mapeamento e a implementação genérica `Repository<T>`.
- **`Contra-Filé.API`** — controllers, Swagger, `GlobalExceptionHandler`, health checks e composição da aplicação (`Program.cs`).

## Domínio e SGBD

- **SGBD:** MySQL (via `Pomelo.EntityFrameworkCore.MySql`)
- **Connection string:** `ConnectionStrings:ContraFiléMySQL` no `appsettings.json` (local) — **não commitar credenciais reais**.
- **Entidades principais:** `Mesa` (número, ocupação), `Pedido` (mesa, usuário, valor total), `Prato` (nome, preço, tempo de preparo, descrição), `User` (nome, e-mail, senha com hash bcrypt, data de nascimento), `Avaliacao` (usuário, nota de 1 a 5, descrição).

## Como subir a API

```bash
# Na raiz da solution
dotnet restore
dotnet run --project Contra-Filé.API
```

A porta exata aparece no console ao subir (ex.: `http://localhost:5074`). URLs principais:

| Recurso | URL |
|---|---|
| Swagger UI | `http://localhost:PORTA/` (raiz — grupos v1.0 e v2.0 no seletor) |
| Health check | `http://localhost:PORTA/health` |
| Pedidos — v2 (atual, default) | `http://localhost:PORTA/api/pedido` |
| Pedidos — v1 (deprecada) | `http://localhost:PORTA/api/pedido?api-version=1.0` |

> `/health` **não aparece** na lista do Swagger — é mapeado fora do MVC, propositalmente.

## 🩺 Health Checks (`GET /health`)

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

## Versionamento de API (CP5)

Recurso escolhido para versionar: **Pedido** (é o que mais cresce no domínio do restaurante). Os
demais recursos (`Mesa`, `Prato`) foram marcados `[ApiVersionNeutral]` — continuam funcionando e
visíveis no Swagger em qualquer versão, sem ganhar um contrato novo.

| Versão | Status | `GET /api/pedido` devolve |
|---|---|---|
| **1.0** | Deprecada | Array simples (contrato antigo do CP3) |
| **2.0** | Atual (default) | Envelope paginado (ver seção de Paginação) |

`GetById`, `POST` e `DELETE` de Pedido **não** têm versão fixa — funcionam em 1.0 e em 2.0, então
o fluxo de abrir/remover pedido do CP3/CP4 continua funcionando com ou sem versão informada.

**Como informar a versão** (os três funcionam):
```http
GET /api/pedido?api-version=1.0
```
```http
GET /api/pedido
X-Api-Version: 1.0
```
```http
GET /api/pedido
```
O terceiro (sem versão nenhuma) cai na **2.0** — é o `DefaultApiVersion` configurado.

Toda resposta inclui os headers `api-supported-versions` (`1.0, 2.0`) e `api-deprecated-versions`
(`1.0`), por causa do `ReportApiVersions = true`.

**[ESPAÇO PARA PRINT REAL]** — cole aqui os headers de resposta (`api-supported-versions` /
`api-deprecated-versions`) e os dois JSONs (`v1` lista, `v2` envelope) do mesmo recurso.

## Paginação (CP5 — só na v2)

A v1 preserva o contrato antigo (array) de propósito — paginar ela seria uma mudança quebrando
contrato sem versionar, exatamente o que o versionamento evita.

| Parâmetro | Padrão | Regra |
|---|---|---|
| `page` | `1` | inteiro ≥ 1 |
| `pageSize` | `20` | inteiro entre 1 e 100 |

Fora do intervalo → **400 Bad Request** com a regra explicada no corpo. Página além do total →
**200 OK** com `items: []` (não é erro). A página é cortada **no banco** (`Count` + `OrderBy` +
`Skip` + `Take` no `IQueryable` de `Repository<T>.GetPaged`, ordenado por `CreatedAt` — só
materializa a página pedida, nunca a tabela inteira).

### Exemplo — `GET /api/pedido?page=1&pageSize=2`

```json
{
  "items": [
    { "id": "3fa85f64-...", "totalValue": 154.80, "mesaId": "...", "userId": "..." },
    { "id": "7d2e4f10-...", "totalValue": 68.50,  "mesaId": "...", "userId": "..." }
  ],
  "page": 1,
  "pageSize": 2,
  "totalItems": 5,
  "totalPages": 3,
  "hasPrevious": false,
  "hasNext": true
}
```

### Exemplo — `GET /api/pedido?page=0&pageSize=9999` (400)

```json
"pageSize deve estar entre 1 e 100."
```


## Rate Limit (CP5)

`POST /api/pedido` está limitado a **10 requisições por minuto** (fixed window, sem fila —
`QueueLimit = 0`). A política é aplicada só nessa action via `[EnableRateLimiting("fixed")]` —
**não é global**, então `GET /health` nunca entra no teto (não precisou de `DisableRateLimiting`
em lugar nenhum, porque a política nunca foi aplicada lá para começo de conversa).

Ao estourar o limite: **429 Too Many Requests**, header `Retry-After: 60` e corpo JSON:
```json
{
  "type": "about:blank",
  "title": "Muitas requisições",
  "status": 429,
  "detail": "Limite de 10 requisições por minuto excedido para este endpoint. Tente novamente em instantes.",
  "instance": "/api/pedido"
}
```

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
- **`Contra-Filé.Application.Tests`** — usa `Moq` para simular `IRepository<T>`. Cobre `PedidoService.Create`: Mesa/Usuário inexistente lança `KeyNotFoundException` e **não** persiste (`Times.Never`); caminho feliz persiste uma vez (`Times.Once`). CP5: também cobre `PaginationQuery.TryGetError` — `[Theory]` com 5 combinações inválidas de `page`/`pageSize` e `[Fact]` para o intervalo válido (sem mock, sem subir API/banco).

```bash
# Na raiz da solution
dotnet test
```

### Exemplo de saída esperada

```
Passed!  - Failed: 0, Passed: 7, Skipped: 0, Total: 7, Duration: 42 ms - Contra-Filé.Domain.Tests.dll
Passed!  - Failed: 0, Passed: 6, Skipped: 0, Total: 6, Duration: 131 ms - Contra-Filé.Application.Tests.dll
```