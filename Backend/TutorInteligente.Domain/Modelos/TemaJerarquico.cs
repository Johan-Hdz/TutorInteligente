namespace TutorInteligente.Domain.Modelos;

public record TemaJerarquico(
    string Nombre,
    string modeloMatematico,
    bool EsPrerrequisito
);
