import axios from "axios";

export const solicitarEvaluacionAPI = async (consulta) => {
  try {
    const response = await axios.post("https://localhost:7237/api/interpretacion/orquestar", {
      consulta
    });

    if (!response.data) throw new Error("El servidor no devolvió datos.");
    
    return response.data;
    
  // 1. Aquí corregimos el error de referencia declarando (err)
  } catch (err) { 
    throw new Error(
      err.response?.data?.error ||
      err.response?.data?.mensajeError ||
      "Error al conectar con el servidor educativo.",
      // 2. Aquí satisfacemos al Linter encapsulando la causa original
      { cause: err } 
    );
  }
};