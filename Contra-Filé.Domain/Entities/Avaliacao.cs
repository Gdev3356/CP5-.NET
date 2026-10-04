using Contra_Filé.Domain.Common;
using Contra_Filé.Domain.Exceptions;

namespace Contra_Filé.Domain.Entities;

public class Avaliacao : BaseEntity
{
    private Avaliacao() { }
    
    public Guid AvaliacaoId { get; private set; }
    
    public Guid UserId { get; private set; }
    
    public string Description { get; private set; }
    
    public int Score { get; private set; }

    public Avaliacao(Guid avaliacaoId, Guid userId, int score, string description)
    {
        AvaliacaoId = avaliacaoId;
        UserId = userId;
        UpdateScore(score);
        UpdateDescription(description);
    }

    public void UpdateScore(int score)
    {
        if (score is < 1 or > 5) throw new DomainException("A avaliação deve ter valor entre 1 e 5");
        Score = score;
    }

    public void UpdateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description)) 
            throw new DomainException("A descrição não pode ser vazia.");
        
        if (description.Length < 12 || description.Length > 100)
            throw new DomainException("A descrição deve ter entre 12 e 100 caracteres.");

        Description = description;
    }
}