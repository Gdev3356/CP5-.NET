using Contra_Filé.Domain.Common;
using Contra_Filé.Domain.Exceptions;

namespace Contra_Filé.Domain.Entities;

public class Mesa : BaseEntity
{
    public int Number { get; private set; }
    
    public bool IsOccupied { get; private set; }
    
    public List<Pedido> Pedidos { get; private set; } = [];

    private Mesa() { }

    public Mesa(int number, bool isOccupied)
    {
        UpdateNumber(number); 
        IsOccupied = isOccupied;
    }

    public void UpdateNumber(int number)
    {
        if (number < -1)
            throw new DomainException("Não se pode ter mesa negativa!");
        Number = number;
    }
}