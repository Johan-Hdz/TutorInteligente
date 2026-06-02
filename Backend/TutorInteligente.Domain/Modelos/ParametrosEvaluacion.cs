namespace TutorInteligente.Domain.Modelos;

public record ParametrosEvaluacion(

    string TemaPrincipal,
    int cantidadPreguntas,
    bool EsValida,
    string MensajeError = ""
);