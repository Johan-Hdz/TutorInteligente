using TutorInteligente.Application.Interfaces;
using TutorInteligente.Domain.Modelos;

public class GenerarEstructuraEvaluacion : IGenerarEstructuraEvaluacion
{
    public Evaluacion GenerarEstructura(string temaPrincipal, List<string> prerrequisitos, int cantidadPreguntas)
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
            // Extraer el tema principal y 3 distractores de la piscina global
            var opcionesPregunta = new List<Inciso>
            {
                new Inciso("", temaPrincipal, true), // La respuesta correcta
                new Inciso("", piscinaDistractores[distractorIndex++], false),
                new Inciso("", piscinaDistractores[distractorIndex++], false),
                new Inciso("", piscinaDistractores[distractorIndex++], false)
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

    private string[] ConstruirPiscinaDistractores(List<string> prerrequisitos, int totalNecesarios)
    {
        var piscina = new List<string>(totalNecesarios);

        // 1. Asignación Garantizada: Todos los prerrequisitos deben aparecer al menos una vez
        piscina.AddRange(prerrequisitos);

        // 2. Relleno (Padding): Completar los espacios faltantes eligiendo prerrequisitos al azar
        int faltantes = totalNecesarios - piscina.Count;
        for (int i = 0; i < faltantes; i++)
        {
            int randomIndex = Random.Shared.Next(prerrequisitos.Count);
            piscina.Add(prerrequisitos[randomIndex]);
        }

        // Validación de seguridad: si k (prerrequisitos) > 3n (espacios), truncamos para no desbordar
        var piscinaFinal = piscina.Take(totalNecesarios).ToArray();

        // 3. Permutación Global (Fisher-Yates nativo)
        Random.Shared.Shuffle(piscinaFinal);

        return piscinaFinal;
    }
}