using System.Collections;

namespace Interfaces {
    public interface ITroop {
        void PopulateInstance(Player player, int level);
        void Run();
        void Attack();
        IEnumerator Die();
        void TakeDamage(int damage, Troop damager);
    }
}