using Contra_Filé.Domain.Entities;
using Contra_Filé.Domain.Exceptions;
using Xunit;

namespace Contra_Filé.Domain.Tests.Entities;

/// <summary>
/// Testes de domínio SEM mock, cobrindo a regra de negócio real da nota (faixa 1-5) e da
/// descrição (12-100 caracteres) da entidade Avaliacao.
/// </summary>
public class AvaliacaoTests
{
    private const string DescricaoValida = "Comida excelente, atendimento rápido.";

    [Fact]
    public void Construtor_DadosValidos_CriaAvaliacaoComPropriedadesCorretas()
    {
        // Arrange
        var avaliacaoId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        const int score = 4;

        // Act
        var avaliacao = new Avaliacao(avaliacaoId, userId, score, DescricaoValida);

        // Assert
        Assert.Equal(avaliacaoId, avaliacao.AvaliacaoId);
        Assert.Equal(userId, avaliacao.UserId);
        Assert.Equal(score, avaliacao.Score);
        Assert.Equal(DescricaoValida, avaliacao.Description);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    [InlineData(10)]
    public void UpdateScore_NotaForaDaFaixaDe1a5_LancaDomainException(int notaInvalida)
    {
        // Arrange
        var avaliacao = new Avaliacao(Guid.NewGuid(), Guid.NewGuid(), 3, DescricaoValida);

        // Act
        var act = () => avaliacao.UpdateScore(notaInvalida);

        // Assert
        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void UpdateScore_NotaValida_AtualizaScore()
    {
        // Arrange
        var avaliacao = new Avaliacao(Guid.NewGuid(), Guid.NewGuid(), 3, DescricaoValida);

        // Act
        avaliacao.UpdateScore(5);

        // Assert
        Assert.Equal(5, avaliacao.Score);
    }

    [Theory]
    [InlineData("")]
    [InlineData("curta")]
    [InlineData("   ")]
    public void UpdateDescription_ForaDoTamanhoOuVazia_LancaDomainException(string descricaoInvalida)
    {
        // Arrange
        var avaliacao = new Avaliacao(Guid.NewGuid(), Guid.NewGuid(), 3, DescricaoValida);

        // Act
        var act = () => avaliacao.UpdateDescription(descricaoInvalida);

        // Assert
        Assert.Throws<DomainException>(act);
    }
}