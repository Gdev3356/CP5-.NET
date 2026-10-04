using Contra_Filé.Application.DTO;
using Xunit;

namespace Contra_Filé.Application.Tests.DTO;

/// <summary>
/// (item recomendado): cobre a regra de page/pageSize inválido e o intervalo válido.
/// Sem mock, sem subir API nem banco, só a regra do próprio PaginationQuery.
/// </summary>
public class PaginationQueryTests
{
    [Theory]
    [InlineData(0, 20)]   // page < 1
    [InlineData(-1, 20)]  // page negativo
    [InlineData(1, 0)]    // pageSize < 1
    [InlineData(1, 101)]  // pageSize > teto (100)
    [InlineData(1, -5)]   // pageSize negativo
    public void TryGetError_PageOuPageSizeForaDoIntervalo_RetornaErro(int page, int pageSize)
    {
        // Arrange
        var query = new PaginationQuery { Page = page, PageSize = pageSize };

        // Act
        var temErro = query.TryGetError(out var mensagem);

        // Assert
        Assert.True(temErro);
        Assert.False(string.IsNullOrWhiteSpace(mensagem));
    }

    [Fact]
    public void TryGetError_PageEPageSizeValidos_NaoRetornaErro()
    {
        // Arrange
        var query = new PaginationQuery { Page = 1, PageSize = 20 };

        // Act
        var temErro = query.TryGetError(out var mensagem);

        // Assert
        Assert.False(temErro);
        Assert.Null(mensagem);
    }

    [Fact]
    public void TryGetError_PageSizeNoTetoMaximo_NaoRetornaErro()
    {
        // Arrange
        var query = new PaginationQuery { Page = 1, PageSize = PaginationQuery.MaxPageSize };

        // Act
        var temErro = query.TryGetError(out var mensagem);

        // Assert
        Assert.False(temErro);
    }
}