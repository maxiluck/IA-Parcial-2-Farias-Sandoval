using UnityEngine;

// Screen-space information is separate from the hunter's world-space effects.
public sealed class HunterHudPanel
{
    private readonly HunterAgent hunter;
    private readonly HunterCombat combat;
    private readonly HunterHealth health;

    public HunterHudPanel(HunterAgent hunter, HunterCombat combat, HunterHealth health)
    {
        this.hunter = hunter;
        this.combat = combat;
        this.health = health;
    }

    public void Draw(Vector2 position, string lastAttack, string lastHit)
    {
        if (hunter == null) return;

        GUI.Box(new Rect(position.x, position.y, 480f, 395f), "Cazador - FSM");
        GUILayout.BeginArea(new Rect(position.x + 10f, position.y + 25f, 460f, 365f));
        DrawHunterStatus();
        DrawGatherInfo();
        DrawFleeInfo();
        DrawTargetInfo(lastAttack, lastHit);
        DrawLegend();
        GUILayout.EndArea();
    }

    private void DrawHunterStatus()
    {
        GUILayout.Label("Estado: " + hunter.StateName + " | TBA: "
            + (combat != null ? combat.AttackRemaining.ToString("F1") : "Sin configurar"));
        if (health != null)
            GUILayout.Label("Vida del cazador: " + health.CurrentHealth.ToString("F0")
                + " / " + health.MaxHealth.ToString("F0"));
        GUILayout.Label(hunter.ActionDescription);
    }

    private void DrawGatherInfo()
    {
        GUILayout.Label("Recolectados: " + hunter.GatherCount + " / " + hunter.RequiredGathers
            + " | Ciclos completados: " + hunter.CompletedCycles);
        if (hunter.IsGathering)
            GUILayout.Label("Tiempo de recolección restante: " + hunter.GatherRemaining.ToString("F1") + " s");
    }

    private void DrawFleeInfo()
    {
        WolfAgent wolf = hunter.Sensor != null ? hunter.Sensor.NearestWolf : null;
        GUILayout.Label("Lobo detectado: " + (wolf != null ? wolf.name : "Ninguno"));
        if (!hunter.IsFleeing) return;
        SafeZone zone = hunter.SelectedSafeZone;
        GUILayout.Label("Refugio: " + (zone != null ? zone.name : "Sin camino")
            + " | Ruta: " + hunter.RefugeRouteLength.ToString("F1"));
        GUILayout.Label("Espera segura restante: " + hunter.FleeWaitRemaining.ToString("F1") + " s");
    }

    private void DrawTargetInfo(string lastAttack, string lastHit)
    {
        GUILayout.Label("Objetivo: " + (hunter.CurrentTarget != null ? hunter.CurrentTarget.name : "Ninguno"));
        if (hunter.CurrentTarget != null)
            GUILayout.Label("Vida del objetivo: " + hunter.CurrentTarget.CurrentHealth.ToString("F0")
                + " / " + hunter.CurrentTarget.MaxHealth.ToString("F0"));
        GUILayout.Label("Último ataque: " + lastAttack);
        GUILayout.Label("Vida tras el último impacto: " + lastHit);
        if (hunter.InterestSpawner != null)
            GUILayout.Label("Objetos de interés activos: " + hunter.InterestSpawner.ActiveCount);
    }

    private void DrawLegend()
    {
        GUILayout.Label("Cazador: amarillo búsqueda | rojo ataque/disparo");
        GUILayout.Label("Cazador: verde recolección | celeste salida");
        GUILayout.Label("Cazador: naranja huida / espera segura");
        GUILayout.Label("Cazador: blanco daño | gris muerte");
        GUILayout.Label("Boids: azul grupo | verde interés | naranja huida");
        GUILayout.Label("Boids: blanco impacto | gris muerto");
    }
}
