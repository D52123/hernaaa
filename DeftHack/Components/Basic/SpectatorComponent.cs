




using UnityEngine;



[Component]
public class SpectatorComponent : MonoBehaviour
{
    public void FixedUpdate()
    {
        bool flag = !DrawUtilities.ShouldRun();
        if (!flag)
        {
            bool flag2 = MiscOptions.SpectatedPlayer != null && !PlayerCoroutines.IsSpying;
            if (flag2)
            {
                // isOrbiting больше не поддерживается в современном Unturned
                // OptimizationVariables.MainPlayer.look.isOrbiting = true;
                // OptimizationVariables.MainPlayer.look.orbitPosition = MiscOptions.SpectatedPlayer.transform.position - OptimizationVariables.MainPlayer.transform.position;
                // OptimizationVariables.MainPlayer.look.orbitPosition += new Vector3(0f, 3f, 0f);
                
                // Альтернативный подход - телепортация камеры
                if (OptimizationVariables.MainPlayer != null && MiscOptions.SpectatedPlayer != null)
                {
                    Vector3 targetPos = MiscOptions.SpectatedPlayer.transform.position + new Vector3(0f, 3f, 0f);
                    OptimizationVariables.MainPlayer.transform.position = targetPos;
                }
            }
            else
            {
                // isOrbiting больше не поддерживается
                // OptimizationVariables.MainPlayer.look.isOrbiting = MiscOptions.Freecam;
            }
        }
    }
}
