using Drawing;
using Moves.Move.Data.Blobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;
using UnityEngine;

namespace Moves.Move.Debug
{

    [WorldSystemFilter(WorldSystemFilterFlags.Editor | WorldSystemFilterFlags.Default)]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(MoveSystem))]
    public partial struct EaseVisualizerSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
#if ALINE && UNITY_EDITOR
            // Rule 5.2.1: Query for all entities that have the components needed for visualization.
            var query = SystemAPI.QueryBuilder().WithAll<EaseStateComponent, LocalTransform, BlobEaseCacheComponent>()
                .Build();
            if (query.IsEmpty)
            {
                return;
            }

            var builder = DrawingManager.GetBuilder();

            var job = new EaseVisualizerJob
            {
                Drawing = builder,
                WorldRenderBoundsLookup = SystemAPI.GetComponentLookup<WorldRenderBounds>(true)
            };

            state.Dependency = job.ScheduleParallel(query, state.Dependency);
            builder.DisposeAfter(state.Dependency);
#endif
        }

        public void OnDestroy(ref SystemState state)
        {
        }
    }

    public partial struct EaseVisualizerJob : IJobEntity
    {
        [NativeDisableParallelForRestriction] public CommandBuilder Drawing;
        [ReadOnly] public ComponentLookup<WorldRenderBounds> WorldRenderBoundsLookup;

        // Define colors for the visualization to make it clear and beautiful.
        private static readonly Color StartColor = new Color(0.2f, 1f, 0.2f, 0.8f); // Bright Green
        private static readonly Color EndColor = new Color(1f, 0.2f, 0.2f, 0.8f); // Bright Red
        private static readonly Color RotationColor = new Color(0.2f, 0.2f, 1f, 0.8f); // Blue
        private static readonly Color ScaleColor = new Color(1f, 0.8f, 0.2f, 0.8f); // Orange

        private const int PathResolution = 30;
        private const int IntermediatePlots = 4;

        private void Execute(
            Entity entity,
            in BlobEaseCacheComponent blobComponent,
            in EaseStateComponent easeState,
            in LocalTransform transform)
        {
            ref var blob = ref blobComponent.Blob.Value;
            if (blob.Cache.Length == 0) return;

            // Get anchor position if available
            float3 anchorPos = transform.Position;

            // Get world render bounds if available
            WorldRenderBounds? bounds = null;
            if (WorldRenderBoundsLookup.HasComponent(entity))
            {
                bounds = WorldRenderBoundsLookup[entity];
            }

            // --- Pass 1: Count the path length to calculate progress ---
            int pathLength = 0;
            byte tracer = easeState.Current;
            // Use a safe upper limit to prevent infinite loops in case of malformed data
            for (int i = 0; i < 256; i++)
            {
                pathLength++;
                if (tracer >= blob.Cache.Length) return; // Invalid index, stop processing.
                byte next = blob.Cache[tracer].Next;
                if (next == tracer) break; // Path ends here (points to itself).
                tracer = next;
                if (tracer == easeState.Current) break; // Cycled back to start.
            }

            if (pathLength == 0) return;

            // Calculate appropriate scale for gizmos based on bounds
            float gizmoScale = 1.0f;
            if (bounds.HasValue)
            {
                var size = bounds.Value.Value.Size;
                gizmoScale = math.max(size.x, math.max(size.y, size.z));
                gizmoScale = math.max(gizmoScale, 0.5f); // Minimum scale
            }

            // --- Pass 2: Draw the path with gradient colors and sizes ---
            tracer = easeState.Current;
            var rotationDrawn = false;
            var scaleDrawn = false;
            
            for (int i = 0; i < pathLength; i++)
            {
                if (tracer >= blob.Cache.Length) return; // Invalid index

                var currentConfig = blob.Cache[tracer];
                var nextIndex = currentConfig.Next;

                // Calculate progress for this segment
                float startProgress = (float)i / pathLength;
                float endProgress = (float)(i + 1) / pathLength;

                // Interpolate color and size based on progress
                var segmentStartColor = Color.Lerp(StartColor, EndColor, startProgress);
                var segmentEndColor = Color.Lerp(StartColor, EndColor, endProgress);
                float startSize = math.lerp(0.12f, 0.06f, startProgress);
                float endSize = math.lerp(0.12f, 0.06f, endProgress);

                // Check the flags and call the appropriate visualization method.
                byte leading3Bit = currentConfig.Ease.Leading3Bit();

                if ((leading3Bit & 0b001) != 0)
                    VisualizePositionPath(
                        ref blob,
                        tracer,
                        nextIndex,
                        currentConfig,
                        segmentStartColor,
                        segmentEndColor,
                        startSize,
                        endSize
                    );
                
                // Only draw rotation gizmo once at the current position
                if ((leading3Bit & 0b010) != 0 && !rotationDrawn)
                {
                    VisualizeRotationGizmo(
                        ref blob, 
                        anchorPos,
                        tracer,
                        nextIndex,
                        currentConfig,
                        gizmoScale
                    );
                    rotationDrawn = true;
                }

                // Only draw scale gizmo once at the current position
                if ((leading3Bit & 0b100) != 0 && !scaleDrawn)
                {
                    VisualizeScaleGizmo(
                        ref blob,
                        anchorPos,
                        tracer,
                        nextIndex,
                        currentConfig,
                        gizmoScale,
                        bounds
                    );
                    scaleDrawn = true;
                }

                if (nextIndex == tracer) break;
                tracer = nextIndex;
            }
        }

        private void VisualizePositionPath(ref EaseCacheBlob blob, byte currentIndex, byte nextIndex,
            in EaseCache currentConfig, Color startColor, Color endColor, float startSize, float endSize)
        {
            if (currentIndex >= blob.Positions.Length || nextIndex >= blob.Positions.Length) return;

            float3 startPos = blob.Positions[currentIndex];
            float3 endPos = blob.Positions[nextIndex];

            Drawing.WireSphere(startPos, startSize, startColor);
            Drawing.WireSphere(endPos, endSize, endColor);

            // Draw intermediate points with interpolated color
            for (int i = 1; i <= IntermediatePlots; i++)
            {
                float progress = (float)i / (IntermediatePlots + 1);
                currentConfig.Ease.Evaluate(progress, out var t);
                var point = math.lerp(startPos, endPos, t);
                var pointColor = Color.Lerp(startColor, endColor, progress);
                Drawing.WireSphere(point, 0.07f, pointColor);
            }

            // Draw the path line with a gradient
            float3 prevPoint = startPos;
            for (int i = 1; i <= PathResolution; i++)
            {
                float progress = (float)i / PathResolution;
                currentConfig.Ease.Evaluate(progress, out var easedT);
                float3 currentPoint = math.lerp(startPos, endPos, easedT);
                Drawing.Line(prevPoint, currentPoint, Color.Lerp(startColor, endColor, progress));
                prevPoint = currentPoint;
            }
        }

        private void VisualizeRotationGizmo(ref EaseCacheBlob blob, float3 anchorPos, byte currentIndex, byte nextIndex,
            in EaseCache currentConfig, float gizmoScale)
        {
            if (currentIndex >= blob.Quaternion.Length || nextIndex >= blob.Quaternion.Length) return;

            var startRot = blob.Quaternion[currentIndex];
            var endRot = blob.Quaternion[nextIndex];

            float visualRadius = gizmoScale * 0.8f;
            var forwardVector = new float3(0, 0, visualRadius);
            var rightVector = new float3(visualRadius, 0, 0);
            var upVector = new float3(0, visualRadius, 0);

            using (Drawing.WithLineWidth(2f))
            {
                // Draw start rotation axes
                var startForward = math.mul(startRot, forwardVector);
                var startRight = math.mul(startRot, rightVector);
                var startUp = math.mul(startRot, upVector);
                
                Drawing.Arrow(anchorPos, anchorPos + startForward, new Color(RotationColor.r, RotationColor.g, RotationColor.b, 0.6f));
                Drawing.Arrow(anchorPos, anchorPos + startRight, new Color(1f, 0.2f, 0.2f, 0.6f));
                Drawing.Arrow(anchorPos, anchorPos + startUp, new Color(0.2f, 1f, 0.2f, 0.6f));

                // Draw end rotation axes with dashed lines
                var endForward = math.mul(endRot, forwardVector);
                var endRight = math.mul(endRot, rightVector);
                var endUp = math.mul(endRot, upVector);
                
                DrawDashedLine(anchorPos, anchorPos + endForward, new Color(RotationColor.r, RotationColor.g, RotationColor.b, 0.4f));
                DrawDashedLine(anchorPos, anchorPos + endRight, new Color(1f, 0.2f, 0.2f, 0.4f));
                DrawDashedLine(anchorPos, anchorPos + endUp, new Color(0.2f, 1f, 0.2f, 0.4f));

                // Draw a simple arc to show rotation direction
                var rotationDiff = math.mul(math.conjugate(startRot), endRot);
                var angle = math.acos(math.abs(rotationDiff.value.w)) * 2f;
                if (angle > 0.1f) // Only draw if there's significant rotation
                {
                    DrawRotationArc(anchorPos, startRot, endRot, visualRadius * 0.5f, RotationColor);
                }
            }
        }

        private void VisualizeScaleGizmo(ref EaseCacheBlob blob, float3 anchorPos, byte currentIndex, byte nextIndex,
            in EaseCache currentConfig, float gizmoScale, WorldRenderBounds? bounds)
        {
            if (currentIndex >= blob.Scale.Length || nextIndex >= blob.Scale.Length) return;

            var startScale = blob.Scale[currentIndex];
            var endScale = blob.Scale[nextIndex];

            // Use bounds if available for better scale visualization
            float3 baseSize = new float3(gizmoScale * 0.3f);
            if (bounds.HasValue)
            {
                baseSize = bounds.Value.Value.Size * 0.5f;
            }

            // Draw start scale box
            Drawing.WireBox(anchorPos, baseSize * startScale, new Color(ScaleColor.r, ScaleColor.g, ScaleColor.b, 0.6f));
            
            // Draw end scale box with dashed lines
            DrawDashedBox(anchorPos, baseSize * endScale, new Color(ScaleColor.r, ScaleColor.g, ScaleColor.b, 0.4f));

            // Draw scale difference indicators
            var scaleDiff = endScale - startScale;
            if (math.abs(scaleDiff) > 0.01f)
            {
                var scaleDir = math.sign(scaleDiff);
                var indicatorSize = math.abs(scaleDiff) * 0.1f;
                var indicatorColor = scaleDiff > 0 ? new Color(0.2f, 1f, 0.2f, 0.8f) : new Color(1f, 0.2f, 0.2f, 0.8f);
                
                // Draw scale direction indicator
                Drawing.WireSphere(anchorPos + new float3(0, baseSize.y * startScale + indicatorSize, 0), 
                    indicatorSize, indicatorColor);
            }
        }

        private void DrawDashedLine(float3 start, float3 end, Color color)
        {
            const int segments = 8;
            const float dashRatio = 0.6f;
            
            for (int i = 0; i < segments; i++)
            {
                float t1 = (float)i / segments;
                float t2 = (float)(i + dashRatio) / segments;
                if (t2 > 1f) t2 = 1f;
                
                float3 segmentStart = math.lerp(start, end, t1);
                float3 segmentEnd = math.lerp(start, end, t2);
                Drawing.Line(segmentStart, segmentEnd, color);
            }
        }

        private void DrawDashedBox(float3 center, float3 size, Color color)
        {
            var halfSize = size * 0.5f;
            var corners = new float3[8]
            {
                center + new float3(-halfSize.x, -halfSize.y, -halfSize.z),
                center + new float3(halfSize.x, -halfSize.y, -halfSize.z),
                center + new float3(halfSize.x, halfSize.y, -halfSize.z),
                center + new float3(-halfSize.x, halfSize.y, -halfSize.z),
                center + new float3(-halfSize.x, -halfSize.y, halfSize.z),
                center + new float3(halfSize.x, -halfSize.y, halfSize.z),
                center + new float3(halfSize.x, halfSize.y, halfSize.z),
                center + new float3(-halfSize.x, halfSize.y, halfSize.z)
            };

            // Draw dashed edges
            var edges = new int[,]
            {
                {0,1}, {1,2}, {2,3}, {3,0}, // Bottom face
                {4,5}, {5,6}, {6,7}, {7,4}, // Top face
                {0,4}, {1,5}, {2,6}, {3,7}  // Vertical edges
            };

            for (int i = 0; i < edges.GetLength(0); i++)
            {
                DrawDashedLine(corners[edges[i,0]], corners[edges[i,1]], color);
            }
        }

        private void DrawRotationArc(float3 center, quaternion startRot, quaternion endRot, float radius, Color color)
        {
            const int arcSegments = 12;
            float3 prevPoint = center + math.mul(startRot, new float3(0, 0, radius));
            
            for (int i = 1; i <= arcSegments; i++)
            {
                float t = (float)i / arcSegments;
                var currentRot = math.slerp(startRot, endRot, t);
                float3 currentPoint = center + math.mul(currentRot, new float3(0, 0, radius));
                Drawing.Line(prevPoint, currentPoint, color);
                prevPoint = currentPoint;
            }
        }
    }
}