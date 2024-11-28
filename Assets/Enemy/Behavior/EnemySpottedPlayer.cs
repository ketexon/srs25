using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/EnemySpottedPlayer")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "EnemySpottedPlayer", message: "[Enemy] Spotted Player", category: "Events", id: "50f3e46ebec0243270faf84a18f3c224")]
public partial class EnemySpottedPlayer : EventChannelBase
{
    public delegate void EnemySpottedPlayerEventHandler(GameObject Enemy);
    public event EnemySpottedPlayerEventHandler Event; 

    public void SendEventMessage(GameObject Enemy)
    {
        Event?.Invoke(Enemy);
    }

    public override void SendEventMessage(BlackboardVariable[] messageData)
    {
        BlackboardVariable<GameObject> EnemyBlackboardVariable = messageData[0] as BlackboardVariable<GameObject>;
        var Enemy = EnemyBlackboardVariable != null ? EnemyBlackboardVariable.Value : default(GameObject);

        Event?.Invoke(Enemy);
    }

    public override Delegate CreateEventHandler(BlackboardVariable[] vars, System.Action callback)
    {
        EnemySpottedPlayerEventHandler del = (Enemy) =>
        {
            BlackboardVariable<GameObject> var0 = vars[0] as BlackboardVariable<GameObject>;
            if(var0 != null)
                var0.Value = Enemy;

            callback();
        };
        return del;
    }

    public override void RegisterListener(Delegate del)
    {
        Event += del as EnemySpottedPlayerEventHandler;
    }

    public override void UnregisterListener(Delegate del)
    {
        Event -= del as EnemySpottedPlayerEventHandler;
    }
}

