using Contra_Filé.Domain.Common;
using Contra_Filé.Domain.Exceptions;
using Contra_Filé.Domain.Helpers;

namespace Contra_Filé.Domain.Entities;

public class User : BaseEntity
{
    private User() { }
    
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string Password { get; private set; }
    
    private string PasswordSalt { get; set; }
    
    private DateOnly BirthDate { get;  set; } 
    
    public List<Avaliacao> Avaliacoes { get; private set; }
    
    public UserConfig Config { get; private set; }

    public User(string name, string email, DateOnly dateBorn, string rawPassword)
    {
        UpdateName(name);
        UpdateEmail(email);
        SetBirthDate(dateBorn);
        ChangePassword(rawPassword);
    }
    
    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new DomainException("Nome não pode ser vazio.");
       
        if (newName.Length < 3 || newName.Length > 50)
            throw new DomainException("O nome deve estar entre 10 e 50 caracteres.");
        
        Name = newName;
    }

    public void UpdateEmail(string newEmail)
    {
        if (string.IsNullOrWhiteSpace(newEmail) || !newEmail.Contains("@"))
            throw new DomainException("E-mail inválido.");
            
        Email = newEmail;
    }

    public void SetBirthDate(DateOnly newDate)
    {
        var age = CalculateAge(newDate);
        
        if (age < 13)
            throw new DomainException("Usuário deve ter pelo menos 13 anos.");

        BirthDate = newDate;
    }

    public void ChangePassword(string newRawPassword)
    {
        if (string.IsNullOrWhiteSpace(newRawPassword) || newRawPassword.Length < 8)
            throw new DomainException("A senha deve ter pelo menos 8 caracteres.");

        // O BCrypt gera e embute o salt automaticamente no hash gerado
        Password = BCrypt.Net.BCrypt.HashPassword(newRawPassword);
    }
    
    public int Age => CalculateAge(BirthDate);

    private static int CalculateAge(DateOnly date)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var age = today.Year - date.Year;
        if (date > today.AddYears(-age)) age--;
        return age;
    } 
}