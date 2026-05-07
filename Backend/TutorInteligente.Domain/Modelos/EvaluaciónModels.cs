using System;
using System.Collections.Generic;
using System.Text;

namespace TutorInteligente.Domain.Modelos;

public record Inciso(string Letra, string TextoTema, bool EsCorrecta, string ValorCalculado = "");
public record Pregunta(string Enunciado, string TemaPrincipal, List<Inciso> Incisos);
public record Evaluacion(string TemaPrincipal, List<Pregunta> Preguntas);