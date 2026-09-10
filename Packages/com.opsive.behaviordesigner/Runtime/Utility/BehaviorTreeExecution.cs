#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Utility
{
    using System;
    using Unity.Entities;
    using UnityEngine;

    /// <summary>
    /// Bridges the public runtime assembly to the ECS execution systems.
    /// </summary>
    public interface IBehaviorTreeExecutionController
    {
        void AddCleanupSystems(World world, ComponentSystemGroup behaviorTreeSystemGroup);
        void AddReevaluateSystem(World world, ComponentSystemGroup beforeTraversalSystemGroup);
        void AddTaskObjectSystem(World world, ComponentSystemGroup traversalTaskSystemGroup);
        Type TaskObjectReevaluateSystemType { get; }
        bool CompleteTraversal(World world, EntityManager entityManager);
        void CompleteEvaluation(World world, EntityManager entityManager, bool stopRunning);
        bool IsActive(World world, EntityManager entityManager);
    }

    /// <summary>
    /// Runtime access point for the Behavior Designer ECS execution systems.
    /// </summary>
    public static class BehaviorTreeExecution
    {
        private const string c_ControllerTypeName = "Opsive.BehaviorDesigner.Runtime.Utility.BehaviorTreeExecutionController, Opsive.BehaviorDesigner.Runtime.Systems";

        private static IBehaviorTreeExecutionController s_Controller;
        private static bool s_ControllerResolveFailed;

        /// <summary>
        /// Registers the execution controller implementation.
        /// </summary>
        /// <param name="controller">The controller implementation.</param>
        public static void Register(IBehaviorTreeExecutionController controller)
        {
            s_Controller = controller;
            s_ControllerResolveFailed = false;
        }

        /// <summary>
        /// Adds the core cleanup systems to the behavior tree group.
        /// </summary>
        /// <param name="world">The ECS world.</param>
        /// <param name="behaviorTreeSystemGroup">The behavior tree system group.</param>
        public static void AddCleanupSystems(World world, ComponentSystemGroup behaviorTreeSystemGroup)
        {
            if (!EnsureController()) {
                return;
            }

            s_Controller.AddCleanupSystems(world, behaviorTreeSystemGroup);
        }

        /// <summary>
        /// Adds the reevaluation system to the before-traversal group.
        /// </summary>
        /// <param name="world">The ECS world.</param>
        /// <param name="beforeTraversalSystemGroup">The before-traversal group.</param>
        public static void AddReevaluateSystem(World world, ComponentSystemGroup beforeTraversalSystemGroup)
        {
            if (!EnsureController()) {
                return;
            }

            s_Controller.AddReevaluateSystem(world, beforeTraversalSystemGroup);
        }

        /// <summary>
        /// Adds the TaskObject system to the traversal task group.
        /// </summary>
        /// <param name="world">The ECS world.</param>
        /// <param name="traversalTaskSystemGroup">The traversal task group.</param>
        public static void AddTaskObjectSystem(World world, ComponentSystemGroup traversalTaskSystemGroup)
        {
            if (!EnsureController()) {
                return;
            }

            s_Controller.AddTaskObjectSystem(world, traversalTaskSystemGroup);
        }

        /// <summary>
        /// Returns the TaskObject reevaluation system type.
        /// </summary>
        public static Type TaskObjectReevaluateSystemType
        {
            get
            {
                return EnsureController() ? s_Controller.TaskObjectReevaluateSystemType : null;
            }
        }

        /// <summary>
        /// Completes one traversal loop and returns true if another evaluation pass should run.
        /// </summary>
        /// <param name="world">The ECS world.</param>
        /// <param name="entityManager">The entity manager.</param>
        /// <returns>True if the traversal group should evaluate again.</returns>
        public static bool CompleteTraversal(World world, EntityManager entityManager)
        {
            return EnsureController() && s_Controller.CompleteTraversal(world, entityManager);
        }

        /// <summary>
        /// Completes the evaluation system.
        /// </summary>
        /// <param name="world">The ECS world.</param>
        /// <param name="entityManager">The entity manager.</param>
        /// <param name="stopRunning">Is the traversal group stopping?</param>
        public static void CompleteEvaluation(World world, EntityManager entityManager, bool stopRunning)
        {
            if (!EnsureController()) {
                return;
            }

            s_Controller.CompleteEvaluation(world, entityManager, stopRunning);
        }

        /// <summary>
        /// Returns true if any behavior tree is still active.
        /// </summary>
        /// <param name="world">The ECS world.</param>
        /// <param name="entityManager">The entity manager.</param>
        /// <returns>True if any behavior tree is still active.</returns>
        public static bool IsActive(World world, EntityManager entityManager)
        {
            return EnsureController() && s_Controller.IsActive(world, entityManager);
        }

        /// <summary>
        /// Ensures the execution controller is available.
        /// </summary>
        /// <returns>True if the controller is available.</returns>
        private static bool EnsureController()
        {
            if (s_Controller != null) {
                return true;
            }
            if (s_ControllerResolveFailed) {
                return false;
            }

            var controllerType = Type.GetType(c_ControllerTypeName);
            if (controllerType == null) {
                Debug.LogError("Behavior Designer could not find the Runtime.Systems assembly. The package must include Opsive.BehaviorDesigner.Runtime.Systems for ECS behavior tree execution.");
                s_ControllerResolveFailed = true;
                return false;
            }

            s_Controller = Activator.CreateInstance(controllerType) as IBehaviorTreeExecutionController;
            if (s_Controller != null) {
                return true;
            }

            Debug.LogError($"Behavior Designer could not create the execution controller type {c_ControllerTypeName}.");
            s_ControllerResolveFailed = true;
            return false;
        }
    }
}
#endif
