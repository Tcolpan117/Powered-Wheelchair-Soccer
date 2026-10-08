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

    private void ResetPlayers()
    {
        WorldRegister.GetItem("AwayLeftFielder").Get<Player>().ResetPlayer();
        WorldRegister.GetItem("AwayRightFielder").Get<Player>().ResetPlayer();
        WorldRegister.GetItem("HomeLeftFielder").Get<Player>().ResetPlayer();
        WorldRegister.GetItem("HomeRightFielder").Get<Player>().ResetPlayer();
        WorldRegister.GetItem("HomeGoaly").Get<Player>().ResetPlayer();
        WorldRegister.GetItem("AwayGoaly").Get<Player>().ResetPlayer();


    }

    private void ScoreGoal(RegistryItem trigger, RegistryItem cause)
    {
        ResetPlayers();
        cause.Get<Ball>().ResetBall();
    }

}
