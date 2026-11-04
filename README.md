# IA · Parcial 2 · Percepción, grafos y pathfinding

Proyecto de Unity 6000.3.17f1. Abrir la escena `Assets/Scenes/Simulation.unity` y darle Play.

## Integrantes
- Laureano Farias
- Matías Sandoval

## Qué hay en la escena
- **Cazador** (FSM: Search, Attack, Gather, Flee, Exit, Dead) que navega con A* sobre el grafo.
- **Boids** (flocking del Parcial 1) encerrados en el Coto de caza.
- **4 lobos** con percepción FOV/LOS, aullido de alerta, búsqueda con A* y persecución.
- Zonas: inicial, Coto de caza, 2 zonas seguras y Salida.

## División del trabajo
**Laureano Farias:** grafo de navegación y A*, seguimiento de caminos, componentes y FSM del Cazador, percepción FOV/LOS, feedback visual compartido y escena de prueba de pathfinding.

**Matías Sandoval:** FSM genérica y base del Parcial 1, escenario y zonas seguras, Boids en el Coto, lobos (comunicación, FSM, captura y feedback de alertas), percepción y combate del Cazador, HUD y escena final.

## Escenas
- `Simulation`: simulación completa.
- `PathfindingTest`: prueba del grafo, A* y la alerta entre lobos.
