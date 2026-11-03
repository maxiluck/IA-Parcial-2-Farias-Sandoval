using UnityEngine;

public sealed class AgentColorFeedback
{
    private readonly Renderer[] bodies;
    private readonly Renderer excludedRenderer;
    private readonly MaterialPropertyBlock properties = new MaterialPropertyBlock();
    private Color appliedColor;
    private bool hasAppliedColor;

    public AgentColorFeedback(Transform agent, Renderer excludedRenderer = null)
    {
        bodies = agent.GetComponentsInChildren<Renderer>(true);
        this.excludedRenderer = excludedRenderer;
    }

    public void Apply(Color color)
    {
        if (hasAppliedColor && appliedColor == color) return;

        for (int i = 0; i < bodies.Length; i++)
        {
            Renderer body = bodies[i];
            if (body == null || body == excludedRenderer) continue;

            body.GetPropertyBlock(properties);
            properties.SetColor("_BaseColor", color);
            properties.SetColor("_Color", color);
            body.SetPropertyBlock(properties);
        }

        appliedColor = color;
        hasAppliedColor = true;
    }
}
