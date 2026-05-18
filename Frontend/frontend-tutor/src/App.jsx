import { useState, useRef, useEffect, useCallback } from "react";
import axios from "axios";
import ReactMarkdown from "react-markdown";
import remarkMath from "remark-math";
import rehypeKatex from "rehype-katex";
import "katex/dist/katex.min.css";

// ==========================================
// HOOK PARA TEXTAREA AUTO-AJUSTABLE
// ==========================================
function useAutoResizeTextarea({ minHeight, maxHeight }) {
  const textareaRef = useRef(null);

  const adjustHeight = useCallback(
    (reset) => {
      const textarea = textareaRef.current;
      if (!textarea) return;

      if (reset) {
        textarea.style.height = `${minHeight}px`;
        return;
      }

      textarea.style.height = `${minHeight}px`;
      const newHeight = Math.max(
        minHeight,
        Math.min(textarea.scrollHeight, maxHeight ?? Number.POSITIVE_INFINITY),
      );
      textarea.style.height = `${newHeight}px`;
    },
    [minHeight, maxHeight],
  );

  useEffect(() => {
    const textarea = textareaRef.current;
    if (textarea) {
      textarea.style.height = `${minHeight}px`;
    }
  }, [minHeight]);

  useEffect(() => {
    const handleResize = () => adjustHeight();
    window.addEventListener("resize", handleResize);
    return () => window.removeEventListener("resize", handleResize);
  }, [adjustHeight]);

  return { textareaRef, adjustHeight };
}

// ==========================================
// CONFIGURACIÓN DE ESTILOS (UI/UX)
// ==========================================
const colors = {
  primary: "#6f1d46",
  secondary: "#008f39",
  textMain: "#333333",
  textLight: "#757575",
  bubbleUser: "#f4f4f4",
  bubbleAi: "transparent",
};

const styles = {
  appContainer: {
    fontFamily: "'Segoe UI', Roboto, Helvetica, Arial, sans-serif",
    backgroundColor: "#ffffff",
    position: "absolute",
    top: 0,
    left: 0,
    width: "100vw",
    height: "100vh",
    margin: 0,
    padding: 0,
    overflow: "hidden",
    display: "flex",
    flexDirection: "column",
    alignItems: "center",
  },
  chatWindow: {
    width: "100%",
    maxWidth: "800px",
    height: "100%",
    display: "flex",
    flexDirection: "column",
    border: "none",
    boxShadow: "none",
  },
  chatMessages: {
    flex: 1,
    padding: "20px",
    overflowY: "auto",
    display: "flex",
    flexDirection: "column",
    gap: "25px",
  },
  bubble: {
    padding: "12px 18px",
    borderRadius: "18px",
    maxWidth: "85%",
    lineHeight: "1.5",
    fontSize: "15px",
    position: "relative",
    wordBreak: "break-word", // FIX DEL BUG: Fuerza el salto de línea en textos largos sin espacios
    textAlign: "left", // <-- AÑADE ESTA LÍNEA
  },
  bubbleUser: {
    alignSelf: "flex-end",
    backgroundColor: colors.bubbleUser,
    color: colors.textMain,
    borderBottomRightRadius: "4px",
  },
  bubbleAi: {
    alignSelf: "flex-start",
    backgroundColor: colors.bubbleAi,
    color: colors.textMain,
    border: "none",
    padding: "12px 0",
  },
  resultsContainer: {
    marginTop: "10px",
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
  loadingDots: {
    display: "flex",
    gap: "5px",
    justifyContent: "flex-start",
    alignItems: "center",
    color: colors.textLight,
    fontStyle: "italic",
    padding: "10px 0",
  },
};

const LoadingChat = () => (
  <div style={{ ...styles.bubble, ...styles.bubbleAi }}>
    <div style={styles.loadingDots}>
      <span className="dot">.</span>
      <span className="dot">.</span>
      <span className="dot">.</span>
      Analizando currículum y generando evaluación
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

  const { textareaRef, adjustHeight } = useAutoResizeTextarea({
    minHeight: 44, // Reducido para que inicie como una sola línea estética
    maxHeight: 200,
  });

  const [historial, setHistorial] = useState([]);
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
    if (e) e.preventDefault();
    if (!consulta.trim() || cargando) return;

    const nuevaConsulta = consulta;
    setError("");
    setConsulta("");
    adjustHeight(true);

    setHistorial((prev) => [
      ...prev,
      { sender: "user", type: "text", content: nuevaConsulta },
    ]);
    setCargando(true);

    try {
      const response = await axios.post(
        "https://localhost:7237/api/interpretacion/orquestar",
        { consulta: nuevaConsulta },
      );

      if (response.data) {
        setHistorial((prev) => [
          ...prev,
          {
            sender: "ai",
            type: "result",
            data: response.data,
          },
        ]);
      } else {
        throw new Error("El servidor no devolvió datos.");
      }
    } catch (err) {
      // Agregamos la búsqueda de la propiedad 'error' u 'Error' que viene del NotFound
      // y mantenemos 'mensajeError' para los BadRequest de los parámetros.
      const msgError =
        err.response?.data?.error ||
        err.response?.data?.Error ||
        err.response?.data?.mensajeError ||
        err.response?.data?.MensajeError ||
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
      generarEvaluacion();
    }
  };

  const renderizarEvaluacion = (evaluacionData) => {
    let datos = evaluacionData;

    if (typeof evaluacionData === "string") {
      try {
        datos = JSON.parse(evaluacionData);
      } catch {
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
                      border: `1px solid ${
                        inciso.EsCorrecta ? "#86efac" : "#d1d5db"
                      }`,
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
      const { temaPrincipal, preguntas } = msg.data;
      return (
        <div style={styles.resultsContainer}>
          <div style={styles.contextCard}>
            <h3 style={{ color: colors.primary, marginTop: 0 }}>
              📊 Contexto Curricular Extraído
            </h3>
            <p style={{ fontSize: "16px" }}>
              <strong>Tema Principal:</strong> {temaPrincipal}
              <span style={{ color: colors.textLight, marginLeft: "10px" }}>
                (Grado: {preguntas}º)
              </span>
            </p>
            <h4 style={{ marginBottom: "10px", color: colors.textMain }}>
              Conceptos Relacionados en Grafo:
            </h4>
            <div style={{ display: "flex", flexWrap: "wrap", gap: "10px" }}>
              {(preguntas || []).map((nodo, index) => (
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

            {renderizarEvaluacion(msg.data)}

            <button
              style={{
                backgroundColor: colors.primary,
                color: "white",
                border: "none",
                padding: "12px 24px",
                borderRadius: "24px",
                fontSize: "15px",
                fontWeight: "bold",
                cursor: "pointer",
                marginTop: "30px",
                width: "100%",
                display: "flex",
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
      <style>{`
        .hide-scrollbar::-webkit-scrollbar {
          display: none;
        }
        .hide-scrollbar {
          -ms-overflow-style: none;
          scrollbar-width: none;
        }
      `}</style>

      <div style={styles.chatWindow}>
        {historial.length > 0 && (
          <div className="hide-scrollbar" style={styles.chatMessages}>
            {historial.map((msg, index) => (
              <div
                key={index}
                style={{
                  ...styles.bubble,
                  ...(msg.sender === "user"
                    ? styles.bubbleUser
                    : styles.bubbleAi),
                  ...(msg.type === "result" ? { maxWidth: "100%" } : {}),
                }}
              >
                {renderMessageContent(msg)}
              </div>
            ))}

            {cargando && <LoadingChat />}
            <div ref={messagesEndRef} />
          </div>
        )}

        {/* ========================================== */}
        {/* ÁREA DE INPUT                              */}
        {/* ========================================== */}
        <div
          className={`w-100 px-3 flex-shrink-0 transition-all ${
            historial.length === 0 ? "m-auto pb-0" : "pb-4 pt-2 mt-auto"
          }`}
        >
          {historial.length === 0 && (
            <h2
              className="text-center fw-bold mb-4"
              style={{ color: "#111827", fontSize: "2rem" }}
            >
              ¿Qué evaluación vamos a generar hoy?
            </h2>
          )}

          {/* 2 y 3. Fondo blanco, bordes grisáceos, en una misma línea y se expande */}
          <div
            className="bg-white rounded-4 shadow-sm d-flex align-items-end p-2"
            style={{ border: "1px solid #ced4da" }}
          >
            <div className="flex-grow-1 overflow-hidden">
              <textarea
                ref={textareaRef}
                value={consulta}
                onChange={(e) => {
                  setConsulta(e.target.value);
                  adjustHeight();
                }}
                onKeyDown={handleKeyDown}
                placeholder="Ej: Necesito una evaluación de fracciones para 4º grado..."
                // 1. Eliminado el "disabled={cargando}" para no bloquear el input
                className="w-100 px-3 py-2 bg-transparent border-0 text-dark hide-scrollbar"
                style={{
                  resize: "none",
                  outline: "none",
                  boxShadow: "none",
                  minHeight: "44px",
                  fontSize: "15px",
                  lineHeight: "1.5",
                }}
                rows={1}
              />
            </div>

            <div className="ms-2 mb-1 me-1">
              <button
                type="button"
                onClick={generarEvaluacion}
                disabled={!consulta.trim()} // Tampoco se bloquea aquí por "cargando", permitiendo que el usuario pueda interactuar (la función por dentro ya previene el doble envío)
                className={`btn d-flex align-items-center justify-content-center rounded-circle p-1 transition-all ${
                  consulta.trim() ? "text-white" : "text-secondary"
                }`}
                style={{
                  // 4. Fondo negro y flecha blanca si hay texto
                  backgroundColor: consulta.trim() ? "#000000" : "#e9ecef",
                  border: "none",
                  width: "36px",
                  height: "36px",
                }}
              >
                <svg
                  width="18"
                  height="18"
                  viewBox="0 0 24 24"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="2.5"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                >
                  <path d="M12 19V5" />
                  <path d="M5 12l7-7 7 7" />
                </svg>
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default App;
