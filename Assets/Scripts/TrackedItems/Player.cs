using UnityEngine;

public class Player : MonoBehaviour, IRegistryProvider
{
    public RegistryItem item {get; private set;}

    [SerializeField] private string id;
    [SerializeField] private Team team;
    [SerializeField] private Role role;

    private void ConstructRegistryItem() => item = new RegistryItem(id, gameObject);
    void Awake() => ConstructRegistryItem();
    void Start() => WorldRegister.Register(item);
    void OnDestroy() => WorldRegister.UnRegister(item);

    public Vector3 GetPosition(){ return transform.position;}
    public void ResetPlayer()
    {
        Vector3 position;

        switch (id)
        {
            case "AwayGoaly":
                position = WorldRegister.GetItem("AwayPen").Get<Pen>().GetPosition();

                transform.rotation = Quaternion.Euler(0f, 270f, 0f);
                transform.localPosition = position;
                break;

            case "AwayLeftFielder":
                position = WorldRegister.GetItem("AwayLeftField").Get<Field>().GetPosition();
                
                transform.rotation = Quaternion.Euler(0f, 270f, 0f);
                transform.localPosition = position;
                break;

            case "AwayRightFielder":
                position = WorldRegister.GetItem("AwayRightField").Get<Field>().GetPosition();

                transform.rotation = Quaternion.Euler(0f, 270f, 0f);
                transform.localPosition = position;
                break;

            case "HomeGoaly":
                position = WorldRegister.GetItem("HomePen").Get<Pen>().GetPosition();

                transform.rotation = Quaternion.Euler(0f, 90f, 0f);
                transform.localPosition = position;
                break;

            case "HomeLeftFielder":
                position = WorldRegister.GetItem("HomeLeftField").Get<Field>().GetPosition();

                transform.rotation = Quaternion.Euler(0f, 90f, 0f);
                transform.localPosition = position;
                break;

            case "HomeRightFielder":
                position = WorldRegister.GetItem("HomeRightField").Get<Field>().GetPosition();

                transform.rotation = Quaternion.Euler(0f, 90f, 0f);
                transform.localPosition = position;
                break;
        }
    }
}