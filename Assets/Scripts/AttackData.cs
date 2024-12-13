using static CombatController;

public class AttackData
{
    public PlayerController PlayerController { get => playerController; }
    private PlayerController playerController;

    public Enemy Enemy { get => enemy; }
    private Enemy enemy;

    public AttackType AttackType { get => attackType; }
    private AttackType attackType;
    
    public AttackData(PlayerController playerController, AttackType attackType)
    {
        this.playerController = playerController;
        this.attackType = attackType;
    }

    public AttackData(Enemy enemy, AttackType attackType)
    {
        this.enemy = enemy;
        this.attackType = attackType;
    }
}