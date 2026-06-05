using TutorInteligente.Application.Interfaces;
using TutorInteligente.Domain.Modelos;

public class GenerarEstructuraEvaluacion : IGenerarEstructuraEvaluacion
{
    public Evaluacion GenerarEstructura(
    string temaPrincipal,
    List<TemaJerarquico> prerrequisitos,
    int cantidadPreguntas)
    {
        if (cantidadPreguntas <= 0)
            throw new ArgumentException("La cantidad de preguntas debe ser mayor a cero.");

        if (prerrequisitos == null || prerrequisitos.Count == 0)
            throw new ArgumentException("Debe haber al menos un prerrequisito extraído del grafo.");

        int totalDistractoresNecesarios = cantidadPreguntas * 3;

        // FASE 1: Construcción y permutación de la piscina global de distractores
        var piscinaDistractores = ConstruirPiscinaDistractores(prerrequisitos, totalDistractoresNecesarios);

        // FASE 2: Ensamblaje y conmutación local por pregunta
        var preguntas = new List<Pregunta>(cantidadPreguntas);
        int distractorIndex = 0;

        for (int i = 0; i < cantidadPreguntas; i++)
        {
            var distractor1 = piscinaDistractores[distractorIndex++];
            var distractor2 = piscinaDistractores[distractorIndex++];
            var distractor3 = piscinaDistractores[distractorIndex++];
            // Extraer el tema principal y 3 distractores de la piscina global
            var opcionesPregunta = new List<Inciso>
{
    new Inciso("", temaPrincipal, true),

    new Inciso(
        "",
        distractor1.Nombre,
        false,
        modeloMatematico: distractor1.modeloMatematico),

    new Inciso(
        "",
        distractor2.Nombre,
        false,
        modeloMatematico: distractor2.modeloMatematico),

    new Inciso(
        "",
        distractor3.Nombre,
        false,
        modeloMatematico: distractor3.modeloMatematico)
};

            // Permutación Local: Mezclar las 4 opciones de esta pregunta específica
            var opcionesArray = opcionesPregunta.ToArray();
            Random.Shared.Shuffle(opcionesArray);

            // Asignar el orden final (A, B, C, D) aprovechando la inmutabilidad de los records
            var incisosFinales = new List<Inciso>(4);
            char[] letras = { 'A', 'B', 'C', 'D' };

            for (int j = 0; j < 4; j++)
            {
                // 'with' crea una copia del record modificando solo la propiedad indicada
                incisosFinales.Add(opcionesArray[j] with { Letra = letras[j].ToString() });
            }

            preguntas.Add(new Pregunta("", temaPrincipal, incisosFinales));
        }

        return new Evaluacion(temaPrincipal, preguntas);
    }

    private TemaJerarquico[] ConstruirPiscinaDistractores(
    List<TemaJerarquico> prerrequisitos,
    int totalNecesarios)
    {
        var piscina = new List<TemaJerarquico>(totalNecesarios);

        piscina.AddRange(prerrequisitos);

        int faltantes = totalNecesarios - piscina.Count;

        for (int i = 0; i < faltantes; i++)
        {
            int randomIndex = Random.Shared.Next(prerrequisitos.Count);
            piscina.Add(prerrequisitos[randomIndex]);
        }

        var piscinaFinal = piscina.Take(totalNecesarios).ToArray();

        Random.Shared.Shuffle(piscinaFinal);

        return piscinaFinal;
    }
}