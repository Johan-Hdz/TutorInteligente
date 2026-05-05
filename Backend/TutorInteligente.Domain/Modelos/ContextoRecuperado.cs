namespace TutorInteligente.Domain.Modelos;

public record ContextoRecuperado(
    ParametrosEvaluacion Parametros,
    List<TemaJerarquico> Subgrafo,
    bool EsExitoso,
    string Mensaje = "",
    string EvaluacionGenerada = "" // <--- Nuevo campo para el texto final
);
