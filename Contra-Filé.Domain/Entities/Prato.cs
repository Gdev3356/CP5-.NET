using Contra_Filé.Domain.Common;
using Contra_Filé.Domain.Exceptions;

namespace Contra_Filé.Domain.Entities;

public class Prato : BaseEntity
{
    private Prato() { }
    
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    
    public int TimePrepareMinutes { get; set; }
    
    public string Description { get; private set; }
    
    public Prato(string name, decimal price, int timePrepareMinutes, string description)
    {
        UpdateName(name);
        UpdatePrice(price);
        UpdateTime(timePrepareMinutes);
        UpdateDescription(description);
    }
    
    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) 
            throw new DomainException("O nome não pode ser vazio.");
        
        if (name.Length < 3 || name.Length > 20)
            throw new DomainException("O nome deve estar entre 3 e 20 caracteres.");

        Name = name;
    }
   
    public void UpdatePrice(decimal price)
    {
        if (price is <= 0) throw new DomainException("O prato não deve ser gratuito.");
        Price = price;
    }
    
    public void UpdateTime(int timePrepareMinutes)
    {
        if (timePrepareMinutes is <= 1 or > 60) throw new DomainException("O valor deve estar entre 1 minuto e 1 hora");
        TimePrepareMinutes = timePrepareMinutes;
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