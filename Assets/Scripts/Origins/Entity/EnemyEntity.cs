using UnityEngine;

namespace Origins {
    public class EnemyEntity : AbsEntity {
        private int actorId;
        
        public EnemyEntity(int roleId) {
            RoleId = roleId;
            InstanceId = EntityManager.instance.AutoIndex++;
        }
        
        public override void OnUpdate() {
            
        }

        public override void OnInit() {
            InitProperty(RoleId);
            
            var actor = ActorManager.instance.GetActorFromPool(this);
            actorId = actor.InstanceId;
        }

        public override void OnClear() {
            
        }

        public void SetPosition(Vector2 value) {
            Position = value;
            ActorManager.instance.SetActorPosition(InstanceId, value);
        }
        
        protected override void InitProperty(int roleId) {
            var config = CharacterTable.Instance.Get(roleId);
            Hp = config.maxHp;
            Mana = config.magic;
            MoveSpeed = config.moveSpeed;
        }
    }
}