import { useState, useRef, useEffect } from "react";
import ReactMarkdown from "react-markdown";
import remarkMath from "remark-math";
import rehypeKatex from "rehype-katex";
import "katex/dist/katex.min.css";
import { RenderizadorEvaluacion } from "./components/RenderizadorEvaluacion";

// Importamos nuestra infraestructura y estilos
import { solicitarEvaluacionAPI } from "./services/apiService"; 
import { useAutoResizeTextarea } from "./hooks/useAutoResizeTextarea"; // Asumimos que moviste tu hook a un archivo
import "./App.css";

function App() {
  // 1. ESTADO (Capa de Aplicación)
  const [consulta, setConsulta] = useState("");
  const [cargando, setCargando] = useState(false);
  const [historial, setHistorial] = useState([]);
  
  const messagesEndRef = useRef(null);
  const { textareaRef, adjustHeight } = useAutoResizeTextarea({ minHeight: 44, maxHeight: 200 });

  // 2. EFECTOS (Ciclo de vida)
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [historial, cargando]);

  // 3. LÓGICA DE NEGOCIO
  const prepararMatematicas = (texto) => {
    if (!texto) return "";
    return texto.replace(/\\\(/g, "$").replace(/\\\)/g, "$").replace(/\\\[/g, "$$").replace(/\\\]/g, "$$");
  };

  const generarEvaluacion = async (e) => {
    if (e) e.preventDefault();
    if (!consulta.trim() || cargando) return;

    const nuevaConsulta = consulta;
    setConsulta("");
    adjustHeight(true);

    // Actualizamos UI con la pregunta del usuario
    setHistorial(prev => [...prev, { sender: "user", type: "text", content: nuevaConsulta }]);
    setCargando(true);

    try {
      // Llamada limpia a la capa de infraestructura
      const data = await solicitarEvaluacionAPI(nuevaConsulta);
      setHistorial(prev => [...prev, { sender: "ai", type: "result", data: data }]);
    } catch (error) {
      setHistorial(prev => [...prev, { sender: "ai", type: "text", content: `❌ ${error.message}` }]);
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

  // 4. PRESENTACIÓN (Vista)
  // Nota: Lo ideal es extraer estos renderizadores a componentes puros (ej. <TarjetaGrafo />, <FormatoEvaluacion />)
  const renderMessageContent = (msg) => {
    if (msg.type === "text") return <p>{msg.content}</p>;

    if (msg.type === "result") {
      const { temaPrincipal, subgrafo, evaluacion } = msg.data;
      
      // Renderizado simplificado de la estructura del grafo
      return (
        <div className="resultados-container">
          <div className="context-card">
            <h3>📊 Contexto Curricular: {temaPrincipal}</h3>
            <div style={{ display: "flex", gap: "10px" }}>
              {(subgrafo || []).map((nodo, index) => (
                <span key={index} className={`tag ${nodo.esPrerrequisito ? 'tag-prereq' : 'tag-central'}`}>
                  {nodo.nombre} {nodo.esPrerrequisito ? "🔑" : "🎯"}
                </span>
              ))}
            </div>
          </div>
          
          <div className="evaluacion-card prose">
            <h3 style={{ color: "var(--primary)", marginTop: 0, borderBottom: "1px solid #eee", paddingBottom: "10px", marginBottom: "20px" }}>
              📝 Propuesta de Evaluación
            </h3>
            
            {/* AQUÍ ESTÁ LA MAGIA */}
            {typeof evaluacion === "string" ? (
               <ReactMarkdown remarkPlugins={[remarkMath]} rehypePlugins={[rehypeKatex]}>
                 {prepararMatematicas(evaluacion)}
               </ReactMarkdown>
             ) : (
               <RenderizadorEvaluacion datos={evaluacion} />
             )}
             
          </div>
        </div>
      );
    }
  };

  return (
    <div className="app-container">
      <div className="chat-window">
        {historial.length > 0 && (
          <div className="chat-messages">
            {historial.map((msg, index) => (
              <div key={index} className={`bubble ${msg.sender === "user" ? "bubble-user" : "bubble-ai"}`}>
                {renderMessageContent(msg)}
              </div>
            ))}
            {cargando && <div className="bubble bubble-ai"><em>Analizando currículum...</em></div>}
            <div ref={messagesEndRef} />
          </div>
        )}

       {/* ÁREA DE INPUT RESTAURADA CON BOOTSTRAP */}
<div className={`w-100 px-3 flex-shrink-0 transition-all ${ historial.length === 0 ? "m-auto pb-0" : "pb-4 pt-2 mt-auto" }`}>
  {historial.length === 0 && (
    <h2 className="text-center fw-bold mb-4" style={{ color: "#111827", fontSize: "2rem" }}>
      ¿Qué evaluación vamos a generar hoy?
    </h2>
  )}

  <div className="bg-white rounded-4 shadow-sm d-flex align-items-end p-2" style={{ border: "1px solid #ced4da" }}>
    <div className="flex-grow-1 overflow-hidden">
      <textarea
        ref={textareaRef}
        value={consulta}
        onChange={(e) => {
          setConsulta(e.target.value);
          adjustHeight();
        }}
        onKeyDown={handleKeyDown}
        placeholder="Ej: genera una evaluación del tema de suma de fracciones..."
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
        disabled={!consulta.trim()}
        className={`btn d-flex align-items-center justify-content-center rounded-circle p-1 transition-all ${
          consulta.trim() ? "text-white" : "text-secondary"
        }`}
        style={{
          backgroundColor: consulta.trim() ? "#000000" : "#e9ecef",
          border: "none",
          width: "36px",
          height: "36px",
        }}
      >
        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
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