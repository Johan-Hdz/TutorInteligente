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
+-------------------------------------------------------------------------------+
|                            CAPA DE PRESENTACIÓN (UI)                          |
|  - Interfaz conversacional SPA en React + Vite                                |
|  - Entrada en lenguaje natural (texto plano) y renderizado interactivo MCQ    |
+---------------------------------------+---------------------------------------+
| HTTPS / REST (JSON)
+---------------------------------------v---------------------------------------+
|                          CAPA DE LÓGICA DE APLICACIÓN                         |
|  - Módulo de Interpretación: Extracción de parámetros con LLM                 |
|  - Motor de Recuperación Híbrida: Vector search (score >= 0.85) + DAG         |
|  - Generador Autocorrectivo: Prompts contextualizados con NEM                 |
|  - Validador Matemático: Auditoría con NCalc (Tolerancia decimal y reintentos)|
+---------------------------------------+---------------------------------------+
| Protocolo Bolt / HTTPS
+---------------------------------------v---------------------------------------+
|                          CAPA DE DATOS Y SERVICIOS                            |
|  - Neo4j: Grafo DAG (:Tema, [:REQUIERE_DE]) con índice HNSW                   |
|  - OpenAI API: text-embedding-3-small & gpt-4o-mini                           |
+-------------------------------------------------------------------------------+

La solución opera bajo una arquitectura desacoplada por capas:# TutorInteligente

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
