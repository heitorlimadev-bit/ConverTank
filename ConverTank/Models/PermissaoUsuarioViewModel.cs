using ConverTank.Models;

public class PermissaoUsuarioVM
{
    public int UsuarioId { get; set; }

    public string NomeUsuario { get; set; }

    public bool Administrador { get; set; }

    public List<Posto> TodosPostos { get; set; } = new();

    public List<int> PostosPermitidos { get; set; } = new();
}