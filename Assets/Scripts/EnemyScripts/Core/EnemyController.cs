using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private EnemySensors sensors;
    [SerializeField] private EnemyMovement movement;
    [SerializeField] private EnemyStateMachine stateMachine;
    
    [Header("References")]
    public Transform player;
    public AStarPathfinding2D pathfinder;
    
    private void Awake()
    {
        InitializeComponents();
        AutoFindReferences();
    }
    
    private void Update()
    {
        if (!IsInitialized()) return;
        
        sensors.UpdateSensors(player.position);
        stateMachine.UpdateState(sensors.GetSensorData());
        movement.UpdateMovement(stateMachine.GetMovementTarget(), stateMachine.GetMovementSpeed());
    }
    
    private void InitializeComponents()
    {
        sensors = GetComponent<EnemySensors>() ?? gameObject.AddComponent<EnemySensors>();
        movement = GetComponent<EnemyMovement>() ?? gameObject.AddComponent<EnemyMovement>();
        stateMachine = GetComponent<EnemyStateMachine>() ?? gameObject.AddComponent<EnemyStateMachine>();
        
        movement.Initialize(pathfinder);
        stateMachine.Initialize();
    }
    
    private void AutoFindReferences()
    {
        pathfinder ??= FindAnyObjectByType<AStarPathfinding2D>();
        player ??= GameObject.FindGameObjectWithTag("Player")?.transform;
    }
    
    private bool IsInitialized() => player != null && pathfinder != null;
}