using Contra_Filé.Application.DTO;
using Contra_Filé.Application.Repositories;
using Contra_Filé.Application.Services;
using Contra_Filé.Domain.Entities;
using Moq;
using Xunit;

namespace Contra_Filé.Application.Tests.Services;

/// <summary>
/// Testes de aplicação com mock das interfaces de repositório (Moq), cobrindo o cenário de
/// "dependência ausente" (Mesa/Usuário inexistentes) exigido pelo enunciado.
/// </summary>
public class PedidoServiceTests
{
    private readonly Mock<IRepository<Pedido>> _pedidoRepoMock = new();
    private readonly Mock<IRepository<Mesa>> _mesaRepoMock = new();
    private readonly Mock<IRepository<User>> _userRepoMock = new();

    private PedidoService CriarService() =>
        new(_pedidoRepoMock.Object, _mesaRepoMock.Object, _userRepoMock.Object);

    [Fact]
    public void Create_MesaInexistente_LancaKeyNotFoundExceptionENaoPersiste()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _mesaRepoMock.Setup(r => r.ExistsById(mesaId)).Returns(false);
        var request = new PedidoRequest(mesaId, userId, null);
        var service = CriarService();

        // Act
        var act = () => service.Create(request);

        // Assert
        Assert.Throws<KeyNotFoundException>(act);
        _pedidoRepoMock.Verify(r => r.Add(It.IsAny<Pedido>()), Times.Never);
    }

    [Fact]
    public void Create_UsuarioInexistente_LancaKeyNotFoundExceptionENaoPersiste()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _mesaRepoMock.Setup(r => r.ExistsById(mesaId)).Returns(true);
        _userRepoMock.Setup(r => r.ExistsById(userId)).Returns(false);
        var request = new PedidoRequest(mesaId, userId, null);
        var service = CriarService();

        // Act
        var act = () => service.Create(request);

        // Assert
        Assert.Throws<KeyNotFoundException>(act);
        _pedidoRepoMock.Verify(r => r.Add(It.IsAny<Pedido>()), Times.Never);
    }

    [Fact]
    public void Create_MesaEUsuarioExistentes_PersisteUmaVezERetornaResponse()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _mesaRepoMock.Setup(r => r.ExistsById(mesaId)).Returns(true);
        _userRepoMock.Setup(r => r.ExistsById(userId)).Returns(true);
        _pedidoRepoMock.Setup(r => r.Add(It.IsAny<Pedido>())).Returns((Pedido p) => p);
        var request = new PedidoRequest(mesaId, userId, null);
        var service = CriarService();

        // Act
        var response = service.Create(request);

        // Assert
        Assert.Equal(mesaId, response.MesaId);
        Assert.Equal(userId, response.UserId);
        _pedidoRepoMock.Verify(r => r.Add(It.IsAny<Pedido>()), Times.Once);
    }
}