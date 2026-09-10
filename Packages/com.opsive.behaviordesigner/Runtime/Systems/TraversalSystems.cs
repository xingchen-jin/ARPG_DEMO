#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Systems
{
    using Opsive.BehaviorDesigner.Runtime.Components;
    using Opsive.BehaviorDesigner.Runtime.Core;
    using Opsive.BehaviorDesigner.Runtime.Groups;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
    using UnityEngine;

    /// <summary>
    /// Traverses and ensures the correct tasks are active.
    /// </summary>
    [UpdateInGroup(typeof(TraversalSystemGroup))]
    [UpdateAfter(typeof(TraversalTaskSystemGroup))]
    public partial struct EvaluationSystem : ISystem
    {
        private bool m_JobScheduled;
        private EntityQuery m_Query;
        private JobHandle m_Dependency;
        private EntityCommandBuffer m_EntityCommandBuffer;

        /// <summary>
        /// The system has been created.
        /// </summary>
        /// <param name="state">The state of the system.</param>
        private void OnCreate(ref SystemState state)
        {
            m_JobScheduled = false;
            m_Query = SystemAPI.QueryBuilder().WithAllRW<BranchComponent, TaskComponent>().WithAbsent<BakedBehaviorTree>().Build();
        }

        /// <summary>
        /// Starts the job which traverses the tree.
        /// </summary>
        /// <param name="state">The current state of the system.</param>
        private void OnUpdate(ref SystemState state)
        {
            m_JobScheduled = true;

            m_EntityCommandBuffer = new EntityCommandBuffer(state.WorldUpdateAllocator);
            m_Dependency = state.Dependency = new EvaluationJob()
            {
                EntityCommandBuffer = m_EntityCommandBuffer.AsParallelWriter(),
            }.ScheduleParallel(m_Query, state.Dependency);
        }

        /// <summary>
        /// Completes the job and releases any memory.
        /// </summary>
        /// <param name="entityManager">The running EntityManager.</param>
        /// <param name="stopRunning">Has the system been stopped?</param>
        #if !UNITY_EDITOR
        [BurstCompile]
        #endif
        public void Complete(EntityManager entityManager, bool stopRunning = false)
        {
            if (!m_JobScheduled) {
                return;
            }

            if (!stopRunning) {
                m_Dependency.Complete();
                m_EntityCommandBuffer.Playback(entityManager);
                m_EntityCommandBuffer.Dispose();
            }

            m_JobScheduled = false;
        }

        /// <summary>
        /// Job which traverses the tree.
        /// </summary>
        #if !UNITY_EDITOR
        [BurstCompile]
        #endif
        public partial struct EvaluationJob : IJobEntity
        {
            [Tooltip("CommandBuffer which sets the component data.")]
            public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;

            /// <summary>
            /// Executes the job.
            /// </summary>
            /// <param name="entity">The entity that is being acted upon.</param>
            /// <param name="entityIndex">The index of the entity.</param
            /// <param name="branchComponents">An array of branch components.</param>
            /// <param name="taskComponents">An array of task components.</param>
            #if !UNITY_EDITOR
            [BurstCompile]
            #endif
            public void Execute(Entity entity, [EntityIndexInQuery] int entityIndex, ref DynamicBuffer<BranchComponent> branchComponents, ref DynamicBuffer<TaskComponent> taskComponents)
            {
                BehaviorTraversalCore.Evaluate(entity, entityIndex, ref branchComponents, ref taskComponents, EntityCommandBuffer);
            }
        }
    }

    /// <summary>
    /// Loops through the active tasks to determine if the system should stay active for the current tick.
    /// </summary>
    [UpdateInGroup(typeof(TraversalSystemGroup))]
    [UpdateAfter(typeof(EvaluationSystem))]
    public partial struct DetermineEvaluationSystem : ISystem
    {
        [Tooltip("Should the group stay active? An inactive tree does not run.")]
        public bool Active { get; private set; }
        [Tooltip("Should the group be evaluated? This bool indicates if the entire tree should be evaluated instead of the reevaluation" +
                 "concept for conditional aborts. The tree will be reevaluated if any of the leaf tasks have a status of running.")]
        public bool Evaluate { get; private set; }

        private bool m_JobScheduled;
        private JobHandle m_Dependency;

        private EntityQuery m_Query32;
        private EntityQuery m_Query64;
        private EntityQuery m_Query128;
        private EntityQuery m_Query512;
        private EntityQuery m_Query4096;
        private EntityCommandBuffer m_EntityCommandBuffer;

        private NativeArray<bool> m_Results;

        /// <summary>
        /// The system has been created.
        /// </summary>
        /// <param name="state">The state of the system.</param>
        private void OnCreate(ref SystemState state)
        {
            Active = Evaluate = true;
            m_JobScheduled = false;
            m_Query32 = SystemAPI.QueryBuilder().WithAllRW<BranchComponent>().WithAll<TaskComponent, EvaluationComponent32, EvaluateFlag>().WithAbsent<BakedBehaviorTree>().Build();
            m_Query64 = SystemAPI.QueryBuilder().WithAllRW<BranchComponent>().WithAll<TaskComponent, EvaluationComponent64, EvaluateFlag>().WithAbsent<BakedBehaviorTree>().Build();
            m_Query128 = SystemAPI.QueryBuilder().WithAllRW<BranchComponent>().WithAll<TaskComponent, EvaluationComponent128, EvaluateFlag>().WithAbsent<BakedBehaviorTree>().Build();
            m_Query512 = SystemAPI.QueryBuilder().WithAllRW<BranchComponent>().WithAll<TaskComponent, EvaluationComponent512, EvaluateFlag>().WithAbsent<BakedBehaviorTree>().Build();
            m_Query4096 = SystemAPI.QueryBuilder().WithAllRW<BranchComponent>().WithAll<TaskComponent, EvaluationComponent4096, EvaluateFlag>().WithAbsent<BakedBehaviorTree>().Build();
        }

        /// <summary>
        /// Executes the job to determine if the system should stay active and evaluating.
        /// </summary>
        /// <param name="state">The current state of the system.</param>
        #if !UNITY_EDITOR
        [BurstCompile]
        #endif
        private void OnUpdate(ref SystemState state)
        {
            Active = Evaluate = true;
            m_JobScheduled = true;

            if (m_Query32.IsEmptyIgnoreFilter && m_Query64.IsEmptyIgnoreFilter && m_Query128.IsEmptyIgnoreFilter && m_Query512.IsEmptyIgnoreFilter && m_Query4096.IsEmptyIgnoreFilter) {
                Active = Evaluate = false;
                m_JobScheduled = false;
                return;
            }

            m_EntityCommandBuffer = new EntityCommandBuffer(Allocator.TempJob);

            m_Results = new NativeArray<bool>(3, Allocator.TempJob, NativeArrayOptions.ClearMemory);
            var entityCommandBufferParallelWriter = m_EntityCommandBuffer.AsParallelWriter();

            // Chain jobs sequentially since they all write to the shared Results array.
            m_Dependency = new DetermineEvaluationJob32()
            {
                EntityCommandBuffer = entityCommandBufferParallelWriter,
                Results = m_Results
            }.ScheduleParallel(m_Query32, state.Dependency);
            m_Dependency = new DetermineEvaluationJob64()
            {
                EntityCommandBuffer = entityCommandBufferParallelWriter,
                Results = m_Results
            }.ScheduleParallel(m_Query64, m_Dependency);
            m_Dependency = new DetermineEvaluationJob128()
            {
                EntityCommandBuffer = entityCommandBufferParallelWriter,
                Results = m_Results
            }.ScheduleParallel(m_Query128, m_Dependency);
            m_Dependency = new DetermineEvaluationJob512()
            {
                EntityCommandBuffer = entityCommandBufferParallelWriter,
                Results = m_Results
            }.ScheduleParallel(m_Query512, m_Dependency);
            m_Dependency = new DetermineEvaluationJob4096()
            {
                EntityCommandBuffer = entityCommandBufferParallelWriter,
                Results = m_Results
            }.ScheduleParallel(m_Query4096, m_Dependency);

            state.Dependency = m_Dependency;
        }

        /// <summary>
        /// Completes the job and releases any memory.
        /// </summary>
        /// <param name="entityManager">The running EntityManager.</param>
        #if !UNITY_EDITOR
        [BurstCompile]
        #endif
        public void Complete(EntityManager entityManager)
        {
            if (!m_JobScheduled) {
                return;
            }

            m_Dependency.Complete();
            if (m_EntityCommandBuffer.IsCreated) {
                m_EntityCommandBuffer.Playback(entityManager);
                m_EntityCommandBuffer.Dispose();
                m_EntityCommandBuffer = default;
            }

            if (m_Results.IsCreated) {
                if (m_Results[0]) {
                    Active = m_Results[1];
                    Evaluate = m_Results[2];
                } else {
                    // If the first element is false then no trees executed.
                    Active = Evaluate = false;
                }
                m_Results.Dispose();
                m_Results = default;
            }
            m_JobScheduled = false;
        }

        /// <summary>
        /// The system has been destroyed.
        /// </summary>
        /// <param name="state">The current state of the system.</param>
        private void OnDestroy(ref SystemState state)
        {
            if (!m_JobScheduled) {
                return;
            }

            // During world teardown these containers may already be released by ECS internals.
            m_Dependency.Complete();
            m_EntityCommandBuffer = default;
            m_Results = default;
            m_JobScheduled = false;
        }

        /// <summary>
        /// Job which determine if the system should stay active. If any behavior tree should stay active then the entire system must remain active.
        /// </summary>
        #if !UNITY_EDITOR
        [BurstCompile]
        #endif
        public partial struct DetermineEvaluationJob32 : IJobEntity
        {
            [Tooltip("CommandBuffer which sets the component data.")]
            public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;
            [Tooltip("The computed results.")]
            [NativeDisableParallelForRestriction] public NativeArray<bool> Results;

            /// <summary>
            /// Executes the job.
            /// </summary>
            /// <param name="entity">The entity that is being acted upon.</param>
            /// <param name="entityIndex">The index of the entity.</param>
            /// <param name="branchComponents">An array of branch components.</param>
            /// <param name="taskComponents">An array of task components.</param>
            /// <param name="evaluationComponent">The EvaluationComponent that belongs to the entity.</param>
            #if !UNITY_EDITOR
            [BurstCompile]
            #endif
            private void Execute(Entity entity, [EntityIndexInQuery] int entityIndex, ref DynamicBuffer<BranchComponent> branchComponents, in DynamicBuffer<TaskComponent> taskComponents, ref EvaluationComponent32 evaluationComponent)
            {
                var evaluatedTasks = evaluationComponent.EvaluatedTasks;
                BehaviorTraversalCore.DetermineEvaluation(entity, entityIndex, ref branchComponents, taskComponents, ref evaluatedTasks, evaluationComponent.EvaluationType, evaluationComponent.MaxEvaluationCount, EntityCommandBuffer, Results);
                evaluationComponent.EvaluatedTasks = evaluatedTasks;
            }
        }

        /// <summary>
        /// Job which determine if the system should stay active. If any behavior tree should stay active then the entire system must remain active.
        /// </summary>
        #if !UNITY_EDITOR
        [BurstCompile]
        #endif
        public partial struct DetermineEvaluationJob64 : IJobEntity
        {
            [Tooltip("CommandBuffer which sets the component data.")]
            public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;
            [Tooltip("The computed results.")]
            [NativeDisableParallelForRestriction] public NativeArray<bool> Results;

            /// <summary>
            /// Executes the job.
            /// </summary>
            /// <param name="entity">The entity that is being acted upon.</param>
            /// <param name="entityIndex">The index of the entity.</param>
            /// <param name="branchComponents">An array of branch components.</param>
            /// <param name="taskComponents">An array of task components.</param>
            /// <param name="evaluationComponent">The EvaluationComponent that belongs to the entity.</param>
            #if !UNITY_EDITOR
            [BurstCompile]
            #endif
            private void Execute(Entity entity, [EntityIndexInQuery] int entityIndex, ref DynamicBuffer<BranchComponent> branchComponents, in DynamicBuffer<TaskComponent> taskComponents, ref EvaluationComponent64 evaluationComponent)
            {
                var evaluatedTasks = evaluationComponent.EvaluatedTasks;
                BehaviorTraversalCore.DetermineEvaluation(entity, entityIndex, ref branchComponents, taskComponents, ref evaluatedTasks, evaluationComponent.EvaluationType, evaluationComponent.MaxEvaluationCount, EntityCommandBuffer, Results);
                evaluationComponent.EvaluatedTasks = evaluatedTasks;
            }
        }

        /// <summary>
        /// Job which determine if the system should stay active. If any behavior tree should stay active then the entire system must remain active.
        /// </summary>
        #if !UNITY_EDITOR
        [BurstCompile]
        #endif
        public partial struct DetermineEvaluationJob128 : IJobEntity
        {
            [Tooltip("CommandBuffer which sets the component data.")]
            public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;
            [Tooltip("The computed results.")]
            [NativeDisableParallelForRestriction] public NativeArray<bool> Results;

            /// <summary>
            /// Executes the job.
            /// </summary>
            /// <param name="entity">The entity that is being acted upon.</param>
            /// <param name="entityIndex">The index of the entity.</param>
            /// <param name="branchComponents">An array of branch components.</param>
            /// <param name="taskComponents">An array of task components.</param>
            /// <param name="evaluationComponent">The EvaluationComponent that belongs to the entity.</param>
            #if !UNITY_EDITOR
            [BurstCompile]
            #endif
            private void Execute(Entity entity, [EntityIndexInQuery] int entityIndex, ref DynamicBuffer<BranchComponent> branchComponents, in DynamicBuffer<TaskComponent> taskComponents, ref EvaluationComponent128 evaluationComponent)
            {
                var evaluatedTasks = evaluationComponent.EvaluatedTasks;
                BehaviorTraversalCore.DetermineEvaluation(entity, entityIndex, ref branchComponents, taskComponents, ref evaluatedTasks, evaluationComponent.EvaluationType, evaluationComponent.MaxEvaluationCount, EntityCommandBuffer, Results);
                evaluationComponent.EvaluatedTasks = evaluatedTasks;
            }
        }

        /// <summary>
        /// Job which determine if the system should stay active. If any behavior tree should stay active then the entire system must remain active.
        /// </summary>
        #if !UNITY_EDITOR
        [BurstCompile]
        #endif
        public partial struct DetermineEvaluationJob512 : IJobEntity
        {
            [Tooltip("CommandBuffer which sets the component data.")]
            public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;
            [Tooltip("The computed results.")]
            [NativeDisableParallelForRestriction] public NativeArray<bool> Results;

            /// <summary>
            /// Executes the job.
            /// </summary>
            /// <param name="entity">The entity that is being acted upon.</param>
            /// <param name="entityIndex">The index of the entity.</param>
            /// <param name="branchComponents">An array of branch components.</param>
            /// <param name="taskComponents">An array of task components.</param>
            /// <param name="evaluationComponent">The EvaluationComponent that belongs to the entity.</param>
            #if !UNITY_EDITOR
            [BurstCompile]
            #endif
            private void Execute(Entity entity, [EntityIndexInQuery] int entityIndex, ref DynamicBuffer<BranchComponent> branchComponents, in DynamicBuffer<TaskComponent> taskComponents, ref EvaluationComponent512 evaluationComponent)
            {
                var evaluatedTasks = evaluationComponent.EvaluatedTasks;
                BehaviorTraversalCore.DetermineEvaluation(entity, entityIndex, ref branchComponents, taskComponents, ref evaluatedTasks, evaluationComponent.EvaluationType, evaluationComponent.MaxEvaluationCount, EntityCommandBuffer, Results);
                evaluationComponent.EvaluatedTasks = evaluatedTasks;
            }
        }

        /// <summary>
        /// Job which determine if the system should stay active. If any behavior tree should stay active then the entire system must remain active.
        /// </summary>
        #if !UNITY_EDITOR
        [BurstCompile]
        #endif
        public partial struct DetermineEvaluationJob4096 : IJobEntity
        {
            [Tooltip("CommandBuffer which sets the component data.")]
            public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;
            [Tooltip("The computed results.")]
            [NativeDisableParallelForRestriction] public NativeArray<bool> Results;

            /// <summary>
            /// Executes the job.
            /// </summary>
            /// <param name="entity">The entity that is being acted upon.</param>
            /// <param name="entityIndex">The index of the entity.</param>
            /// <param name="branchComponents">An array of branch components.</param>
            /// <param name="taskComponents">An array of task components.</param>
            /// <param name="evaluationComponent">The EvaluationComponent that belongs to the entity.</param>
            #if !UNITY_EDITOR
            [BurstCompile]
            #endif
            private void Execute(Entity entity, [EntityIndexInQuery] int entityIndex, ref DynamicBuffer<BranchComponent> branchComponents, in DynamicBuffer<TaskComponent> taskComponents, ref EvaluationComponent4096 evaluationComponent)
            {
                var evaluatedTasks = evaluationComponent.EvaluatedTasks;
                BehaviorTraversalCore.DetermineEvaluation(entity, entityIndex, ref branchComponents, taskComponents, ref evaluatedTasks, evaluationComponent.EvaluationType, evaluationComponent.MaxEvaluationCount, EntityCommandBuffer, Results);
                evaluationComponent.EvaluatedTasks = evaluatedTasks;
            }
        }
    }

    /// <summary>
    /// Utility functions for the task evaluation.
    /// </summary>
    public struct EvaluationUtility
    {
        /// <summary>
        /// Core evaluation logic that works with any FixedList type for EvaluatedTasks.
        /// </summary>
        /// <typeparam name="TFixedList">The type of FixedList for EvaluatedTasks.</typeparam>
        /// <param name="entity">The entity that is being acted upon.</param>
        /// <param name="entityIndex">The index of the entity.</param>
        /// <param name="branchComponents">An array of branch components.</param>
        /// <param name="taskComponents">An array of task components.</param>
        /// <param name="evaluatedTasks">The evaluated tasks list.</param>
        /// <param name="evaluationType">The evaluation type.</param>
        /// <param name="maxEvaluationCount">The maximum evaluation count.</param>
        /// <param name="entityCommandBuffer">The command buffer for setting component data.</param>
        /// <param name="results">The computed results array.</param>
        public static void DetermineEvaluation<TFixedList>(Entity entity, int entityIndex, ref DynamicBuffer<BranchComponent> branchComponents, DynamicBuffer<TaskComponent> taskComponents, ref TFixedList evaluatedTasks, EvaluationType evaluationType, ushort maxEvaluationCount, EntityCommandBuffer.ParallelWriter entityCommandBuffer, NativeArray<bool> results) where TFixedList : struct, INativeList<ulong>
        {
            BehaviorTraversalCore.DetermineEvaluation(entity, entityIndex, ref branchComponents, taskComponents, ref evaluatedTasks, evaluationType, maxEvaluationCount, entityCommandBuffer, results);
        }

        /// <summary>
        /// Is the task at the specified index a parent task.
        /// </summary>
        /// <param name="taskComponents">An array of task components.</param>
        /// <param name="index">The index to check if it is a parent.</param>
        /// <returns>True if the task at the specified index is a parent task.</returns>
        public static bool IsParentTask(ref DynamicBuffer<TaskComponent> taskComponents, int index)
        {
            return BehaviorTraversalCore.IsParentTask(ref taskComponents, index);
        }
    }
}
#endif