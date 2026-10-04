using Asp.Versioning;
using Contra_Filé.Application.DTO;
using Contra_Filé.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Contra_Filé.Controllers;

/// <summary>
/// Gerenciamento de pedidos do restaurante.
/// </summary>
/// <remarks>
/// Recurso escolhido para versionamento. v1 (deprecada) mantém a listagem antiga (array,
/// contrato do CP3); v2 (atual, default quando a requisição não informa versão) pagina a listagem
/// num envelope. GetById/Create/Delete não têm [MapToApiVersion], ficam disponíveis nas duas
/// versões, então o fluxo de escrita do CP3/CP4 continua funcionando com ou sem versão explícita.
/// </remarks>
[Route("api/[controller]")]
[ApiController]
[ApiVersion("1.0", Deprecated = true)]
[ApiVersion("2.0")]
[Produces("application/json")]
public class PedidoController(IPedidoService pedidoService, ILogger<PedidoController> logger) : ControllerBase
{
    /// <summary>[v1 - DEPRECADA] Lista todos os pedidos (contrato antigo: array, sem paginação).</summary>
    /// <response code="200">Lista retornada com sucesso.</response>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IReadOnlyList<PedidoResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAllV1() => Ok(pedidoService.GetAll());

    /// <summary>[v2] Lista pedidos paginados (envelope com page/pageSize/totalItems/totalPages/items).</summary>
    /// <param name="pagination">Parâmetros de paginação (page ≥ 1, pageSize entre 1 e 100).</param>
    /// <response code="200">Página retornada com sucesso (pode vir com <c>items</c> vazio).</response>
    /// <response code="400">page ou pageSize fora do intervalo permitido.</response>
    [HttpGet]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(typeof(PagedResponse<PedidoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetAllV2([FromQuery] PaginationQuery pagination)
    {
        if (pagination.TryGetError(out var message))
            return BadRequest(message);

        var (items, total) = pedidoService.GetPaged(pagination.Page, pagination.PageSize);

        return Ok(new PagedResponse<PedidoResponse>(
            items.Select(PedidoResponse.FromDomain).ToList(),
            pagination.Page,
            pagination.PageSize,
            total));
    }

    /// <summary>Obtém um pedido pelo id. Disponível em v1 e v2.</summary>
    /// <param name="id">Identificador único do pedido.</param>
    /// <response code="200">Pedido encontrado.</response>
    /// <response code="404">Pedido não encontrado.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PedidoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        var pedido = pedidoService.GetById(id);
        return pedido is null ? NotFound() : Ok(pedido);
    }

    /// <summary>Abre um novo pedido para uma mesa. Disponível em v1 e v2. Limitado a 10 requisições/minuto.</summary>
    /// <param name="request">Dados do pedido (mesa, usuário, pratos).</param>
    /// <response code="201">Pedido criado.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="404">Mesa ou usuário referenciados não existem.</response>
    /// <response code="429">Limite de requisições excedido — tente novamente depois do tempo indicado em Retry-After.</response>
    [HttpPost]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(PedidoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public IActionResult Create([FromBody] PedidoRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        logger.LogInformation(
            "Abrindo pedido para Mesa {MesaId} e Usuario {UserId}. TraceId: {TraceId}",
            request.MesaId, request.UserId, HttpContext.TraceIdentifier);

        var pedido = pedidoService.Create(request);

        logger.LogInformation(
            "Pedido {PedidoId} aberto com sucesso (Mesa {MesaId}, Usuario {UserId}). TraceId: {TraceId}",
            pedido.Id, pedido.MesaId, pedido.UserId, HttpContext.TraceIdentifier);

        return CreatedAtAction(nameof(GetById), new { id = pedido.Id }, pedido);
    }

    /// <summary>Remove um pedido pelo id. Disponível em v1 e v2.</summary>
    /// <param name="id">Identificador único do pedido.</param>
    /// <response code="204">Removido com sucesso.</response>
    /// <response code="404">Pedido não encontrado.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id) =>
        pedidoService.Delete(id) ? NoContent() : NotFound();
}