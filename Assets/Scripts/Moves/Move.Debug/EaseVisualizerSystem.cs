using Drawing;
using Moves.Move.Data.Blobs;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Moves.Move.Debug
{
    /// <summary>
    /// Visualizes the paths of entities controlled by the Ease/Move system.
    /// This system runs in the editor and uses the Aline drawing library to draw paths,
    /// rotation arcs, and scale changes for entities with an EaseStateComponent.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.Editor | WorldSystemFilterFlags.Default)]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(MoveSystem))]
    public partial struct EaseVisualizerSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
#if ALINE && UNITY_EDITOR
            // Get the Aline command builder for drawing.
            var builder = DrawingManager.GetBuilder();

            // The job is now simpler, as it gets all data from the component.
            var job = new EaseVisualizerJob
            {
                Drawing = builder
            };
            state.Dependency = job.ScheduleParallel(state.Dependency);

            // Ensure the builder is disposed after the job completes.
            builder.DisposeAfter(state.Dependency);
#endif
        }
    }

    public partial struct EaseVisualizerJob : IJobEntity
    {
        private const int Segment = 5;
        private const int LoopLimit = 16;

        [NativeDisableParallelForRestriction] public CommandBuilder Drawing;

        // Define colors for the visualization to make it clear and beautiful.
        private static readonly Color StartColor = new Color(0.2f, 1f, 0.2f, 0.8f); // Bright Green
        private static readonly Color EndColor = new Color(1f, 0.2f, 0.2f, 0.8f); // Bright Red
        private static readonly Color MidColor = new Color(1f, 1f, 0.2f, 0.8f); // Bright Yellow
        private static readonly Color PathColor = new Color(0.5f, 0.7f, 1f, 0.6f); // Light Blue

        private const int PathResolution = 30; // Number of segments to draw for the path.

        private void Execute(
            in BlobEaseCacheComponent blobComponent,
            in EaseStateComponent easeState,
            in LocalTransform transform
        )
        {
            // Get the blob asset reference from the component.
            ref var blob = ref blobComponent.Blob.Value;
            ref var easeCacheArray = ref blob.Cache;

            var current = easeState.Current;
            if (current >= easeCacheArray.Length) return;


            int limit = 10;
            var end = current;
            while (limit != LoopLimit)
            {
                var currentConfig = easeCacheArray[current];
                var nextIndex = currentConfig.Next;

                // Check the flags and call the appropriate visualization method.
                byte leading3Bit = currentConfig.Ease.Leading3Bit();
                if ((leading3Bit & 0b001) != 0)
                    VisualizePositionPath(ref blob, current, nextIndex, currentConfig);
                if ((leading3Bit & 0b010) != 0)
                    VisualizeRotationPath(ref blob, transform.Position, current, nextIndex, currentConfig);
                if ((leading3Bit & 0b100) != 0)
                    VisualizeScalePath(ref blob, transform.Position, current, nextIndex, currentConfig);
                current = nextIndex;
                if (end == current) break;
                limit++;
            }
        }

        private void VisualizePositionPath(ref EaseCacheBlob blob, byte currentIndex, byte nextIndex,
            in EaseCache currentConfig)
        {
            // Safety check for array bounds.
            if (currentIndex >= blob.Positions.Length || nextIndex >= blob.Positions.Length) return;

            // Read from the unified blob.
            float3 startPos = blob.Positions[currentIndex];
            float3 endPos = blob.Positions[nextIndex];

            // Draw markers for start and end points
            Drawing.WireSphere(startPos, 0.1f, StartColor);
            Drawing.WireSphere(endPos, 0.1f, EndColor);

            // Draw 4 intermediate points to show the easing curve shape.
            for (int i = 1; i <= 4; i++)
            {
                float progress = i / 5f;
                currentConfig.Ease.Evaluate(progress, out var t);
                var point = math.lerp(startPos, endPos, t);
                Drawing.WireSphere(point, 0.07f, MidColor);
            }

            // Draw the eased path
            float3 prevPoint = startPos;
            for (int i = 1; i <= PathResolution; i++)
            {
                float progress = (float)i / PathResolution;
                currentConfig.Ease.Evaluate(progress, out var easedT);
                float3 currentPoint = math.lerp(startPos, endPos, easedT);
                Drawing.Line(prevPoint, currentPoint, PathColor);
                prevPoint = currentPoint;
            }
        }

        private void VisualizeRotationPath(ref EaseCacheBlob blob, float3 anchorPos, byte currentIndex, byte nextIndex,
            in EaseCache currentConfig)
        {
            // Safety check for array bounds.
            if (currentIndex >= blob.Quaternion.Length || nextIndex >= blob.Quaternion.Length) return;

            // Read quaternions from the unified blob.
            var startRot = blob.Quaternion[currentIndex];
            var endRot = blob.Quaternion[nextIndex];

            const float visualRadius = 1.0f;
            var forwardVector = new float3(0, 0, visualRadius);

            using (Drawing.WithLineWidth(2f))
            {
                // Draw start and end direction vectors
                var startDir = math.mul(startRot, forwardVector);
                var endDir = math.mul(endRot, forwardVector);
                Drawing.Arrow(anchorPos, anchorPos + startDir, StartColor);
                Drawing.Arrow(anchorPos, anchorPos + endDir, EndColor);

                // Draw 4 intermediate points on the arc to show the easing curve shape.
                for (int i = 1; i <= Segment - 1; i++)
                {
                    float progress = i / (float)Segment;
                    currentConfig.Ease.Evaluate(progress, out var t);
                    var rot = math.slerp(startRot, endRot, t);
                    var dir = math.mul(rot, forwardVector);
                    Drawing.WireSphere(anchorPos + dir, 0.05f, MidColor);
                }

                // Draw the eased arc of rotation
                float3 prevPoint = anchorPos + startDir;
                for (int i = 1; i <= PathResolution; i++)
                {
                    float progress = (float)i / PathResolution;
                    currentConfig.Ease.Evaluate(progress, out var easedT);
                    var currentRot = math.slerp(startRot, endRot, easedT);
                    var currentDir = math.mul(currentRot, forwardVector);
                    float3 currentPoint = anchorPos + currentDir;
                    Drawing.Line(prevPoint, currentPoint, PathColor);
                    prevPoint = currentPoint;
                }
            }
        }

        private void VisualizeScalePath(ref EaseCacheBlob blob, float3 anchorPos, byte currentIndex, byte nextIndex,
            in EaseCache currentConfig)
        {
            // Safety check for array bounds.
            if (currentIndex >= blob.Scale.Length || nextIndex >= blob.Scale.Length) return;

            // Read scales from the unified blob.
            var startScale = blob.Scale[currentIndex];
            var endScale = blob.Scale[nextIndex];

            // Draw boxes representing start and end scales
            Drawing.WireBox(anchorPos, new float3(startScale), StartColor);
            Drawing.WireBox(anchorPos, new float3(endScale), EndColor);

            // Draw 4 intermediate wire boxes to show the scale easing.
            for (int i = 1; i <= Segment - 1; i++)
            {
                float progress = i / (float)Segment;
                currentConfig.Ease.Evaluate(progress, out var t);
                var scale = math.lerp(startScale, endScale, t);
                Drawing.WireBox(anchorPos, new float3(scale), MidColor);
            }

            // Draw a line from start to end scale origin to indicate connection
            Drawing.Line(anchorPos - new float3(0, startScale * 0.5f, 0), anchorPos - new float3(0, endScale * 0.5f, 0),
                PathColor);
        }
    }
}