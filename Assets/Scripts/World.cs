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
        string id = triggerId;

        switch (id)
        {
            case "AwayGoal" or "HomeGoal":
                if (causeId == "Ball") ScoreGoal(causeId);
                break;
        }
    }

    private void ScoreGoal(string causeId)
    {
        GameObject ball = WorldRegister.GetItem(causeId).gameObject;

        Rigidbody rb = ball.GetComponent<Rigidbody>();

        rb.linearVelocity = Vector3.zero; 
        rb.angularVelocity = Vector3.zero;

        ball.transform.localPosition = Vector3.zero + Vector3.up;
    }

}
