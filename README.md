# Generador Automático de Evaluaciones Matemáticas basado en GraphRAG y Grafos de Conocimiento

Sistema de generación automática de instrumentos de evaluación de opción múltiple (MCQ) enfocado en operaciones básicas con fracciones para educación primaria. La solución integra un Grafo Dirigido Acíclico (DAG) en **Neo4j**, recuperación semántica por embeddings vectoriales, modelos de lenguaje grande (**LLMs**) y un validador de consistencia aritmética determinista.

---

## 📌 Descripción del Proyecto

El diseño tradicional de reactivos de opción múltiple con distractores pedagógicamente coherentes representa un alto consumo de tiempo para los docentes de educación básica. Asimismo, la generación directa mediante modelos de lenguaje (LLMs) presenta riesgos de **alucinaciones aritméticas** y carece de una representación explícita sobre la jerarquía conceptual y los prerrequisitos de aprendizaje.

Este proyecto implementa una arquitectura **GraphRAG** que desacopla la recuperación del conocimiento estructurado y la validación matemática de la generación de lenguaje natural:
* **Grafo de Conocimiento (DAG):** Modela las dependencias jerárquicas y prerrequisitos conceptuales del aprendizaje de fracciones.
* **Recuperación Híbrida:** Combina búsqueda semántica con índices vectoriales (HNSW) y recorrido de relaciones estructuradas `[:REQUIERE_DE]`.
* **Algoritmo de Doble Permutación (Fisher-Yates):** Garantiza la distribución no predecible y balanceada de distractores y claves de respuesta.
* **Validación de Ciclo Cerrado:** Audita la consistencia numérica de cada reactivo con el motor **NCalc**, activando ciclos de autocorrección antes de la entrega.

---

## 🛠️ Stack Tecnológico

* **Backend:** C# 11, .NET 10, ASP.NET Core Web API
* **Frontend:** React 18, JavaScript, Vite, HTML5, CSS3, Bootstrap 5
* **Base de Datos de Grafos:** Neo4j DBMS 5.x, Plugin Neo4j GenAI, Índice Vectorial HNSW
* **Modelos de IA (OpenAI API):**
  * `text-embedding-3-small` (Vectorización semántica, 1536 dimensiones)
  * `gpt-4o-mini` (Extracción de parámetros e inferencia generativa)
* **Motor Aritmético:** NCalc (Evaluación determinista de expresiones fraccionarias)
* **Control de Versiones & Gestión:** Git, GitHub, Trello (Scrum)

---

## 🏛️ Arquitectura del Sistema

La solución opera bajo una arquitectura desacoplada por capas:# TutorInteligente
## Arquitectura del Sistema

```text
+-------------------------------------------------------------------------------+
|                            CAPA DE PRESENTACIÓN (UI)                          |
|                                                                               |
|  - Interfaz conversacional SPA en React + Vite                                |
|  - Entrada en lenguaje natural (texto plano) y renderizado interactivo MCQ    |
+---------------------------------------+---------------------------------------+
                                        |
                                        | HTTPS / REST (JSON)
                                        ▼
+-------------------------------------------------------------------------------+
|                          CAPA DE LÓGICA DE APLICACIÓN                         |
|                                                                               |
|  - Módulo de Interpretación: Extracción de parámetros con LLM                 |
|  - Motor de Recuperación Híbrida: Vector search (score >= 0.85) + DAG         |
|  - Generador Autocorrectivo: Prompts contextualizados con NEM                 |
|  - Validador Matemático: Auditoría con NCalc (Tolerancia decimal y reintentos)|
+---------------------------------------+---------------------------------------+
                                        |
                                        | Protocolo Bolt / HTTPS
                                        ▼
+-------------------------------------------------------------------------------+
|                          CAPA DE DATOS Y SERVICIOS                            |
|                                                                               |
|  - Neo4j: Grafo DAG (:Tema, [:REQUIERE_DE]) con índice HNSW                   |
|  - OpenAI API: text-embedding-3-small & gpt-4o-mini                           |
+-------------------------------------------------------------------------------+
```
## 🚀 Instalación y Configuración Local

### Requisitos Previos
* [.NET 10 SDK](https://dotnet.microsoft.com/)
* [Node.js](https://nodejs.org/) (versión 18 o superior)
* [Neo4j Desktop](https://neo4j.com/download/) o Neo4j Community Server 5.x (con Plugin GenAI instalado)
* Cuenta activa con clave de API de [OpenAI](https://platform.openai.com/)

### 1. Clonar el Repositorio
```bash
git clone [https://github.com/JoahanHernandez/GraphRAG-MathEvaluator.git](https://github.com/JoahanHernandez/GraphRAG-MathEvaluator.git)
cd GraphRAG-MathEvaluator
```

### 2. Configurar la Base de Datos Neo4j

1. Inicia tu instancia local de Neo4j en el puerto por defecto:

   ```text
   bolt://localhost:7687
   ```

2. Asegúrate de tener habilitado el plugin **Neo4j GenAI**.

3. Crea el índice vectorial ejecutando el siguiente comando en la consola de Cypher:

   ```cypher
   CREATE VECTOR INDEX tema_embedding_index IF NOT EXISTS
   FOR (n:Tema) ON (n.nombreTemaEmbedding)
   OPTIONS {
     indexConfig: {
       `vector.dimensions`: 1536,
       `vector.similarity_function`: 'cosine'
     }
   };
   ```

---

### 3. Configuración del Backend (.NET 10)

Navega a la carpeta del proyecto backend y configura tus credenciales en `appsettings.json` o mediante variables de entorno:

```json
{
  "Neo4j": {
    "Uri": "bolt://localhost:7687",
    "User": "neo4j",
    "Password": "TU_PASSWORD"
  },
  "OpenAI": {
    "ApiKey": "TU_OPENAI_API_KEY",
    "EmbeddingModel": "text-embedding-3-small",
    "ChatModel": "gpt-4o-mini"
  }
}
```

Ejecuta el servidor backend:

```bash
cd backend/MathEvaluator.Api
dotnet restore
dotnet run
```

La API estará disponible en:

```text
https://localhost:5001
```

> O en el puerto configurado en el proyecto.

---

### 4. Configuración del Frontend (React + Vite)

En otra terminal, accede al directorio del frontend:

```bash
cd frontend
npm install
npm run dev
```

Abre tu navegador en:

```text
http://localhost:5173
```

---

## 💻 Flujo de Uso del Sistema

1. **Entrada de Consulta:** El docente escribe una solicitud en lenguaje natural en la interfaz. Por ejemplo:

   > *"Genera una evaluación de 3 preguntas del tema suma de fracciones con distinto denominador"*

2. **Procesamiento:** El backend interpreta los parámetros de la solicitud, localiza el nodo correspondiente en Neo4j, extrae los prerrequisitos conceptuales —por ejemplo, `suma_enteros` y `division_enteros`—, ensambla el contexto de generación y solicita la redacción de los reactivos al LLM.

3. **Validación Automática:** NCalc evalúa las expresiones matemáticas generadas. Si existen discrepancias numéricas o inconsistencias en los resultados, se activa un ciclo interno de autocorrección.

4. **Visualización:** La interfaz despliega los reactivos generados mediante tarjetas interactivas, mostrando la respuesta correcta, los distractores y su respectiva justificación pedagógica.

---

## 👨‍💻 Autor y Créditos

* **Autor:** Joahan Israel Hernández Granados
* **Institución:** Instituto Politécnico Nacional (IPN) — Escuela Superior de Ingeniería Mecánica y Eléctrica, Unidad Culhuacán (ESIME Culhuacán)
* **Carrera:** Ingeniería en Computación
* **Asesores:** Dr. Juan Arturo Pérez Cebreros y M. en C. Luis Carlos Castro Madrid
