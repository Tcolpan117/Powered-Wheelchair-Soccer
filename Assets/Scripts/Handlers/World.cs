using UnityEngine;

public class World : MonoBehaviour
{

    private void OnEnable()
    {
        Goal.TriggerEvent += HandleTrigger;
    }

    private void OnDisable()
    {
        Goal.TriggerEvent -= HandleTrigger;
    }

    private void HandleTrigger(string triggerId, string causeId)
    {
        RegistryItem trigger = WorldRegister.GetItem(triggerId);
        RegistryItem cause = WorldRegister.GetItem(causeId);

        switch (triggerId)
        {
            case "AwayGoal" or "HomeGoal":
                if (causeId == "Ball") ScoreGoal(trigger, cause);
                break;
        }
    }

    

    private void ScoreGoal(RegistryItem trigger, RegistryItem cause)
    {
        cause.Get<Ball>().ResetBall();
    }

}
