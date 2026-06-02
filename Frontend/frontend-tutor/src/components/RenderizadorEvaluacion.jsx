
export function RenderizadorEvaluacion({ datos }) {
  // Verificación de seguridad: si no hay datos, no renderizamos nada
  if (!datos || !datos.preguntas) return null;

  return (
    <div className="d-flex flex-column gap-4">
      {datos.preguntas.map((preg, pIndex) => (
        <div 
          key={pIndex} 
          className="bg-light p-4 rounded-4 border"
        >
          <p className="fw-bold fs-5 mb-3">
            {pIndex + 1}. {preg.enunciado}
          </p>

          <div className="d-flex flex-column gap-2 ps-3">
            {(preg.incisos || []).map((inciso, iIndex) => {
              const esCorrecta = inciso.esCorrecta;
              
              return (
                <div
                  key={iIndex}
                  className={`p-3 rounded-3 border d-flex flex-column ${
                    esCorrecta ? "bg-success bg-opacity-10 border-success" : "bg-white border-light-subtle"
                  }`}
                >
                  <div className="d-flex align-items-center">
                    <strong className="text-primary me-2">
                      {inciso.letra})
                    </strong>
                    {/* El valor calculado mantiene su formato fraccionario nativo (ej. "7/12") */}
                    <span className="fs-6 font-monospace">
                      {inciso.valorCalculado}
                    </span>

                    {esCorrecta && (
                      <span className="text-success fw-bold ms-auto small d-flex align-items-center gap-1">
                        ✓ Opción Correcta
                      </span>
                    )}
                  </div>
                  
                  <span className="text-secondary mt-2 pt-2 border-top border-secondary-subtle border-dashed small">
                    <em>Evalúa: {inciso.textoTema}</em>
                  </span>
                </div>
              );
            })}
          </div>
        </div>
      ))}
    </div>
  );
}