using System.Collections;

namespace Interfaces {
    public interface ITroop {
        void PopulateInstance(int level);
        void Move();
        IEnumerator Attack();
        IEnumerator Die();
        IEnumerator Check();
        void TakeDamage(int damage);
    }
}