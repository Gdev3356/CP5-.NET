namespace Contra_Filé.Controllers;

using Asp.Versioning;
using Contra_Filé.Application.DTO;
using Contra_Filé.Application.Services;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Cardápio, gerenciamento de pratos disponíveis.
/// </summary>
/// <remarks>Marcado como neutro de versão (mesmo motivo do MesaController).</remarks>
[Route("api/[controller]")]
[ApiController]
[ApiVersionNeutral]
[Produces("application/json")]
public class PratoController(IPratoService pratoService) : ControllerBase
{
    /// <summary>Lista todos os pratos do cardápio.</summary>
    /// <response code="200">Lista retornada com sucesso.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PratoResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAll() => Ok(pratoService.GetAll());

    /// <summary>Obtém um prato pelo id.</summary>
    /// <param name="id">Identificador único do prato.</param>
    /// <response code="200">Prato encontrado.</response>
    /// <response code="404">Prato não encontrado.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PratoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        var prato = pratoService.GetById(id);
        return prato is null ? NotFound() : Ok(prato);
    }

    /// <summary>
    /// Adiciona um prato ao cardápio.
    /// </summary>
    /// <remarks>
    /// O nome deve ter entre 3 e 20 caracteres.
    /// O preço deve ser maior que zero.
    /// O tempo de preparo deve estar entre 2 e 60 minutos.
    /// </remarks>
    /// <param name="request">Dados do prato.</param>
    /// <response code="201">Prato criado.</response>
    /// <response code="400">Dados inválidos ou regra de negócio violada.</response>
    [HttpPost]
    [ProducesResponseType(typeof(PratoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] PratoRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var prato = pratoService.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = prato.Id }, prato);
    }

    /// <summary>Remove um prato do cardápio.</summary>
    /// <param name="id">Identificador único do prato.</param>
    /// <response code="204">Removido com sucesso.</response>
    /// <response code="404">Prato não encontrado.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id) =>
        pratoService.Delete(id) ? NoContent() : NotFound();
}