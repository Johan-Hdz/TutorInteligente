import { useState } from "react";
import axios from "axios";
import ReactMarkdown from "react-markdown";
import remarkMath from "remark-math";
import rehypeKatex from "rehype-katex";
import "katex/dist/katex.min.css"; // Importante para que las fracciones se vean bien

function App() {
  // Función para formatear los delimitadores de LaTeX para remark-math
  const prepararMatematicas = (texto) => {
    if (!texto) return "";
    return texto
      .replace(/\\\(/g, "$") // Convierte \( en $
      .replace(/\\\)/g, "$") // Convierte \) en $
      .replace(/\\\[/g, "$$") // Convierte \[ en $$
      .replace(/\\\]/g, "$$"); // Convierte \] en $$
  };
  const [consulta, setConsulta] = useState("");
  const [cargando, setCargando] = useState(false);
  const [resultado, setResultado] = useState(null);
  const [error, setError] = useState("");

  const generarEvaluacion = async (e) => {
    e.preventDefault();
    setCargando(true);
    setError("");
    setResultado(null);

    try {
      // Reemplaza el puerto 7237 por el que estés usando en tu API
      const response = await axios.post(
        "https://localhost:7237/api/recuperacion/orquestar",
        {
          consulta: consulta,
        },
      );
      setResultado(response.data);
    } catch (err) {
      setError(
        err.response?.data?.Error || "Error al conectar con el servidor.",
      );
    } finally {
      setCargando(false);
    }
  };

  return (
    <div
      style={{
        maxWidth: "800px",
        margin: "0 auto",
        padding: "20px",
        fontFamily: "sans-serif",
      }}
    >
      <h1>Generador de Evaluaciones Diagnósticas</h1>
      <p>Diseñado para educación primaria (1ro a 4to grado).</p>

      {/* Formulario exclusivo para el docente */}
      <form onSubmit={generarEvaluacion} style={{ marginBottom: "20px" }}>
        <textarea
          value={consulta}
          onChange={(e) => setConsulta(e.target.value)}
          placeholder="Ej: Necesito generar una evaluación diagnóstica sobre fracciones para mis alumnos de cuarto grado."
          rows={4}
          style={{ width: "100%", padding: "10px", fontSize: "16px" }}
          required
        />
        <br />
        <button
          type="submit"
          disabled={cargando}
          style={{
            marginTop: "10px",
            padding: "10px 20px",
            fontSize: "16px",
            cursor: "pointer",
          }}
        >
          {cargando ? "Analizando grafo y generando..." : "Generar Evaluación"}
        </button>
      </form>

      {/* Manejo de Errores */}
      {error && (
        <div
          style={{
            color: "red",
            padding: "10px",
            border: "1px solid red",
            backgroundColor: "#fee",
          }}
        >
          <strong>Error:</strong> {error}
        </div>
      )}

      {/* Visualización del Resultado */}
      {resultado && resultado.esExitoso && (
        <div
          style={{
            marginTop: "30px",
            padding: "20px",
            border: "1px solid #ccc",
            borderRadius: "8px",
          }}
        >
          <h2>Resultados de GraphRAG</h2>

          <div
            style={{
              backgroundColor: "#f9f9f9",
              padding: "10px",
              marginBottom: "20px",
            }}
          >
            <h3>Contexto Extraído (Neo4j)</h3>
            <p>
              <strong>Tema:</strong> {resultado.parametros.temaPrincipal} (Grado{" "}
              {resultado.parametros.gradoEscolar})
            </p>
            <ul>
              {resultado.subgrafo.map((nodo, index) => (
                <li key={index}>
                  {nodo.nombre}{" "}
                  {nodo.esPrerrequisito ? "(Prerrequisito)" : "(Tema Central)"}
                </li>
              ))}
            </ul>
          </div>

          <div>
            <h3>Evaluación Generada</h3>
            <ReactMarkdown
              remarkPlugins={[remarkMath]}
              rehypePlugins={[rehypeKatex]}
            >
              {prepararMatematicas(resultado.evaluacionGenerada)}
            </ReactMarkdown>
          </div>x
        </div>
      )}
    </div>
  );
}

export default App;
