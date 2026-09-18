using UnityEngine;

public class Governer : MonoBehaviour
{


    private void OnEnable()
    {
        Trigger.TriggerEvent += HandleTrigger;
    }

    private void OnDisable()
    {
        Trigger.TriggerEvent -= HandleTrigger;
    }

    private void HandleTrigger(GameObject trigger, GameObject cause)
    {
        string name = trigger.name;

        switch (name)
        {
            case "GoalBox":
                if (cause.CompareTag("Ball")) ScoreGoal(cause);
                break;
        }
    }

    private void ScoreGoal(GameObject ball)
    {
        Debug.Log($"ScoreGoal on {ball.name}");
        Rigidbody rb = ball.GetComponent<Rigidbody>();

        rb.linearVelocity = Vector3.zero; 
        rb.angularVelocity = Vector3.zero;

        ball.transform.localPosition = Vector3.zero + Vector3.up;
    }

}
