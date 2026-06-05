using TutorInteligente.Domain.Modelos;

namespace TutorInteligente.Application.Interfaces;

public interface IGenerarEstructuraEvaluacion
{
    Evaluacion GenerarEstructura(
    string temaPrincipal,
    List<TemaJerarquico> prerrequisitos,
    int cantidadPreguntas);
}