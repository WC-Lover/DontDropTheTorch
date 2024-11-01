public class EnemySpawnAttributes
{
    public float SpawnCooldown;
    public int SurroundingEnemySpawnAmount; 
    public int PressureEnemySpawnAmount; 
    public int ChasingEnemySpawnAmount; 
    public float WaveSpawnCooldown;

    public EnemySpawnAttributes()
    {
        SpawnCooldown = 2f;
        WaveSpawnCooldown = 5f;
        SurroundingEnemySpawnAmount = 1;
    }
}
