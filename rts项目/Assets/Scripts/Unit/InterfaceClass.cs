using UnityEngine;

namespace IUnit
{
    public enum ResourceType
    {
        wood,
        meat,
        gold
    }
    public interface IResouceUnit
    {
        public bool IsDead { get; }
        public bool HasAssignedWorker { get; }
        void AssignWorker(WorkerUnit _worker);
        Transform transform { get; }
    }
}