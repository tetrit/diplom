using System.Collections;
using Surveillance.Events;
using Surveillance.Reactions;
using UnityEngine;

public class LightController : AbstractReaction
{
    [SerializeField] private Light light;
    [SerializeField] private int cooldownChangeColorToNormal;


    protected override void ExecuteReaction(SystemEvent sysEvent)
    {
        StartCoroutine(Alert());
    }
    

    private void ChangeLight(Color color)
    {
        light.color = color;
    }

    private IEnumerator Alert()
    {
        ChangeLight(Color.red);
        yield return new WaitForSeconds(cooldownChangeColorToNormal);
        ChangeLight(Color.white);
    }


}
