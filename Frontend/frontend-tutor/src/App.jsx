import { useState, useRef, useEffect } from "react";
import axios from "axios";
import ReactMarkdown from "react-markdown";
import remarkMath from "remark-math";
import rehypeKatex from "rehype-katex";
import "katex/dist/katex.min.css";

// ==========================================
// CONFIGURACIÓN DE ESTILOS (UI/UX)
// ==========================================
const colors = {
  primary: "#6f1d46", // Burdeos (Académico)
  secondary: "#008f39", // Verde (Acción/Educación)
  bgBody: "#f0f2f5",
  bgChat: "#ffffff",
  textMain: "#333333",
  textLight: "#757575",
  bubbleUser: "#e1f5fe", // Azul muy claro para el usuario
  bubbleAi: "#f1f0f0", // Gris claro para la IA
};

const styles = {
  appContainer: {
    fontFamily: "'Segoe UI', Roboto, Helvetica, Arial, sans-serif",
    backgroundColor: colors.bgBody,
    minHeight: "100vh",
    padding: "20px",
    color: colors.textMain,
    display: "flex",
    flexDirection: "column",
    alignItems: "center",
  },
  header: {
    textAlign: "center",
    marginBottom: "30px",
    borderBottom: `3px solid ${colors.primary}`,
    paddingBottom: "15px",
    width: "100%",
    maxWidth: "1000px",
  },
  title: {
    color: colors.primary,
    fontSize: "2.2rem",
    margin: "0 0 5px 0",
    fontWeight: "800",
  },
  subtitle: {
    color: colors.textLight,
    fontSize: "1.1rem",
    margin: 0,
  },
  chatWindow: {
    backgroundColor: colors.bgChat,
    width: "100%",
    maxWidth: "1000px",
    height: "70vh",
    borderRadius: "12px",
    boxShadow: "0 4px 15px rgba(0,0,0,0.1)",
    display: "flex",
    flexDirection: "column",
    overflow: "hidden",
  },
  chatMessages: {
    flex: 1,
    padding: "25px",
    overflowY: "auto",
    display: "flex",
    flexDirection: "column",
    gap: "15px",
  },
  // Burbujas de Chat
  bubble: {
    padding: "12px 18px",
    borderRadius: "18px",
    maxWidth: "75%",
    lineHeight: "1.5",
    fontSize: "15px",
    position: "relative",
  },
  bubbleUser: {
    alignSelf: "flex-end",
    backgroundColor: colors.secondary,
    color: "white",
    borderBottomRightRadius: "4px",
  },
  bubbleAi: {
    alignSelf: "flex-start",
    backgroundColor: colors.bubbleAi,
    color: colors.textMain,
    borderTopLeftRadius: "4px",
    border: "1px solid #e0e0e0",
  },
  // Input Area
  inputArea: {
    padding: "15px 25px",
    borderTop: "1px solid #eee",
    backgroundColor: "#fff",
    display: "flex",
    gap: "10px",
    alignItems: "center",
  },
  textarea: {
    flex: 1,
    padding: "12px",
    borderRadius: "24px",
    border: "1px solid #ccc",
    fontSize: "15px",
    resize: "none",
    outline: "none",
    transition: "border-color 0.2s",
    fontFamily: "inherit",
  },
  sendButton: {
    backgroundColor: colors.secondary,
    color: "white",
    border: "none",
    padding: "12px 24px",
    borderRadius: "24px",
    fontSize: "15px",
    fontWeight: "bold",
    cursor: "pointer",
    transition: "background-color 0.2s, transform 0.1s",
    display: "flex",
    alignItems: "center",
    gap: "5px",
  },
  sendButtonDisabled: {
    backgroundColor: "#a5d6a7",
    cursor: "not-allowed",
  },
  // Resultados y Contexto
  resultsContainer: {
    marginTop: "20px",
    display: "flex",
    flexDirection: "column",
    gap: "20px",
    width: "100%",
  },
  contextCard: {
    backgroundColor: "#fff",
    padding: "20px",
    borderRadius: "8px",
    borderLeft: `5px solid ${colors.primary}`,
    boxShadow: "0 2px 5px rgba(0,0,0,0.05)",
  },
  evaluacionCard: {
    backgroundColor: "#fff",
    padding: "30px",
    borderRadius: "8px",
    boxShadow: "0 2px 10px rgba(0,0,0,0.08)",
    lineHeight: "1.8",
    textAlign: "left",
  },
  tag: {
    display: "inline-block",
    padding: "4px 8px",
    borderRadius: "4px",
    fontSize: "12px",
    fontWeight: "bold",
    marginLeft: "10px",
  },
  tagCentral: {
    backgroundColor: "#e1f5fe",
    color: "#0277bd",
  },
  tagPrereq: {
    backgroundColor: "#fff3e0",
    color: "#ef6c00",
  },
  errorBox: {
    backgroundColor: "#ffebee",
    color: "#c62828",
    padding: "15px",
    borderRadius: "8px",
    border: "1px solid #ef9a9a",
    margin: "10px 0",
    fontWeight: "bold",
  },
  loadingDots: {
    display: "flex",
    gap: "5px",
    justifyContent: "center",
    alignItems: "center",
    color: colors.textLight,
    fontStyle: "italic",
    padding: "10px",
  },
};

const LoadingChat = () => (
  <div style={{ ...styles.bubble, ...styles.bubbleAi }}>
    <div style={styles.loadingDots}>
      <span className="dot">.</span>
      <span className="dot">.</span>
      <span className="dot">.</span>
      Analizando grafo y generando evaluación curricular
    </div>
    <style>{`
            @keyframes dotFlashing { 0% { opacity: 0.2; } 50% { opacity: 1; } 100% { opacity: 0.2; } }
            .dot { animation: dotFlashing 1s infinite linear; }
            .dot:nth-child(2) { animation-delay: 0.2s; }
            .dot:nth-child(3) { animation-delay: 0.4s; }
        `}</style>
  </div>
);

// ==========================================
// COMPONENTE PRINCIPAL
// ==========================================
function App() {
  const [consulta, setConsulta] = useState("");
  const [cargando, setCargando] = useState(false);
  const [, setError] = useState("");

  const [historial, setHistorial] = useState([
    {
      sender: "ai",
      type: "text",
      content:
        "¡Hola, docente! 👋 Soy tu asistente pedagógico. Describe qué tema y grado necesitas evaluar (primaria 1º a 4º) y diseñaré una propuesta basada en el currículum.",
    },
  ]);

  const messagesEndRef = useRef(null);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  };

  useEffect(scrollToBottom, [historial, cargando]);

  const prepararMatematicas = (texto) => {
    if (!texto) return "";
    return texto
      .replace(/\\\(/g, "$")
      .replace(/\\\)/g, "$")
      .replace(/\\\[/g, "$$")
      .replace(/\\\]/g, "$$");
  };

  const generarEvaluacion = async (e) => {
    e.preventDefault();
    if (!consulta.trim()) return;

    const nuevaConsulta = consulta;
    setError("");
    setConsulta("");

    setHistorial((prev) => [
      ...prev,
      { sender: "user", type: "text", content: nuevaConsulta },
    ]);
    setCargando(true);

    try {
      const response = await axios.post(
        "https://localhost:7237/api/recuperacion/orquestar",
        { consulta: nuevaConsulta },
      );

      if (response.data && response.data.esExitoso) {
        setHistorial((prev) => [
          ...prev,
          {
            sender: "ai",
            type: "result",
            data: response.data,
          },
        ]);
      } else {
        throw new Error("El servidor no devolvió un resultado exitoso.");
      }
    } catch (err) {
      const msgError =
        err.response?.data?.error ||
        "Error al conectar con el servidor educativo.";

      setError(msgError);

      setHistorial((prev) => [
        ...prev,
        {
          sender: "ai",
          type: "text",
          content: `❌ Lo siento, ocurrió un error: ${msgError}`,
        },
      ]);
    } finally {
      setCargando(false);
    }
  };

  const handleKeyDown = (e) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      generarEvaluacion(e);
    }
  };

  // ------------------------------------------------------------------
  // NUEVA FUNCIÓN: Procesa el JSON y lo convierte en una UI amigable
  // ------------------------------------------------------------------
  const renderizarEvaluacion = (evaluacionData) => {
    let datos = evaluacionData;

   // 1. Intentamos convertir el string a objeto JSON si viene como texto
if (typeof evaluacionData === 'string') {
  try {
    datos = JSON.parse(evaluacionData);
  } catch { // <-- Simplemente quita la (e) aquí
    // Si falla (porque no es JSON sino texto normal/markdown), usamos el renderizado anterior
    return (
      <ReactMarkdown
        remarkPlugins={[remarkMath]}
        rehypePlugins={[rehypeKatex]}
      >
        {prepararMatematicas(evaluacionData)}
      </ReactMarkdown>
    );
  }
}

    // 2. Si logramos obtener el objeto y tiene la estructura "Preguntas"
    if (datos && datos.Preguntas) {
      return (
        <div style={{ display: "flex", flexDirection: "column", gap: "25px" }}>
          {datos.TemaPrincipal && (
            <div
              style={{
                backgroundColor: "#e3f2fd",
                padding: "10px 15px",
                borderRadius: "6px",
                borderLeft: "4px solid #1976d2",
              }}
            >
              <strong>Tema de la evaluación:</strong> {datos.TemaPrincipal}
            </div>
          )}

          {datos.Preguntas.map((preg, pIndex) => (
            <div
              key={pIndex}
              style={{
                backgroundColor: "#f9fafb",
                padding: "20px",
                borderRadius: "10px",
                border: "1px solid #e5e7eb",
              }}
            >
              <p
                style={{
                  fontWeight: "600",
                  fontSize: "16px",
                  margin: "0 0 15px 0",
                }}
              >
                {pIndex + 1}. {preg.Enunciado}
              </p>

              <div
                style={{
                  display: "flex",
                  flexDirection: "column",
                  gap: "10px",
                  paddingLeft: "10px",
                }}
              >
                {preg.Incisos.map((inciso, iIndex) => (
                  <div
                    key={iIndex}
                    style={{
                      padding: "12px",
                      borderRadius: "8px",
                      backgroundColor: inciso.EsCorrecta
                        ? "#f0fdf4"
                        : "#ffffff",
                      border: `1px solid ${inciso.EsCorrecta ? "#86efac" : "#d1d5db"}`,
                      display: "flex",
                      flexDirection: "column",
                    }}
                  >
                    <div style={{ display: "flex", alignItems: "center" }}>
                      <strong
                        style={{ color: colors.primary, marginRight: "10px" }}
                      >
                        {inciso.Letra})
                      </strong>
                      <span style={{ fontSize: "15px" }}>
                        {inciso.ValorCalculado}
                      </span>

                      {inciso.EsCorrecta && (
                        <span
                          style={{
                            color: "#16a34a",
                            fontWeight: "bold",
                            marginLeft: "auto",
                            fontSize: "13px",
                            display: "flex",
                            alignItems: "center",
                            gap: "4px",
                          }}
                        >
                          ✓ Opción Correcta
                        </span>
                      )}
                    </div>
                    <span
                      style={{
                        fontSize: "12px",
                        color: colors.textLight,
                        marginTop: "6px",
                        borderTop: "1px dashed #e5e7eb",
                        paddingTop: "6px",
                      }}
                    >
                      <em>Evalúa: {inciso.TextoTema}</em>
                    </span>
                  </div>
                ))}
              </div>
            </div>
          ))}
        </div>
      );
    }

    // 3. Fallback: Si es un objeto JSON pero no tiene la estructura que esperamos, lo mostramos limpio
    return (
      <pre
        style={{
          whiteSpace: "pre-wrap",
          background: "#f4f4f4",
          padding: "15px",
          borderRadius: "8px",
        }}
      >
        {JSON.stringify(datos, null, 2)}
      </pre>
    );
  };

  const renderMessageContent = (msg) => {
    if (msg.type === "text") {
      return msg.content;
    }

    if (msg.type === "result") {
      const { parametros, subgrafo, evaluacionGenerada } = msg.data;
      return (
        <div style={styles.resultsContainer}>
          <div style={styles.contextCard}>
            <h3 style={{ color: colors.primary, marginTop: 0 }}>
              📊 Contexto Curricular Extraído
            </h3>
            <p style={{ fontSize: "16px" }}>
              <strong>Tema Principal:</strong> {parametros.temaPrincipal}
              <span style={{ color: colors.textLight, marginLeft: "10px" }}>
                (Grado: {parametros.gradoEscolar}º)
              </span>
            </p>
            <h4 style={{ marginBottom: "10px", color: colors.textMain }}>
              Conceptos Relacionados en Grafo:
            </h4>
            <div style={{ display: "flex", flexWrap: "wrap", gap: "10px" }}>
              {subgrafo.map((nodo, index) => (
                <span
                  key={index}
                  style={{
                    ...styles.tag,
                    ...(nodo.esPrerrequisito
                      ? styles.tagPrereq
                      : styles.tagCentral),
                  }}
                >
                  {nodo.nombre} {nodo.esPrerrequisito ? "🔑" : "🎯"}
                </span>
              ))}
            </div>
          </div>

          <div style={styles.evaluacionCard} className="prose">
            <h3
              style={{
                color: colors.primary,
                marginTop: 0,
                borderBottom: "1px solid #eee",
                paddingBottom: "10px",
                marginBottom: "20px",
              }}
            >
              📝 Propuesta de Evaluación
            </h3>

            {/* AQUÍ UTILIZAMOS LA NUEVA FUNCIÓN */}
            {renderizarEvaluacion(evaluacionGenerada)}

            <button
              style={{
                ...styles.sendButton,
                marginTop: "30px",
                backgroundColor: colors.primary,
                width: "100%",
                justifyContent: "center",
              }}
            >
              💾 Descargar / Imprimir PDF
            </button>
          </div>
        </div>
      );
    }
  };

  return (
    <div style={styles.appContainer}>
      <header style={styles.header}>
        <h1 style={styles.title}>Generador de Evaluaciones Diagnósticas</h1>
        <p style={styles.subtitle}>
          Inteligencia Artificial para Educación Primaria (1º a 4º grado)
        </p>
      </header>

      <div style={styles.chatWindow}>
        <div style={styles.chatMessages}>
          {historial.map((msg, index) => (
            <div
              key={index}
              style={{
                ...styles.bubble,
                ...(msg.sender === "user"
                  ? styles.bubbleUser
                  : styles.bubbleAi),
                ...(msg.type === "result" ? { maxWidth: "95%" } : {}),
              }}
            >
              {renderMessageContent(msg)}
            </div>
          ))}

          {cargando && <LoadingChat />}
          <div ref={messagesEndRef} />
        </div>

        <form onSubmit={generarEvaluacion} style={styles.inputArea}>
          <textarea
            value={consulta}
            onChange={(e) => setConsulta(e.target.value)}
            onKeyDown={handleKeyDown}
            placeholder="Ej: Necesito una evaluación de fracciones para 4º grado..."
            rows={
              consulta.split("\n").length > 3
                ? 3
                : consulta.split("\n").length || 1
            }
            style={styles.textarea}
            required
            disabled={cargando}
          />
          <button
            type="submit"
            disabled={cargando || !consulta.trim()}
            style={{
              ...styles.sendButton,
              ...(cargando || !consulta.trim()
                ? styles.sendButtonDisabled
                : {}),
            }}
          >
            {cargando ? "Procesando..." : "Generar →"}
          </button>
        </form>
      </div>

      <p
        style={{ color: colors.textLight, fontSize: "12px", marginTop: "15px" }}
      >
        Desarrollado para el cuerpo docente. Basado en tecnología GraphRAG y
        Neo4j.
      </p>
    </div>
  );
}

export default App;
