namespace Interfaces {
    public interface ITroop {
        void PopulateInstance(int level);
        int Damage { get; }
        void Move();
        void Attack();
        void Die();
        void Check();
        void TakeDamage(int damage);
        
    }
}