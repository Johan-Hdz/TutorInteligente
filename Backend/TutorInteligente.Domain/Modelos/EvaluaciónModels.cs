namespace TutorInteligente.Domain.Modelos;

public record Inciso(string Letra, string TextoTema, bool EsCorrecta, string ExpresionMatematica = "", string ValorCalculado = "", string modeloMatematico = "");
public record Pregunta(string Enunciado, string TemaPrincipal, List<Inciso> Incisos);
public record Evaluacion(string TemaPrincipal, List<Pregunta> Preguntas);