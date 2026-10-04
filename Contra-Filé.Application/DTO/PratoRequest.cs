using Contra_Filé.Domain.Entities;
using System.ComponentModel.DataAnnotations;
 
namespace Contra_Filé.Application.DTO;
 
/// <summary>Dados para criação de um prato.</summary>
public record PratoRequest(
    [Required][StringLength(20, MinimumLength = 3)]
    string Name,
 
    [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser positivo.")]
    decimal Price,
 
    [Range(2, 60, ErrorMessage = "Tempo de preparo deve ser entre 2 e 60 minutos.")]
    int TimePrepareMinutes,
 
    [Required][StringLength(100, MinimumLength = 12)]
    string Description)
{
    /// <summary>Constrói a entidade <see cref="Prato"/>.</summary>
    public Prato ToDomain() => new Prato(Name, Price, TimePrepareMinutes, Description);
}