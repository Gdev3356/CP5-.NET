using Contra_Filé.Application.DTO;
using Contra_Filé.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Contra_Filé.Controllers;

/// <summary>
/// Gerenciamento de pedidos do restaurante.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class PedidoController(IPedidoService pedidoService, ILogger<PedidoController> logger) : ControllerBase
{
    /// <summary>Lista todos os pedidos.</summary>
    /// <response code="200">Lista retornada com sucesso.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PedidoResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAll() => Ok(pedidoService.GetAll());

    /// <summary>Obtém um pedido pelo id.</summary>
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

    /// <summary>Abre um novo pedido para uma mesa.</summary>
    /// <param name="request">Dados do pedido (mesa, usuário, pratos).</param>
    /// <response code="201">Pedido criado.</response>
    /// <response code="400">Dados inválidos ou regra de negócio violada.</response>
    /// <response code="404">Mesa ou usuário referenciados não existem.</response>
    [HttpPost]
    [ProducesResponseType(typeof(PedidoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Create([FromBody] PedidoRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // CP4: log estruturado de início do fluxo, com propriedades nomeadas + traceId (não string concatenada).
        logger.LogInformation(
            "Abrindo pedido para Mesa {MesaId} e Usuario {UserId}. TraceId: {TraceId}",
            request.MesaId, request.UserId, HttpContext.TraceIdentifier);

        var pedido = pedidoService.Create(request);

        // CP4: log estruturado de sucesso do fluxo, mesma correlação (traceId).
        logger.LogInformation(
            "Pedido {PedidoId} aberto com sucesso (Mesa {MesaId}, Usuario {UserId}). TraceId: {TraceId}",
            pedido.Id, pedido.MesaId, pedido.UserId, HttpContext.TraceIdentifier);

        return CreatedAtAction(nameof(GetById), new { id = pedido.Id }, pedido);
    }

    /// <summary>Remove um pedido pelo id.</summary>
    /// <param name="id">Identificador único do pedido.</param>
    /// <response code="204">Removido com sucesso.</response>
    /// <response code="404">Pedido não encontrado.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id) =>
        pedidoService.Delete(id) ? NoContent() : NotFound();
}