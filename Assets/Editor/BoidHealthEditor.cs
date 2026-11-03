using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoidHealth))]
public class BoidHealthEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (!Application.isPlaying) return;

        DrawTestControls();
    }

    private void DrawTestControls()
    {
        BoidHealth health = (BoidHealth)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Pruebas durante Play", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Vida", health.CurrentHealth + " / " + health.MaxHealth);

        using (new EditorGUI.DisabledScope(health.IsDead))
        {
            if (GUILayout.Button("Probar impacto: 25% de la vida maxima"))
                health.TakeDamage(health.MaxHealth * 0.25f);
        }

        using (new EditorGUI.DisabledScope(!health.IsDead || health.IsCollected))
        {
            if (GUILayout.Button("Probar recoleccion del cadaver"))
                health.Collect();
        }
    }
}
