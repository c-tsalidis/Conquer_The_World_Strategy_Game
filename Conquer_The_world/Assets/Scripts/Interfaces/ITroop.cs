using System.Collections;

namespace Interfaces {
    public interface ITroop {
        void PopulateInstance(Player player, int level);
        void Run();
        IEnumerator Attack();
        IEnumerator Die();
        IEnumerator Check();
        void TakeDamage(int damage, Troop damager);
    }
}