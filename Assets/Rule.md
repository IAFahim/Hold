~~~~## **0. General Principles & Philosophy**

These are the overarching principles that guide the architecture and style of the codebase.

*   **Rule 0.1: Performance by Default:** Prioritize performance in runtime code. Use `structs`, `unmanaged` types, Burst compilation, and the Job System wherever possible. Avoid managed allocations in systems and components.
*   **Rule 0.2: Explicit Separation of Concerns:** Strictly separate data, logic, and presentation.
    *   **Data:** `IComponentData` / `IBufferElementData` structs should contain only data, no logic.
    *   **Logic:** `ISystem` / `SystemBase` implementations contain all runtime logic.
    *   **Authoring:** `MonoBehaviour` classes are used exclusively for authoring data in the editor and baking it into entities.
    *   **Editor:** `Editor` / `PropertyDrawer` classes are for custom inspector UI only.
    *   **UI:** A clear Model-View-ViewModel (MVVM) pattern separates UI rendering from UI state and application logic.
*   **Rule 0.3: Extensibility through Data:** Design systems to be data-driven. Instead of hardcoding behaviors, create systems that react to data components. This makes the framework extensible without modifying core systems (e.g., `ReactionAuthoringAttribute`, `[ConfigVar]`).
*   **Rule 0.4: Automation Reduces Error:** Automate as much of the setup and configuration as possible. Use editor scripts (`[InitializeOnLoad]`), source generators, and custom attributes to configure the project, generate boilerplate, and validate data, minimizing manual setup for developers.
---

## **1. Project & Code Structure**

### **1.1. Assembly Organization**

*   **Rule 1.1.1: Modular Assemblies:** Structure the project into small, feature-specific assemblies (e.g., `BovineLabs.Reaction.Core`, `BovineLabs.Reaction.Actions`, `BovineLabs.Reaction.Timeline`).
*   **Rule 1.1.2: Suffix-Based Assembly Naming:** Use consistent suffixes to denote the purpose of an assembly.
    *   `.Core`: Core runtime data and systems.
    *   `.Authoring`: `MonoBehaviour` and `Baker` classes.
    *   `.Editor`: `CustomEditor` and `CustomPropertyDrawer` classes.
    *   `.Tests`: Unit and performance tests.
    *   `.Debug`: Debug-only systems and views.
*   **Rule 1.1.3: Strict Dependencies:** An assembly should only reference what it absolutely needs. For example, a `.Data` assembly should have minimal to no dependencies, while a `.System` assembly would reference the `.Data` assembly. The `.Editor` assembly references the `.Authoring` assembly, but not vice-versa.

### **1.2. File and Folder Organization**

*   **Rule 1.2.1: Group by Feature:** Within an assembly, group files by feature. For example, in `BovineLabs.Reaction.Timeline`, all position-related track files are in a `Position` sub-folder.
*   **Rule 1.2.2: Separate Data, Authoring, and Systems:** Within a feature folder, maintain a clear separation between data definitions (`.Data`), authoring components (`.Authoring`), and systems.

### **1.3. Access Modifiers & API Surface**

*   **Rule 1.3.1: Use `InternalsVisibleTo` for Controlled Access:** Instead of making types and methods `public` for use in an associated editor or test assembly, keep them `internal` and use the `[assembly: InternalsVisibleTo("MyProject.Editor")]` attribute. This keeps the public API clean and intentional.
    *   **Example:** The `BovineLabs.Reaction` assembly uses `[assembly: InternalsVisibleTo("BovineLabs.Reaction.Editor")]` to allow the editor assembly to access internal authoring components and data structures.

---

## **2. Naming Conventions**

*   **Rule 2.1: System Naming:** Systems should be named after their primary function and end with `System` (e.g., `InitializeTransformSystem`, `ActionCreateSystem`).
*   **Rule 2.2: Component Naming:**
    *   Data components should be named clearly based on their data (e.g., `ActionTimeline`, `PhysicsMassOverride`).
    *   Tag components (zero-sized `IComponentData`) should be named as statements of fact (e.g., `PositionMoveToStart`, `RotationLookAtStart`).
    *   Enableable components should follow the same naming as tags (e.g., `Active`, `SubSceneLoaded`).
*   **Rule 2.3: Authoring Component Naming:** Authoring `MonoBehaviour`s should be named identically to the runtime component they are baking, but with an `Authoring` suffix (e.g., `ActionTimelineAuthoring`).
*   **Rule 2.4: Private Fields:** Private fields should be `camelCase` (e.g., `initialTime`, `deactivate`). Underscore prefixes (`_`) are not used.

---

## **3. Coding Style & Formatting**

*   **Rule 3.1: Namespace Usage:** All code must reside within a namespace. `using` directives should be placed inside the namespace block to avoid polluting the global scope.
*   **Rule 3.2: `this` Keyword:** Avoid using the `this` keyword unless necessary to disambiguate between a field and a parameter.

---

## **4. C# Language Features & Best Practices**

*   **Rule 4.1: Use `readonly` where possible:** Mark fields that are only set in the constructor as `readonly` to ensure immutability.
*   **Rule 4.2: Prefer `struct` for Data:** Use `struct` for all `IComponentData` and `IBufferElementData` to align with DOTS principles and avoid heap allocations.
*   **Rule 4.3: Nullable Reference Types:** Use nullable reference types (`?`) to explicitly declare intent for variables that can be null.

---

## **5. Unity & ECS Best Practices**

### **5.1. Components & Data (`IComponentData`, `IBufferElementData`)**

*   **Rule 5.1.1: Pure Data:** Components must only contain data. All logic should reside in systems.
*   **Rule 5.1.2: Use `unmanaged` Types:** All component and buffer data should be `unmanaged` to allow for Burst compilation and direct memory access.
*   **Rule 5.1.3: Use `FixedString`:** For short, fixed-length strings in components, use `FixedString` variants (`FixedString32Bytes`, etc.) instead of `string` to avoid managed memory.
*   **Rule 5.1.4: Use `IEnableableComponent` for State Toggling:** For components that represent a boolean state (e.g., Active, Loaded), implement `IEnableableComponent`. This is more efficient than adding/removing components, as it avoids structural changes.

### **5.2. Systems (`ISystem`, `SystemBase`)**

*   **Rule 5.2.1: Prefer `ISystem`:** Use `ISystem` over `SystemBase` for runtime systems to ensure they are Burst-compilable and avoid managed allocations. Use `SystemBase` only when managed functionality is absolutely required (e.g., `Object.FindObjectsByType`).
*   **Rule 5.2.2: Explicit System Ordering with Groups:** Define custom `ComponentSystemGroup`s to manage the execution order of related systems. This creates a clear, maintainable update flow.
    *   **Example:** The codebase defines `ActiveEnabledSystemGroup`, `ActiveDisabledSystemGroup`, `ConditionsSystemGroup`, etc., to ensure actions are executed in a predictable order relative to state changes.
*   **Rule 5.2.3: Use `[UpdateInGroup]`, `[UpdateBefore]`, `[UpdateAfter]`:** Use these attributes to explicitly declare system dependencies and ordering.
*   **Rule 5.2.4: Use `[WorldSystemFilter]`:** Explicitly declare which worlds a system should run in. This is crucial for projects with client, server, and service worlds.
*   **Rule 5.2.5: Share State via Singleton Components:** To share state between systems (especially `ISystem`s), create a singleton entity with a component holding the shared data. A common pattern is to attach this component to the system's own entity (`state.SystemHandle`).
    *   **Example:** `ActionTagSystem` creates a `Singleton` component on its own handle to share the `NativeHashMap` of applied tags with `ActionTagDeactivatedSystem`.

### **5.3. Authoring & Baking (`MonoBehaviour`, `Baker<T>`)**

*   **Rule 5.3.1: Isolate Baking Logic:** All logic for converting `MonoBehaviour` data to ECS data must be contained within a nested `Baker<T>` class.
*   **Rule 5.3.2: Use `DependsOn`:** Call `baker.DependsOn(someObject)` for any `ScriptableObject` or `GameObject` asset that your baking process reads from. This ensures the baker re-runs if the dependency changes.
*   **Rule 5.3.3: Abstract Baking Logic with `IEntityCommands`:** For complex component setups, create a builder struct that accepts an `IEntityCommands` interface. This allows the same setup logic to be used by `BakerCommands`, `CommandBufferCommands`, and `EntityManagerCommands`, promoting code reuse.
    *   **Example:** `ConditionBuilder` takes a `ref T builder where T : struct, IEntityCommands` and can be used during baking or at runtime.

### **5.4. Custom Collections & Data Structures**

*   **Rule 5.4.1: Use `DynamicBuffer` for Dynamic Collections:** When an entity needs a collection of data, use a `DynamicBuffer<T>`.
*   **Rule 5.4.2: Implement `IDynamicHashMap` for Key-Value Storage:** For efficient key-value storage on an entity, create a struct that implements `IDynamicHashMap<TKey, TValue>` and reinterpret a `DynamicBuffer` to use it. This provides a high-performance, Burst-compatible dictionary on a per-entity basis.
    *   **Example:** `ConditionEvent` implements `IDynamicHashMap<ConditionKey, int>` and is used via `buffer.AsMap()`.
*   **Rule 5.4.3: Use `PooledNativeList` for Temporary Job Collections:** For temporary lists inside jobs, use `PooledNativeList<T>.Make()` within a `using` block to avoid allocations. It is thread-safe and highly performant.

### **5.5. Entity Commands & ECBs**

*   **Rule 5.5.1: Use `EntityCommandBuffer` for Structural Changes in Jobs:** When creating, destroying, or adding/removing components from entities inside a job, use an `EntityCommandBuffer`.
*   **Rule 5.5.2: Use Custom `IEntityCommands` Abstraction:** For reusable logic that performs structural changes, create a helper method that accepts an `IEntityCommands` struct. This allows the same logic to be used with an `EntityManager` on the main thread, an `EntityCommandBuffer` in a job, or an `IBaker` during baking.
    *   **Example:** The `Action...Builder` structs all use an `IEntityCommands` parameter in their `ApplyTo` method.

### **5.6. Performance & Optimization**

*   **Rule 5.6.1: Use `[BurstCompile]` Everywhere Possible:** Apply `[BurstCompile]` to all systems and jobs unless they explicitly require managed code.
*   **Rule 5.6.2: Use `[ReadOnly]` and `[WriteOnly]`:** Correctly attribute all native containers and component lookups in jobs to allow the safety system and job scheduler to maximize parallelism.
*   **Rule 5.6.3: Use Atomic Operations for Parallel Writes:** When multiple threads need to write to the same shared data (e.g., a bitmask), use atomic operations like `Interlocked.Or` or `Interlocked.And`. This requires the `UNITY_BURST_EXPERIMENTAL_ATOMIC_INTRINSICS` define.
    *   **Example:** `ReactionUtil.WriteState` uses `Common.InterlockedOr` to safely set bits in a `ConditionActive` component from a parallel job.
*   **Rule 5.6.4: Direct Chunk & Bitmask Manipulation for Extreme Performance:** For systems that are critical bottlenecks, bypass `IJobEntity` and use `IJobChunk` to operate directly on chunk data and enabled masks. This is an advanced technique that offers the highest performance.
    *   **Example:** `ActiveSystem.IsActiveJob` directly reads and writes the `EnabledMask` bits for multiple components to calculate the final `Active` state with minimal overhead.
*   **Rule 5.6.5: Use `Hint.Likely` and `Hint.Unlikely`:** In performance-critical, Burst-compiled code, use these attributes to guide the compiler's branch prediction.
    *   **Example:** `InitializeTransformSystem` uses `Hint.Likely` and `Hint.Unlikely` when checking for component existence.

---

## **6. Editor & Tooling**

### **6.1. Custom Inspectors (`CustomPropertyDrawer`, `CustomEditor`)**

*   **Rule 6.1.1: Cache UI Elements and Properties:** In custom UIElements inspectors, use a nested `Cache` class to store references to `VisualElement`s and `SerializedProperty`s. Populate the cache in `CreateElement` or `CreatePropertyGUI` to avoid expensive queries in callbacks or update loops.
    *   **Example:** `ConditionAuthoringConditionDataEditor` uses a `Cache` class to hold references to its `PropertyField`s.
*   **Rule 6.1.2: Create Dynamic and Reactive Inspectors:** Design inspectors that update their layout and visibility based on other field values. Use `RegisterValueChangeCallback` to trigger updates. This provides a much more intuitive user experience.
*   **Rule 6.1.3: Proactively Validate and Correct User Input:** Do not wait for runtime errors. If a user enters an invalid state (e.g., `max < min`), automatically correct it within the inspector code.
*   **Rule 6.1.4: Use `[PrefabElement]` for Prefab-Only Fields:** For fields that should only be edited on the prefab asset and not on scene instances, use the custom `[PrefabElement]` attribute and `PrefabElementProperty` drawer. This prevents accidental overrides on instances.

### **6.2. Source Generation & Reflection**

*   **Rule 6.2.1: Use Reflection for Extensible Editor Tools:** Build editor tools that use reflection (e.g., `GetAllWithAttribute<T>`) to discover relevant types. This allows new features to be integrated simply by adding an attribute, without changing the tool's code.
    *   **Example:** `ReactionAuthoringEditor` finds all valid reaction components by searching for the `[ReactionAuthoring]` attribute.
*   **Rule 6.2.2: Use Source Generators to Eliminate Boilerplate:** For patterns that require repetitive boilerplate code (like property wrappers for data binding), implement a source generator.
    *   **Example:** `SystemPropertyGenerator` creates property wrappers for `SystemObservableObject` data structs, automating the `SetProperty` calls needed for UI notifications.

### **6.3. Configurability & Modularity**

*   **Rule 6.3.1: Automate Project Configuration:** Use `[InitializeOnLoad]` editor scripts to automatically perform required project setup, such as adding scripting define symbols. This ensures a correct and consistent development environment for all team members.
    *   **Example:** `EnableAtomicIntrinsic` automatically adds `UNITY_BURST_EXPERIMENTAL_ATOMIC_INTRINSICS`.
*   **Rule 6.3.2: Use `[ConfigVar]` for Tweakable Settings:** Expose internal settings and debug flags through a `[ConfigVar]` attribute on a `SharedStatic<T>` field. This allows for easy tweaking in the editor or via command line without modifying code.
*   **Rule 6.3.3: Use ScriptableObjects for Settings:** Centralize configuration in `ScriptableObject` assets. This provides a robust, version-controllable, and editor-friendly way to manage game settings. The `SettingsBase` class is a great foundation for this.

---

## **7. UI (MVVM with AppUI)**

### **7.1. ViewModels & Views**

*   **Rule 7.1.1: Adhere to MVVM:** Strictly separate the View (UI layout and controls) from the ViewModel (UI state and commands). The View should be "dumb" and only know how to display data from the ViewModel.
*   **Rule 7.1.2: Use `[IsService]` for Dependency Injection:** Mark Views and ViewModels with `[IsService]` to have them managed by the `AnchorApp` dependency injection container.

### **7.2. Data Binding**

*   **Rule 7.2.1: Bridge ECS and UI with `SystemObservableObject<T>`:** To bind ECS data to the UI, use the `SystemObservableObject<T>` pattern. The generic parameter `T` must be an `unmanaged` struct that holds the ViewModel's state. This struct can be safely accessed and modified by Burst-compiled systems.
*   **Rule 7.2.2: Use `[SystemProperty]` for Bindable Fields:** Mark fields within the `unmanaged` `Data` struct with `[SystemProperty]`. The source generator will create the necessary public property wrapper in the ViewModel class, which calls `SetProperty` to notify the UI of changes.
*   **Rule 7.2.3: Encapsulate System-UI Interaction with Helpers:** Create helper structs (like `ToolbarHelper` or `UIHelper`) to manage the lifecycle of a UI panel that is driven by an ECS system. This helper should handle loading/unloading the view, binding the system to the ViewModel's data, and providing safe access to that data from a job.

---

## **8. Documentation & Comments**

*   **Rule 8.1: Use XML Documentation Comments:** All public and internal types, methods, and properties should have XML documentation comments (`<summary>`, `<param>`, `<returns>`).
*   **Rule 8.2: Use `<inheritdoc/>`:** For overrides and interface implementations, use `<inheritdoc/>` to avoid duplicating comments.
*   **Rule 8.3: Explain the "Why":** Comments should explain *why* the code is doing something, not just *what* it is doing, especially for complex or non-obvious logic.
    *   **Example:** The comments in `ActiveSystem.IsActiveJob` explain the purpose of the complex bitwise logic.


Always use a debugger to view the system Using Aline. Make it beautiful and to the point.
```csharp
using System;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;
using Unity.Burst;
using UnityEngine.Profiling;
using Unity.Collections;
using Unity.Jobs;

namespace Drawing {
using static DrawingData;
using static CommandBuilder;
using Drawing.Text;
using Unity.Profiling;
using System.Collections.Generic;
using UnityEngine.Rendering;

	static class GeometryBuilder {
		public struct CameraInfo {
			public float3 cameraPosition;
			public quaternion cameraRotation;
			public float2 cameraDepthToPixelSize;
			public bool cameraIsOrthographic;

			public CameraInfo(Camera camera) {
				var tr = camera?.transform;
				cameraPosition = tr != null ? (float3)tr.position : float3.zero;
				cameraRotation = tr != null ? (quaternion)tr.rotation : quaternion.identity;
				cameraDepthToPixelSize = (camera != null ? CameraDepthToPixelSize(camera) : 0);
				cameraIsOrthographic = camera != null ? camera.orthographic : false;
			}
		}

		internal static unsafe JobHandle Build (DrawingData gizmos, ProcessedBuilderData.MeshBuffers* buffers, ref CameraInfo cameraInfo, JobHandle dependency) {
			// Create a new builder and schedule it.
			// Why is characterInfo passed as a pointer and a length instead of just a NativeArray?
			// 	This is because passing it as a NativeArray invokes the safety system which adds some tracking to the NativeArray.
			//  This is normally not a problem, but we may be scheduling hundreds of jobs that use that particular NativeArray and this causes a bit of a slowdown
			//  in the safety checking system. Passing it as a pointer + length makes the whole scheduling code about twice as fast compared to passing it as a NativeArray.
			return new GeometryBuilderJob {
					   buffers = buffers,
					   currentMatrix = Matrix4x4.identity,
					   currentLineWidthData = new LineWidthData {
						   pixels = 1,
						   automaticJoins = false,
					   },
					   lineWidthMultiplier = DrawingManager.lineWidthMultiplier,
					   currentColor = (Color32)Color.white,
					   cameraPosition = cameraInfo.cameraPosition,
					   cameraRotation = cameraInfo.cameraRotation,
					   cameraDepthToPixelSize = cameraInfo.cameraDepthToPixelSize,
					   cameraIsOrthographic = cameraInfo.cameraIsOrthographic,
					   characterInfo = (SDFCharacter*)gizmos.fontData.characters.GetUnsafeReadOnlyPtr(),
					   characterInfoLength = gizmos.fontData.characters.Length,
					   maxPixelError = GeometryBuilderJob.MaxCirclePixelError / math.max(0.1f, gizmos.settingsRef.curveResolution),
			}.Schedule(dependency);
		}

		/// <summary>
		/// Helper for determining how large a pixel is at a given depth.
		/// A a distance D from the camera a pixel corresponds to roughly value.x * D + value.y world units.
		/// Where value is the return value from this function.
		/// </summary>
		private static float2 CameraDepthToPixelSize (Camera camera) {
			if (camera.orthographic) {
				return new float2(0.0f, 2.0f * camera.orthographicSize / camera.pixelHeight);
			} else {
				return new float2(Mathf.Tan(camera.fieldOfView * Mathf.Deg2Rad * 0.5f) / (0.5f * camera.pixelHeight), 0.0f);
			}
		}

		private static NativeArray<T> ConvertExistingDataToNativeArray<T>(UnsafeAppendBuffer data) where T : struct {
			unsafe {
				var arr = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(data.Ptr, data.Length / UnsafeUtility.SizeOf<T>(), Allocator.Invalid);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref arr, AtomicSafetyHandle.GetTempMemoryHandle());
#endif
return arr;
}
}

		internal static unsafe void BuildMesh (DrawingData gizmos, List<MeshWithType> meshes, ProcessedBuilderData.MeshBuffers* inputBuffers) {
			if (inputBuffers->triangles.Length > 0) {
				CommandBuilderSamplers.MarkerUpdateBuffer.Begin();
				var mesh = AssignMeshData<GeometryBuilderJob.Vertex>(gizmos, inputBuffers->bounds, inputBuffers->vertices, inputBuffers->triangles, MeshLayouts.MeshLayout);
				meshes.Add(new MeshWithType { mesh = mesh, type = MeshType.Lines });
				CommandBuilderSamplers.MarkerUpdateBuffer.End();
			}

			if (inputBuffers->solidTriangles.Length > 0) {
				var mesh = AssignMeshData<GeometryBuilderJob.Vertex>(gizmos, inputBuffers->bounds, inputBuffers->solidVertices, inputBuffers->solidTriangles, MeshLayouts.MeshLayout);
				meshes.Add(new MeshWithType { mesh = mesh, type = MeshType.Solid });
			}

			if (inputBuffers->textTriangles.Length > 0) {
				var mesh = AssignMeshData<GeometryBuilderJob.TextVertex>(gizmos, inputBuffers->bounds, inputBuffers->textVertices, inputBuffers->textTriangles, MeshLayouts.MeshLayoutText);
				meshes.Add(new MeshWithType { mesh = mesh, type = MeshType.Text });
			}
		}

		private static Mesh AssignMeshData<VertexType>(DrawingData gizmos, Bounds bounds, UnsafeAppendBuffer vertices, UnsafeAppendBuffer triangles, VertexAttributeDescriptor[] layout) where VertexType : struct {
			CommandBuilderSamplers.MarkerConvert.Begin();
			var verticesView = ConvertExistingDataToNativeArray<VertexType>(vertices);
			var trianglesView = ConvertExistingDataToNativeArray<int>(triangles);
			CommandBuilderSamplers.MarkerConvert.End();
			var mesh = gizmos.GetMesh(verticesView.Length);

			CommandBuilderSamplers.MarkerSetLayout.Begin();
			// Resize the vertex buffer if necessary
			// Note: also resized if the vertex buffer is significantly larger than necessary.
			//       This is because apparently when executing the command buffer Unity does something with the whole buffer for some reason (shows up as Mesh.CreateMesh in the profiler)
			// TODO: This could potentially cause bad behaviour if multiple meshes are used each frame and they have differing sizes.
			// We should query for meshes that already have an appropriately sized buffer.
			// if (mesh.vertexCount < verticesView.Length || mesh.vertexCount > verticesView.Length * 2) {

			// }
			// TODO: Use Mesh.GetVertexBuffer/Mesh.GetIndexBuffer once they stop being buggy.
			// Currently they don't seem to get refreshed properly after resizing them (2022.2.0b1)
			mesh.SetVertexBufferParams(math.ceilpow2(verticesView.Length), layout);
			mesh.SetIndexBufferParams(math.ceilpow2(trianglesView.Length), IndexFormat.UInt32);
			CommandBuilderSamplers.MarkerSetLayout.End();

			CommandBuilderSamplers.MarkerUpdateVertices.Begin();
			// Update the mesh data
			mesh.SetVertexBufferData(verticesView, 0, 0, verticesView.Length);
			CommandBuilderSamplers.MarkerUpdateVertices.End();
			CommandBuilderSamplers.MarkerUpdateIndices.Begin();
			// Update the index buffer and assume all our indices are correct
			mesh.SetIndexBufferData(trianglesView, 0, 0, trianglesView.Length, MeshUpdateFlags.DontValidateIndices);
			CommandBuilderSamplers.MarkerUpdateIndices.End();


			CommandBuilderSamplers.MarkerSubmesh.Begin();
			mesh.subMeshCount = 1;
			var submesh = new SubMeshDescriptor(0, trianglesView.Length, MeshTopology.Triangles) {
				vertexCount = verticesView.Length,
				bounds = bounds
			};
			mesh.SetSubMesh(0, submesh, MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontNotifyMeshUsers);
			mesh.bounds = bounds;
			CommandBuilderSamplers.MarkerSubmesh.End();
			return mesh;
		}
	}

	/// <summary>Some static fields that need to be in a separate class because Burst doesn't support them</summary>
	static class MeshLayouts {
		internal static readonly VertexAttributeDescriptor[] MeshLayout = {
			new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
			new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
			new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4),
			new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
		};

		internal static readonly VertexAttributeDescriptor[] MeshLayoutText = {
			new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
			new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4),
			new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
		};
	}

	/// <summary>
	/// Job to build the geometry from a stream of rendering commands.
	///
	/// See: <see cref="CommandBuilder"/>
	/// </summary>
	// Note: Setting FloatMode to Fast causes visual artificats when drawing circles.
	// I think it is because math.sin(float4) produces slightly different results
	// for each component in the input.
	[BurstCompile(FloatMode = FloatMode.Default)]
	internal struct GeometryBuilderJob : IJob {
		[NativeDisableUnsafePtrRestriction]
		public unsafe ProcessedBuilderData.MeshBuffers* buffers;

		[NativeDisableUnsafePtrRestriction]
		public unsafe SDFCharacter* characterInfo;
		public int characterInfoLength;

		public Color32 currentColor;
		public float4x4 currentMatrix;
		public LineWidthData currentLineWidthData;
		public float lineWidthMultiplier;
		float3 minBounds;
		float3 maxBounds;
		public float3 cameraPosition;
		public quaternion cameraRotation;
		public float2 cameraDepthToPixelSize;
		public float maxPixelError;
		public bool cameraIsOrthographic;

		[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
		public struct Vertex {
			public float3 position;
			public float3 uv2;
			public Color32 color;
			public float2 uv;
		}

		[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
		public struct TextVertex {
			public float3 position;
			public Color32 color;
			public float2 uv;
		}

		static unsafe void Add<T>(UnsafeAppendBuffer* buffer, T value) where T : unmanaged {
			int size = UnsafeUtility.SizeOf<T>();
			// We know that the buffer has enough capacity, so we can just write to the buffer without
			// having to add branches for the overflow case (like buffer->Add will do).
#if ENABLE_UNITY_COLLECTIONS_CHECKS
UnityEngine.Assertions.Assert.IsTrue(buffer->Length + size <= buffer->Capacity);
#endif
*(T*)(buffer->Ptr + buffer->Length) = value;
buffer->Length = buffer->Length + size;
}

		static unsafe void Reserve (UnsafeAppendBuffer* buffer, int size) {
			var newSize = buffer->Length + size;

			if (newSize > buffer->Capacity) {
				buffer->SetCapacity(math.max(newSize, buffer->Capacity * 2));
			}
		}

		internal static float3 PerspectiveDivide (float4 p) {
			return p.xyz * math.rcp(p.w);
		}

		unsafe void AddText (System.UInt16* text, TextData textData, Color32 color) {
			var pivot = PerspectiveDivide(math.mul(currentMatrix, new float4(textData.center, 1.0f)));

			AddTextInternal(
				text,
				pivot,
				math.mul(cameraRotation, new float3(1, 0, 0)),
				math.mul(cameraRotation, new float3(0, 1, 0)),
				textData.alignment,
				textData.sizeInPixels,
				true,
				textData.numCharacters,
				color
				);
		}

		unsafe void AddText3D (System.UInt16* text, TextData3D textData, Color32 color) {
			var pivot = PerspectiveDivide(math.mul(currentMatrix, new float4(textData.center, 1.0f)));
			var m = math.mul(currentMatrix, new float4x4(textData.rotation, float3.zero));

			AddTextInternal(
				text,
				pivot,
				m.c0.xyz,
				m.c1.xyz,
				textData.alignment,
				textData.size,
				false,
				textData.numCharacters,
				color
				);
		}


		unsafe void AddTextInternal (System.UInt16* text, float3 pivot, float3 right, float3 up, LabelAlignment alignment, float size, bool sizeIsInPixels, int numCharacters, Color32 color) {
			var distance = math.abs(math.dot(pivot - cameraPosition, math.mul(cameraRotation, new float3(0, 0, 1))));
			var pixelSize = cameraDepthToPixelSize.x * distance + cameraDepthToPixelSize.y;
			float fontWorldSize = size;

			if (sizeIsInPixels) fontWorldSize *= pixelSize;

			right *= fontWorldSize;
			up *= fontWorldSize;

			// Calculate the total width (in pixels divided by fontSize) of the text
			float maxWidth = 0;
			float currentWidth = 0;
			float numLines = 1;

			for (int i = 0; i < numCharacters; i++) {
				var characterInfoIndex = text[i];
				if (characterInfoIndex == SDFLookupData.Newline) {
					maxWidth = math.max(maxWidth, currentWidth);
					currentWidth = 0;
					numLines++;
				} else {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
if (characterInfoIndex >= characterInfoLength) throw new System.Exception("Invalid character. No info exists. This is a bug.");
#endif
currentWidth += characterInfo[characterInfoIndex].advance;
}
}
maxWidth = math.max(maxWidth, currentWidth);

			// Calculate the world space position of the text given the camera and text alignment
			var pos = pivot;
			pos -= right * maxWidth * alignment.relativePivot.x;
			// Size of a character as a fraction of a whole line using the current font
			const float FontCharacterFractionOfLine = 0.75f;
			// Where the upper and lower parts of the text will be assuming we start to write at y=0
			var lower = 1 - numLines;
			var upper = FontCharacterFractionOfLine;
			var yAdjustment = math.lerp(lower, upper, alignment.relativePivot.y);
			pos -= up * yAdjustment;
			pos += math.mul(cameraRotation, new float3(1, 0, 0)) * (pixelSize * alignment.pixelOffset.x);
			pos += math.mul(cameraRotation, new float3(0, 1, 0)) * (pixelSize * alignment.pixelOffset.y);

			var textVertices = &buffers->textVertices;
			var textTriangles = &buffers->textTriangles;

			// Reserve all buffer space beforehand
			Reserve(textVertices, numCharacters * VerticesPerCharacter * UnsafeUtility.SizeOf<TextVertex>());
			Reserve(textTriangles, numCharacters * TrianglesPerCharacter * UnsafeUtility.SizeOf<int>());

			var lineStart = pos;

			for (int i = 0; i < numCharacters; i++) {
				var characterInfoIndex = text[i];

				if (characterInfoIndex == SDFLookupData.Newline) {
					lineStart -= up;
					pos = lineStart;
					continue;
				}

				// Get character rendering information from the font
				SDFCharacter ch = characterInfo[characterInfoIndex];

				int vertexIndexStart = textVertices->Length / UnsafeUtility.SizeOf<TextVertex>();

				float3 v;

				v = pos + ch.vertexTopLeft.x * right + ch.vertexTopLeft.y * up;
				minBounds = math.min(minBounds, v);
				maxBounds = math.max(maxBounds, v);
				Add(textVertices, new TextVertex {
					position = v,
					uv = ch.uvTopLeft,
					color = color,
				});

				v = pos + ch.vertexTopRight.x * right + ch.vertexTopRight.y * up;
				minBounds = math.min(minBounds, v);
				maxBounds = math.max(maxBounds, v);
				Add(textVertices, new TextVertex {
					position = v,
					uv = ch.uvTopRight,
					color = color,
				});

				v = pos + ch.vertexBottomRight.x * right + ch.vertexBottomRight.y * up;
				minBounds = math.min(minBounds, v);
				maxBounds = math.max(maxBounds, v);
				Add(textVertices, new TextVertex {
					position = v,
					uv = ch.uvBottomRight,
					color = color,
				});

				v = pos + ch.vertexBottomLeft.x * right + ch.vertexBottomLeft.y * up;
				minBounds = math.min(minBounds, v);
				maxBounds = math.max(maxBounds, v);
				Add(textVertices, new TextVertex {
					position = v,
					uv = ch.uvBottomLeft,
					color = color,
				});

				Add(textTriangles, vertexIndexStart + 0);
				Add(textTriangles, vertexIndexStart + 1);
				Add(textTriangles, vertexIndexStart + 2);

				Add(textTriangles, vertexIndexStart + 0);
				Add(textTriangles, vertexIndexStart + 2);
				Add(textTriangles, vertexIndexStart + 3);

				// Advance character position
				pos += right * ch.advance;
			}
		}

		float3 lastNormalizedLineDir;
		float lastLineWidth;

		public const float MaxCirclePixelError = 0.5f;

		public const int VerticesPerCharacter = 4;
		public const int TrianglesPerCharacter = 6;

		void AddLine (LineData line) {
			// Store the line direction in the vertex.
			// A line consists of 4 vertices. The line direction will be used to
			// offset the vertices to create a line with a fixed pixel thickness
			var a = PerspectiveDivide(math.mul(currentMatrix, new float4(line.a, 1.0f)));
			var b = PerspectiveDivide(math.mul(currentMatrix, new float4(line.b, 1.0f)));

			float lineWidth = currentLineWidthData.pixels;
			var normalizedLineDir = math.normalizesafe(b - a);

			if (math.any(math.isnan(normalizedLineDir))) throw new Exception("Nan line coordinates");
			if (lineWidth <= 0) {
				return;
			}

			// Update the bounding box
			minBounds = math.min(minBounds, math.min(a, b));
			maxBounds = math.max(maxBounds, math.max(a, b));

			unsafe {
				var outlineVertices = &buffers->vertices;

				// Make sure there is enough allocated capacity for 4 more vertices
				Reserve(outlineVertices, 4 * UnsafeUtility.SizeOf<Vertex>());

				// Insert 4 vertices
				// Doing it with pointers is faster, and this is the hottest
				// code of the whole gizmo drawing process.
				var ptr = (Vertex*)((byte*)outlineVertices->Ptr + outlineVertices->Length);

				var startLineDir = normalizedLineDir * lineWidth;
				var endLineDir = normalizedLineDir * lineWidth;

				// If dot(last dir, this dir) >= 0 => use join
				if (lineWidth > 1 && currentLineWidthData.automaticJoins && outlineVertices->Length > 2*UnsafeUtility.SizeOf<Vertex>()) {
					// has previous vertex
					Vertex* lastVertex1 = (Vertex*)(ptr - 1);
					Vertex* lastVertex2 = (Vertex*)(ptr - 2);

					var cosAngle = math.dot(normalizedLineDir, lastNormalizedLineDir);
					if (math.all(lastVertex2->position == a) && lastLineWidth == lineWidth && cosAngle >= -0.6f) {
						// Safety: tangent cannot be 0 because cosAngle > -1
						var tangent = normalizedLineDir + lastNormalizedLineDir;
						// From the law of cosines we get that
						// tangent.magnitude = sqrt(2)*sqrt(1+cosAngle)

						// Create join!
						// Trigonometry gives us
						// joinRadius = lineWidth / (2*cos(alpha / 2))
						// Using half angle identity for cos we get
						// joinRadius = lineWidth / (sqrt(2)*sqrt(1 + cos(alpha))
						// Since the tangent already has mostly the same factors we can simplify the calculation
						// normalize(tangent) * joinRadius * 2
						// = tangent / (sqrt(2)*sqrt(1+cosAngle)) * joinRadius * 2
						// = tangent * lineWidth / (1 + cos(alpha)
						var joinLineDir = tangent * lineWidth / (1 + cosAngle);

						startLineDir = joinLineDir;
						lastVertex1->uv2 = startLineDir;
						lastVertex2->uv2 = startLineDir;
					}
				}

				outlineVertices->Length = outlineVertices->Length + 4 * UnsafeUtility.SizeOf<Vertex>();
				*ptr++ = new Vertex {
					position = a,
					color = currentColor,
					uv = new float2(0, 0),
					uv2 = startLineDir,
				};
				*ptr++ = new Vertex {
					position = a,
					color = currentColor,
					uv = new float2(1, 0),
					uv2 = startLineDir,
				};

				*ptr++ = new Vertex {
					position = b,
					color = currentColor,
					uv = new float2(0, 1),
					uv2 = endLineDir,
				};
				*ptr++ = new Vertex {
					position = b,
					color = currentColor,
					uv = new float2(1, 1),
					uv2 = endLineDir,
				};

				lastNormalizedLineDir = normalizedLineDir;
				lastLineWidth = lineWidth;
			}
		}

		/// <summary>Calculate number of steps to use for drawing a circle at the specified point and radius to get less than the specified pixel error.</summary>
		internal static int CircleSteps (float3 center, float radius, float maxPixelError, ref float4x4 currentMatrix, float2 cameraDepthToPixelSize, float3 cameraPosition) {
			var centerv4 = math.mul(currentMatrix, new float4(center, 1.0f));

			if (math.abs(centerv4.w) < 0.0000001f) return 3;
			var cc = PerspectiveDivide(centerv4);
			// Take the maximum scale factor among the 3 axes.
			// If the current matrix has a uniform scale then they are all the same.
			var maxScaleFactor = math.sqrt(math.max(math.max(math.lengthsq(currentMatrix.c0.xyz), math.lengthsq(currentMatrix.c1.xyz)), math.lengthsq(currentMatrix.c2.xyz))) / centerv4.w;
			var realWorldRadius = radius * maxScaleFactor;
			var distance = math.length(cc - cameraPosition);

			var pixelSize = cameraDepthToPixelSize.x * distance + cameraDepthToPixelSize.y;
			// realWorldRadius += pixelSize * this.currentLineWidthData.pixels * 0.5f;
			var cosAngle = 1 - (maxPixelError * pixelSize) / realWorldRadius;
			int steps = cosAngle < 0 ? 3 : (int)math.ceil(math.PI / (math.acos(cosAngle)));
			return steps;
		}

		void AddCircle (CircleData circle) {
			// If the circle has a zero normal then just ignore it
			if (math.all(circle.normal == 0)) return;

			circle.normal = math.normalize(circle.normal);
			// Canonicalize
			if (circle.normal.y < 0) circle.normal = -circle.normal;

			float3 tangent1;
			if (math.all(math.abs(circle.normal - new float3(0, 1, 0)) < 0.001f)) {
				// The normal was (almost) identical to (0, 1, 0)
				tangent1 = new float3(0, 0, 1);
			} else {
				// Common case
				tangent1 = math.normalizesafe(math.cross(circle.normal, new float3(0, 1, 0)));
			}

			var ex = tangent1;
			var ey = circle.normal;
			var ez = math.cross(ey, ex);
			var oldMatrix = currentMatrix;

			currentMatrix = math.mul(currentMatrix, new float4x4(
				new float4(ex, 0) * circle.radius,
				new float4(ey, 0) * circle.radius,
				new float4(ez, 0) * circle.radius,
				new float4(circle.center, 1)
				));

			AddCircle(new CircleXZData {
				center = new float3(0, 0, 0),
				radius = 1,
				startAngle = 0,
				endAngle = 2 * math.PI,
			});

			currentMatrix = oldMatrix;
		}

		void AddDisc (CircleData circle) {
			// If the circle has a zero normal then just ignore it
			if (math.all(circle.normal == 0)) return;

			var steps = CircleSteps(circle.center, circle.radius, maxPixelError, ref currentMatrix, cameraDepthToPixelSize, cameraPosition);

			circle.normal = math.normalize(circle.normal);
			float3 tangent1;
			if (math.all(math.abs(circle.normal - new float3(0, 1, 0)) < 0.001f)) {
				// The normal was (almost) identical to (0, 1, 0)
				tangent1 = new float3(0, 0, 1);
			} else {
				// Common case
				tangent1 = math.cross(circle.normal, new float3(0, 1, 0));
			}

			float invSteps = 1.0f / steps;

			unsafe {
				var solidVertices = &buffers->solidVertices;
				var solidTriangles = &buffers->solidTriangles;
				Reserve(solidVertices, steps * UnsafeUtility.SizeOf<Vertex>());
				Reserve(solidTriangles, 3*(steps-2) * UnsafeUtility.SizeOf<int>());

				var matrix = math.mul(currentMatrix, Matrix4x4.TRS(circle.center, Quaternion.LookRotation(circle.normal, tangent1), new Vector3(circle.radius, circle.radius, circle.radius)));

				var mn = minBounds;
				var mx = maxBounds;
				int vertexCount = solidVertices->Length / UnsafeUtility.SizeOf<Vertex>();

				for (int i = 0; i < steps; i++) {
					var t = math.lerp(0, 2*Mathf.PI, i * invSteps);
					math.sincos(t, out float sin, out float cos);

					var p = PerspectiveDivide(math.mul(matrix, new float4(cos, sin, 0, 1)));
					// Update the bounding box
					mn = math.min(mn, p);
					mx = math.max(mx, p);

					Add(solidVertices, new Vertex {
						position = p,
						color = currentColor,
						uv = new float2(0, 0),
						uv2 = new float3(0, 0, 0),
					});
				}

				minBounds = mn;
				maxBounds = mx;

				for (int i = 0; i < steps - 2; i++) {
					Add(solidTriangles, vertexCount);
					Add(solidTriangles, vertexCount + i + 1);
					Add(solidTriangles, vertexCount + i + 2);
				}
			}
		}

		void AddSphereOutline (SphereData circle) {
			var centerv4 = math.mul(currentMatrix, new float4(circle.center, 1.0f));

			if (math.abs(centerv4.w) < 0.0000001f) return;
			var center = PerspectiveDivide(centerv4);
			// Figure out the actual radius of the sphere after all the matrix multiplications.
			// In case of a non-uniform scale, pick the largest radius
			var maxScaleFactor = math.sqrt(math.max(math.max(math.lengthsq(currentMatrix.c0.xyz), math.lengthsq(currentMatrix.c1.xyz)), math.lengthsq(currentMatrix.c2.xyz))) / centerv4.w;
			var realWorldRadius = circle.radius * maxScaleFactor;

			if (cameraIsOrthographic) {
				var prevMatrix = this.currentMatrix;
				this.currentMatrix = float4x4.identity;
				AddCircle(new CircleData {
					center = center,
					normal = math.mul(this.cameraRotation, new float3(0, 0, 1)),
					radius = realWorldRadius,
				});
				this.currentMatrix = prevMatrix;
			} else {
				var dist = math.length(this.cameraPosition - center);
				// Camera is inside the sphere, cannot draw
				if (dist <= realWorldRadius) return;

				var offsetTowardsCamera = realWorldRadius * realWorldRadius / dist;
				var outlineRadius = math.sqrt(realWorldRadius * realWorldRadius - offsetTowardsCamera * offsetTowardsCamera);
				var normal = math.normalize(this.cameraPosition - center);
				var prevMatrix = this.currentMatrix;
				this.currentMatrix = float4x4.identity;
				AddCircle(new CircleData {
					center = center + normal * offsetTowardsCamera,
					normal = normal,
					radius = outlineRadius,
				});
				this.currentMatrix = prevMatrix;
			}
		}

		void AddCircle (CircleXZData circle) {
			circle.endAngle = math.clamp(circle.endAngle, circle.startAngle - Mathf.PI * 2, circle.startAngle + Mathf.PI * 2);

			unsafe {
				var m = math.mul(currentMatrix, new float4x4(
					new float4(circle.radius, 0, 0, 0),
					new float4(0, circle.radius, 0, 0),
					new float4(0, 0, circle.radius, 0),
					new float4(circle.center, 1)
					));
				var steps = CircleSteps(float3.zero, 1.0f, maxPixelError, ref m, cameraDepthToPixelSize, cameraPosition);
				var lineWidth = currentLineWidthData.pixels;
				if (lineWidth < 0) return;

				var byteSize = steps * 4 * UnsafeUtility.SizeOf<Vertex>();
				Reserve(&buffers->vertices, byteSize);
				var ptr = (Vertex*)(buffers->vertices.Ptr + buffers->vertices.Length);
				buffers->vertices.Length += byteSize;
				math.sincos(circle.startAngle, out float sin0, out float cos0);
				var prev = PerspectiveDivide(math.mul(m, new float4(cos0, 0, sin0, 1)));
				var prevTangent = math.normalizesafe(math.mul(m, new float4(-sin0, 0, cos0, 0)).xyz) * lineWidth;
				var invSteps = math.rcp(steps);

				for (int i = 1; i <= steps; i++) {
					var t = math.lerp(circle.startAngle, circle.endAngle, i * invSteps);
					math.sincos(t, out float sin, out float cos);
					var next = PerspectiveDivide(math.mul(m, new float4(cos, 0, sin, 1)));
					var tangent = math.normalizesafe(math.mul(m, new float4(-sin, 0, cos, 0)).xyz) * lineWidth;
					*ptr++ = new Vertex {
						position = prev,
						color = currentColor,
						uv = new float2(0, 0),
						uv2 = prevTangent,
					};
					*ptr++ = new Vertex {
						position = prev,
						color = currentColor,
						uv = new float2(1, 0),
						uv2 = prevTangent,
					};
					*ptr++ = new Vertex {
						position = next,
						color = currentColor,
						uv = new float2(0, 1),
						uv2 = tangent,
					};
					*ptr++ = new Vertex {
						position = next,
						color = currentColor,
						uv = new float2(1, 1),
						uv2 = tangent,
					};

					prev = next;
					prevTangent = tangent;
				}

				// Update the global bounds with the bounding box of the circle
				var b0 = PerspectiveDivide(math.mul(m, new float4(-1, 0, 0, 1)));
				var b1 = PerspectiveDivide(math.mul(m, new float4(0, -1, 0, 1)));
				var b2 = PerspectiveDivide(math.mul(m, new float4(+1, 0, 0, 1)));
				var b3 = PerspectiveDivide(math.mul(m, new float4(0, +1, 0, 1)));
				minBounds = math.min(math.min(math.min(math.min(b0, b1), b2), b3), minBounds);
				maxBounds = math.max(math.max(math.max(math.max(b0, b1), b2), b3), maxBounds);
			}
		}

		void AddDisc (CircleXZData circle) {
			var steps = CircleSteps(circle.center, circle.radius, maxPixelError, ref currentMatrix, cameraDepthToPixelSize, cameraPosition);

			circle.endAngle = math.clamp(circle.endAngle, circle.startAngle - Mathf.PI * 2, circle.startAngle + Mathf.PI * 2);

			float invSteps = 1.0f / steps;

			unsafe {
				var solidVertices = &buffers->solidVertices;
				var solidTriangles = &buffers->solidTriangles;
				Reserve(solidVertices, (2+steps) * UnsafeUtility.SizeOf<Vertex>());
				Reserve(solidTriangles, 3*steps * UnsafeUtility.SizeOf<int>());

				var matrix = math.mul(currentMatrix, Matrix4x4.Translate(circle.center) * Matrix4x4.Scale(new Vector3(circle.radius, circle.radius, circle.radius)));

				var worldCenter = PerspectiveDivide(math.mul(matrix, new float4(0, 0, 0, 1)));
				Add(solidVertices, new Vertex {
					position = worldCenter,
					color = currentColor,
					uv = new float2(0, 0),
					uv2 = new float3(0, 0, 0),
				});

				var mn = math.min(minBounds, worldCenter);
				var mx = math.max(maxBounds, worldCenter);
				int vertexCount = solidVertices->Length / UnsafeUtility.SizeOf<Vertex>();

				for (int i = 0; i <= steps; i++) {
					var t = math.lerp(circle.startAngle, circle.endAngle, i * invSteps);
					math.sincos(t, out float sin, out float cos);

					var p = PerspectiveDivide(math.mul(matrix, new float4(cos, 0, sin, 1)));
					// Update the bounding box
					mn = math.min(mn, p);
					mx = math.max(mx, p);

					Add(solidVertices, new Vertex {
						position = p,
						color = currentColor,
						uv = new float2(0, 0),
						uv2 = new float3(0, 0, 0),
					});
				}

				minBounds = mn;
				maxBounds = mx;

				for (int i = 0; i < steps; i++) {
					// Center vertex
					Add(solidTriangles, vertexCount - 1);
					Add(solidTriangles, vertexCount + i + 0);
					Add(solidTriangles, vertexCount + i + 1);
				}
			}
		}

		void AddSolidTriangle (TriangleData triangle) {
			unsafe {
				var solidVertices = &buffers->solidVertices;
				var solidTriangles = &buffers->solidTriangles;
				Reserve(solidVertices, 3 * UnsafeUtility.SizeOf<Vertex>());
				Reserve(solidTriangles, 3 * UnsafeUtility.SizeOf<int>());
				var matrix = currentMatrix;
				var a = PerspectiveDivide(math.mul(matrix, new float4(triangle.a, 1)));
				var b = PerspectiveDivide(math.mul(matrix, new float4(triangle.b, 1)));
				var c = PerspectiveDivide(math.mul(matrix, new float4(triangle.c, 1)));
				int startVertex = solidVertices->Length / UnsafeUtility.SizeOf<Vertex>();

				minBounds = math.min(math.min(math.min(minBounds, a), b), c);
				maxBounds = math.max(math.max(math.max(maxBounds, a), b), c);

				Add(solidVertices, new Vertex {
					position = a,
					color = currentColor,
					uv = new float2(0, 0),
					uv2 = new float3(0, 0, 0),
				});
				Add(solidVertices, new Vertex {
					position = b,
					color = currentColor,
					uv = new float2(0, 0),
					uv2 = new float3(0, 0, 0),
				});
				Add(solidVertices, new Vertex {
					position = c,
					color = currentColor,
					uv = new float2(0, 0),
					uv2 = new float3(0, 0, 0),
				});

				Add(solidTriangles, startVertex + 0);
				Add(solidTriangles, startVertex + 1);
				Add(solidTriangles, startVertex + 2);
			}
		}

		void AddWireBox (BoxData box) {
			var min = box.center - box.size * 0.5f;
			var max = box.center + box.size * 0.5f;
			AddLine(new LineData { a = new float3(min.x, min.y, min.z), b = new float3(max.x, min.y, min.z) });
			AddLine(new LineData { a = new float3(max.x, min.y, min.z), b = new float3(max.x, min.y, max.z) });
			AddLine(new LineData { a = new float3(max.x, min.y, max.z), b = new float3(min.x, min.y, max.z) });
			AddLine(new LineData { a = new float3(min.x, min.y, max.z), b = new float3(min.x, min.y, min.z) });

			AddLine(new LineData { a = new float3(min.x, max.y, min.z), b = new float3(max.x, max.y, min.z) });
			AddLine(new LineData { a = new float3(max.x, max.y, min.z), b = new float3(max.x, max.y, max.z) });
			AddLine(new LineData { a = new float3(max.x, max.y, max.z), b = new float3(min.x, max.y, max.z) });
			AddLine(new LineData { a = new float3(min.x, max.y, max.z), b = new float3(min.x, max.y, min.z) });

			AddLine(new LineData { a = new float3(min.x, min.y, min.z), b = new float3(min.x, max.y, min.z) });
			AddLine(new LineData { a = new float3(max.x, min.y, min.z), b = new float3(max.x, max.y, min.z) });
			AddLine(new LineData { a = new float3(max.x, min.y, max.z), b = new float3(max.x, max.y, max.z) });
			AddLine(new LineData { a = new float3(min.x, min.y, max.z), b = new float3(min.x, max.y, max.z) });
		}

		void AddPlane (PlaneData plane) {
			var oldMatrix = currentMatrix;

			currentMatrix = math.mul(currentMatrix, float4x4.TRS(plane.center, plane.rotation, new float3(plane.size.x * 0.5f, 1, plane.size.y * 0.5f)));

			AddLine(new LineData { a = new float3(-1, 0, -1), b = new float3(1, 0, -1) });
			AddLine(new LineData { a = new float3(1, 0, -1), b = new float3(1, 0, 1) });
			AddLine(new LineData { a = new float3(1, 0, 1), b = new float3(-1, 0, 1) });
			AddLine(new LineData { a = new float3(-1, 0, 1), b = new float3(-1, 0, -1) });

			currentMatrix = oldMatrix;
		}

		internal static readonly float4[] BoxVertices = {
			new float4(-1, -1, -1, 1),
			new float4(-1, -1, +1, 1),
			new float4(-1, +1, -1, 1),
			new float4(-1, +1, +1, 1),
			new float4(+1, -1, -1, 1),
			new float4(+1, -1, +1, 1),
			new float4(+1, +1, -1, 1),
			new float4(+1, +1, +1, 1),
		};

		internal static readonly int[] BoxTriangles = {
			// Bottom two triangles
			0, 1, 5,
			0, 5, 4,

			// Top
			7, 3, 2,
			7, 2, 6,

			// -X
			0, 1, 3,
			0, 3, 2,

			// +X
			4, 5, 7,
			4, 7, 6,

			// +Z
			1, 3, 7,
			1, 7, 5,

			// -Z
			0, 2, 6,
			0, 6, 4,
		};

		void AddBox (BoxData box) {
			unsafe {
				var solidVertices = &buffers->solidVertices;
				var solidTriangles = &buffers->solidTriangles;
				Reserve(solidVertices, BoxVertices.Length * UnsafeUtility.SizeOf<Vertex>());
				Reserve(solidTriangles, BoxTriangles.Length * UnsafeUtility.SizeOf<int>());

				var scale = box.size * 0.5f;
				var matrix = math.mul(currentMatrix, new float4x4(
					new float4(scale.x, 0, 0, 0),
					new float4(0, scale.y, 0, 0),
					new float4(0, 0, scale.z, 0),
					new float4(box.center, 1)
					));

				var mn = minBounds;
				var mx = maxBounds;
				int vertexOffset = solidVertices->Length / UnsafeUtility.SizeOf<Vertex>();
				var ptr = (Vertex*)(solidVertices->Ptr + solidVertices->Length);
				for (int i = 0; i < BoxVertices.Length; i++) {
					var p = PerspectiveDivide(math.mul(matrix, BoxVertices[i]));
					// Update the bounding box
					mn = math.min(mn, p);
					mx = math.max(mx, p);

					*ptr++ = new Vertex {
						position = p,
						color = currentColor,
						uv = new float2(0, 0),
						uv2 = new float3(0, 0, 0),
					};
				}
				solidVertices->Length += BoxVertices.Length * UnsafeUtility.SizeOf<Vertex>();

				minBounds = mn;
				maxBounds = mx;

				var triPtr = (int*)(solidTriangles->Ptr + solidTriangles->Length);
				for (int i = 0; i < BoxTriangles.Length; i++) {
					*triPtr++ = vertexOffset + BoxTriangles[i];
				}
				solidTriangles->Length += BoxTriangles.Length * UnsafeUtility.SizeOf<int>();
			}
		}

		// AggressiveInlining because this is only called from a single location, and burst doesn't inline otherwise
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public void Next (ref UnsafeAppendBuffer.Reader reader, ref NativeArray<float4x4> matrixStack, ref NativeArray<Color32> colorStack, ref NativeArray<LineWidthData> lineWidthStack, ref int matrixStackSize, ref int colorStackSize, ref int lineWidthStackSize) {
			var fullCmd = reader.ReadNext<Command>();
			var cmd = fullCmd & (Command)0xFF;
			Color32 oldColor = default;

			if ((fullCmd & Command.PushColorInline) != 0) {
				oldColor = currentColor;
				currentColor = reader.ReadNext<Color32>();
			}

			switch (cmd) {
			case Command.PushColor:
#if ENABLE_UNITY_COLLECTIONS_CHECKS
if (colorStackSize >= colorStack.Length) throw new System.Exception("Too deeply nested PushColor calls");
#else
if (colorStackSize >= colorStack.Length) colorStackSize--;
#endif
colorStack[colorStackSize] = currentColor;
colorStackSize++;
currentColor = reader.ReadNext<Color32>();
break;
case Command.PopColor:
#if ENABLE_UNITY_COLLECTIONS_CHECKS
if (colorStackSize <= 0) throw new System.Exception("PushColor and PopColor are not matched");
#else
if (colorStackSize <= 0) break;
#endif
colorStackSize--;
currentColor = colorStack[colorStackSize];
break;
case Command.PushMatrix:
#if ENABLE_UNITY_COLLECTIONS_CHECKS
if (matrixStackSize >= matrixStack.Length) throw new System.Exception("Too deeply nested PushMatrix calls");
#else
if (matrixStackSize >= matrixStack.Length) matrixStackSize--;
#endif
matrixStack[matrixStackSize] = currentMatrix;
matrixStackSize++;
currentMatrix = math.mul(currentMatrix, reader.ReadNext<float4x4>());
break;
case Command.PushSetMatrix:
#if ENABLE_UNITY_COLLECTIONS_CHECKS
if (matrixStackSize >= matrixStack.Length) throw new System.Exception("Too deeply nested PushMatrix calls");
#else
if (matrixStackSize >= matrixStack.Length) matrixStackSize--;
#endif
matrixStack[matrixStackSize] = currentMatrix;
matrixStackSize++;
currentMatrix = reader.ReadNext<float4x4>();
break;
case Command.PopMatrix:
#if ENABLE_UNITY_COLLECTIONS_CHECKS
if (matrixStackSize <= 0) throw new System.Exception("PushMatrix and PopMatrix are not matched");
#else
if (matrixStackSize <= 0) break;
#endif
matrixStackSize--;
currentMatrix = matrixStack[matrixStackSize];
break;
case Command.PushLineWidth:
#if ENABLE_UNITY_COLLECTIONS_CHECKS
if (lineWidthStackSize >= lineWidthStack.Length) throw new System.Exception("Too deeply nested PushLineWidth calls");
#else
if (lineWidthStackSize >= lineWidthStack.Length) lineWidthStackSize--;
#endif
lineWidthStack[lineWidthStackSize] = currentLineWidthData;
lineWidthStackSize++;
currentLineWidthData = reader.ReadNext<LineWidthData>();
currentLineWidthData.pixels *= lineWidthMultiplier;
break;
case Command.PopLineWidth:
#if ENABLE_UNITY_COLLECTIONS_CHECKS
if (lineWidthStackSize <= 0) throw new System.Exception("PushLineWidth and PopLineWidth are not matched");
#else
if (lineWidthStackSize <= 0) break;
#endif
lineWidthStackSize--;
currentLineWidthData = lineWidthStack[lineWidthStackSize];
break;
case Command.Line:
AddLine(reader.ReadNext<LineData>());
break;
case Command.SphereOutline:
AddSphereOutline(reader.ReadNext<SphereData>());
break;
case Command.CircleXZ:
AddCircle(reader.ReadNext<CircleXZData>());
break;
case Command.Circle:
AddCircle(reader.ReadNext<CircleData>());
break;
case Command.DiscXZ:
AddDisc(reader.ReadNext<CircleXZData>());
break;
case Command.Disc:
AddDisc(reader.ReadNext<CircleData>());
break;
case Command.Box:
AddBox(reader.ReadNext<BoxData>());
break;
case Command.WirePlane:
AddPlane(reader.ReadNext<PlaneData>());
break;
case Command.WireBox:
AddWireBox(reader.ReadNext<BoxData>());
break;
case Command.SolidTriangle:
AddSolidTriangle(reader.ReadNext<TriangleData>());
break;
case Command.PushPersist:
// This command does not need to be handled by the builder
reader.ReadNext<PersistData>();
break;
case Command.PopPersist:
// This command does not need to be handled by the builder
break;
case Command.Text:
var data = reader.ReadNext<TextData>();
unsafe {
System.UInt16* ptr = (System.UInt16*)reader.ReadNext(UnsafeUtility.SizeOf<System.UInt16>() * data.numCharacters);
AddText(ptr, data, currentColor);
}
break;
case Command.Text3D:
var data2 = reader.ReadNext<TextData3D>();
unsafe {
System.UInt16* ptr = (System.UInt16*)reader.ReadNext(UnsafeUtility.SizeOf<System.UInt16>() * data2.numCharacters);
AddText3D(ptr, data2, currentColor);
}
break;
case Command.CaptureState:
unsafe {
buffers->capturedState.Add(new ProcessedBuilderData.CapturedState {
color = this.currentColor,
matrix = this.currentMatrix,
});
}
break;
default:
#if ENABLE_UNITY_COLLECTIONS_CHECKS
throw new System.Exception("Unknown command");
#else
break;
#endif
}

			if ((fullCmd & Command.PushColorInline) != 0) {
				currentColor = oldColor;
			}
		}

		void CreateTriangles () {
			// Create triangles for all lines
			// A triangle consists of 3 indices
			// A line (4 vertices) consists of 2 triangles, so 6 triangle indices
			unsafe {
				var outlineVertices = &buffers->vertices;
				var outlineTriangles = &buffers->triangles;
				var vertexCount = outlineVertices->Length / UnsafeUtility.SizeOf<Vertex>();
				// Each line is made out of 4 vertices
				var lineCount = vertexCount / 4;
				var trianglesSizeInBytes = lineCount * 6 * UnsafeUtility.SizeOf<int>();
				if (trianglesSizeInBytes >= outlineTriangles->Capacity) {
					outlineTriangles->SetCapacity(math.ceilpow2(trianglesSizeInBytes));
				}

				int* ptr = (int*)outlineTriangles->Ptr;
				for (int i = 0, vi = 0; i < lineCount; i++, vi += 4) {
					// First triangle
					*ptr++ = vi + 0;
					*ptr++ = vi + 1;
					*ptr++ = vi + 2;

					// Second triangle
					*ptr++ = vi + 1;
					*ptr++ = vi + 3;
					*ptr++ = vi + 2;
				}
				outlineTriangles->Length = trianglesSizeInBytes;
			}
		}

		public const int MaxStackSize = 32;

		public void Execute () {
			unsafe {
				buffers->vertices.Reset();
				buffers->triangles.Reset();
				buffers->solidVertices.Reset();
				buffers->solidTriangles.Reset();
				buffers->textVertices.Reset();
				buffers->textTriangles.Reset();
				buffers->capturedState.Reset();
			}

			currentLineWidthData.pixels *= lineWidthMultiplier;

			minBounds = new float3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
			maxBounds = new float3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

			var matrixStack = new NativeArray<float4x4>(MaxStackSize, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
			var colorStack = new NativeArray<Color32>(MaxStackSize, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
			var lineWidthStack = new NativeArray<LineWidthData>(MaxStackSize, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
			int matrixStackSize = 0;
			int colorStackSize = 0;
			int lineWidthStackSize = 0;

			CommandBuilderSamplers.MarkerProcessCommands.Begin();
			unsafe {
				var reader = buffers->splitterOutput.AsReader();
				while (reader.Offset < reader.Size) Next(ref reader, ref matrixStack, ref colorStack, ref lineWidthStack, ref matrixStackSize, ref colorStackSize, ref lineWidthStackSize);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
if (reader.Offset != reader.Size) throw new Exception("Didn't reach the end of the buffer");
#endif
}
CommandBuilderSamplers.MarkerProcessCommands.End();

			CommandBuilderSamplers.MarkerCreateTriangles.Begin();
			CreateTriangles();
			CommandBuilderSamplers.MarkerCreateTriangles.End();

			unsafe {
				var outBounds = &buffers->bounds;
				*outBounds = new Bounds((minBounds + maxBounds) * 0.5f, maxBounds - minBounds);

				if (math.any(math.isnan(outBounds->min)) && (buffers->vertices.Length > 0 || buffers->solidTriangles.Length > 0)) {
					// Fall back to a bounding box that covers everything
					*outBounds = new Bounds(Vector3.zero, new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity));
#if ENABLE_UNITY_COLLECTIONS_CHECKS
throw new Exception("NaN bounds. A Draw.* command may have been given NaN coordinates.");
#endif
}
}
}
}
}
using UnityEngine;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using System;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using UnityEngine.Rendering;
using System.Diagnostics;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine.Profiling;
using System.Linq;

namespace Drawing {
using Drawing.Text;
using Unity.Profiling;

	public static class SharedDrawingData {
		/// <summary>
		/// Same as Time.time, but not updated as frequently.
		/// Used since burst jobs cannot access Time.time.
		/// </summary>
		public static readonly Unity.Burst.SharedStatic<float> BurstTime = Unity.Burst.SharedStatic<float>.GetOrCreate<DrawingManager, BurstTimeKey>(4);

		private class BurstTimeKey {}
	}

	/// <summary>
	/// Used to cache drawing data over multiple frames.
	/// This is useful as a performance optimization when you are drawing the same thing over multiple consecutive frames.
	///
	/// <code>
	/// private RedrawScope redrawScope;
	///
	/// void Start () {
	///     redrawScope = DrawingManager.GetRedrawScope();
	///     using (var builder = DrawingManager.GetBuilder(redrawScope)) {
	///         builder.WireSphere(Vector3.zero, 1.0f, Color.red);
	///     }
	/// }
	///
	/// void OnDestroy () {
	///     redrawScope.Dispose();
	/// }
	/// </code>
	///
	/// See: <see cref="DrawingManager.GetRedrawScope"/>
	/// </summary>
	public struct RedrawScope : System.IDisposable {
		// Stored as a GCHandle to allow storing this struct in an unmanaged ECS component or system
		internal System.Runtime.InteropServices.GCHandle gizmos;
		/// <summary>
		/// ID of the scope.
		/// Zero means no or invalid scope.
		/// </summary>
		internal int id;

		static int idCounter = 1;

		/// <summary>True if the scope has been created</summary>
		public bool isValid => id != 0;

		internal RedrawScope (DrawingData gizmos, int id) {
			this.gizmos = gizmos.gizmosHandle;
			this.id = id;
		}

		internal RedrawScope (DrawingData gizmos) {
			this.gizmos = gizmos.gizmosHandle;
			// Should be enough with 4 billion ids before they wrap around.
			id = idCounter++;
		}

		/// <summary>
		/// Everything rendered with this scope and which is not older than one frame is drawn again.
		/// This is useful if you for some reason cannot draw some items during a frame (e.g. some asynchronous process is modifying the contents)
		/// but you still want to draw the same thing as the last frame to at least draw *something*.
		///
		/// Note: The items age will be reset. So the next frame you can call
		/// this method again to draw the items yet again.
		/// </summary>
		internal void Draw () {
			if (gizmos.IsAllocated) {
				if (gizmos.Target is DrawingData gizmosTarget) gizmosTarget.Draw(this);
			}
		}

		/// <summary>
		/// Stops keeping all previously rendered items alive, and starts a new scope.
		/// Equivalent to first calling Dispose on the old scope and then creating a new one.
		/// </summary>
		public void Rewind () {
			GameObject associatedGameObject = null;
			if (gizmos.IsAllocated) {
				if (gizmos.Target is DrawingData gizmosTarget) associatedGameObject = gizmosTarget.GetAssociatedGameObject(this);
			}
			Dispose();
			this = DrawingManager.GetRedrawScope(associatedGameObject);
		}

		internal void DrawUntilDispose (GameObject associatedGameObject) {
			if (gizmos.Target is DrawingData gizmosTarget) gizmosTarget.DrawUntilDisposed(this, associatedGameObject);
		}

		/// <summary>
		/// Dispose the redraw scope to stop rendering the items.
		///
		/// You must do this when you are done with the scope, even if it was never used to actually render anything.
		/// The items will stop rendering immediately: the next camera to render will not render the items unless kept alive in some other way.
		/// However, items are always rendered at least once.
		/// </summary>
		public void Dispose () {
			if (gizmos.IsAllocated) {
				if (gizmos.Target is DrawingData gizmosTarget) gizmosTarget.DisposeRedrawScope(this);
			}
			gizmos = default;
			id = 0;
		}
	};

	/// <summary>Helper for drawing Gizmos in a performant way</summary>
	public class DrawingData {
		/// <summary>Combines hashes into a single hash value</summary>
		public struct Hasher : IEquatable<Hasher> {
			ulong hash;

			public static Hasher NotSupplied => new Hasher { hash = ulong.MaxValue };

			[System.Obsolete("Use the constructor instead")]
			public static Hasher Create<T>(T init) {
				var h = new Hasher();

				h.Add(init);
				return h;
			}

			/// <summary>
			/// Includes the given data in the final hash.
			/// You can call this as many times as you want.
			/// </summary>
			public void Add<T>(T hash) {
				// Just a regular hash function. The + 12289 is to make sure that hashing zeros doesn't just produce a zero (and generally that hashing one X doesn't produce a hash of X)
				// (with a struct we can't provide default initialization)
				this.hash = (1572869UL * this.hash) ^ (ulong)hash.GetHashCode() + 12289;
			}

			public readonly ulong Hash => hash;

			public override int GetHashCode () {
				return (int)hash;
			}

			public bool Equals (Hasher other) {
				return hash == other.hash;
			}
		}

		internal struct ProcessedBuilderData {
			public enum Type {
				Invalid = 0,
				Static,
				Dynamic,
				Persistent,
			}

			public Type type;
			public BuilderData.Meta meta;
			bool submitted;

			// A single instance of a MeshBuffers struct.
			// This needs to be stored in a NativeArray because we will use it as a pointer
			// and it needs to be guaranteed to stay in the same position in memory.
			public NativeArray<MeshBuffers> temporaryMeshBuffers;
			JobHandle buildJob, splitterJob;
			public List<MeshWithType> meshes;

			public bool isValid {
				get {
					return type != Type.Invalid;
				}
			}

			public struct CapturedState {
				public Matrix4x4 matrix;
				public Color color;
			}

			public struct MeshBuffers {
				public UnsafeAppendBuffer splitterOutput, vertices, triangles, solidVertices, solidTriangles, textVertices, textTriangles, capturedState;
				public Bounds bounds;

				public MeshBuffers(Allocator allocator) {
					splitterOutput = new UnsafeAppendBuffer(0, 4, allocator);
					vertices = new UnsafeAppendBuffer(0, 4, allocator);
					triangles = new UnsafeAppendBuffer(0, 4, allocator);
					solidVertices = new UnsafeAppendBuffer(0, 4, allocator);
					solidTriangles = new UnsafeAppendBuffer(0, 4, allocator);
					textVertices = new UnsafeAppendBuffer(0, 4, allocator);
					textTriangles = new UnsafeAppendBuffer(0, 4, allocator);
					capturedState = new UnsafeAppendBuffer(0, 4, allocator);
					bounds = new Bounds();
				}

				public void Dispose () {
					splitterOutput.Dispose();
					vertices.Dispose();
					triangles.Dispose();
					solidVertices.Dispose();
					solidTriangles.Dispose();
					textVertices.Dispose();
					textTriangles.Dispose();
					capturedState.Dispose();
				}

				static void DisposeIfLarge (ref UnsafeAppendBuffer ls) {
					if (ls.Length*3 < ls.Capacity && ls.Capacity > 1024) {
						var alloc = ls.Allocator;
						ls.Dispose();
						ls = new UnsafeAppendBuffer(0, 4, alloc);
					}
				}

				public void DisposeIfLarge () {
					DisposeIfLarge(ref splitterOutput);
					DisposeIfLarge(ref vertices);
					DisposeIfLarge(ref triangles);
					DisposeIfLarge(ref solidVertices);
					DisposeIfLarge(ref solidTriangles);
					DisposeIfLarge(ref textVertices);
					DisposeIfLarge(ref textTriangles);
					DisposeIfLarge(ref capturedState);
				}
			}

			public unsafe UnsafeAppendBuffer* splitterOutputPtr => & ((MeshBuffers*)temporaryMeshBuffers.GetUnsafePtr())->splitterOutput;

			public void Init (Type type, BuilderData.Meta meta) {
				submitted = false;
				this.type = type;
				this.meta = meta;

				if (meshes == null) meshes = new List<MeshWithType>();
				if (!temporaryMeshBuffers.IsCreated) {
					temporaryMeshBuffers = new NativeArray<MeshBuffers>(1, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
					temporaryMeshBuffers[0] = new MeshBuffers(Allocator.Persistent);
				}
			}

			static int SubmittedJobs = 0;

			public void SetSplitterJob (DrawingData gizmos, JobHandle splitterJob) {
				this.splitterJob = splitterJob;
				if (type == Type.Static) {
					var cameraInfo = new GeometryBuilder.CameraInfo(null);
					unsafe {
						buildJob = GeometryBuilder.Build(gizmos, (MeshBuffers*)NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(temporaryMeshBuffers), ref cameraInfo, splitterJob);
					}

					SubmittedJobs++;
					// ScheduleBatchedJobs is expensive, so only do it once in a while
					if (SubmittedJobs % 8 == 0) {
						MarkerScheduleJobs.Begin();
						JobHandle.ScheduleBatchedJobs();
						MarkerScheduleJobs.End();
					}
				}
			}

			public void SchedulePersistFilter (int version, int lastTickVersion, float time, int sceneModeVersion) {
				if (type != Type.Persistent) throw new System.InvalidOperationException();

				// If data was from a different game mode then it shouldn't live any longer.
				// E.g. editor mode => game mode
				if (meta.sceneModeVersion != sceneModeVersion) {
					meta.version = -1;
					return;
				}

				// Guarantee that all drawing commands survive at least one frame
				// Don't filter them until they have had the opportunity to be drawn once at least.
				// (they may not actually have been drawn because no cameras may be active)
				if (meta.version < lastTickVersion || submitted) {
					splitterJob.Complete();
					meta.version = version;

					// If the command buffer is empty then this instance should not live longer
					var splitterOutput = temporaryMeshBuffers[0].splitterOutput;
					if (splitterOutput.Length == 0) {
						meta.version = -1;
						return;
					}

					buildJob.Complete();
					unsafe {
						splitterJob = new PersistentFilterJob {
							buffer = &((MeshBuffers*)NativeArrayUnsafeUtility.GetUnsafePtr(temporaryMeshBuffers))->splitterOutput,
							time = time,
						}.Schedule(splitterJob);
					}
				}
			}

			public bool IsValidForCamera (Camera camera, bool allowGizmos, bool allowCameraDefault) {
				if (!allowGizmos && meta.isGizmos) return false;

				if (meta.cameraTargets != null) {
					return meta.cameraTargets.Contains(camera);
				} else {
					return allowCameraDefault;
				}
			}

			public void Schedule (DrawingData gizmos, ref GeometryBuilder.CameraInfo cameraInfo) {
				// The job for Static will already have been scheduled in SetSplitterJob
				if (type != Type.Static) {
					unsafe {
						buildJob = GeometryBuilder.Build(gizmos, (MeshBuffers*)NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(temporaryMeshBuffers), ref cameraInfo, splitterJob);
					}
				}
			}

			public void BuildMeshes (DrawingData gizmos) {
				if (type == Type.Static && submitted) return;
				buildJob.Complete();
				unsafe {
					GeometryBuilder.BuildMesh(gizmos, meshes, (MeshBuffers*)temporaryMeshBuffers.GetUnsafePtr());
				}
				submitted = true;
			}

			public void CollectMeshes (List<RenderedMeshWithType> meshes) {
				var itemMeshes = this.meshes;
				var customMeshIndex = 0;
				var capturedState = temporaryMeshBuffers[0].capturedState;
				var maxCustomMeshes = capturedState.Length / UnsafeUtility.SizeOf<CapturedState>();

				for (int i = 0; i < itemMeshes.Count; i++) {
					Color color;
					Matrix4x4 matrix;
					int drawOrderIndex;
					if ((itemMeshes[i].type & MeshType.Custom) != 0) {
						UnityEngine.Assertions.Assert.IsTrue(customMeshIndex < maxCustomMeshes);

						// The color and orientation of custom meshes are stored in the captured state array.
						// It is indexed in the same order as the custom meshes in the #meshes list.
						unsafe {
							var state = *((CapturedState*)capturedState.Ptr + customMeshIndex);
							color = state.color;
							matrix = state.matrix;
							customMeshIndex += 1;
						}
						// Custom meshes are rendered *after* all similar builders.
						// In practice this means all custom meshes are drawn after all dynamic items.
						drawOrderIndex = meta.drawOrderIndex + 1;
					} else {
						// All other meshes use default colors and identity matrices
						// since their data is already baked into the vertex colors and positions
						color = Color.white;
						matrix = Matrix4x4.identity;
						drawOrderIndex = meta.drawOrderIndex;
					}
					meshes.Add(new RenderedMeshWithType {
						mesh = itemMeshes[i].mesh,
						type = itemMeshes[i].type,
						drawingOrderIndex = drawOrderIndex,
						color = color,
						matrix = matrix,
					});
				}
			}

			void PoolMeshes (DrawingData gizmos, bool includeCustom) {
				if (!isValid) throw new System.InvalidOperationException();
				var outIndex = 0;
				for (int i = 0; i < meshes.Count; i++) {
					// Custom meshes should only be pooled if the Pool flag is set.
					// Otherwise they are supplied by the user and it's up to them how to handle it.
					if ((meshes[i].type & MeshType.Custom) == 0 || (includeCustom && (meshes[i].type & MeshType.Pool) != 0)) {
						gizmos.PoolMesh(meshes[i].mesh);
					} else {
						// Retain custom meshes
						meshes[outIndex] = meshes[i];
						outIndex += 1;
					}
				}
				meshes.RemoveRange(outIndex, meshes.Count - outIndex);
			}

			public void PoolDynamicMeshes (DrawingData gizmos) {
				if (type == Type.Static && submitted) return;
				PoolMeshes(gizmos, false);
			}

			public void Release (DrawingData gizmos) {
				if (!isValid) throw new System.InvalidOperationException();
				PoolMeshes(gizmos, true);
				// Clear custom meshes too
				meshes.Clear();
				type = Type.Invalid;
				splitterJob.Complete();
				buildJob.Complete();
				var bufs = this.temporaryMeshBuffers[0];
				bufs.DisposeIfLarge();
				this.temporaryMeshBuffers[0] = bufs;
			}

			public void Dispose () {
				if (isValid) throw new System.InvalidOperationException();
				splitterJob.Complete();
				buildJob.Complete();
				if (temporaryMeshBuffers.IsCreated) {
					temporaryMeshBuffers[0].Dispose();
					temporaryMeshBuffers.Dispose();
				}
			}
		}

		internal struct SubmittedMesh {
			public Mesh mesh;
			public bool temporary;
		}

		[BurstCompile]
		internal struct BuilderData : IDisposable {
			public enum State {
				Free,
				Reserved,
				Initialized,
				WaitingForSplitter,
				WaitingForUserDefinedJob,
			}

			public struct Meta {
				public Hasher hasher;
				public RedrawScope redrawScope1;
				public RedrawScope redrawScope2;
				public int version;
				public bool isGizmos;
				/// <summary>Used to invalidate gizmos when the scene mode changes</summary>
				public int sceneModeVersion;
				public int drawOrderIndex;
				public Camera[] cameraTargets;
			}

			public struct BitPackedMeta {
				uint flags;

				const int UniqueIDBitshift = 17;
				const int IsBuiltInFlagIndex = 16;
				const int IndexMask = (1 << IsBuiltInFlagIndex) - 1;
				const int MaxDataIndex = IndexMask;
				public const int UniqueIdMask = (1 << (32 - UniqueIDBitshift)) - 1;


				public BitPackedMeta (int dataIndex, int uniqueID, bool isBuiltInCommandBuilder) {
					// Important to make ensure bitpacking doesn't collide
					if (dataIndex > MaxDataIndex) throw new System.Exception("Too many command builders active. Are some command builders not being disposed?");
					UnityEngine.Assertions.Assert.IsTrue(uniqueID <= UniqueIdMask && uniqueID >= 0);

					flags = (uint)(dataIndex | uniqueID << UniqueIDBitshift | (isBuiltInCommandBuilder ? 1 << IsBuiltInFlagIndex : 0));
				}

				public int dataIndex {
					get {
						return (int)(flags & IndexMask);
					}
				}

				public int uniqueID {
					get {
						return (int)(flags >> UniqueIDBitshift);
					}
				}

				public bool isBuiltInCommandBuilder {
					get {
						return (flags & (1 << IsBuiltInFlagIndex)) != 0;
					}
				}

				public static bool operator== (BitPackedMeta lhs, BitPackedMeta rhs) {
					return lhs.flags == rhs.flags;
				}

				public static bool operator!= (BitPackedMeta lhs, BitPackedMeta rhs) {
					return lhs.flags != rhs.flags;
				}

				public override bool Equals (object obj) {
					if (obj is BitPackedMeta meta) {
						return flags == meta.flags;
					}
					return false;
				}

				public override int GetHashCode () {
					return (int)flags;
				}
			}

			public BitPackedMeta packedMeta;
			public List<SubmittedMesh> meshes;
			public NativeArray<UnsafeAppendBuffer> commandBuffers;
			public State state { get; private set; }
			// TODO?
			public bool preventDispose;
			JobHandle splitterJob;
			JobHandle disposeDependency;
			AllowedDelay disposeDependencyDelay;
			System.Runtime.InteropServices.GCHandle disposeGCHandle;
			public Meta meta;

			public void Reserve (int dataIndex, bool isBuiltInCommandBuilder) {
				if (state != State.Free) throw new System.InvalidOperationException();
				state = BuilderData.State.Reserved;
				packedMeta = new BitPackedMeta(dataIndex, (UniqueIDCounter++) & BitPackedMeta.UniqueIdMask, isBuiltInCommandBuilder);
			}

			static int UniqueIDCounter = 0;

			public void Init (Hasher hasher, RedrawScope frameRedrawScope, RedrawScope customRedrawScope, bool isGizmos, int drawOrderIndex, int sceneModeVersion) {
				if (state != State.Reserved) throw new System.InvalidOperationException();

				meta = new Meta {
					hasher = hasher,
					redrawScope1 = frameRedrawScope,
					redrawScope2 = customRedrawScope,
					isGizmos = isGizmos,
					version = 0, // Will be filled in later
					drawOrderIndex = drawOrderIndex,
					sceneModeVersion = sceneModeVersion,
					cameraTargets = null,
				};

				if (meshes == null) meshes = new List<SubmittedMesh>();
				if (!commandBuffers.IsCreated) {
#if UNITY_2022_3_OR_NEWER
commandBuffers = new NativeArray<UnsafeAppendBuffer>(JobsUtility.ThreadIndexCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
#else
commandBuffers = new NativeArray<UnsafeAppendBuffer>(JobsUtility.MaxJobThreadCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
#endif
for (int i = 0; i < commandBuffers.Length; i++) commandBuffers[i] = new UnsafeAppendBuffer(0, 4, Allocator.Persistent);
}

				state = State.Initialized;
			}

			public unsafe UnsafeAppendBuffer* bufferPtr {
				get {
					return (UnsafeAppendBuffer*)commandBuffers.GetUnsafePtr();
				}
			}

			[BurstCompile]
			[AOT.MonoPInvokeCallback(typeof(AnyBuffersWrittenToDelegate))]
			unsafe static bool AnyBuffersWrittenTo (UnsafeAppendBuffer* buffers, int numBuffers) {
				bool any = false;

				for (int i = 0; i < numBuffers; i++) {
					any |= buffers[i].Length > 0;
				}
				return any;
			}

			[BurstCompile]
			[AOT.MonoPInvokeCallback(typeof(AnyBuffersWrittenToDelegate))]
			unsafe static void ResetAllBuffers (UnsafeAppendBuffer* buffers, int numBuffers) {
				for (int i = 0; i < numBuffers; i++) {
					buffers[i].Reset();
				}
			}

			unsafe delegate bool AnyBuffersWrittenToDelegate(UnsafeAppendBuffer* buffers, int numBuffers);
			private readonly unsafe static AnyBuffersWrittenToDelegate AnyBuffersWrittenToInvoke = BurstCompiler.CompileFunctionPointer<AnyBuffersWrittenToDelegate>(AnyBuffersWrittenTo).Invoke;
			unsafe delegate void ResetAllBuffersToDelegate(UnsafeAppendBuffer* buffers, int numBuffers);
			private readonly unsafe static ResetAllBuffersToDelegate ResetAllBuffersToInvoke = BurstCompiler.CompileFunctionPointer<ResetAllBuffersToDelegate>(ResetAllBuffers).Invoke;

			public void SubmitWithDependency (System.Runtime.InteropServices.GCHandle gcHandle, JobHandle dependency, AllowedDelay allowedDelay) {
				state = State.WaitingForUserDefinedJob;
				disposeDependency = dependency;
				disposeDependencyDelay = allowedDelay;
				disposeGCHandle = gcHandle;
			}

			public void Submit (DrawingData gizmos) {
				if (state != State.Initialized) throw new System.InvalidOperationException();

#if !UNITY_EDITOR
if (meta.isGizmos) {
// Gizmos are never drawn in standalone builds.
// Draw.Line, and similar draw commands, will already have been removed in standalone builds,
// but if users use e.g. Draw.editor directly, then the commands will be added to the command buffer.
// For performance we can just discard the whole buffer here.
Release();
return;
}
#endif


				unsafe {
					// There are about 128 buffers we need to check and it's faster to do that using Burst
					if (meshes.Count == 0 && !AnyBuffersWrittenToInvoke((UnsafeAppendBuffer*)commandBuffers.GetUnsafeReadOnlyPtr(), commandBuffers.Length)) {
						// If no buffers have been written to then simply discard this builder
						Release();
						return;
					}
				}

				meta.version = gizmos.version;

				// Command stream
				// split to static, dynamic and persistent
				// render static
				// render dynamic per camera
				// render persistent per camera
				const int PersistentDrawOrderOffset = 1000000;
				var tmpMeta = meta;
				// Reserve some buffers.
				// We need to set a deterministic order in which things are drawn to avoid flickering.
				// The shaders use the z buffer most of the time, but there are still
				// things which are not order independent.
				// Static stuff is drawn first
				tmpMeta.drawOrderIndex = meta.drawOrderIndex*3 + 0;
				int staticBuffer = gizmos.processedData.Reserve(ProcessedBuilderData.Type.Static, tmpMeta);
				// Dynamic stuff is drawn directly after the static stuff
				// Note that any custom meshes will get this draw order index + 1.
				tmpMeta.drawOrderIndex = meta.drawOrderIndex*3 + 1;
				int dynamicBuffer = gizmos.processedData.Reserve(ProcessedBuilderData.Type.Dynamic, tmpMeta);
				// Persistent stuff is always drawn after everything else
				tmpMeta.drawOrderIndex = meta.drawOrderIndex + PersistentDrawOrderOffset;
				int persistentBuffer = gizmos.processedData.Reserve(ProcessedBuilderData.Type.Persistent, tmpMeta);

				unsafe {
					splitterJob = new StreamSplitter {
						inputBuffers = commandBuffers,
						staticBuffer = gizmos.processedData.Get(staticBuffer).splitterOutputPtr,
						dynamicBuffer = gizmos.processedData.Get(dynamicBuffer).splitterOutputPtr,
						persistentBuffer = gizmos.processedData.Get(persistentBuffer).splitterOutputPtr,
					}.Schedule();
				}

				gizmos.processedData.Get(staticBuffer).SetSplitterJob(gizmos, splitterJob);
				gizmos.processedData.Get(dynamicBuffer).SetSplitterJob(gizmos, splitterJob);
				gizmos.processedData.Get(persistentBuffer).SetSplitterJob(gizmos, splitterJob);

				if (meshes.Count > 0) {
					// Custom meshes may be affected by matrices and colors that are set in the command builders.
					// Matrices may in theory be dynamic per camera (though this functionality is not used at the moment).
					// The Command.CaptureState commands are marked as Dynamic so captured state will be written to
					// the meshBuffers.capturedState array in the #dynamicBuffer.
					var customMeshes = gizmos.processedData.Get(dynamicBuffer).meshes;

					// Copy meshes to render
					for (int i = 0; i < meshes.Count; i++) customMeshes.Add(new MeshWithType { mesh = meshes[i].mesh, type = MeshType.Solid | MeshType.Custom | (meshes[i].temporary ? MeshType.Pool : 0) });
					meshes.Clear();
				}

				// TODO: Allocate 3 output objects and pipe splitter to them

				// Only meshes valid for all cameras have been submitted.
				// Meshes that depend on the specific camera will be submitted just before rendering
				// that camera. Line drawing depends on the exact camera.
				// In particular when drawing circles different number of segments
				// are used depending on the distance to the camera.
				state = State.WaitingForSplitter;
			}

			public void CheckJobDependency (DrawingData gizmos, bool allowBlocking) {
				if (state == State.WaitingForUserDefinedJob && (disposeDependency.IsCompleted || (allowBlocking && disposeDependencyDelay == AllowedDelay.EndOfFrame))) {
					disposeDependency.Complete();
					disposeDependency = default;
					disposeGCHandle.Free();
					state = State.Initialized;
					Submit(gizmos);
				}
			}

			public void Release () {
				if (state == State.Free) throw new System.InvalidOperationException();
				state = BuilderData.State.Free;
				ClearData();
			}

			void ClearData () {
				// Wait for any jobs that might be running
				// This is important to avoid memory corruption bugs
				disposeDependency.Complete();
				splitterJob.Complete();
				meta = default;
				disposeDependency = default;
				preventDispose = false;
				meshes.Clear();
				unsafe {
					// There are about 128 buffers we need to reset and it's faster to do that using Burst
					ResetAllBuffers((UnsafeAppendBuffer*)commandBuffers.GetUnsafePtr(), commandBuffers.Length);
				}
			}

			public void Dispose () {
				if (state == State.WaitingForUserDefinedJob) {
					disposeDependency.Complete();
					disposeGCHandle.Free();
					// We would call Submit here, but we are deleting the data anyway, so who cares.
					state = State.WaitingForSplitter;
				}

				if (state == State.Reserved || state == State.Initialized || state == State.WaitingForUserDefinedJob) {
					UnityEngine.Debug.LogError("Drawing data is being destroyed, but a drawing instance is still active. Are you sure you have called Dispose on all drawing instances? This will cause a memory leak!");
					return;
				}

				splitterJob.Complete();
				if (commandBuffers.IsCreated) {
					for (int i = 0; i < commandBuffers.Length; i++) {
						commandBuffers[i].Dispose();
					}
					commandBuffers.Dispose();
				}
			}
		}

		internal struct BuilderDataContainer : IDisposable {
			BuilderData[] data;

			public int memoryUsage {
				get {
					int sum = 0;
					if (data != null) {
						for (int i = 0; i < data.Length; i++) {
							var cmds = data[i].commandBuffers;
							for (int j = 0; j < cmds.Length; j++) {
								sum += cmds[j].Capacity;
							}
							unsafe {
								sum += data[i].commandBuffers.Length * sizeof(UnsafeAppendBuffer);
							}
						}
					}
					return sum;
				}
			}


			public BuilderData.BitPackedMeta Reserve (bool isBuiltInCommandBuilder) {
				if (data == null) data = new BuilderData[1];
				for (int i = 0; i < data.Length; i++) {
					if (data[i].state == BuilderData.State.Free) {
						data[i].Reserve(i, isBuiltInCommandBuilder);
						return data[i].packedMeta;
					}
				}

				var newData = new BuilderData[data.Length * 2];
				data.CopyTo(newData, 0);
				data = newData;
				return Reserve(isBuiltInCommandBuilder);
			}

			public void Release (BuilderData.BitPackedMeta meta) {
				data[meta.dataIndex].Release();
			}

			public bool StillExists (BuilderData.BitPackedMeta meta) {
				int index = meta.dataIndex;

				if (data == null || index >= data.Length) return false;
				return data[index].packedMeta == meta;
			}

			public ref BuilderData Get (BuilderData.BitPackedMeta meta) {
				int index = meta.dataIndex;

				if (data[index].state == BuilderData.State.Free) throw new System.ArgumentException("Data is not reserved");
				if (data[index].packedMeta != meta) throw new System.ArgumentException("This command builder has already been disposed");
				return ref data[index];
			}

			public void DisposeCommandBuildersWithJobDependencies (DrawingData gizmos) {
				if (data == null) return;
				for (int i = 0; i < data.Length; i++) data[i].CheckJobDependency(gizmos, false);
				MarkerAwaitUserDependencies.Begin();
				for (int i = 0; i < data.Length; i++) data[i].CheckJobDependency(gizmos, true);
				MarkerAwaitUserDependencies.End();
			}

			public void ReleaseAllUnused () {
				if (data == null) return;
				for (int i = 0; i < data.Length; i++) {
					if (data[i].state == BuilderData.State.WaitingForSplitter) {
						data[i].Release();
					}
				}
			}

			public void Dispose () {
				if (data != null) {
					for (int i = 0; i < data.Length; i++) data[i].Dispose();
				}
				// Ensures calling Dispose multiple times is a NOOP
				data = null;
			}
		}

		internal struct ProcessedBuilderDataContainer {
			ProcessedBuilderData[] data;
			Dictionary<ulong, List<int> > hash2index;
			Stack<int> freeSlots;
			Stack<List<int> > freeLists;

			public bool isEmpty => data == null || freeSlots.Count == data.Length;

			public int memoryUsage {
				get {
					int sum = 0;
					if (data != null) {
						for (int i = 0; i < data.Length; i++) {
							var bufs = data[i].temporaryMeshBuffers;
							for (int j = 0; j < bufs.Length; j++) {
								var psum = 0;
								psum += bufs[j].textVertices.Capacity;
								psum += bufs[j].textTriangles.Capacity;
								psum += bufs[j].solidVertices.Capacity;
								psum += bufs[j].solidTriangles.Capacity;
								psum += bufs[j].vertices.Capacity;
								psum += bufs[j].triangles.Capacity;
								psum += bufs[j].capturedState.Capacity;
								psum += bufs[j].splitterOutput.Capacity;
								sum += psum;
								UnityEngine.Debug.Log(i + ":" + j + " " + psum);
							}
						}
					}
					return sum;
				}
			}

			public int Reserve (ProcessedBuilderData.Type type, BuilderData.Meta meta) {
				if (data == null) {
					data = new ProcessedBuilderData[0];
					freeSlots = new Stack<int>();
					freeLists = new Stack<List<int> >();
					hash2index = new Dictionary<ulong, List<int> >();
				}
				if (freeSlots.Count == 0) {
					var newData = new ProcessedBuilderData[math.max(4, data.Length*2)];
					data.CopyTo(newData, 0);
					for (int i = data.Length; i < newData.Length; i++) freeSlots.Push(i);
					data = newData;
				}
				int index = freeSlots.Pop();
				data[index].Init(type, meta);
				if (!meta.hasher.Equals(Hasher.NotSupplied)) {
					List<int> ls;
					if (!hash2index.TryGetValue(meta.hasher.Hash, out ls)) {
						if (freeLists.Count == 0) freeLists.Push(new List<int>());
						ls = hash2index[meta.hasher.Hash] = freeLists.Pop();
					}
					ls.Add(index);
				}
				return index;
			}

			public ref ProcessedBuilderData Get (int index) {
				if (!data[index].isValid) throw new System.ArgumentException();
				return ref data[index];
			}

			void Release (DrawingData gizmos, int i) {
				var h = data[i].meta.hasher.Hash;

				if (!data[i].meta.hasher.Equals(Hasher.NotSupplied)) {
					if (hash2index.TryGetValue(h, out var ls)) {
						ls.Remove(i);
						if (ls.Count == 0) {
							freeLists.Push(ls);
							hash2index.Remove(h);
						}
					}
				}
				data[i].Release(gizmos);
				freeSlots.Push(i);
			}

			public void SubmitMeshes (DrawingData gizmos, Camera camera, int versionThreshold, bool allowGizmos, bool allowCameraDefault) {
				if (data == null) return;
				MarkerSchedule.Begin();
				var cameraInfo = new GeometryBuilder.CameraInfo(camera);
				int c = 0;
				for (int i = 0; i < data.Length; i++) {
					if (data[i].isValid && data[i].meta.version >= versionThreshold && data[i].IsValidForCamera(camera, allowGizmos, allowCameraDefault)) {
						c++;
						data[i].Schedule(gizmos, ref cameraInfo);
					}
				}

				MarkerSchedule.End();

				// Ensure all jobs start to be executed on the worker threads now
				JobHandle.ScheduleBatchedJobs();

				MarkerBuild.Begin();
				for (int i = 0; i < data.Length; i++) {
					if (data[i].isValid && data[i].meta.version >= versionThreshold && data[i].IsValidForCamera(camera, allowGizmos, allowCameraDefault)) {
						data[i].BuildMeshes(gizmos);
					}
				}
				MarkerBuild.End();
			}

			/// <summary>
			/// Remove any existing dynamic meshes since we know we will not need them after this frame.
			/// We do not remove custom meshes or static ones because those may be kept between frames and cameras.
			/// </summary>
			public void PoolDynamicMeshes (DrawingData gizmos) {
				if (data == null) return;
				MarkerPool.Begin();
				for (int i = 0; i < data.Length; i++) {
					if (data[i].isValid) {
						data[i].PoolDynamicMeshes(gizmos);
					}
				}
				MarkerPool.End();
			}

			public void CollectMeshes (int versionThreshold, List<RenderedMeshWithType> meshes, Camera camera, bool allowGizmos, bool allowCameraDefault) {
				if (data == null) return;
				for (int i = 0; i < data.Length; i++) {
					if (data[i].isValid && data[i].meta.version >= versionThreshold && data[i].IsValidForCamera(camera, allowGizmos, allowCameraDefault)) {
						data[i].CollectMeshes(meshes);
					}
				}
			}

			public void FilterOldPersistentCommands (int version, int lastTickVersion, float time, int sceneModeVersion) {
				if (data == null) return;
				for (int i = 0; i < data.Length; i++) {
					if (data[i].isValid && data[i].type == ProcessedBuilderData.Type.Persistent) {
						data[i].SchedulePersistFilter(version, lastTickVersion, time, sceneModeVersion);
					}
				}
			}

			public bool SetVersion (Hasher hasher, int version) {
				if (data == null) return false;

				if (hash2index.TryGetValue(hasher.Hash, out var indices)) {
					UnityEngine.Assertions.Assert.IsTrue(indices.Count > 0);
					for (int id = 0; id < indices.Count; id++) {
						var i = indices[id];
						UnityEngine.Assertions.Assert.IsTrue(data[i].isValid);
						UnityEngine.Assertions.Assert.AreEqual(data[i].meta.hasher.Hash, hasher.Hash);
						data[i].meta.version = version;
					}
					return true;
				} else {
					return false;
				}
			}

			public bool SetVersion (RedrawScope scope, int version) {
				if (data == null) return false;
				bool found = false;

				for (int i = 0; i < data.Length; i++) {
					if (data[i].isValid && (data[i].meta.redrawScope1.id == scope.id || data[i].meta.redrawScope2.id == scope.id)) {
						data[i].meta.version = version;
						found = true;
					}
				}
				return found;
			}

			public bool SetCustomScope (Hasher hasher, RedrawScope scope) {
				if (data == null) return false;

				if (hash2index.TryGetValue(hasher.Hash, out var indices)) {
					UnityEngine.Assertions.Assert.IsTrue(indices.Count > 0);
					for (int id = 0; id < indices.Count; id++) {
						var i = indices[id];
						UnityEngine.Assertions.Assert.IsTrue(data[i].isValid);
						UnityEngine.Assertions.Assert.AreEqual(data[i].meta.hasher.Hash, hasher.Hash);
						data[i].meta.redrawScope2 = scope;
					}
					return true;
				} else {
					return false;
				}
			}

			public void ReleaseDataOlderThan (DrawingData gizmos, int version) {
				if (data == null) return;
				for (int i = 0; i < data.Length; i++) {
					if (data[i].isValid && data[i].meta.version < version) {
						Release(gizmos, i);
					}
				}
			}

			public void ReleaseAllWithHash (DrawingData gizmos, Hasher hasher) {
				if (data == null) return;
				for (int i = 0; i < data.Length; i++) {
					if (data[i].isValid && data[i].meta.hasher.Hash == hasher.Hash) {
						Release(gizmos, i);
					}
				}
			}

			public void Dispose (DrawingData gizmos) {
				if (data == null) return;
				for (int i = 0; i < data.Length; i++) {
					if (data[i].isValid) Release(gizmos, i);
					data[i].Dispose();
				}
				// Ensures calling Dispose multiple times is a NOOP
				data = null;
			}
		}

		[System.Flags]
		internal enum MeshType {
			Solid = 1 << 0,
			Lines = 1 << 1,
			Text = 1 << 2,
			// Set if the mesh is not a built-in mesh. These may have non-identity matrices set.
			Custom = 1 << 3,
			// If set for a custom mesh, the mesh will be pooled.
			// This is used for temporary custom meshes that are created by ALINE
			Pool = 1 << 4,
			BaseType = Solid | Lines | Text,
		}

		internal struct MeshWithType {
			public Mesh mesh;
			public MeshType type;
		}

		internal struct RenderedMeshWithType {
			public Mesh mesh;
			public MeshType type;
			public int drawingOrderIndex;
			// May only be set to non-white if type contains MeshType.Custom
			public Color color;
			// May only be set to a non-identity matrix if type contains MeshType.Custom
			public Matrix4x4 matrix;
		}

		internal BuilderDataContainer data;
		internal ProcessedBuilderDataContainer processedData;
		List<RenderedMeshWithType> meshes = new List<RenderedMeshWithType>();
		List<Mesh> cachedMeshes = new List<Mesh>();
		List<Mesh> stagingCachedMeshes = new List<Mesh>();
#if USE_RAW_GRAPHICS_BUFFERS
List<Mesh> stagingCachedMeshesDelay = new List<Mesh>();
#endif
int lastTimeLargestCachedMeshWasUsed = 0;
internal SDFLookupData fontData;
int currentDrawOrderIndex = 0;

		/// <summary>
		/// Incremented every time the editor goes from play mode -> edit mode, or edit mode -> play mode.
		/// Used to ensure that no WithDuration scopes survive this transition.
		///
		/// Normally it is not important, but when Unity's enter play mode settings have reload domain disabled
		/// then it can become important since this manager will survive the transition.
		/// </summary>
		internal int sceneModeVersion = 0;

		/// <summary>
		/// Slightly adjusted scene mode version.
		/// This takes into account `Application.isPlaying` too. It is possible for <see cref="sceneModeVersion"/> to be modified
		/// and then some gizmos are drawn before the actual play mode change happens (with the old Application.isPlaying) mode.
		///
		/// More precisely, what could happen without this adjustment is
		/// 1. EditorApplication.playModeStateChanged (PlayModeStateChange.ExitingPlayMode) fires which increments sceneModeVersion.
		/// 2. A final update loop runs with Application.isPlaying = true.
		/// 3. During this loop, a new command builder is created with the new sceneModeVersion and Application.isPlaying=true and is drawn to using a WithDuration scope.
		/// 4. The play mode changes to editor mode.
		/// 5. The WithDuration scope survives!
		///
		/// We cannot increment sceneModeVersion on PlayModeStateChange.ExitedPlayMode (not Exiting) instead, because some gizmos which we want to keep may
		/// be drawn before that event fires. Yay, Unity is so helpful.
		/// </summary>
		int adjustedSceneModeVersion {
			get {
				return sceneModeVersion + (Application.isPlaying ? 1000 : 0);
			}
		}

		internal int GetNextDrawOrderIndex () {
			currentDrawOrderIndex++;
			return currentDrawOrderIndex;
		}

		internal void PoolMesh (Mesh mesh) {
			// Note: clearing the mesh here will deallocate the vertex/index buffers
			// This is not good for performance as it will have to be allocated again (likely with the same size) in the next frame
			//mesh.Clear();
			stagingCachedMeshes.Add(mesh);
		}

		void SortPooledMeshes () {
			// TODO: Is accessing the vertex count slow?
			cachedMeshes.Sort((a, b) => b.vertexCount - a.vertexCount);
		}

		internal Mesh GetMesh (int desiredVertexCount) {
			if (cachedMeshes.Count > 0) {
				// Do a binary search to find the smallest cached mesh which is larger or equal to the desired vertex count
				// TODO: We should actually compare the byte size of the vertex buffer, not the number of vertices because
				// the vertex size can change depending on the mesh attribute layout.
				int mn = 0;
				int mx = cachedMeshes.Count;
				while (mx > mn + 1) {
					int mid = (mn+mx)/2;
					if (cachedMeshes[mid].vertexCount < desiredVertexCount) {
						mx = mid;
					} else {
						mn = mid;
					}
				}

				var res = cachedMeshes[mn];
				if (mn == 0) lastTimeLargestCachedMeshWasUsed = version;
				cachedMeshes.RemoveAt(mn);
				return res;
			} else {
				var mesh = new Mesh {
					hideFlags = HideFlags.DontSave
				};
				mesh.MarkDynamic();
				return mesh;
			}
		}

		internal void LoadFontDataIfNecessary () {
			if (fontData.material == null) {
				var font = DefaultFonts.LoadDefaultFont();
				fontData.Dispose();
				fontData = new SDFLookupData(font);
			}
		}

		static float CurrentTime {
			get {
				return Application.isPlaying ? Time.time : Time.realtimeSinceStartup;
			}
		}

		static void UpdateTime () {
			// Time.time cannot be accessed in the job system, so create a global variable which *can* be accessed.
			// It's not updated as frequently, but it's only used for the WithDuration method, so it should be ok
			SharedDrawingData.BurstTime.Data = CurrentTime;
		}

		/// <summary>
		/// Get an empty builder for queuing drawing commands.
		///
		/// <code>
		/// // Create a new CommandBuilder
		/// using (var draw = DrawingManager.GetBuilder()) {
		///     // Use the exact same API as the global Draw class
		///     draw.WireBox(Vector3.zero, Vector3.one);
		/// }
		/// </code>
		/// See: <see cref="Drawing.CommandBuilder"/>
		/// </summary>
		/// <param name="renderInGame">If true, this builder will be rendered in standalone games and in the editor even if gizmos are disabled.
		/// If false, it will only be rendered in the editor when gizmos are enabled.</param>
		public CommandBuilder GetBuilder (bool renderInGame = false) {
			UpdateTime();
			return new CommandBuilder(this, Hasher.NotSupplied, frameRedrawScope, default, !renderInGame, false, adjustedSceneModeVersion);
		}

		internal CommandBuilder GetBuiltInBuilder (bool renderInGame = false) {
			UpdateTime();
			return new CommandBuilder(this, Hasher.NotSupplied, frameRedrawScope, default, !renderInGame, true, adjustedSceneModeVersion);
		}

		/// <summary>
		/// Get an empty builder for queuing drawing commands.
		///
		/// See: <see cref="Drawing.CommandBuilder"/>
		/// </summary>
		/// <param name="renderInGame">If true, this builder will be rendered in standalone games and in the editor even if gizmos are disabled.</param>
		public CommandBuilder GetBuilder (RedrawScope redrawScope, bool renderInGame = false) {
			UpdateTime();
			return new CommandBuilder(this, Hasher.NotSupplied, frameRedrawScope, redrawScope, !renderInGame, false, adjustedSceneModeVersion);
		}

		/// <summary>
		/// Get an empty builder for queuing drawing commands.
		///
		/// See: <see cref="Drawing.CommandBuilder"/>
		/// </summary>
		/// <param name="renderInGame">If true, this builder will be rendered in standalone games and in the editor even if gizmos are disabled.</param>
		public CommandBuilder GetBuilder (Hasher hasher, RedrawScope redrawScope = default, bool renderInGame = false) {
			// The user is going to rebuild the data with the given hash
			// Let's clear the previous data with that hash since we know it is not needed any longer.
			// Do not do this if a hash is not given.
			if (!hasher.Equals(Hasher.NotSupplied)) DiscardData(hasher);
			UpdateTime();
			return new CommandBuilder(this, hasher, frameRedrawScope, redrawScope, !renderInGame, false, adjustedSceneModeVersion);
		}

		/// <summary>Material to use for surfaces</summary>
		public Material surfaceMaterial;

		/// <summary>Material to use for lines</summary>
		public Material lineMaterial;

		/// <summary>Material to use for text</summary>
		public Material textMaterial;

		public DrawingSettings settingsAsset;

		public DrawingSettings.Settings settingsRef {
			get {
				if (settingsAsset == null) {
					settingsAsset = DrawingSettings.GetSettingsAsset();
					if (settingsAsset == null) {
						throw new System.InvalidOperationException("ALINE settings could not be found");
					}
				}
				return settingsAsset.settings;
			}
		}

		public int version { get; private set; } = 1;
		int lastTickVersion;
		int lastTickVersion2;
		Dictionary<int, GameObject> persistentRedrawScopes = new Dictionary<int, GameObject>();
#if ALINE_TRACK_REDRAW_SCOPE_LEAKS
Dictionary<int, String> persistentRedrawScopeInfos = new Dictionary<int, String>();
#endif
internal System.Runtime.InteropServices.GCHandle gizmosHandle;

		public RedrawScope frameRedrawScope;

		public GameObject GetAssociatedGameObject (RedrawScope scope) {
			if (persistentRedrawScopes.TryGetValue(scope.id, out var go)) return go;
			return null;
		}

		struct Range {
			public int start;
			public int end;
		}

		Dictionary<Camera, Range> cameraVersions = new Dictionary<Camera, Range>();

		internal static readonly ProfilerMarker MarkerScheduleJobs = new ProfilerMarker("ScheduleJobs");
		internal static readonly ProfilerMarker MarkerAwaitUserDependencies = new ProfilerMarker("Await user dependencies");
		internal static readonly ProfilerMarker MarkerSchedule = new ProfilerMarker("Schedule");
		internal static readonly ProfilerMarker MarkerBuild = new ProfilerMarker("Build");
		internal static readonly ProfilerMarker MarkerPool = new ProfilerMarker("Pool");
		internal static readonly ProfilerMarker MarkerRelease = new ProfilerMarker("Release");
		internal static readonly ProfilerMarker MarkerBuildMeshes = new ProfilerMarker("Build Meshes");
		internal static readonly ProfilerMarker MarkerCollectMeshes = new ProfilerMarker("Collect Meshes");
		internal static readonly ProfilerMarker MarkerSortMeshes = new ProfilerMarker("Sort Meshes");
		internal static readonly ProfilerMarker LeakTracking = new ProfilerMarker("RedrawScope Leak Tracking");

		void DiscardData (Hasher hasher) {
			processedData.ReleaseAllWithHash(this, hasher);
		}

		internal void OnChangingPlayMode () {
			sceneModeVersion++;

#if UNITY_EDITOR
// If we are in the editor, we schedule a callback to check if any RedrawScope objects were not disposed.
// OnChangingPlayMode will run before the scene is destroyed. So we know that any persistent redraw scopes
// that are alive right now should be destroyed soon.
// We wait a few updates to allow the scene to be destroyed before we check for leaks.
// EditorApplication.delayCall may be called before the scene has actually been destroyed.
// Usually it has, but in particular if the user double-clicks the play button to start and then immediately
// stop the game, then it may run before the scene has been destroyed.
var shouldBeDestroyed = this.persistentRedrawScopes.Keys.ToArray();
UnityEditor.EditorApplication.CallbackFunction checkLeaks = null;
int remainingFrames = 2;
checkLeaks = () => {
if (remainingFrames > 0) {
remainingFrames--;
return;
}
UnityEditor.EditorApplication.delayCall -= checkLeaks;
int leaked = 0;
foreach (var v in shouldBeDestroyed) {
if (persistentRedrawScopes.ContainsKey(v)) leaked++;
}
if (leaked > 0) {
#if ALINE_TRACK_REDRAW_SCOPE_LEAKS
UnityEngine.Debug.LogError(leaked + " RedrawScope objects were not disposed. Make sure to dispose them when you are done with them, otherwise this will lead to a memory leak and potentially a performance issue.");
foreach (var v in shouldBeDestroyed) {
if (persistentRedrawScopes.ContainsKey(v)) {
UnityEngine.Debug.LogError("RedrawScope leaked. Allocated from:\n" + persistentRedrawScopeInfos[v]);
}
}
#else
UnityEngine.Debug.LogError(leaked + " RedrawScope objects were not disposed. Make sure to dispose them when you are done with them, otherwise this will lead to a memory leak and potentially a performance issue.\nEnable ALINE_TRACK_REDRAW_SCOPE_LEAKS in the scripting define symbols to track the leaks more accurately.");
#endif
foreach (var v in shouldBeDestroyed) {
persistentRedrawScopes.Remove(v);
#if ALINE_TRACK_REDRAW_SCOPE_LEAKS
persistentRedrawScopeInfos.Remove(v);
#endif
}
}
};
UnityEditor.EditorApplication.delayCall += checkLeaks;
#endif
}

		/// <summary>
		/// Schedules the meshes for the specified hash to be drawn.
		/// Returns: False if there is no cached mesh for this hash, you may want to
		///  submit one in that case. The draw command will be issued regardless of the return value.
		/// </summary>
		public bool Draw (Hasher hasher) {
			if (hasher.Equals(Hasher.NotSupplied)) throw new System.ArgumentException("Invalid hash value");
			return processedData.SetVersion(hasher, version);
		}

		/// <summary>
		/// Schedules the meshes for the specified hash to be drawn.
		/// Returns: False if there is no cached mesh for this hash, you may want to
		///  submit one in that case. The draw command will be issued regardless of the return value.
		///
		/// This overload will draw all meshes within the specified redraw scope.
		/// Note that if they had been drawn with another redraw scope earlier they will be removed from that scope.
		/// </summary>
		public bool Draw (Hasher hasher, RedrawScope scope) {
			if (hasher.Equals(Hasher.NotSupplied)) throw new System.ArgumentException("Invalid hash value");
			if (scope.isValid) processedData.SetCustomScope(hasher, scope);
			return processedData.SetVersion(hasher, version);
		}

		/// <summary>Schedules all meshes that were drawn the last frame with this redraw scope to be drawn again</summary>
		internal void Draw (RedrawScope scope) {
			if (scope.isValid) processedData.SetVersion(scope, version);
		}

		internal void DrawUntilDisposed (RedrawScope scope, GameObject associatedGameObject) {
			if (scope.isValid) {
				Draw(scope);
				persistentRedrawScopes.Add(scope.id, associatedGameObject);
#if ALINE_TRACK_REDRAW_SCOPE_LEAKS && UNITY_EDITOR
LeakTracking.Begin();
persistentRedrawScopeInfos[scope.id] = new System.Diagnostics.StackTrace().ToString();
LeakTracking.End();
#endif
}
}

		internal void DisposeRedrawScope (RedrawScope scope) {
			if (scope.isValid) {
				processedData.SetVersion(scope, -1);
				persistentRedrawScopes.Remove(scope.id);
#if ALINE_TRACK_REDRAW_SCOPE_LEAKS && UNITY_EDITOR
persistentRedrawScopeInfos.Remove(scope.id);
#endif
}
}

		void RefreshRedrawScopes () {
#if UNITY_EDITOR && UNITY_2020_1_OR_NEWER
var currentStage = UnityEditor.SceneManagement.StageUtility.GetCurrentStage();
var isInNonMainStage = currentStage != UnityEditor.SceneManagement.StageUtility.GetMainStage();
#endif
foreach (var scope in persistentRedrawScopes) {
#if UNITY_EDITOR && UNITY_2020_1_OR_NEWER
// True if the scene is in isolation mode (e.g. focusing on a single prefab) and this object is not part of that sub-stage
var disabledDueToIsolationMode = isInNonMainStage && scope.Value != null && UnityEditor.SceneManagement.StageUtility.GetStage(scope.Value) != currentStage;
if (disabledDueToIsolationMode) continue;
#endif
processedData.SetVersion(new RedrawScope(this, scope.Key), version);
}
}

		void CleanupOldCameras () {
			// Remove cameras that have not been used for a while, to avoid memory leaks.
			// We keep them for a few frames for debugging purposes.
			foreach (var item in cameraVersions) {
				if (item.Value.end < lastTickVersion - 10) {
					cameraVersions.Remove(item.Key);
					// Break to avoid modifying the collection while iterating over it
					// In the rare case that multiple cameras needed to be removed, we can continue removing them next frame.
					break;
				}
			}
		}

		public void TickFramePreRender () {
			data.DisposeCommandBuildersWithJobDependencies(this);
			// Remove persistent commands that have timed out.
			// When not playing then persistent commands are never drawn twice
			processedData.FilterOldPersistentCommands(version, lastTickVersion, CurrentTime, adjustedSceneModeVersion);
			CleanupOldCameras();

			RefreshRedrawScopes();

			// All cameras rendered between the last tick and this one will have
			// a version that is at least lastTickVersion + 1.
			// However the user may want to reuse meshes from the previous frame (see Draw(Hasher)).
			// This requires us to keep data from one more frame and thus we use lastTickVersion2 + 1
			// TODO: One frame should be enough, right?
			processedData.ReleaseDataOlderThan(this, lastTickVersion2 + 1);
			lastTickVersion2 = lastTickVersion;
			lastTickVersion = version;
			currentDrawOrderIndex = 0;

			// Pooled meshes from two frames ago can now be used.
#if USE_RAW_GRAPHICS_BUFFERS
// One would think that pooled meshes from only one frame ago can be used.
// And yes, Unity will allow this, but the GPU may still be working on the meshes from the previous frame.
// Therefore, when we try to write to the raw mesh vertex buffers Unity will block until the previous
// frame's GPU work is done, which may take a long time.
// Using "double buffering" for the meshes that are updated every frame is more efficient.
// When we use simplified methods for setting the vertex/index data we don't have to do this
// because Unity seems to manage an upload buffer or something for us.
cachedMeshes.AddRange(stagingCachedMeshesDelay);
// Move stagingCachedMeshes to stagingCachedMeshesDelay, and make stagingCachedMeshes an empty list.
stagingCachedMeshesDelay.Clear();
var tmp = stagingCachedMeshesDelay;
stagingCachedMeshesDelay = stagingCachedMeshes;
stagingCachedMeshes = tmp;
#else
cachedMeshes.AddRange(stagingCachedMeshes);
stagingCachedMeshes.Clear();
#endif
SortPooledMeshes();

			// If the largest cached mesh hasn't been used in a while, then remove it to free up the memory
			if (version - lastTimeLargestCachedMeshWasUsed > 60 && cachedMeshes.Count > 0) {
				Mesh.DestroyImmediate(cachedMeshes[0]);
				cachedMeshes.RemoveAt(0);
				lastTimeLargestCachedMeshWasUsed = version;
			}

			// TODO: Filter cameraVersions to avoid memory leak
		}

		public void PostRenderCleanup () {
			MarkerRelease.Begin();
			data.ReleaseAllUnused();
			MarkerRelease.End();
			version++;
		}

		class MeshCompareByDrawingOrder : IComparer<RenderedMeshWithType> {
			public int Compare (RenderedMeshWithType a, RenderedMeshWithType b) {
				// Extract if the meshes are Solid/Lines/Text
				var ta = (int)a.type & 0x7;
				var tb = (int)b.type & 0x7;
				return ta != tb ? ta - tb : a.drawingOrderIndex - b.drawingOrderIndex;
			}
		}

		static readonly MeshCompareByDrawingOrder meshSorter = new MeshCompareByDrawingOrder();
		// Temporary array, cached to avoid allocations
		Plane[] frustrumPlanes = new Plane[6];
		// Temporary block, cached to avoid allocations
		MaterialPropertyBlock customMaterialProperties = new MaterialPropertyBlock();

		int totalMemoryUsage => this.data.memoryUsage + this.processedData.memoryUsage;

		void LoadMaterials () {
			// Make sure the material references are correct

			// Note: When importing the package for the first time the asset database may not be up to date and Resources.Load may return null.

			if (surfaceMaterial == null) {
				surfaceMaterial = Resources.Load<Material>("aline_surface_mat");
			}
			if (lineMaterial == null) {
				lineMaterial = Resources.Load<Material>("aline_outline_mat");
			}
			if (fontData.material == null) {
				var font = DefaultFonts.LoadDefaultFont();
				fontData.Dispose();
				fontData = new SDFLookupData(font);
			}
		}

		public DrawingData() {
			gizmosHandle = System.Runtime.InteropServices.GCHandle.Alloc(this, System.Runtime.InteropServices.GCHandleType.Weak);
			LoadMaterials();
		}

		static int CeilLog2 (int x) {
			// Should use `math.ceillog2` whenever we next raise the minimum compatible version of the mathematics package.
			// This variant is prone to floating point errors.
			return (int)math.ceil(math.log2(x));
		}

		/// <summary>
		/// Wrapper for different kinds of commands buffers.
		///
		/// Annoyingly, they all use a CommandBuffer in the end, but the universal render pipeline wraps it in a RasterCommandBuffer,
		/// and it's not possible to get the underlaying CommandBuffer.
		/// </summary>
		public struct CommandBufferWrapper {
			public CommandBuffer cmd;
#if MODULE_RENDER_PIPELINES_UNIVERSAL_17_0_0_OR_NEWER
public bool allowDisablingWireframe;
public RasterCommandBuffer cmd2;
#endif

#if UNITY_2023_1_OR_NEWER
public void SetWireframe (bool enable) {
if (cmd != null) {
cmd.SetWireframe(enable);
}
#if MODULE_RENDER_PIPELINES_UNIVERSAL_17_0_0_OR_NEWER
else if (cmd2 != null) {
if (allowDisablingWireframe) cmd2.SetWireframe(enable);
}
#endif
}
#endif

			public void DrawMesh (Mesh mesh, Matrix4x4 matrix, Material material, int submeshIndex, int shaderPass, MaterialPropertyBlock properties) {
				if (cmd != null) {
					cmd.DrawMesh(mesh, matrix, material, submeshIndex, shaderPass, properties);
				}
#if MODULE_RENDER_PIPELINES_UNIVERSAL_17_0_0_OR_NEWER
else if (cmd2 != null) {
cmd2.DrawMesh(mesh, matrix, material, submeshIndex, shaderPass, properties);
}
#endif
}
}

		/// <summary>Call after all <see cref="Draw"/> commands for the frame have been done to draw everything.</summary>
		/// <param name="allowCameraDefault">Indicates if built-in command builders and custom ones without a custom CommandBuilder.cameraTargets should render to this camera.</param>
		public void Render (Camera cam, bool allowGizmos, CommandBufferWrapper commandBuffer, bool allowCameraDefault) {
			// Early out when there's nothing to render
			if (processedData.isEmpty) return;

			LoadMaterials();

			// Warn if the materials could not be found
			if (surfaceMaterial == null || lineMaterial == null) {
				// Note that when the package is installed Unity may start rendering things and call this method before it has initialized the Resources folder with the materials.
				// We don't want to throw exceptions in that case because once the import finishes everything will be good.
				// UnityEngine.Debug.LogWarning("Looks like you just installed ALINE. The ALINE package will start working after the next script recompilation.");
				return;
			}

			if (!cameraVersions.TryGetValue(cam, out Range cameraRenderingRange)) {
				cameraRenderingRange = new Range { start = int.MinValue, end = int.MinValue };
			}

			// Check if the last time the camera was rendered
			// was during the current frame.
			if (cameraRenderingRange.end > lastTickVersion) {
				// In some cases a camera is rendered multiple times per frame.
				// In this case we just extend the end of the drawing range up to the current version.
				// The reasoning is that all times the camera is rendered in a frame
				// all things should be drawn.
				// If we did update the start of the range then things would only be drawn
				// the first time the camera was rendered in the frame.

				// Sometimes the scene view will be rendered twice in a single frame
				// due to some internal Unity tooltip code.
				// Without this fix the scene view camera may end up showing no gizmos
				// for a single frame.
				cameraRenderingRange.end = version + 1;
			} else {
				// This is the common case: the previous time the camera was rendered
				// it rendered all versions lower than cameraRenderingRange.end.
				// So now we start by rendering from that version.
				cameraRenderingRange = new Range  { start = cameraRenderingRange.end, end = version + 1 };
			}

			// Don't show anything rendered before the last frame.
			// If the camera has been turned off for a while and then suddenly starts rendering again
			// we want to make sure that we don't render meshes from multiple frames.
			// This happens often in the unity editor as the scene view and game view often skip
			// rendering many frames when outside of play mode.
			cameraRenderingRange.start = Mathf.Max(cameraRenderingRange.start, lastTickVersion2 + 1);

			var settings = settingsRef;

#if UNITY_2023_1_OR_NEWER
bool skipDueToWireframe = false;
commandBuffer.SetWireframe(false);
#else
// If GL.wireframe is enabled (the Wireframe mode in the scene view settings)
// then I have found no way to draw gizmos in a good way.
// It's best to disable gizmos altogether to avoid drawing wireframe versions of gizmo meshes.
bool skipDueToWireframe = GL.wireframe;
#endif

			if (!skipDueToWireframe) {
				MarkerBuildMeshes.Begin();
				processedData.SubmitMeshes(this, cam, cameraRenderingRange.start, allowGizmos, allowCameraDefault);
				MarkerBuildMeshes.End();
				MarkerCollectMeshes.Begin();
				meshes.Clear();
				processedData.CollectMeshes(cameraRenderingRange.start, meshes, cam, allowGizmos, allowCameraDefault);
				processedData.PoolDynamicMeshes(this);
				MarkerCollectMeshes.End();

				// Early out if nothing is being rendered
				if (meshes.Count > 0) {
					MarkerSortMeshes.Begin();
					// Note that a stable sort is required as some meshes may have the same sorting index
					// but those meshes will have a consistent ordering between them in the list
					meshes.Sort(meshSorter);
					MarkerSortMeshes.End();

					var planes = frustrumPlanes;
					GeometryUtility.CalculateFrustumPlanes(cam, planes);

					int colorID = Shader.PropertyToID("_Color");
					int colorFadeID = Shader.PropertyToID("_FadeColor");
					var solidBaseColor = new Color(1, 1, 1, settings.solidOpacity);
					var solidFadeColor = new Color(1, 1, 1, settings.solidOpacityBehindObjects);
					var lineBaseColor = new Color(1, 1, 1, settings.lineOpacity);
					var lineFadeColor = new Color(1, 1, 1, settings.lineOpacityBehindObjects);
					var textBaseColor = new Color(1, 1, 1, settings.textOpacity);
					var textFadeColor = new Color(1, 1, 1, settings.textOpacityBehindObjects);

					// The meshes list is already sorted as first surfaces, then lines, then text
					for (int i = 0; i < meshes.Count;) {
						int meshEndIndex = i+1;
						var tp = meshes[i].type & MeshType.BaseType;
						while (meshEndIndex < meshes.Count && (meshes[meshEndIndex].type & MeshType.BaseType) == tp) meshEndIndex++;

						Material mat;
						customMaterialProperties.Clear();
						switch (tp) {
						case MeshType.Solid:
							mat = surfaceMaterial;
							customMaterialProperties.SetColor(colorID, solidBaseColor);
							customMaterialProperties.SetColor(colorFadeID, solidFadeColor);
							break;
						case MeshType.Lines:
							mat = lineMaterial;
							customMaterialProperties.SetColor(colorID, lineBaseColor);
							customMaterialProperties.SetColor(colorFadeID, lineFadeColor);
							break;
						case MeshType.Text:
							mat = fontData.material;
							customMaterialProperties.SetColor(colorID, textBaseColor);
							customMaterialProperties.SetColor(colorFadeID, textFadeColor);
							break;
						default:
							throw new System.InvalidOperationException("Invalid mesh type");
						}

						for (int pass = 0; pass < mat.passCount; pass++) {
							for (int j = i; j < meshEndIndex; j++) {
								var mesh = meshes[j];
								if ((mesh.type & MeshType.Custom) != 0) {
									// This mesh type may have a matrix set. So we need to handle that
									if (GeometryUtility.TestPlanesAABB(planes, TransformBoundingBox(mesh.matrix, mesh.mesh.bounds))) {
										// Custom meshes may have different colors
										customMaterialProperties.SetColor(colorID, solidBaseColor * mesh.color);
										commandBuffer.DrawMesh(mesh.mesh, mesh.matrix, mat, 0, pass, customMaterialProperties);
										customMaterialProperties.SetColor(colorID, solidBaseColor);
									}
								} else if (GeometryUtility.TestPlanesAABB(planes, mesh.mesh.bounds)) {
									// This mesh is drawn with an identity matrix
									commandBuffer.DrawMesh(mesh.mesh, Matrix4x4.identity, mat, 0, pass, customMaterialProperties);
								}
							}
						}

						i = meshEndIndex;
					}

					meshes.Clear();
				}
			}

			cameraVersions[cam] = cameraRenderingRange;
		}

		/// <summary>Returns a new axis aligned bounding box that contains the given bounding box after being transformed by the matrix</summary>
		static Bounds TransformBoundingBox (Matrix4x4 matrix, Bounds bounds) {
			var mn = bounds.min;
			var mx = bounds.max;
			// Create the bounding box from the bounding box of the transformed
			// 8 points of the original bounding box.
			var newBounds = new Bounds(matrix.MultiplyPoint(mn), Vector3.zero);

			newBounds.Encapsulate(matrix.MultiplyPoint(new Vector3(mn.x, mn.y, mx.z)));

			newBounds.Encapsulate(matrix.MultiplyPoint(new Vector3(mn.x, mx.y, mn.z)));
			newBounds.Encapsulate(matrix.MultiplyPoint(new Vector3(mn.x, mx.y, mx.z)));

			newBounds.Encapsulate(matrix.MultiplyPoint(new Vector3(mx.x, mn.y, mn.z)));
			newBounds.Encapsulate(matrix.MultiplyPoint(new Vector3(mx.x, mn.y, mx.z)));

			newBounds.Encapsulate(matrix.MultiplyPoint(new Vector3(mx.x, mx.y, mn.z)));
			newBounds.Encapsulate(matrix.MultiplyPoint(new Vector3(mx.x, mx.y, mx.z)));
			return newBounds;
		}

		/// <summary>
		/// Destroys all cached meshes.
		/// Used to make sure that no memory leaks happen in the Unity Editor.
		/// </summary>
		public void ClearData () {
			gizmosHandle.Free();
			data.Dispose();
			processedData.Dispose(this);

			for (int i = 0; i < cachedMeshes.Count; i++) {
				Mesh.DestroyImmediate(cachedMeshes[i]);
			}
			cachedMeshes.Clear();

			UnityEngine.Assertions.Assert.IsTrue(meshes.Count == 0);
			fontData.Dispose();
		}
	}
}
// Disable the warning: "Field 'DependencyCheck.Dependency.name' is never assigned to, and will always have its default value null"
#pragma warning disable 649
using UnityEditor;
using System.Linq;

namespace Drawing.Util {
[InitializeOnLoad]
static class DependencyCheck {
struct Dependency {
public string name;
public string version;
}

		static DependencyCheck() {
			var missingDependencies = new Dependency[] {
#if !MODULE_BURST
new Dependency {
name = "com.unity.burst",
version = "1.2.1-preview",
},
#endif
#if !MODULE_MATHEMATICS
new Dependency {
name = "com.unity.mathematics",
version = "1.1.0",
},
#endif
#if !MODULE_COLLECTIONS
new Dependency {
name = "com.unity.collections",
version = "0.4.0-preview",
},
#endif
};

			if (missingDependencies.Length > 0) {
				string missing = string.Join(", ", missingDependencies.Select(p => p.name + " (" + p.version + ")"));
				bool res = EditorUtility.DisplayDialog("Missing dependencies", "The packages " + missing + " are required by ALINE but they are not installed, or the installed versions are too old. Do you want to install the latest versions of the packages?", "Ok", "Cancel");
				if (res) {
					foreach (var dep in missingDependencies) {
						UnityEditor.PackageManager.Client.Add(dep.name);
					}
				}
			}
		}
	}
}
## 1.7.8 (2025-05-06)
    - Fixed a minor GC allocation happening every frame when using URP.
    - Improved performance in standalone builds when nothing is being rendered.
    - Fixed a significant memory leak when starting unity in batch mode.

## 1.7.7 (2025-03-20)
    - Added a new tutorial on using caching to improve performance: \ref caching.

    - Fixed \reflink{Draw.xz.SolidRectangle} would render the rectangle in the XY plane, instead of the XZ plane.
    - Fixed an exception could be thrown when cameras were rendered without a color target.
    - Added \reflink{PolylineWithSymbol.up}, to allow you to configure the orientation of the symbols. Previously it was hardcoded to Vector3.up.
    - Added an offset parameter to \reflink{PolylineWithSymbol}, to allow shifting all symbols along the polyline. This is useful for animations.
    - Fixed various minor glitches that could happen when using \reflink{PolylineWithSymbol}.

## 1.7.6 (2024-10-14)
    - Fixed a compatibility issue with the high definition render pipeline, accidentally introduced in 1.7.5.
    - Fixed gizmos were not rendered when opening prefab assets in isolation mode and the high definition render pipeline was used.

## 1.7.5 (2024-08-06)
    - Fixed a memory leak causing references to destroyed cameras to be kept around.
    - Fixed \reflink{Draw.xy.SolidCircle(float3,float,float,float)} and \reflink{Draw.xz.SolidCircle(float3,float,float,float)} would render the circles in the wrong location.
    - Reduced overhead when rendering gizmos.
    - Each component type now shows up as a scope in the Unity Profiler when rendering their gizmos.
    - Worked around a limitation in Unity's HDRP renderer caused errors to be logged constantly when forward rendering MSAA was enabled. Depth testing will now be disabled in this case, and a single warning will be logged.
        Unfortunately there's nothing I can do to fix the underlying issue, since it's a limitation in Unity's HDRP renderer.

## 1.7.4 (2024-02-13)
    - Fixed compatibility with HDRP render pipeline.
    - Improved performance when there are many cameras rendered during the same frame.

## 1.7.3 (2024-02-07)
    - Improved performance when there are lots of components inheriting from \reflink{MonoBehaviourGizmos}, but they do not actually override the DrawGizmos method.
    - Fixed compatibility with Universal Render Pipeline package version 15 and 16 (regression in 1.7.2).

## 1.7.2 (2024-02-06)
    - Improved performance of \reflink{Draw.WireCylinder} and \reflink{Draw.WireCapsule}.
    - Fixed a memory leak that could happen if you used a lot of custom command builders.
    - Added an option to the project settings to increase or decrease the resolution of circles.
    - Improved compatibility with Universal Render Pipeline package version 17.

## 1.7.1 (2023-11-14)
    - Removed "com.unity.jobs" as a dependency, since it has been replaced by the collections package.
    - Added support for rendering gizmos while the scene view is in wireframe mode. This is supported in Unity 2023.1 and up.
    - Added \reflink{CommandBuilder.DashedLine}.
        \shadowimage{rendered/dashedline.png}
    - Added \reflink{CommandBuilder.DashedPolyline}.
        \shadowimage{rendered/dashedpolyline.png}

## 1.7.0 (2023-10-17)
    - Added a much more ergonomic way to draw using 2D coordinates. Take a look at \ref 2d-drawing for more info.
        \shadowimage{rendered/drawxy@8x.png}
    - Deprecated several methods like \reflink{Draw.CircleXY} and \reflink{Draw.CircleXZ} to instead use the new 2D methods (Draw.xy.Circle and Draw.xz.Circle).
        The old ones will continue to work for the time being, but they will be removed in a future update.
    - Removed some shader code which was not supported on WebGL.
    - Added \reflink{CommandBuilder2D.WirePill}
        \shadowimage{rendered/wirepill.png}
    - Added \reflink{CommandBuilder.SolidTriangle}
        \shadowimage{rendered/solidtriangle.png}
    - Added an overload of \reflink{Draw.Polyline} which takes an IReadOnlyList<T>.
    - Added \reflink{CommandBuilder.PolylineWithSymbol}
        \shadowimage{rendered/polylinewithsymbol.png}
    - Added an overload of \reflink{CommandBuilder.WireMesh} that takes a NativeArray with vertices, and one with triangles.
    - Improved look of \reflink{Draw.ArrowheadArc} when using a line width greater than 1.
    - Improved performance when there are lots of objects in the scene inheriting from \reflink{MonoBehaviourGizmos}.
    - Significantly reduced main-thread load when drawing in many situations by improving the Color to Color32 conversion performance.
        Turns out Unity's built-in one is not the fastest.
        In Burst I've cranked it up even more by using a SIMDed conversion function.
        Common improvements are around 10% faster, but in tight loops it can be up to 50% faster.
    - Improved performance of \reflink{Draw.WireBox}.
    - Improved performance of drawing circles and arcs.
    - Fixed name collision when both the A* Pathfinding Project and ALINE were installed in a project. This could cause the warning "There are 2 settings providers with the same name Project/ALINE." to be logged to the console.
    - Fixed Draw.WireBox reserving the wrong amount of memory, which could lead to an exception being thrown.
    - Fixed lines would be drawn slightly incorrectly at very shallow camera angles.
    - Fixed a memory leak which could happen if the game was not running, and the scene view was not being re-rendered, and a script was queuing drawing commands from an editor script repeatedly.
        Drawing commands will now get discarded after 10 seconds if no rendering happens to avoid leaking memory indefinitely.
    - Fixed a memory leak which could happen if the game was not running in the editor, and no cameras were being rendered (e.g. on a server).
    - Fixed shader compilation errors when deploying for PlayStation 5.
    - Fixed circles with a normal of exactly (0,-1,0) would not be rendered.
    - Changed \reflink{RedrawScope} to continue drawing items until it is disposed, instead of requiring one to call the scope.Draw method every frame.
    - Allow a \reflink{RedrawScope} to be stored in unmanaged ECS components and systems.
    - Fixed \reflink{Draw.Arrow} would draw a slightly narrower arrow head when the line was pointed in certain directions.
    - Added an overload for 3x3 matrices: \reflink{Draw.WithMatrix(float3x3)}.
    - Changed the behaviour for \reflink{RedrawScope}s. Previously they would continue drawing as long as you called RedrawScope.Draw every frame.
        Now they will continue drawing until you dispose them. This makes them just nicer to use for most cases.
        This is a breaking change, but since RedrawSopes have so far been a completely undocumented feature, I expect that no, or very few people, use them.
    - Fixed compatibility with XBox.
    - Fixed only the base camera in a camera stack would render gizmos.

## 1.6.4 (2022-09-17)
    - \reflink{CommandBuilder.DisposeAfter} will now block on the given dependency before rendering the current frame by default.
        This reduces the risk of flickering when using ECS systems as they may otherwise not have completed their work before the frame is rendered.
        You can pass \reflink{AllowedDelay.Infinite} to disable this behavior for long-running jobs.
    - Fixed recent regression causing drawing to fail in standalone builds.

## 1.6.3 (2022-09-15)
    - Added \reflink{LabelAlignment.withPixelOffset}.
    - Fixed \reflink{LabelAlignment} had top and bottom alignment swapped. So for example \reflink{LabelAlignment.TopLeft} was actually \reflink{LabelAlignment.BottomLeft}.
    - Fixed shaders would sometimes cause compilation errors, especially if you changed render pipelines.
    - Improved sharpness of \reflink{Draw.Label2D} and \reflink{Draw.Label3D} when using small font-sizes.\n
        <table>
        <tr><td>Before</td><td>After</td></tr>
        <tr>
        <td>
        \shadowimage{changelog/text_blurry_small.png}
        </td>
        <td>
        \shadowimage{changelog/text_sharp_small.png}
        </td>
        </table>
    - Text now fades out slightly when behind or inside other objects. The fade out amount can be controlled in the project settings:
        \shadowimage{changelog/text_opacity.png}
    - Fixed \reflink{Draw.Label2D} and \reflink{Draw.Label3D} font sizes would be incorrect (half as large) when the camera was in orthographic mode.
    - Fixed \reflink{Draw.WireCapsule} and \reflink{Draw.WireCylinder} would render incorrectly in certain orientations.

## 1.6.2 (2022-09-05)
    - Fix typo causing prefabs to always be drawn in the scene view in Unity versions earlier than 2022.1, even if they were not even added to the scene.

## 1.6.1 (2022-08-31)
    - Fix vertex buffers not getting resized correctly. This could cause exceptions to be logged sometimes. Regression in 1.6.

## 1.6 (2022-08-27)
    - Fixed documentation and changelog URLs in the package manager.
    - Fixed dragging a prefab into the scene view would instantiate it, but gizmos for scripts attached to it would not work.
    - Fixed some edge cases in \reflink{Draw.WireCapsule} and \reflink{Draw.WireCapsule} which could cause NaNs and other subtle errors.
    - Improved compatibility with WebGL as well as Intel GPUs on Mac.
    - Added warning when using HDRP and custom passes are disabled.
    - Improved performance of watching for destroyed objects.
    - Reduced overhead when having lots of objects inheriting from \reflink{MonoBehaviourGizmos}.
    - It's now possible to enable/disable gizmos for component types via the Unity Scene View Gizmos menu when using render pipelines in Unity 2022.1+.
        In earlier versions of Unity, a limited API made this impossible.
    - Made it possible to adjust the global opacity of gizmos in the Unity Project Settings.
        \shadowimage{changelog/settings.png}

## 1.5.3 (2022-05-14)
    - Breaking changes
        - The minimum supported Unity version is now 2020.3.
    - The URP 2D renderer now has support for all features required by ALINE. So the warning about it not being supported has been removed.
    - Fixed windows newlines (\\n\\r) would show up as a newline and a question mark instead of just a newline.
    - Fixed compilation errors when using the Unity.Collections package between version 0.8 and 0.11.
    - Improved performance in some edge cases.
    - Fixed \reflink{Draw.SolidMesh} with a non-white color could affect the color of unrelated rendered lines. Thanks Chris for finding and reporting the bug.
    - Fixed an exception could be logged when drawing circles with a zero or negative line width.
    - Fixed various compilation errors that could show up when using newer versions of the burst package.

## 1.5.2 (2021-11-09)
    - Fix gizmos would not show up until you selected the camera if you had just switched to the universal render pipeline.
    - Improved performance of drawing lines by more efficiently sending the data to the shader.
        This has the downside that shader target 4.5 is now required. I don't think this should be a big deal nowadays, but let me know if things don't work on your platform.
        This was originally introduced in 1.5.0, but reverted in 1.5.1 due to some compatibility issues causing rendering to fail for some project configurations. I think those issues should be resolved now.

## 1.5.1 (2021-10-28)
    - Reverted "Improved performance of drawing lines by more efficiently sending the data to the shader." from 1.5.0.
        It turns out this caused issues for some users and could result in gizmos not showing at all.
        I'll try to figure out a solution and bring the performance improvements back.

## 1.5 (2021-10-27)
    - Added support FixedStrings in \reflink{Draw.Label2D(float3,FixedString32Bytes,float)}, which means it can be used inside burst jobs (C# managed strings cannot be used in burst jobs).
    - Fixed a 'NativeArray has not been disposed' error message that could show up if the whole project's assets were re-imported.
    - Added \reflink{Draw.SolidCircle}.
       \shadowimage{rendered/solidcircle.png}
    - Added \reflink{Draw.SolidCircleXZ}.
       \shadowimage{rendered/solidcirclexz.png}
    - Added \reflink{Draw.SolidArc}.
       \shadowimage{rendered/solidarc.png}
    - Added \reflink{Draw.Label3D}
        \shadowimage{rendered/label3d.png}
    - Improved performance of \reflink{Draw.WirePlane} and \reflink{Draw.WireRectangle} by making them primitives instead of just calling \reflink{Draw.Line} 4 times.
    - Improved performance in general by more efficiently re-using existing vertex buffers.
    - Fixed some warnings related to ENABLE_UNITY_COLLECTIONS_CHECKS which burst would log when building a standalone player.
    - Changed more functions in the \reflink{Draw} class to take a Unity.Mathematics.quaternion instead of a UnityEngine.Quaternion.
        Implicit conversions exist in both directions, so there is no need to change your code.

## 1.4.3 (2021-09-04)
    - Fixed some debug printout had been included by mistake. A "Disposing" message could sometimes show up in the console.

## 1.4.2 (2021-08-22)
    - Reduced overhead in standalone builds if you have many objects in the scene.
    - Fixed \reflink{Draw.WireCapsule(float3,float3,float)} could render incorrectly if the start and end parameters were identical.
    - Fixed \reflink{Draw.WithDuration} scopes could survive until the next time the game started if no game or scene cameras were ever rendered while in edit mode.
    - Added \reflink{Draw.SphereOutline(float3,float)}.
       \shadowimage{rendered/sphereoutline.png}
    - \reflink{Draw.WireSphere(float3,float)} has changed to always include an outline of the sphere. This makes it a lot nicer to look at.
       \shadowimage{rendered/wiresphere.png}

## 1.4.1 (2021-02-28)
    - Added \reflink{CommandBuilder.DisposeAfter} to dispose a command builder after a job has completed.
    - Fixed gizmos would be rendered for other objects when the scene view was in prefab isolation mode. Now they will be hidden, which matches what Unity does.
    - Fixed a deprecation warning when unity the HDRP package version 9.0 or higher.
    - Improved docs for \reflink{RedrawScope}.
    - Fixed documentation for scopes (e.g. \reflink{Draw.WithColor}) would show up as missing in the online documentation.

## 1.4 (2021-01-27)
    - Breaking changes
        - \reflink{Draw.WireCapsule(float3,float3,float)} with the bottom/top parameterization was incorrect and the behavior did not match the documentation for it.
            This method has been changed so that it now matches the documentation as this was the intended behavior all along.
            The documentation and parameter names have also been clarified.
    - Added \reflink{Draw.SolidRectangle(Rect)}.
    - Fixed \reflink{Draw.SolidBox(float3,quaternion,float3)} and \reflink{Draw.WireBox(float3,quaternion,float3)} rendered a box that was offset by 0.5 times the size of the box.
        This bug only applied to the overload with a rotation, not for example to \reflink{Draw.SolidBox(float3,float3)}.
    - Fixed Draw.SolidMesh would always be rendered at the world origin with a white color. Now it picks up matrices and colors properly.
    - Fixed a bug which could cause a greyed out object called 'RetainedGizmos' to appear in the scene hierarchy.
    - Fixed some overloads of WireCylinder, WireCapsule, WireBox and SolidBox throwing errors when you tried to use them in a Burst job.
    - Improved compatibility with some older versions of the Universal Render Pipeline.

## 1.3.1 (2020-10-10)
    - Improved performance in standalone builds by more aggressively compiling out drawing commands that would never render anything anyway.
    - Reduced overhead in some cases, in particular when nothing is being rendered.

## 1.3 (2020-09-12)
    - Added support for line widths.
        See \reflink{Draw.WithLineWidth}.
        \shadowimage{features/line_widths.png}
    - Added warning message when using the Experimental URP 2D Renderer. The URP 2D renderer unfortunately does not have enough features yet
        to be able to support ALINE. It doesn't have an extensible post processing system. The 2D renderer will be supported as soon as it is technically possible.
    - Fixed \reflink{Draw.SolidPlane(float3,float3,float2)} and \reflink{Draw.WirePlane(float3,float3,float2)} not working for all normals.
    - Fixed the culling bounding box for text and lines could be calculated incorrectly if text labels were used.
        This could result in text and lines randomly disappearing when the camera was looking in particular directions.
    - Renamed \reflink{Draw.PushPersist} and \reflink{Draw.PopPersist} to \reflink{Draw.PushDuration} and \reflink{Draw.PopDuration} for consistency with the \reflink{Draw.WithDuration} scope.
        The previous names will still work, but they are marked as deprecated.
    - Known bugs
        - \reflink{Draw.SolidMesh(Mesh)} does not respect matrices and will always be drawn with the pivot at the world origin.

## 1.2.3 (2020-07-26)
    - Fixed solid drawing not working when using VR rendering.
    - Fixed nothing was visible when using the Universal Render Pipeline and post processing was enabled.
        Note that ALINE will render before post processing effects when using the URP.
        This is because as far as I can tell the Universal Render Pipeline does not expose any way to render objects
        after post processing effects because it renders to hidden textures that custom passes cannot access.
    - Fixed drawing sometimes not working when using the High Definition Render Pipeline.
        In contrast to the URP, ALINE can actually render after post processing effects with the HDRP since it has a nicer API. So it does that.
    - Known bugs
        - \reflink{Draw.SolidMesh(Mesh)} does not respect matrices and will always be drawn with the pivot at the world origin.

## 1.2.2 (2020-07-11)
    - Added \reflink{Draw.Arc(float3,float3,float3)}.
        \shadowimage{rendered/arc.png}
    - Fixed drawing sometimes not working when using the Universal Render Pipeline, in particular when either HDR or anti-aliasing was enabled.
    - Fixed drawing not working when using VR rendering.
    - Hopefully fixed the issue that could sometimes cause "The ALINE package installation seems to be corrupt. Try reinstalling the package." to be logged when first installing
        the package (even though the package wasn't corrupt at all).
    - Incremented required burst package version from 1.3.0-preview.7 to 1.3.0.
    - Fixed the offline documentation showing the wrong page instead of the get started guide.

## 1.2.1 (2020-06-21)
    - Breaking changes
        - Changed the size parameter of Draw.WireRect to be a float2 instead of a float3.
            It made no sense for it to be a float3 since a rectangle is two-dimensional. The y coordinate of the parameter was never used.
    - Added <a href="ref:Draw.WirePlane(float3,float3,float2)">Draw.WirePlane</a>.
        \shadowimage{rendered/wireplane.png}
    - Added <a href="ref:Draw.SolidPlane(float3,float3,float2)">Draw.SolidPlane</a>.
        \shadowimage{rendered/solidplane.png}
    - Added <a href="ref:Draw.PlaneWithNormal(float3,float3,float2)">Draw.PlaneWithNormal</a>.
        \shadowimage{rendered/planewithnormal.png}
    - Fixed Drawing.DrawingUtilities class missed an access modifier. Now all methods are properly public and can be accessed without any issues.
    - Fixed an error could be logged after using the WireMesh method and then exiting/entering play mode.
    - Fixed Draw.Arrow not drawing the arrowhead properly when the arrow's direction was a multiple of (0,1,0).

## 1.2 (2020-05-22)
    - Added page showing some advanced usages: \ref advanced.
    - Added \link Drawing.Draw.WireMesh Draw.WireMesh\endlink.
        \shadowimage{rendered/wiremesh.png}
    - Added \link Drawing.CommandBuilder.cameraTargets CommandBuilder.cameraTargets\endlink.
    - The WithDuration scope can now be used even outside of play mode. Outside of play mode it will use Time.realtimeSinceStartup to measure the duration.
    - The WithDuration scope can now be used inside burst jobs and on different threads.
    - Fixed WireCylinder and WireCapsule logging a warning if the normalized direction from the start to the end was exactly (1,1,1).normalized. Thanks Billy Attaway for reporting this.
    - Fixed the documentation showing the wrong namespace for classes. It listed \a Pathfinding.Drawing but the correct namespace is just \a %Drawing.

## 1.1.1 (2020-05-04)
    - Breaking changes
        - The vertical alignment of Label2D has changed slightly. Previously the Top and Center alignments were a bit off from the actual top/center.
    - Fixed conflicting assembly names when used in a project that also has the A* Pathfinding Project package installed.
    - Fixed a crash when running on iOS.
    - Improved alignment of \link Drawing.Draw.Label2D Draw.Label2D\endlink when using the Top or Center alignment.

## 1.1 (2020-04-20)
    - Added \link Drawing.Draw.Label2D Draw.Label2D\endlink which allows you to easily render text from your code.
        It uses a signed distance field font renderer which allows you to render crisp text even at high resolution.
        At very small font sizes it falls back to a regular font texture.
        \shadowimage{rendered/label2d.png}
    - Improved performance of drawing lines by about 5%.
    - Fixed a potential crash after calling the Draw.Line(Vector3,Vector3,Color) method.

## 1.0.2 (2020-04-09)
    - Breaking changes
        - A few breaking changes may be done as the package matures. I strive to keep these to as few as possible, while still not sacrificing good API design.
        - Changed the behaviour of \link Drawing.Draw.Arrow(float3,float3,float3,float) Draw.Arrow\endlink to use an absolute size head.
            This behaviour is probably the desired one more often when one wants to explicitly set the size.
            The default Draw.Arrow(float3,float3) function which does not take a size parameter continues to use a relative head size of 20% of the length of the arrow.
            \shadowimage{rendered/arrow_multiple.png}
    - Added \link Drawing.Draw.ArrowRelativeSizeHead Draw.ArrowRelativeSizeHead\endlink which uses a relative size head.
        \shadowimage{rendered/arrowrelativesizehead.png}
    - Added \link Drawing.DrawingManager.GetBuilder DrawingManager.GetBuilder\endlink instead of the unnecessarily convoluted DrawingManager.instance.gizmos.GetBuilder.
    - Added \link Drawing.Draw.CatmullRom(List<Vector3>) Draw.CatmullRom\endlink for drawing a smooth curve through a list of points.
        \shadowimage{rendered/catmullrom.png}
    - Made it easier to draw things that are visible in standalone games. You can now use for example Draw.ingame.WireBox(Vector3.zero, Vector3.one) instead of having to create a custom command builder.
        See \ref ingame for more details.

## 1.0.1 (2020-04-06)
    - Fix burst example scene not having using burst enabled (so it was much slower than it should have been).
    - Fix text color in the SceneEditor example scene was so dark it was hard to read.
    - Various minor documentation fixes.

## 1.0 (2020-04-05)
    - Initial release
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

namespace Drawing {
namespace Text {
/// <summary>Represents a single character in a font texture</summary>
internal struct SDFCharacter {
public char codePoint;
float2 uvtopleft, uvbottomright;
float2 vtopleft, vbottomright;
public float advance;

			public float2 uvTopLeft => uvtopleft;
			public float2 uvTopRight => new float2(uvbottomright.x, uvtopleft.y);
			public float2 uvBottomLeft => new float2(uvtopleft.x, uvbottomright.y);
			public float2 uvBottomRight => uvbottomright;

			public float2 vertexTopLeft => vtopleft;
			public float2 vertexTopRight => new float2(vbottomright.x, vtopleft.y);
			public float2 vertexBottomLeft => new float2(vtopleft.x, vbottomright.y);
			public float2 vertexBottomRight => vbottomright;

			public SDFCharacter(char codePoint, int x, int y, int width, int height, int originX, int originY, int advance, int textureWidth, int textureHeight, float defaultSize) {
				float2 texSize = new float2(textureWidth, textureHeight);

				this.codePoint = codePoint;
				var uvMin = new float2(x, y) / texSize;
				var uvMax = new float2(x + width, y + height) / texSize;

				// UV (0,0) is at the bottom-left in Unity
				uvtopleft = new float2(uvMin.x, 1.0f - uvMin.y);
				uvbottomright = new float2(uvMax.x, 1.0f - uvMax.y);

				var pivot = new float2(-originX, originY);
				this.vtopleft = (pivot + new float2(0, 0)) / defaultSize;
				this.vbottomright = (pivot + new float2(width, -height)) / defaultSize;
				this.advance = advance / defaultSize;
			}
		}

		/// <summary>Represents an SDF font</summary>
		internal struct SDFFont {
			public string name;
			public int size, width, height;
			public bool bold, italic;
			public SDFCharacter[] characters;
			public UnityEngine.Material material;
		}

		/// <summary>Optimzed lookup for accessing font data from the unity job system</summary>
		internal struct SDFLookupData {
			public NativeArray<SDFCharacter> characters;
			Dictionary<char, int> lookup;
			public Material material;

			public const System.UInt16 Newline = System.UInt16.MaxValue;

			public SDFLookupData (SDFFont font) {
				// Create a native array with the character data.
				// Note that the 'char' type is non-blittable in C# and this is required
				// for the NativeArray constructor that takes a T[] to copy.
				// However native arrays can store 'char's, so we copy them one by one instead.
				int nonAscii = 0;
				SDFCharacter questionMark = font.characters[0];

				for (int i = 0; i < font.characters.Length; i++) {
					if (font.characters[i].codePoint == '?') {
						questionMark = font.characters[i];
					}
					if (font.characters[i].codePoint >= 128) {
						nonAscii++;
					}
				}
				characters = new NativeArray<SDFCharacter>(128 + nonAscii, Allocator.Persistent);
				for (int i = 0; i < characters.Length; i++) {
					characters[i] = questionMark;
				}
				lookup = new Dictionary<char, int>();
				material = font.material;

				nonAscii = 0;
				for (int i = 0; i < font.characters.Length; i++) {
					var sdfChar = font.characters[i];
					int targetIndex = sdfChar.codePoint;
					if (sdfChar.codePoint >= 128) {
						targetIndex = 128 + nonAscii;
						nonAscii++;
					}
					characters[targetIndex] = sdfChar;
					lookup[sdfChar.codePoint] = targetIndex;
				}
			}

			public int GetIndex (char c) {
				if (lookup.TryGetValue(c, out int index)) {
					return index;
				} else {
					if (c == '\n') return Newline;
					return lookup['?'];
				}
			}

			public void Dispose () {
				if (characters.IsCreated) {
					characters.Dispose();
				}
			}
		}

		static class DefaultFonts {
			internal static SDFFont LoadDefaultFont () {
				var font = new SDFFont {
					name = "Droid Sans Mono",
					size = 32,
					bold = false,
					italic = false,
					width = 1024,
					height = 128,
					characters = null,
					material = UnityEngine.Resources.Load<UnityEngine.Material>("aline_text_mat")
				};

				// Generated by https://evanw.github.io/font-texture-generator/
				SDFCharacter[] characters_Droid_Sans_Mono = {
					new SDFCharacter(' ', 414, 79, 12, 12, 6, 6, 19, font.width, font.height, font.size),
					new SDFCharacter('!', 669, 44, 16, 35, -2, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('"', 258, 79, 23, 20, 2, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('#', 919, 0, 30, 35, 5, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('$', 231, 0, 26, 38, 3, 30, 19, font.width, font.height, font.size),
					new SDFCharacter('%', 393, 0, 31, 36, 6, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('&', 424, 0, 31, 36, 5, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('\'', 281, 79, 16, 20, -2, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('(', 115, 0, 22, 40, 1, 29, 19, font.width, font.height, font.size),
					new SDFCharacter(')', 137, 0, 22, 40, 1, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('*', 159, 79, 27, 26, 4, 30, 19, font.width, font.height, font.size),
					new SDFCharacter('+', 186, 79, 27, 26, 4, 24, 19, font.width, font.height, font.size),
					new SDFCharacter(',', 240, 79, 18, 21, -1, 10, 19, font.width, font.height, font.size),
					new SDFCharacter('-', 359, 79, 23, 15, 2, 16, 19, font.width, font.height, font.size),
					new SDFCharacter('.', 315, 79, 17, 17, -1, 11, 19, font.width, font.height, font.size),
					new SDFCharacter('/', 500, 44, 25, 35, 3, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('0', 569, 0, 27, 36, 4, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('1', 649, 44, 20, 35, 2, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('2', 313, 44, 27, 35, 4, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('3', 758, 0, 26, 36, 4, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('4', 60, 44, 29, 35, 5, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('5', 448, 44, 26, 35, 3, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('6', 596, 0, 27, 36, 4, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('7', 340, 44, 27, 35, 4, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('8', 623, 0, 27, 36, 4, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('9', 650, 0, 27, 36, 4, 29, 19, font.width, font.height, font.size),
					new SDFCharacter(':', 861, 44, 16, 30, -2, 23, 19, font.width, font.height, font.size),
					new SDFCharacter(';', 711, 44, 18, 34, 0, 23, 19, font.width, font.height, font.size),
					new SDFCharacter('<', 77, 79, 27, 28, 4, 25, 19, font.width, font.height, font.size),
					new SDFCharacter('=', 213, 79, 27, 21, 4, 22, 19, font.width, font.height, font.size),
					new SDFCharacter('>', 104, 79, 27, 28, 4, 25, 19, font.width, font.height, font.size),
					new SDFCharacter('?', 784, 0, 26, 36, 3, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('@', 200, 0, 31, 38, 6, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('A', 949, 0, 30, 35, 5, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('B', 89, 44, 28, 35, 4, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('C', 513, 0, 28, 36, 4, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('D', 117, 44, 28, 35, 4, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('E', 474, 44, 26, 35, 3, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('F', 525, 44, 25, 35, 2, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('G', 541, 0, 28, 36, 4, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('H', 367, 44, 27, 35, 4, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('I', 625, 44, 24, 35, 2, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('J', 550, 44, 25, 35, 4, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('K', 145, 44, 28, 35, 3, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('L', 575, 44, 25, 35, 2, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('M', 173, 44, 28, 35, 4, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('N', 394, 44, 27, 35, 4, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('O', 455, 0, 29, 36, 5, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('P', 421, 44, 27, 35, 3, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('Q', 38, 0, 29, 42, 5, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('R', 201, 44, 28, 35, 3, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('S', 677, 0, 27, 36, 4, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('T', 229, 44, 28, 35, 4, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('U', 257, 44, 28, 35, 4, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('V', 979, 0, 30, 35, 5, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('W', 888, 0, 31, 35, 6, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('X', 0, 44, 30, 35, 5, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('Y', 30, 44, 30, 35, 5, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('Z', 285, 44, 28, 35, 4, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('[', 159, 0, 21, 40, 0, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('\\', 600, 44, 25, 35, 3, 29, 19, font.width, font.height, font.size),
					new SDFCharacter(']', 180, 0, 20, 40, 1, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('^', 131, 79, 28, 26, 4, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('_', 382, 79, 32, 14, 6, 3, 19, font.width, font.height, font.size),
					new SDFCharacter('`', 297, 79, 18, 17, -1, 31, 19, font.width, font.height, font.size),
					new SDFCharacter('a', 784, 44, 26, 30, 4, 23, 19, font.width, font.height, font.size),
					new SDFCharacter('b', 285, 0, 27, 37, 4, 30, 19, font.width, font.height, font.size),
					new SDFCharacter('c', 810, 44, 26, 30, 3, 23, 19, font.width, font.height, font.size),
					new SDFCharacter('d', 312, 0, 27, 37, 4, 30, 19, font.width, font.height, font.size),
					new SDFCharacter('e', 757, 44, 27, 30, 4, 23, 19, font.width, font.height, font.size),
					new SDFCharacter('f', 704, 0, 27, 36, 4, 30, 19, font.width, font.height, font.size),
					new SDFCharacter('g', 257, 0, 28, 37, 4, 23, 19, font.width, font.height, font.size),
					new SDFCharacter('h', 810, 0, 26, 36, 3, 30, 19, font.width, font.height, font.size),
					new SDFCharacter('i', 836, 0, 26, 36, 3, 30, 19, font.width, font.height, font.size),
					new SDFCharacter('j', 0, 0, 23, 44, 4, 30, 19, font.width, font.height, font.size),
					new SDFCharacter('k', 731, 0, 27, 36, 3, 30, 19, font.width, font.height, font.size),
					new SDFCharacter('l', 862, 0, 26, 36, 3, 30, 19, font.width, font.height, font.size),
					new SDFCharacter('m', 909, 44, 29, 29, 5, 23, 19, font.width, font.height, font.size),
					new SDFCharacter('n', 995, 44, 26, 29, 3, 23, 19, font.width, font.height, font.size),
					new SDFCharacter('o', 729, 44, 28, 30, 4, 23, 19, font.width, font.height, font.size),
					new SDFCharacter('p', 339, 0, 27, 37, 4, 23, 19, font.width, font.height, font.size),
					new SDFCharacter('q', 366, 0, 27, 37, 4, 23, 19, font.width, font.height, font.size),
					new SDFCharacter('r', 52, 79, 25, 29, 2, 23, 19, font.width, font.height, font.size),
					new SDFCharacter('s', 836, 44, 25, 30, 3, 23, 19, font.width, font.height, font.size),
					new SDFCharacter('t', 685, 44, 26, 34, 4, 28, 19, font.width, font.height, font.size),
					new SDFCharacter('u', 0, 79, 26, 29, 3, 23, 19, font.width, font.height, font.size),
					new SDFCharacter('v', 938, 44, 29, 29, 5, 23, 19, font.width, font.height, font.size),
					new SDFCharacter('w', 877, 44, 32, 29, 6, 23, 19, font.width, font.height, font.size),
					new SDFCharacter('x', 967, 44, 28, 29, 4, 23, 19, font.width, font.height, font.size),
					new SDFCharacter('y', 484, 0, 29, 36, 5, 23, 19, font.width, font.height, font.size),
					new SDFCharacter('z', 26, 79, 26, 29, 3, 23, 19, font.width, font.height, font.size),
					new SDFCharacter('{', 67, 0, 24, 40, 2, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('|', 23, 0, 15, 44, -2, 30, 19, font.width, font.height, font.size),
					new SDFCharacter('}', 91, 0, 24, 40, 2, 29, 19, font.width, font.height, font.size),
					new SDFCharacter('~', 332, 79, 27, 16, 4, 19, 19, font.width, font.height, font.size),
				};

				font.characters = characters_Droid_Sans_Mono;

				return font;
			}
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Drawing.Examples {
[HelpURL("http://arongranberg.com/aline/documentation/stable/timedspawner.html")]
public class TimedSpawner : MonoBehaviour {
public float interval = 1;
public float lifeTime = 5;
public GameObject prefab;

		// Start is called before the first frame update
		IEnumerator Start () {
			while (true) {
				var go = GameObject.Instantiate(prefab, transform.position + Random.insideUnitSphere * 0.01f, Random.rotation);
				StartCoroutine(DestroyAfter(go, lifeTime));
				yield return new WaitForSeconds(interval);
			}
		}

		IEnumerator DestroyAfter (GameObject go, float delay) {
			yield return new WaitForSeconds(delay);
			GameObject.Destroy(go);
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Drawing;

namespace Drawing.Examples {
[HelpURL("http://arongranberg.com/aline/documentation/stable/gizmocharacterexample.html")]
public class GizmoCharacterExample : MonoBehaviourGizmos {
public Color gizmoColor = new Color(1.0f, 88/255f, 85/255f);
public Color gizmoColor2 = new Color(79/255f, 204/255f, 237/255f);

		public float movementNoiseScale = 0.2f;
		public float startPointAttractionStrength = 0.05f;
		public int futurePathPlotSteps = 100;
		public int plotStartStep = 10;
		public int plotEveryNSteps = 10;

		float seed;
		Vector3 startPosition;
		void Start () {
			seed = Random.value * 1000;
			startPosition = transform.position;
		}

		Vector3 GetSmoothRandomVelocity (float time, Vector3 position) {
			// Use perlin noise to get a smoothly varying vector
			float t = time * movementNoiseScale + seed;
			var dx = 2*Mathf.PerlinNoise(t, t + 5341.23145f) - 1;
			var dy = 2*Mathf.PerlinNoise(t + 92.9842f, -t + 231.85145f) - 1;
			var velocity = new Vector3(dx, 0, dy);

			// Make a weak attractor to the start position of the agent. To make sure the agent doesn't move too far out of view
			velocity += (startPosition - position) * startPointAttractionStrength;
			velocity.y = 0;
			return velocity;
		}

		void PlotFuturePath (float time, Vector3 position) {
			float dt = 0.05f;

			for (int i = 0; i < futurePathPlotSteps; i++) {
				var v = GetSmoothRandomVelocity(time + i*dt, position);

				var idx = i - plotStartStep;
				if (idx >= 0 && idx % plotEveryNSteps == 0) {
					Draw.Arrowhead(position, v, 0.1f, gizmoColor);
				}
				position += v.normalized * dt;
			}
		}

		// Update is called once per frame
		void Update () {
			PlotFuturePath(Time.time, transform.position);
			Vector3 velocity = GetSmoothRandomVelocity(Time.time, transform.position);
			transform.rotation = Quaternion.LookRotation(velocity);
			transform.position += transform.forward * Time.deltaTime;
		}

		public override void DrawGizmos () {
			using (Draw.InLocalSpace(transform)) {
				Draw.WireCylinder(Vector3.zero, Vector3.up, 2, 0.5f, gizmoColor);
				Draw.ArrowheadArc(Vector3.zero, Vector3.forward, 0.55f, gizmoColor);
				Draw.Label2D(Vector3.zero, gameObject.name, 14, LabelAlignment.TopCenter.withPixelOffset(0, -20), gizmoColor2);
			}
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Drawing;
using System.Linq;

namespace Drawing.Examples {
[HelpURL("http://arongranberg.com/aline/documentation/stable/gizmosphereexample.html")]
public class GizmoSphereExample : MonoBehaviourGizmos {
public Color gizmoColor = new Color(1.0f, 88/255f, 85/255f);
public Color gizmoColor2 = new Color(79/255f, 204/255f, 237/255f);

		public override void DrawGizmos () {
			using (Draw.InLocalSpace(transform)) {
				Draw.WireSphere(Vector3.zero, 0.5f, gizmoColor);

				foreach (var contact in contactForces.Values) {
					Draw.Circle(contact.lastPoint, contact.lastNormal, 0.1f * contact.impulse, gizmoColor2);
					Draw.SolidCircle(contact.lastPoint, contact.lastNormal, 0.1f * contact.impulse, gizmoColor2);
				}
			}
		}

		void FixedUpdate () {
			foreach (var collider in contactForces.Keys.ToList()) {
				var c = contactForces[collider];
				if (c.impulse > 0.1f) {
					c.impulse = Mathf.Lerp(c.impulse, 0, 10 * Time.fixedDeltaTime);
					c.smoothImpulse = Mathf.Lerp(c.impulse, c.smoothImpulse, 20 * Time.fixedDeltaTime);
					contactForces[collider] = c;
				} else {
					contactForces.Remove(collider);
				}
			}
		}

		struct Contact {
			public float impulse;
			public float smoothImpulse;
			public Vector3 lastPoint;
			public Vector3 lastNormal;
		}
		Dictionary<Collider, Contact> contactForces = new Dictionary<Collider, Contact>();

		void OnCollisionStay (Collision collision) {
			foreach (ContactPoint contact in collision.contacts) {
				if (!contactForces.ContainsKey(collision.collider)) {
					contactForces.Add(collision.collider, new Contact { impulse = 2f });
				}

				var c = contactForces[collision.collider];
				c.impulse = Mathf.Max(c.impulse, 1);
				c.lastPoint = transform.InverseTransformPoint(contact.point);
				c.lastNormal = transform.InverseTransformVector(contact.normal);
				contactForces[collision.collider] = c;
				break;
			}
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Drawing;
using Unity.Mathematics;

namespace Drawing.Examples {
/// <summary>Example that shows line widths, colors and line joins</summary>
[HelpURL("http://arongranberg.com/aline/documentation/stable/alinestyling.html")]
public class AlineStyling : MonoBehaviour {
public Color gizmoColor = new Color(1.0f, 88/255f, 85/255f);
public Color gizmoColor2 = new Color(79/255f, 204/255f, 237/255f);

		// Update is called once per frame
		void Update () {
			// Draw in-game.
			// This will draw the things even in standalone games
			var draw = Draw.ingame;

			using (draw.InScreenSpace(Camera.main)) {
				// Use a matrix to be able to draw in normalized space. I.e. (0,0) is the center of the screen, (0.5, 0.0) is the right side of the screen etc.
				using (draw.WithMatrix(Matrix4x4.TRS(new Vector3(Screen.width/2.0f, Screen.height/2.0f, 0), Quaternion.identity, new Vector3(Screen.width, Screen.width, 1)))) {
					for (int i = 0; i < 4; i++) {
						// Draw with a few different line widths
						using (draw.WithLineWidth(i*i+1)) {
							float angle = Mathf.PI * 0.25f * (i+1) + Time.time * i;
							Vector3 offset = new Vector3(-0.3f + i * 0.2f, 0, 0);
							float radius = 0.075f;
							// Draw a rotating line
							draw.Line(offset + new Vector3(math.cos(angle)*radius, math.sin(angle)*radius, 0), offset, gizmoColor);
							// Draw a fixed line
							draw.Line(offset, offset + new Vector3(radius, 0, 0), gizmoColor);
							// Draw a circle
							draw.xy.Circle(offset, radius, gizmoColor2);
						}
					}
				}
			}
		}
	}
}
using Drawing;
using Unity.Jobs;
using Unity.Burst;
using UnityEngine;
using Unity.Mathematics;

namespace Drawing.Examples {
[HelpURL("http://arongranberg.com/aline/documentation/stable/burstexample.html")]
public class BurstExample : MonoBehaviour {
// Use [BurstCompile] to allow Unity to compile the job using the Burst compiler
[BurstCompile]
struct DrawingJob : IJob {
public float2 offset;
// The job takes a command builder which we can use to draw things with
public CommandBuilder builder;

			Color Colormap (float x) {
				// Simple color map that goes from black through red to yellow
				float r = math.clamp(8.0f / 3.0f * x, 0.0f, 1.0f);
				float g = math.clamp(8.0f / 3.0f * x - 1.0f, 0.0f, 1.0f);
				float b = math.clamp(4.0f * x - 3.0f, 0.0f, 1.0f);

				return new Color(r, g, b, 1.0f);
			}

			public void Execute (int index) {
				int x = index / 100;
				int z = index % 100;

				// Draw a solid box and a wire box
				// Use Perlin noise to generate a procedural heightmap
				var noise = Mathf.PerlinNoise(x * 0.05f + offset.x, z * 0.05f + offset.y);
				Bounds bounds = new Bounds(new float3(x, 0, z), new float3(1, 14 * noise, 1));

				//builder.WireBox(bounds, new Color(0, 0, 0, 0.2f));
				builder.SolidBox(bounds, Colormap(noise));
			}

			public void Execute () {
				for (int index = 0; index < 100 * 100; index++) {
					Execute(index);
				}
			}
		}

		public void Update () {
			var builder = DrawingManager.GetBuilder(true);

			// Create a new job struct and schedule it using the Unity Job System
			var job = new DrawingJob {
				builder = builder,
				offset = new float2(Time.time * 0.2f, Time.time * 0.2f),
			}.Schedule();
			// Dispose the builder after the job is complete
			builder.DisposeAfter(job);

			job.Complete();
		}
	}
}
using System.Collections.Generic;
using UnityEngine;
using Drawing;

namespace Drawing.Examples {
/// <summary>Simple bezier curve editor</summary>
[HelpURL("http://arongranberg.com/aline/documentation/stable/curveeditor.html")]
public class CurveEditor : MonoBehaviour {
List<CurvePoint> curves = new List<CurvePoint>();
Camera cam;
public Color curveColor;

		class CurvePoint {
			public Vector2 position, controlPoint0, controlPoint1;
		}

		void Awake () {
			cam = Camera.main;
		}

		void Update () {
			// Add a new control point when clicking
			if (Input.GetKeyDown(KeyCode.Mouse0)) {
				curves.Add(new CurvePoint {
					position = (Vector2)Input.mousePosition,
					controlPoint0 = Vector2.zero,
					controlPoint1 = Vector2.zero,
				});
			}

			// Keep adjusting the position of the control point while the mouse is pressed
			if (curves.Count > 0 && Input.GetKey(KeyCode.Mouse0) && ((Vector2)Input.mousePosition - curves[curves.Count - 1].position).magnitude > 2*2) {
				var point = curves[curves.Count - 1];
				point.controlPoint1 = (Vector2)Input.mousePosition - point.position;
				point.controlPoint0 = -point.controlPoint1;
			}

			Render();
		}

		void Render () {
			// Use a custom builder which renders even in standalone games
			// and in the editor even if gizmos are disabled.
			// Usually you would use the static Draw class instead.
			using (var draw = DrawingManager.GetBuilder(true)) {
				// Draw the curves in 2D using pixel coordinates
				using (draw.InScreenSpace(cam)) {
					// Draw a circle at each curve control point
					for (int i = 0; i < curves.Count; i++) {
						draw.xy.Circle((Vector3)curves[i].position, 2, Color.blue);
					}

					// Draw each bezier curve segment
					for (int i = 0; i < curves.Count - 1; i++) {
						var p0 = curves[i].position;
						var p1 = p0 + curves[i].controlPoint1;
						var p3 = curves[i+1].position;
						var p2 = p3 + curves[i+1].controlPoint0;
						draw.Bezier((Vector3)p0, (Vector3)p1, (Vector3)p2, (Vector3)p3, curveColor);
					}
				}
			}
		}
	}
}
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Drawing {
using static CommandBuilder;

	[BurstCompile]
	internal struct PersistentFilterJob : IJob {
		[NativeDisableUnsafePtrRestriction]
		public unsafe UnsafeAppendBuffer* buffer;
		public float time;

		public void Execute () {
			var stackPersist = new NativeArray<bool>(GeometryBuilderJob.MaxStackSize, Allocator.Temp, NativeArrayOptions.ClearMemory);
			var stackScope = new NativeArray<int>(GeometryBuilderJob.MaxStackSize, Allocator.Temp, NativeArrayOptions.ClearMemory);

			unsafe {
				// Store in local variables for performance (makes it possible to use registers for a lot of fields)
				var bufferPersist = *buffer;

				long writeOffset = 0;
				long readOffset = 0;
				bool shouldWrite = false;
				int stackSize = 0;
				long lastNonMetaWrite = -1;

				while (readOffset < bufferPersist.Length) {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
UnityEngine.Assertions.Assert.IsTrue(readOffset + UnsafeUtility.SizeOf<Command>() <= bufferPersist.Length);
#endif
var cmd = *(Command*)((byte*)bufferPersist.Ptr + readOffset);
var cmdBit = 1 << ((int)cmd & 0xFF);
bool isMeta = (cmdBit & StreamSplitter.MetaCommands) != 0;
int size = StreamSplitter.CommandSizes[(int)cmd & 0xFF] + ((cmd & Command.PushColorInline) != 0 ? UnsafeUtility.SizeOf<Color32>() : 0);

					if ((cmd & (Command)0xFF) == Command.Text) {
						// Very pretty way of reading the TextData struct right after the command label and optional Color32
						var data = *((TextData*)((byte*)bufferPersist.Ptr + readOffset + size) - 1);
						// Add the size of the embedded string in the buffer
						size += data.numCharacters * UnsafeUtility.SizeOf<System.UInt16>();
					} else if ((cmd & (Command)0xFF) == Command.Text3D) {
						// Very pretty way of reading the TextData struct right after the command label and optional Color32
						var data = *((TextData3D*)((byte*)bufferPersist.Ptr + readOffset + size) - 1);
						// Add the size of the embedded string in the buffer
						size += data.numCharacters * UnsafeUtility.SizeOf<System.UInt16>();
					}

#if ENABLE_UNITY_COLLECTIONS_CHECKS
UnityEngine.Assertions.Assert.IsTrue(readOffset + size <= bufferPersist.Length);
UnityEngine.Assertions.Assert.IsTrue(writeOffset + size <= bufferPersist.Length);
#endif

					if (shouldWrite || isMeta) {
						if (!isMeta) lastNonMetaWrite = writeOffset;
						if (writeOffset != readOffset) {
							// We need to use memmove instead of memcpy because the source and destination regions may overlap
							UnsafeUtility.MemMove((byte*)bufferPersist.Ptr + writeOffset, (byte*)bufferPersist.Ptr + readOffset, size);
						}
						writeOffset += size;
					}

					if ((cmdBit & StreamSplitter.PushCommands) != 0) {
						if ((cmd & (Command)0xFF) == Command.PushPersist) {
							// Very pretty way of reading the PersistData struct right after the command label and optional Color32
							// (even though a PushColorInline command is not usually combined with PushPersist)
							var data = *((PersistData*)((byte*)bufferPersist.Ptr + readOffset + size) - 1);
							// Scopes only survive if this condition is true
							shouldWrite = time <= data.endTime;
						}

						stackScope[stackSize] = (int)(writeOffset - size);
						stackPersist[stackSize] = shouldWrite;
						stackSize++;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
if (stackSize >= GeometryBuilderJob.MaxStackSize) throw new System.Exception("Push commands are too deeply nested. This can happen if you have deeply nested WithMatrix or WithColor scopes.");
#else
if (stackSize >= GeometryBuilderJob.MaxStackSize) {
buffer->Length = 0;
return;
}
#endif
} else if ((cmdBit & StreamSplitter.PopCommands) != 0) {
stackSize--;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
if (stackSize < 0) throw new System.Exception("Trying to issue a pop command but there is no corresponding push command");
#else
if (stackSize < 0) {
buffer->Length = 0;
return;
}
#endif
// If a scope was pushed and later popped, but no actual draw commands were written to the buffers
// inside that scope then we erase the whole scope.
if ((int)lastNonMetaWrite < stackScope[stackSize]) {
writeOffset = (long)stackScope[stackSize];
}

						shouldWrite = stackPersist[stackSize];
					}

					readOffset += size;
				}

				bufferPersist.Length = (int)writeOffset;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
if (stackSize != 0) throw new System.Exception("Inconsistent push/pop commands. Are your push and pop commands properly matched?");
#else
if (stackSize != 0) {
buffer->Length = 0;
return;
}
#endif

				*buffer = bufferPersist;
			}
		}
	}
}
#pragma warning disable CS0169, CS0414 // The field 'DrawingSettings.version' is never used
using UnityEditor;
using UnityEngine;

namespace Drawing {
/// <summary>Stores ALINE project settings</summary>
public class DrawingSettings : ScriptableObject {
public const string SettingsPathCompatibility = "Assets/Settings/ALINE.asset";
public const string SettingsName = "ALINE";
public const string SettingsPath = "Assets/Settings/Resources/" + SettingsName + ".asset";

		/// <summary>Stores ALINE project settings</summary>
		[System.Serializable]
		public class Settings {
			/// <summary>Opacity of lines when in front of objects</summary>
			public float lineOpacity = 1.0f;

			/// <summary>Opacity of solid objects when in front of other objects</summary>

			public float solidOpacity = 0.55f;

			/// <summary>Opacity of text when in front of other objects</summary>

			public float textOpacity = 1.0f;

			/// <summary>Additional opacity multiplier of lines when behind or inside objects</summary>

			public float lineOpacityBehindObjects = 0.12f;

			/// <summary>Additional opacity multiplier of solid objects when behind or inside other objects</summary>

			public float solidOpacityBehindObjects = 0.45f;

			/// <summary>Additional opacity multiplier of text when behind or inside other objects</summary>

			public float textOpacityBehindObjects = 0.9f;

			/// <summary>
			/// Resolution of curves, as a fraction of the default.
			///
			/// The resolution of curves is dynamic based on the distance to the camera.
			/// This setting will make the curves higher or lower resolution by a factor from the default.
			/// </summary>
			public float curveResolution = 1.0f;
		}

		[SerializeField]
		private int version;
		public Settings settings;

		public static Settings DefaultSettings => new Settings();

		public static DrawingSettings GetSettingsAsset () {
#if UNITY_EDITOR
System.IO.Directory.CreateDirectory(Application.dataPath + "/../" + System.IO.Path.GetDirectoryName(SettingsPath));
var settings = AssetDatabase.LoadAssetAtPath<DrawingSettings>(SettingsPath);
if (settings == null && AssetDatabase.LoadAssetAtPath<DrawingSettings>(SettingsPathCompatibility) != null) {
AssetDatabase.MoveAsset(SettingsPathCompatibility, SettingsPath);
settings = AssetDatabase.LoadAssetAtPath<DrawingSettings>(SettingsPath);
}
if (settings == null) {
settings = ScriptableObject.CreateInstance<DrawingSettings>();
settings.settings = DefaultSettings;
settings.version = 0;
AssetDatabase.CreateAsset(settings, SettingsPath);
AssetDatabase.SaveAssets();
}
#else
var settings = Resources.Load<DrawingSettings>(SettingsName);
#endif
return settings;
}
}
}
using System;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine.Profiling;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;

namespace Drawing {
using static DrawingData;
using BitPackedMeta = DrawingData.BuilderData.BitPackedMeta;
using Drawing.Text;
using Unity.Profiling;

	/// <summary>
	/// Specifies text alignment relative to an anchor point.
	///
	/// <code>
	/// Draw.Label2D(transform.position, "Hello World", 14, LabelAlignment.TopCenter);
	/// </code>
	/// <code>
	/// // Draw the label 20 pixels below the object
	/// Draw.Label2D(transform.position, "Hello World", 14, LabelAlignment.TopCenter.withPixelOffset(0, -20));
	/// </code>
	///
	/// See: <see cref="Draw.Label2D"/>
	/// See: <see cref="Draw.Label3D"/>
	/// </summary>
	public struct LabelAlignment {
		/// <summary>
		/// Where on the text's bounding box to anchor the text.
		///
		/// The pivot is specified in relative coordinates, where (0,0) is the bottom left corner and (1,1) is the top right corner.
		/// </summary>
		public float2 relativePivot;
		/// <summary>How much to move the text in screen-space</summary>
		public float2 pixelOffset;

		public static readonly LabelAlignment TopLeft = new LabelAlignment { relativePivot = new float2(0.0f, 1.0f), pixelOffset = new float2(0, 0) };
		public static readonly LabelAlignment MiddleLeft = new LabelAlignment { relativePivot = new float2(0.0f, 0.5f), pixelOffset = new float2(0, 0) };
		public static readonly LabelAlignment BottomLeft = new LabelAlignment { relativePivot = new float2(0.0f, 0.0f), pixelOffset = new float2(0, 0) };
		public static readonly LabelAlignment BottomCenter = new LabelAlignment { relativePivot = new float2(0.5f, 0.0f), pixelOffset = new float2(0, 0) };
		public static readonly LabelAlignment BottomRight = new LabelAlignment { relativePivot = new float2(1.0f, 0.0f), pixelOffset = new float2(0, 0) };
		public static readonly LabelAlignment MiddleRight = new LabelAlignment { relativePivot = new float2(1.0f, 0.5f), pixelOffset = new float2(0, 0) };
		public static readonly LabelAlignment TopRight = new LabelAlignment { relativePivot = new float2(1.0f, 1.0f), pixelOffset = new float2(0, 0) };
		public static readonly LabelAlignment TopCenter = new LabelAlignment { relativePivot = new float2(0.5f, 1.0f), pixelOffset = new float2(0, 0) };
		public static readonly LabelAlignment Center = new LabelAlignment { relativePivot = new float2(0.5f, 0.5f), pixelOffset = new float2(0, 0) };

		/// <summary>
		/// Moves the text by the specified amount of pixels in screen-space.
		///
		/// <code>
		/// // Draw the label 20 pixels below the object
		/// Draw.Label2D(transform.position, "Hello World", 14, LabelAlignment.TopCenter.withPixelOffset(0, -20));
		/// </code>
		/// </summary>
		public LabelAlignment withPixelOffset (float x, float y) {
			return new LabelAlignment {
					   relativePivot = this.relativePivot,
					   pixelOffset = new float2(x, y),
			};
		}
	}

	/// <summary>Maximum allowed delay for a job that is drawing to a command buffer</summary>
	public enum AllowedDelay {
		/// <summary>
		/// If the job is not complete at the end of the frame, drawing will block until it is completed.
		/// This is recommended for most jobs that are expected to complete within a single frame.
		/// </summary>
		EndOfFrame,
		/// <summary>
		/// Wait indefinitely for the job to complete, and only submit the results for rendering once it is done.
		/// This is recommended for long running jobs that may take many frames to complete.
		/// </summary>
		Infinite,
	}

	/// <summary>Some static fields that need to be in a separate class because Burst doesn't support them</summary>
	static class CommandBuilderSamplers {
		internal static readonly ProfilerMarker MarkerConvert = new ProfilerMarker("Convert");
		internal static readonly ProfilerMarker MarkerSetLayout = new ProfilerMarker("SetLayout");
		internal static readonly ProfilerMarker MarkerUpdateVertices = new ProfilerMarker("UpdateVertices");
		internal static readonly ProfilerMarker MarkerUpdateIndices = new ProfilerMarker("UpdateIndices");
		internal static readonly ProfilerMarker MarkerSubmesh = new ProfilerMarker("Submesh");
		internal static readonly ProfilerMarker MarkerUpdateBuffer = new ProfilerMarker("UpdateComputeBuffer");

		internal static readonly ProfilerMarker MarkerProcessCommands = new ProfilerMarker("Commands");
		internal static readonly ProfilerMarker MarkerCreateTriangles = new ProfilerMarker("CreateTriangles");
	}

	/// <summary>
	/// Builder for drawing commands.
	/// You can use this to queue many drawing commands. The commands will be queued for rendering when you call the Dispose method.
	/// It is recommended that you use the <a href="https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/using-statement">using statement</a> which automatically calls the Dispose method.
	///
	/// <code>
	/// // Create a new CommandBuilder
	/// using (var draw = DrawingManager.GetBuilder()) {
	///     // Use the exact same API as the global Draw class
	///     draw.WireBox(Vector3.zero, Vector3.one);
	/// }
	/// </code>
	///
	/// Warning: You must call either <see cref="Dispose"/> or <see cref="DiscardAndDispose"/> when you are done with this object to avoid memory leaks.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	[BurstCompile]
	public partial struct CommandBuilder : IDisposable {
		// Note: Many fields/methods are explicitly marked as private. This is because doxygen otherwise thinks they are public by default (like struct members are in c++)

		[NativeDisableUnsafePtrRestriction]
		internal unsafe UnsafeAppendBuffer* buffer;

		private GCHandle gizmos;

		[NativeSetThreadIndex]
		private int threadIndex;

		private DrawingData.BuilderData.BitPackedMeta uniqueID;

		internal unsafe CommandBuilder(UnsafeAppendBuffer* buffer, GCHandle gizmos, int threadIndex, DrawingData.BuilderData.BitPackedMeta uniqueID) {
			this.buffer = buffer;
			this.gizmos = gizmos;
			this.threadIndex = threadIndex;
			this.uniqueID = uniqueID;
		}


		internal CommandBuilder(DrawingData gizmos, Hasher hasher, RedrawScope frameRedrawScope, RedrawScope customRedrawScope, bool isGizmos, bool isBuiltInCommandBuilder, int sceneModeVersion) {
			// We need to use a GCHandle instead of a normal reference to be able to pass this object to burst compiled function pointers.
			// The NativeSetClassTypeToNullOnSchedule unfortunately only works together with the job system, not with raw functions.
			this.gizmos = GCHandle.Alloc(gizmos, GCHandleType.Normal);

			threadIndex = 0;
			uniqueID = gizmos.data.Reserve(isBuiltInCommandBuilder);
			gizmos.data.Get(uniqueID).Init(hasher, frameRedrawScope, customRedrawScope, isGizmos, gizmos.GetNextDrawOrderIndex(), sceneModeVersion);
			unsafe {
				buffer = gizmos.data.Get(uniqueID).bufferPtr;
			}
		}

		internal unsafe int BufferSize {
			get {
				return buffer->Length;
			}
			set {
				buffer->Length = value;
			}
		}

		/// <summary>
		/// Wrapper for drawing in the XY plane.
		///
		/// <code>
		/// var p1 = new Vector2(0, 1);
		/// var p2 = new Vector2(5, 7);
		///
		/// // Draw it in the XY plane
		/// Draw.xy.Line(p1, p2);
		///
		/// // Draw it in the XZ plane
		/// Draw.xz.Line(p1, p2);
		/// </code>
		///
		/// See: 2d-drawing (view in online documentation for working links)
		/// See: <see cref="Draw.xz"/>
		/// </summary>
		public CommandBuilder2D xy => new CommandBuilder2D(this, true);

		/// <summary>
		/// Wrapper for drawing in the XZ plane.
		///
		/// <code>
		/// var p1 = new Vector2(0, 1);
		/// var p2 = new Vector2(5, 7);
		///
		/// // Draw it in the XY plane
		/// Draw.xy.Line(p1, p2);
		///
		/// // Draw it in the XZ plane
		/// Draw.xz.Line(p1, p2);
		/// </code>
		///
		/// See: 2d-drawing (view in online documentation for working links)
		/// See: <see cref="Draw.xy"/>
		/// </summary>
		public CommandBuilder2D xz => new CommandBuilder2D(this, false);

		static readonly float3 DEFAULT_UP = new float3(0, 1, 0);

		/// <summary>
		/// Can be set to render specifically to these cameras.
		/// If you set this property to an array of cameras then this command builder will only be rendered
		/// to the specified cameras. Setting this property bypasses <see cref="Drawing.DrawingManager.allowRenderToRenderTextures"/>.
		/// The camera will be rendered to even if it renders to a render texture.
		///
		/// A null value indicates that all valid cameras should be rendered to. This is the default value.
		///
		/// <code>
		/// var draw = DrawingManager.GetBuilder(true);
		///
		/// draw.cameraTargets = new Camera[] { myCamera };
		/// // This sphere will only be rendered to myCamera
		/// draw.WireSphere(Vector3.zero, 0.5f, Color.black);
		/// draw.Dispose();
		/// </code>
		///
		/// See: advanced (view in online documentation for working links)
		/// </summary>
		public Camera[] cameraTargets {
			get {
				if (gizmos.IsAllocated && gizmos.Target != null) {
					var target = gizmos.Target as DrawingData;
					if (target.data.StillExists(uniqueID)) {
						return target.data.Get(uniqueID).meta.cameraTargets;
					}
				}
				throw new System.Exception("Cannot get cameraTargets because the command builder has already been disposed or does not exist.");
			}
			set {
				if (uniqueID.isBuiltInCommandBuilder) throw new System.Exception("You cannot set the camera targets for a built-in command builder. Create a custom command builder instead.");
				if (gizmos.IsAllocated && gizmos.Target != null) {
					var target = gizmos.Target as DrawingData;
					if (!target.data.StillExists(uniqueID)) {
						throw new System.Exception("Cannot set cameraTargets because the command builder has already been disposed or does not exist.");
					}
					target.data.Get(uniqueID).meta.cameraTargets = value;
				}
			}
		}

		/// <summary>Submits this command builder for rendering</summary>
		public void Dispose () {
			if (uniqueID.isBuiltInCommandBuilder) throw new System.Exception("You cannot dispose a built-in command builder");
			DisposeInternal();
		}

		/// <summary>
		/// Disposes this command builder after the given job has completed.
		///
		/// This is convenient if you are using the entity-component-system/burst in Unity and don't know exactly when the job will complete.
		///
		/// You will not be able to use this command builder on the main thread anymore.
		///
		/// See: job-system (view in online documentation for working links)
		/// </summary>
		/// <param name="dependency">The job that must complete before this command builder is disposed.</param>
		/// <param name="allowedDelay">Whether to block on this dependency before rendering the current frame or not.
		///    If the job is expected to complete during a single frame, leave at the default of \reflink{AllowedDelay.EndOfFrame}.
		///    But if the job is expected to take multiple frames to complete, you can set this to \reflink{AllowedDelay.Infinite}.</param>
		public void DisposeAfter (JobHandle dependency, AllowedDelay allowedDelay = AllowedDelay.EndOfFrame) {
			if (!gizmos.IsAllocated) throw new System.Exception("You cannot dispose an invalid command builder. Are you trying to dispose it twice?");
			try {
				if (gizmos.IsAllocated && gizmos.Target != null) {
					var target = gizmos.Target as DrawingData;
					if (!target.data.StillExists(uniqueID)) {
						throw new System.Exception("Cannot dispose the command builder because the drawing manager has been destroyed");
					}
					target.data.Get(uniqueID).SubmitWithDependency(gizmos, dependency, allowedDelay);
				}
			} finally {
				this = default;
			}
		}

		internal void DisposeInternal () {
			if (!gizmos.IsAllocated) throw new System.Exception("You cannot dispose an invalid command builder. Are you trying to dispose it twice?");
			try {
				if (gizmos.IsAllocated && gizmos.Target != null) {
					var target = gizmos.Target as DrawingData;
					if (!target.data.StillExists(uniqueID)) {
						throw new System.Exception("Cannot dispose the command builder because the drawing manager has been destroyed");
					}
					target.data.Get(uniqueID).Submit(gizmos.Target as DrawingData);
				}
			} finally {
				gizmos.Free();
				this = default;
			}
		}

		/// <summary>
		/// Discards the contents of this command builder without rendering anything.
		/// If you are not going to draw anything (i.e. you do not call the <see cref="Dispose"/> method) then you must call this method to avoid
		/// memory leaks.
		/// </summary>
		public void DiscardAndDispose () {
			if (uniqueID.isBuiltInCommandBuilder) throw new System.Exception("You cannot dispose a built-in command builder");
			DiscardAndDisposeInternal();
		}

		internal void DiscardAndDisposeInternal () {
			try {
				if (gizmos.IsAllocated && gizmos.Target != null) {
					var target = gizmos.Target as DrawingData;
					if (!target.data.StillExists(uniqueID)) {
						throw new System.Exception("Cannot dispose the command builder because the drawing manager has been destroyed");
					}
					target.data.Release(uniqueID);
				}
			} finally {
				if (gizmos.IsAllocated) gizmos.Free();
				this = default;
			}
		}

		/// <summary>
		/// Pre-allocates the internal buffer to an additional size bytes.
		/// This can give you a minor performance boost if you are drawing a lot of things.
		///
		/// Note: Only resizes the buffer for the current thread.
		/// </summary>
		public void Preallocate (int size) {
			Reserve(size);
		}

		/// <summary>Internal rendering command</summary>
		[System.Flags]
		internal enum Command {
			PushColorInline = 1 << 8,
			PushColor = 0,
			PopColor,
			PushMatrix,
			PushSetMatrix,
			PopMatrix,
			Line,
			Circle,
			CircleXZ,
			Disc,
			DiscXZ,
			SphereOutline,
			Box,
			WirePlane,
			WireBox,
			SolidTriangle,
			PushPersist,
			PopPersist,
			Text,
			Text3D,
			PushLineWidth,
			PopLineWidth,
			CaptureState,
		}

		internal struct TriangleData {
			public float3 a, b, c;
		}

		/// <summary>Holds rendering data for a line</summary>
		internal struct LineData {
			public float3 a, b;
		}

		internal struct LineDataV3 {
			public Vector3 a, b;
		}

		/// <summary>Holds rendering data for a circle</summary>
		internal struct CircleXZData {
			public float3 center;
			public float radius, startAngle, endAngle;
		}

		/// <summary>Holds rendering data for a circle</summary>
		internal struct CircleData {
			public float3 center;
			public float3 normal;
			public float radius;
		}

		/// <summary>Holds rendering data for a sphere</summary>
		internal struct SphereData {
			public float3 center;
			public float radius;
		}

		/// <summary>Holds rendering data for a box</summary>
		internal struct BoxData {
			public float3 center;
			public float3 size;
		}

		internal struct PlaneData {
			public float3 center;
			public quaternion rotation;
			public float2 size;
		}

		internal struct PersistData {
			public float endTime;
		}

		internal struct LineWidthData {
			public float pixels;
			public bool automaticJoins;
		}



		internal struct TextData {
			public float3 center;
			public LabelAlignment alignment;
			public float sizeInPixels;
			public int numCharacters;
		}

		internal struct TextData3D {
			public float3 center;
			public quaternion rotation;
			public LabelAlignment alignment;
			public float size;
			public int numCharacters;
		}

		/// <summary>Ensures the buffer has room for at least N more bytes</summary>
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		private void Reserve (int additionalSpace) {
			unsafe {
				if (Unity.Burst.CompilerServices.Hint.Unlikely(threadIndex >= 0)) {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
if (threadIndex < 0 || threadIndex >= JobsUtility.MaxJobThreadCount) throw new System.Exception("Thread index outside the expected range");
if (threadIndex > 0 && uniqueID.isBuiltInCommandBuilder) throw new System.Exception("You should use a custom command builder when using the Unity Job System. Take a look at the documentation for more info.");
if (buffer == null) throw new System.Exception("CommandBuilder does not have a valid buffer. Is it properly initialized?");

					// Exploit the fact that right after this package has drawn gizmos the buffers will be empty
					// and the next task is that Unity will render its own internal gizmos.
					// We can therefore easily (and without a high performance cost)
					// trap accidental Draw.* calls from OnDrawGizmos functions
					// by doing this check when the first Reserve call is made.
					AssertNotRendering();
#endif

					buffer += threadIndex;
					threadIndex = -1;
				}

				var newLength = buffer->Length + additionalSpace;
				if (newLength > buffer->Capacity) {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
// This really should run every time we access the buffer... but that would be a bit slow
// This code will catch the error eventually.
AssertBufferExists();
const int MAX_BUFFER_SIZE = 1024 * 1024 * 256; // 256 MB
if (buffer->Length * 2 > MAX_BUFFER_SIZE) {
throw new System.Exception("CommandBuilder buffer is very large. Are you trying to draw things in an infinite loop?");
}
#endif
buffer->SetCapacity(math.max(newLength, buffer->Length * 2));
}
}
}

		[BurstDiscard]
		private void AssertBufferExists () {
			if (!gizmos.IsAllocated || gizmos.Target == null || !(gizmos.Target as DrawingData).data.StillExists(uniqueID)) {
				// This command builder is invalid, clear all data on it to prevent it being used again
				this = default;
				throw new System.Exception("This command builder no longer exists. Are you trying to draw to a command builder which has already been disposed?");
			}
		}

		[BurstDiscard]
		static void AssertNotRendering () {
			// Some checking to see if drawing is being done from inside OnDrawGizmos
			// This check is relatively fast (about 0.05 ms), but we still do it only every 128th frame for performance reasons
			if (!GizmoContext.drawingGizmos && !JobsUtility.IsExecutingJob && (Time.renderedFrameCount & 127) == 0) {
				// Inspect the stack-trace to be able to provide more helpful error messages
				var st = StackTraceUtility.ExtractStackTrace();
				if (st.Contains("OnDrawGizmos")) {
					throw new System.Exception("You are trying to use Draw.* functions from within Unity's OnDrawGizmos function. Use this package's gizmo callbacks instead (see the documentation).");
				}
			}
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		internal void Reserve<A>() where A : struct {
			Reserve(UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<A>());
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		internal void Reserve<A, B>() where A : struct where B : struct {
			Reserve(UnsafeUtility.SizeOf<Command>() * 2 + UnsafeUtility.SizeOf<A>() + UnsafeUtility.SizeOf<B>());
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		internal void Reserve<A, B, C>() where A : struct where B : struct where C : struct {
			Reserve(UnsafeUtility.SizeOf<Command>() * 3 + UnsafeUtility.SizeOf<A>() + UnsafeUtility.SizeOf<B>() + UnsafeUtility.SizeOf<C>());
		}

		/// <summary>
		/// Converts a Color to a Color32.
		/// This method is faster than Unity's native color conversion, especially when using Burst.
		/// </summary>
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		internal static unsafe uint ConvertColor (Color color) {
			// If SSE2 is supported (which it is on essentially all X86 CPUs)
			// then we can use a much faster conversion from Color to Color32.
			// This will only be possible inside Burst.
			if (Unity.Burst.Intrinsics.X86.Sse2.IsSse2Supported) {
				// Convert from 0-1 float range to 0-255 integer range
				var ci = (int4)(255 * new float4(color.r, color.g, color.b, color.a) + 0.5f);
				var v32 = new Unity.Burst.Intrinsics.v128(ci.x, ci.y, ci.z, ci.w);
				// Convert four 32-bit numbers to four 16-bit numbers
				var v16 = Unity.Burst.Intrinsics.X86.Sse2.packs_epi32(v32, v32);
				// Convert four 16-bit numbers to four 8-bit numbers
				var v8 = Unity.Burst.Intrinsics.X86.Sse2.packus_epi16(v16, v16);
				return v8.UInt0;
			} else {
				// If we don't have SSE2 (most likely we are not running inside Burst),
				// then we will do a manual conversion from Color to Color32.
				// This is significantly faster than just casting to a Color32.
				var r = (uint)Mathf.Clamp((int)(color.r*255f + 0.5f), 0, 255);
				var g = (uint)Mathf.Clamp((int)(color.g*255f + 0.5f), 0, 255);
				var b = (uint)Mathf.Clamp((int)(color.b*255f + 0.5f), 0, 255);
				var a = (uint)Mathf.Clamp((int)(color.a*255f + 0.5f), 0, 255);
				return (a << 24) | (b << 16) | (g << 8) | r;
			}
		}

		internal unsafe void Add<T>(T value) where T : struct {
			int num = UnsafeUtility.SizeOf<T>();
			var buffer = this.buffer;
			var bufferSize = buffer->Length;
			// We assume this because the Reserve function has already taken care of that.
			// This removes a few branches from the assembly when running in burst.
			Unity.Burst.CompilerServices.Hint.Assume(buffer->Ptr != null);
			Unity.Burst.CompilerServices.Hint.Assume(buffer->Ptr + bufferSize != null);

			unsafe {
				UnsafeUtility.CopyStructureToPtr(ref value, (void*)((byte*)buffer->Ptr + bufferSize));
				buffer->Length = bufferSize + num;
			}
		}

		public struct ScopeMatrix : IDisposable {
			internal CommandBuilder builder;
			public void Dispose () {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
if (!builder.gizmos.IsAllocated || !(builder.gizmos.Target is DrawingData data) || !data.data.StillExists(builder.uniqueID)) throw new System.InvalidOperationException("The drawing instance this matrix scope belongs to no longer exists. Matrix scopes cannot survive for longer than a frame unless you have a custom drawing instance. Are you using a matrix scope inside a coroutine?");
#endif
unsafe {
builder.PopMatrix();
builder.buffer = null;
}
}
}

		public struct ScopeColor : IDisposable {
			internal CommandBuilder builder;
			public void Dispose () {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
if (!builder.gizmos.IsAllocated || !(builder.gizmos.Target is DrawingData data) || !data.data.StillExists(builder.uniqueID)) throw new System.InvalidOperationException("The drawing instance this color scope belongs to no longer exists. Color scopes cannot survive for longer than a frame unless you have a custom drawing instance. Are you using a color scope inside a coroutine?");
#endif
unsafe {
builder.PopColor();
builder.buffer = null;
}
}
}

		public struct ScopePersist : IDisposable {
			internal CommandBuilder builder;
			public void Dispose () {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
if (!builder.gizmos.IsAllocated || !(builder.gizmos.Target is DrawingData data) || !data.data.StillExists(builder.uniqueID)) throw new System.InvalidOperationException("The drawing instance this persist scope belongs to no longer exists. Persist scopes cannot survive for longer than a frame unless you have a custom drawing instance. Are you using a persist scope inside a coroutine?");
#endif
unsafe {
builder.PopDuration();
builder.buffer = null;
}
}
}

		/// <summary>
		/// Scope that does nothing.
		/// Used for optimization in standalone builds.
		/// </summary>
		public struct ScopeEmpty : IDisposable {
			public void Dispose () {
			}
		}

		public struct ScopeLineWidth : IDisposable {
			internal CommandBuilder builder;
			public void Dispose () {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
if (!builder.gizmos.IsAllocated || !(builder.gizmos.Target is DrawingData data) || !data.data.StillExists(builder.uniqueID)) throw new System.InvalidOperationException("The drawing instance this line width scope belongs to no longer exists. Line width scopes cannot survive for longer than a frame unless you have a custom drawing instance. Are you using a line width scope inside a coroutine?");
#endif
unsafe {
builder.PopLineWidth();
builder.buffer = null;
}
}
}

		/// <summary>
		/// Scope to draw multiple things with an implicit matrix transformation.
		/// All coordinates for items drawn inside the scope will be multiplied by the matrix.
		/// If WithMatrix scopes are nested then coordinates are multiplied by all nested matrices in order.
		///
		/// <code>
		/// using (Draw.InLocalSpace(transform)) {
		///     // Draw a box at (0,0,0) relative to the current object
		///     // This means it will show up at the object's position
		///     Draw.WireBox(Vector3.zero, Vector3.one);
		/// }
		///
		/// // Equivalent code using the lower level WithMatrix scope
		/// using (Draw.WithMatrix(transform.localToWorldMatrix)) {
		///     Draw.WireBox(Vector3.zero, Vector3.one);
		/// }
		/// </code>
		///
		/// See: <see cref="InLocalSpace"/>
		/// </summary>
		[BurstDiscard]
		public ScopeMatrix WithMatrix (Matrix4x4 matrix) {
			PushMatrix(matrix);
			// TODO: Keep track of alive scopes and prevent dispose unless all scopes have been disposed
			unsafe {
				return new ScopeMatrix { builder = this };
			}
		}

		/// <summary>
		/// Scope to draw multiple things with an implicit matrix transformation.
		/// All coordinates for items drawn inside the scope will be multiplied by the matrix.
		/// If WithMatrix scopes are nested then coordinates are multiplied by all nested matrices in order.
		///
		/// <code>
		/// using (Draw.InLocalSpace(transform)) {
		///     // Draw a box at (0,0,0) relative to the current object
		///     // This means it will show up at the object's position
		///     Draw.WireBox(Vector3.zero, Vector3.one);
		/// }
		///
		/// // Equivalent code using the lower level WithMatrix scope
		/// using (Draw.WithMatrix(transform.localToWorldMatrix)) {
		///     Draw.WireBox(Vector3.zero, Vector3.one);
		/// }
		/// </code>
		///
		/// See: <see cref="InLocalSpace"/>
		/// </summary>
		[BurstDiscard]
		public ScopeMatrix WithMatrix (float3x3 matrix) {
			PushMatrix(new float4x4(matrix, float3.zero));
			// TODO: Keep track of alive scopes and prevent dispose unless all scopes have been disposed
			unsafe {
				return new ScopeMatrix { builder = this };
			}
		}

		/// <summary>
		/// Scope to draw multiple things with the same color.
		///
		/// <code>
		/// void Update () {
		///     using (Draw.WithColor(Color.red)) {
		///         Draw.Line(new Vector3(0, 0, 0), new Vector3(1, 1, 1));
		///         Draw.Line(new Vector3(0, 0, 0), new Vector3(0, 1, 2));
		///     }
		/// }
		/// </code>
		///
		/// Any command that is passed an explicit color parameter will override this color.
		/// If another color scope is nested inside this one then that scope will override this color.
		/// </summary>
		[BurstDiscard]
		public ScopeColor WithColor (Color color) {
			PushColor(color);
			unsafe {
				return new ScopeColor { builder = this };
			}
		}

		/// <summary>
		/// Scope to draw multiple things for a longer period of time.
		///
		/// Normally drawn items will only be rendered for a single frame.
		/// Using a persist scope you can make the items be drawn for any amount of time.
		///
		/// <code>
		/// void Update () {
		///     using (Draw.WithDuration(1.0f)) {
		///         var offset = Time.time;
		///         Draw.Line(new Vector3(offset, 0, 0), new Vector3(offset, 0, 1));
		///     }
		/// }
		/// </code>
		///
		/// Note: Outside of play mode the duration is measured against Unity's Time.realtimeSinceStartup.
		///
		/// Warning: It is recommended not to use this inside a DrawGizmos callback since DrawGizmos is called every frame anyway.
		/// </summary>
		/// <param name="duration">How long the drawn items should persist in seconds.</param>

		[BurstDiscard]
		public ScopePersist WithDuration (float duration) {
			PushDuration(duration);
			unsafe {
				return new ScopePersist { builder = this };
			}
		}

		/// <summary>
		/// Scope to draw multiple things with a given line width.
		///
		/// Note that the line join algorithm is a quite simple one optimized for speed. It normally looks good on a 2D plane, but if the polylines curve a lot in 3D space then
		/// it can look odd from some angles.
		///
		/// [Open online documentation to see images]
		///
		/// In the picture the top row has automaticJoins enabled and in the bottom row it is disabled.
		/// </summary>
		/// <param name="pixels">Line width in pixels</param>
		/// <param name="automaticJoins">If true then sequences of lines that are adjacent will be automatically joined at their vertices. This typically produces nicer polylines without weird gaps.</param>
		[BurstDiscard]
		public ScopeLineWidth WithLineWidth (float pixels, bool automaticJoins = true) {
			PushLineWidth(pixels, automaticJoins);
			unsafe {
				return new ScopeLineWidth { builder = this };
			}
		}

		/// <summary>
		/// Scope to draw multiple things relative to a transform object.
		/// All coordinates for items drawn inside the scope will be multiplied by the transform's localToWorldMatrix.
		///
		/// <code>
		/// void Update () {
		///     using (Draw.InLocalSpace(transform)) {
		///         // Draw a box at (0,0,0) relative to the current object
		///         // This means it will show up at the object's position
		///         // The box is also rotated and scaled with the transform
		///         Draw.WireBox(Vector3.zero, Vector3.one);
		///     }
		/// }
		/// </code>
		///
		/// [Open online documentation to see videos]
		/// </summary>
		[BurstDiscard]
		public ScopeMatrix InLocalSpace (Transform transform) {
			return WithMatrix(transform.localToWorldMatrix);
		}

		/// <summary>
		/// Scope to draw multiple things in screen space of a camera.
		/// If you draw 2D coordinates (i.e. (x,y,0)) they will be projected onto a plane approximately [2*near clip plane of the camera] world units in front of the camera (but guaranteed to be between the near and far planes).
		///
		/// The lower left corner of the camera is (0,0,0) and the upper right is (camera.pixelWidth, camera.pixelHeight, 0)
		///
		/// Note: As a corollary, the centers of pixels are offset by 0.5. So for example the center of the top left pixel is at (0.5, 0.5, 0).
		/// Therefore, if you want to draw 1 pixel wide lines in screen space, you may want to offset the coordinates by 0.5 pixels.
		///
		/// See: <see cref="InLocalSpace"/>
		/// See: <see cref="WithMatrix"/>
		/// </summary>
		[BurstDiscard]
		public ScopeMatrix InScreenSpace (Camera camera) {
			return WithMatrix(camera.cameraToWorldMatrix * camera.nonJitteredProjectionMatrix.inverse * Matrix4x4.TRS(new Vector3(-1.0f, -1.0f, 0), Quaternion.identity, new Vector3(2.0f/camera.pixelWidth, 2.0f/camera.pixelHeight, 1)));
		}

		/// <summary>
		/// Multiply all coordinates until the next <see cref="PopMatrix"/> with the given matrix.
		///
		/// PushMatrix and PushSetMatrix are slightly different:
		///
		/// - PushMatrix stacks with all previously pushed matrices. The active matrix becomes the product of the given matrix and the previously active one.
		/// - PushSetMatrix sets the current matrix directly. The active matrix becomes the last pushed matrix.
		/// </summary>
		public void PushMatrix (Matrix4x4 matrix) {
			Reserve<float4x4>();
			Add(Command.PushMatrix);
			Add(matrix);
		}

		/// <summary>
		/// Multiply all coordinates until the next <see cref="PopMatrix"/> with the given matrix.
		///
		/// PushMatrix and PushSetMatrix are slightly different:
		///
		/// - PushMatrix stacks with all previously pushed matrices. The active matrix becomes the product of the given matrix and the previously active one.
		/// - PushSetMatrix sets the current matrix directly. The active matrix becomes the last pushed matrix.
		/// </summary>
		public void PushMatrix (float4x4 matrix) {
			Reserve<float4x4>();
			Add(Command.PushMatrix);
			Add(matrix);
		}

		/// <summary>
		/// Multiply all coordinates until the next <see cref="PopMatrix"/> with the given matrix.
		///
		/// PushMatrix and PushSetMatrix are slightly different:
		///
		/// - PushMatrix stacks with all previously pushed matrices. The active matrix becomes the product of the given matrix and the previously active one.
		/// - PushSetMatrix sets the current matrix directly. The active matrix becomes the last pushed matrix.
		/// </summary>
		public void PushSetMatrix (Matrix4x4 matrix) {
			Reserve<float4x4>();
			Add(Command.PushSetMatrix);
			Add((float4x4)matrix);
		}

		/// <summary>
		/// Multiply all coordinates until the next PopMatrix with the given matrix.
		///
		/// PushMatrix and PushSetMatrix are slightly different:
		///
		/// - PushMatrix stacks with all previously pushed matrices. The active matrix becomes the product of the given matrix and the previously active one.
		/// - PushSetMatrix sets the current matrix directly. The active matrix becomes the last pushed matrix.
		/// </summary>
		public void PushSetMatrix (float4x4 matrix) {
			Reserve<float4x4>();
			Add(Command.PushSetMatrix);
			Add(matrix);
		}

		/// <summary>
		/// Pops a matrix from the stack.
		///
		/// See: <see cref="PushMatrix"/>
		/// See: <see cref="PushSetMatrix"/>
		/// </summary>
		public void PopMatrix () {
			Reserve(4);
			Add(Command.PopMatrix);
		}

		/// <summary>
		/// Draws everything until the next PopColor with the given color.
		/// Any command that is passed an explicit color parameter will override this color.
		/// If another color scope is nested inside this one then that scope will override this color.
		/// </summary>
		public void PushColor (Color color) {
			Reserve<Color32>();
			Add(Command.PushColor);
			Add(ConvertColor(color));
		}

		/// <summary>Pops a color from the stack</summary>
		public void PopColor () {
			Reserve(4);
			Add(Command.PopColor);
		}

		/// <summary>
		/// Draws everything until the next PopDuration for a number of seconds.
		/// Warning: This is not recommended inside a DrawGizmos callback since DrawGizmos is called every frame anyway.
		/// </summary>
		public void PushDuration (float duration) {
			Reserve<PersistData>();
			Add(Command.PushPersist);
			// We must use the BurstTime variable which is updated more rarely than Time.time.
			// This is necessary because this code may be called from a burst job or from a different thread.
			// Time.time can only be accessed in the main thread.
			Add(new PersistData { endTime = SharedDrawingData.BurstTime.Data + duration });
		}

		/// <summary>Pops a duration scope from the stack</summary>
		public void PopDuration () {
			Reserve(4);
			Add(Command.PopPersist);
		}

		/// <summary>
		/// Draws everything until the next PopPersist for a number of seconds.
		/// Warning: This is not recommended inside a DrawGizmos callback since DrawGizmos is called every frame anyway.
		///
		/// Deprecated: Renamed to <see cref="PushDuration"/>
		/// </summary>
		[System.Obsolete("Renamed to PushDuration for consistency")]
		public void PushPersist (float duration) {
			PushDuration(duration);
		}

		/// <summary>
		/// Pops a persist scope from the stack.
		/// Deprecated: Renamed to <see cref="PopDuration"/>
		/// </summary>
		[System.Obsolete("Renamed to PopDuration for consistency")]
		public void PopPersist () {
			PopDuration();
		}

		/// <summary>
		/// Draws all lines until the next PopLineWidth with a given line width in pixels.
		///
		/// Note that the line join algorithm is a quite simple one optimized for speed. It normally looks good on a 2D plane, but if the polylines curve a lot in 3D space then
		/// it can look odd from some angles.
		///
		/// [Open online documentation to see images]
		///
		/// In the picture the top row has automaticJoins enabled and in the bottom row it is disabled.
		/// </summary>
		/// <param name="pixels">Line width in pixels</param>
		/// <param name="automaticJoins">If true then sequences of lines that are adjacent will be automatically joined at their vertices. This typically produces nicer polylines without weird gaps.</param>
		public void PushLineWidth (float pixels, bool automaticJoins = true) {
			if (pixels < 0) throw new System.ArgumentOutOfRangeException("pixels", "Line width must be positive");

			Reserve<LineWidthData>();
			Add(Command.PushLineWidth);
			Add(new LineWidthData { pixels = pixels, automaticJoins = automaticJoins });
		}

		/// <summary>Pops a line width scope from the stack</summary>
		public void PopLineWidth () {
			Reserve(4);
			Add(Command.PopLineWidth);
		}

		/// <summary>
		/// Draws a line between two points.
		///
		/// [Open online documentation to see images]
		///
		/// <code>
		/// void Update () {
		///     Draw.Line(Vector3.zero, Vector3.up);
		/// }
		/// </code>
		/// </summary>
		public void Line (float3 a, float3 b) {
			Reserve<LineData>();
			Add(Command.Line);
			Add(new LineData { a = a, b = b });
		}

		/// <summary>
		/// Draws a line between two points.
		///
		/// [Open online documentation to see images]
		///
		/// <code>
		/// void Update () {
		///     Draw.Line(Vector3.zero, Vector3.up);
		/// }
		/// </code>
		/// </summary>
		public void Line (Vector3 a, Vector3 b) {
			Reserve<LineData>();
			// Add(Command.Line);
			// Add(new LineDataV3 { a = a, b = b });

			// The code below is equivalent to the commented out code above.
			// But drawing lines is the most common operation so it needs to be really fast.
			// Having this hardcoded improves line rendering performance by about 8%.
			var bufferSize = BufferSize;

			unsafe {
				var newLen = bufferSize + 4 + 24;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
UnityEngine.Assertions.Assert.IsTrue(newLen <= buffer->Capacity);
#endif
var ptr = (byte*)buffer->Ptr + bufferSize;
*(Command*)ptr = Command.Line;
var lineData = (LineDataV3*)(ptr + 4);
lineData->a = a;
lineData->b = b;
buffer->Length = newLen;
}
}

		/// <summary>
		/// Draws a line between two points.
		///
		/// [Open online documentation to see images]
		///
		/// <code>
		/// void Update () {
		///     Draw.Line(Vector3.zero, Vector3.up);
		/// }
		/// </code>
		/// </summary>
		public void Line (Vector3 a, Vector3 b, Color color) {
			Reserve<Color32, LineData>();
			// Add(Command.Line | Command.PushColorInline);
			// Add(ConvertColor(color));
			// Add(new LineDataV3 { a = a, b = b });

			// The code below is equivalent to the code which is commented out above.
			// But drawing lines is the most common operation so it needs to be really fast
			// Having this hardcoded improves line rendering performance by about 8%.
			var bufferSize = BufferSize;

			unsafe {
				var newLen = bufferSize + 4 + 24 + 4;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
UnityEngine.Assertions.Assert.IsTrue(newLen <= buffer->Capacity);
#endif
var ptr = (byte*)buffer->Ptr + bufferSize;
*(Command*)ptr = Command.Line | Command.PushColorInline;
*(uint*)(ptr + 4) = ConvertColor(color);
var lineData = (LineDataV3*)(ptr + 8);
lineData->a = a;
lineData->b = b;
buffer->Length = newLen;
}
}

		/// <summary>
		/// Draws a ray starting at a point and going in the given direction.
		/// The ray will end at origin + direction.
		///
		/// [Open online documentation to see images]
		///
		/// <code>
		/// Draw.Ray(Vector3.zero, Vector3.up);
		/// </code>
		/// </summary>
		public void Ray (float3 origin, float3 direction) {
			Line(origin, origin + direction);
		}

		/// <summary>
		/// Draws a ray with a given length.
		///
		/// [Open online documentation to see images]
		///
		/// <code>
		/// Draw.Ray(Camera.main.ScreenPointToRay(Vector3.zero), 10);
		/// </code>
		/// </summary>
		public void Ray (Ray ray, float length) {
			Line(ray.origin, ray.origin + ray.direction * length);
		}

		/// <summary>
		/// Draws an arc between two points.
		///
		/// The rendered arc is the shortest arc between the two points.
		/// The radius of the arc will be equal to the distance between center and start.
		///
		/// [Open online documentation to see images]
		/// <code>
		/// float a1 = Mathf.PI*0.9f;
		/// float a2 = Mathf.PI*0.1f;
		/// var arcStart = new float3(Mathf.Cos(a1), 0, Mathf.Sin(a1));
		/// var arcEnd = new float3(Mathf.Cos(a2), 0, Mathf.Sin(a2));
		/// Draw.Arc(new float3(0, 0, 0), arcStart, arcEnd, color);
		/// </code>
		///
		/// See: <see cref="CommandBuilder2D.Circle(float3,float,float,float)"/>
		/// </summary>
		/// <param name="center">Center of the imaginary circle that the arc is part of.</param>
		/// <param name="start">Starting point of the arc.</param>
		/// <param name="end">End point of the arc.</param>
		public void Arc (float3 center, float3 start, float3 end) {
			var d1 = start - center;
			var d2 = end - center;
			var normal = math.cross(d2, d1);

			if (math.any(normal != 0) && math.all(math.isfinite(normal))) {
				var m = Matrix4x4.TRS(center, Quaternion.LookRotation(d1, normal), Vector3.one);
				var angle = Vector3.SignedAngle(d1, d2, normal) * Mathf.Deg2Rad;
				PushMatrix(m);
				CircleXZInternal(float3.zero, math.length(d1), 90 * Mathf.Deg2Rad, 90 * Mathf.Deg2Rad - angle);
				PopMatrix();
			}
		}

		/// <summary>
		/// Draws a circle in the XZ plane.
		///
		/// You can draw an arc by supplying the startAngle and endAngle parameters.
		///
		/// [Open online documentation to see images]
		///
		/// See: <see cref="CommandBuilder.Circle(float3,float3,float)"/>
		/// See: <see cref="CircleXY(float3,float,float,float)"/>
		/// See: <see cref="Arc(float3,float3,float3)"/>
		/// </summary>
		/// <param name="center">Center of the circle or arc.</param>
		/// <param name="radius">Radius of the circle or arc.</param>
		/// <param name="startAngle">Starting angle in radians. 0 corrsponds to the positive X axis.</param>
		/// <param name="endAngle">End angle in radians.</param>
		[System.Obsolete("Use Draw.xz.Circle instead")]
		public void CircleXZ (float3 center, float radius, float startAngle = 0f, float endAngle = 2 * Mathf.PI) {
			CircleXZInternal(center, radius, startAngle, endAngle);
		}

		internal void CircleXZInternal (float3 center, float radius, float startAngle = 0f, float endAngle = 2 * Mathf.PI) {
			Reserve<CircleXZData>();
			Add(Command.CircleXZ);
			Add(new CircleXZData { center = center, radius = radius, startAngle = startAngle, endAngle = endAngle });
		}

		internal void CircleXZInternal (float3 center, float radius, float startAngle, float endAngle, Color color) {
			Reserve<Color32, CircleXZData>();
			Add(Command.CircleXZ | Command.PushColorInline);
			Add(ConvertColor(color));
			Add(new CircleXZData { center = center, radius = radius, startAngle = startAngle, endAngle = endAngle });
		}

		internal static readonly float4x4 XZtoXYPlaneMatrix = float4x4.RotateX(-math.PI*0.5f);
		internal static readonly float4x4 XZtoYZPlaneMatrix = float4x4.RotateZ(math.PI*0.5f);

		/// <summary>
		/// Draws a circle in the XY plane.
		///
		/// You can draw an arc by supplying the startAngle and endAngle parameters.
		///
		/// [Open online documentation to see images]
		///
		/// See: <see cref="CommandBuilder.Circle(float3,float3,float)"/>
		/// See: <see cref="Arc(float3,float3,float3)"/>
		/// </summary>
		/// <param name="center">Center of the circle or arc.</param>
		/// <param name="radius">Radius of the circle or arc.</param>
		/// <param name="startAngle">Starting angle in radians. 0 corrsponds to the positive X axis.</param>
		/// <param name="endAngle">End angle in radians.</param>
		[System.Obsolete("Use Draw.xy.Circle instead")]
		public void CircleXY (float3 center, float radius, float startAngle = 0f, float endAngle = 2 * Mathf.PI) {
			PushMatrix(XZtoXYPlaneMatrix);
			CircleXZ(new float3(center.x, -center.z, center.y), radius, startAngle, endAngle);
			PopMatrix();
		}

		/// <summary>
		/// Draws a circle.
		///
		/// [Open online documentation to see images]
		///
		/// Note: This overload does not allow you to draw an arc. For that purpose use <see cref="Arc"/>, <see cref="CircleXY"/> or <see cref="CircleXZ"/> instead.
		/// </summary>
		public void Circle (float3 center, float3 normal, float radius) {
			Reserve<CircleData>();
			Add(Command.Circle);
			Add(new CircleData { center = center, normal = normal, radius = radius });
		}

		/// <summary>
		/// Draws a solid arc between two points.
		///
		/// The rendered arc is the shortest arc between the two points.
		/// The radius of the arc will be equal to the distance between center and start.
		///
		/// [Open online documentation to see images]
		/// <code>
		/// float a1 = Mathf.PI*0.9f;
		/// float a2 = Mathf.PI*0.1f;
		/// var arcStart = new float3(Mathf.Cos(a1), 0, Mathf.Sin(a1));
		/// var arcEnd = new float3(Mathf.Cos(a2), 0, Mathf.Sin(a2));
		/// Draw.SolidArc(new float3(0, 0, 0), arcStart, arcEnd, color);
		/// </code>
		///
		/// See: <see cref="CommandBuilder2D.SolidCircle(float3,float,float,float)"/>
		/// </summary>
		/// <param name="center">Center of the imaginary circle that the arc is part of.</param>
		/// <param name="start">Starting point of the arc.</param>
		/// <param name="end">End point of the arc.</param>
		public void SolidArc (float3 center, float3 start, float3 end) {
			var d1 = start - center;
			var d2 = end - center;
			var normal = math.cross(d2, d1);

			if (math.any(normal)) {
				var m = Matrix4x4.TRS(center, Quaternion.LookRotation(d1, normal), Vector3.one);
				var angle = Vector3.SignedAngle(d1, d2, normal) * Mathf.Deg2Rad;
				PushMatrix(m);
				SolidCircleXZInternal(float3.zero, math.length(d1), 90 * Mathf.Deg2Rad, 90 * Mathf.Deg2Rad - angle);
				PopMatrix();
			}
		}

		/// <summary>
		/// Draws a disc in the XZ plane.
		///
		/// You can draw an arc by supplying the startAngle and endAngle parameters.
		///
		/// [Open online documentation to see images]
		///
		/// See: <see cref="SolidCircle(float3,float3,float)"/>
		/// See: <see cref="CommandBuilder2D.SolidCircle(float3,float,float,float)"/>
		/// See: <see cref="SolidArc(float3,float3,float3)"/>
		/// </summary>
		/// <param name="center">Center of the disc or solid arc.</param>
		/// <param name="radius">Radius of the disc or solid arc.</param>
		/// <param name="startAngle">Starting angle in radians. 0 corrsponds to the positive X axis.</param>
		/// <param name="endAngle">End angle in radians.</param>
		[System.Obsolete("Use Draw.xz.SolidCircle instead")]
		public void SolidCircleXZ (float3 center, float radius, float startAngle = 0f, float endAngle = 2 * Mathf.PI) {
			SolidCircleXZInternal(center, radius, startAngle, endAngle);
		}

		internal void SolidCircleXZInternal (float3 center, float radius, float startAngle = 0f, float endAngle = 2 * Mathf.PI) {
			Reserve<CircleXZData>();
			Add(Command.DiscXZ);
			Add(new CircleXZData { center = center, radius = radius, startAngle = startAngle, endAngle = endAngle });
		}

		internal void SolidCircleXZInternal (float3 center, float radius, float startAngle, float endAngle, Color color) {
			Reserve<Color32, CircleXZData>();
			Add(Command.DiscXZ | Command.PushColorInline);
			Add(ConvertColor(color));
			Add(new CircleXZData { center = center, radius = radius, startAngle = startAngle, endAngle = endAngle });
		}

		/// <summary>
		/// Draws a disc in the XY plane.
		///
		/// You can draw an arc by supplying the startAngle and endAngle parameters.
		///
		/// [Open online documentation to see images]
		///
		/// See: <see cref="SolidCircle(float3,float3,float)"/>
		/// See: <see cref="CommandBuilder2D.SolidCircle(float3,float,float,float)"/>
		/// See: <see cref="SolidArc(float3,float3,float3)"/>
		/// </summary>
		/// <param name="center">Center of the disc or solid arc.</param>
		/// <param name="radius">Radius of the disc or solid arc.</param>
		/// <param name="startAngle">Starting angle in radians. 0 corrsponds to the positive X axis.</param>
		/// <param name="endAngle">End angle in radians.</param>
		[System.Obsolete("Use Draw.xy.SolidCircle instead")]
		public void SolidCircleXY (float3 center, float radius, float startAngle = 0f, float endAngle = 2 * Mathf.PI) {
			PushMatrix(XZtoXYPlaneMatrix);
			SolidCircleXZInternal(new float3(center.x, -center.z, center.y), radius, startAngle, endAngle);
			PopMatrix();
		}

		/// <summary>
		/// Draws a disc.
		///
		/// [Open online documentation to see images]
		///
		/// Note: This overload does not allow you to draw an arc. For that purpose use <see cref="SolidArc"/> or <see cref="CommandBuilder2D.SolidCircle(float3,float,float,float)"/> instead.
		/// </summary>
		public void SolidCircle (float3 center, float3 normal, float radius) {
			Reserve<CircleData>();
			Add(Command.Disc);
			Add(new CircleData { center = center, normal = normal, radius = radius });
		}

		/// <summary>
		/// Draws a circle outline around a sphere.
		///
		/// Visually, this is a circle that always faces the camera, and is resized automatically to fit the sphere.
		///
		/// [Open online documentation to see images]
		/// </summary>
		public void SphereOutline (float3 center, float radius) {
			Reserve<SphereData>();
			Add(Command.SphereOutline);
			Add(new SphereData { center = center, radius = radius });
		}

		/// <summary>
		/// Draws a cylinder.
		/// The cylinder's bottom circle will be centered at the bottom parameter and similarly for the top circle.
		///
		/// <code>
		/// // Draw a tilted cylinder between the points (0,0,0) and (1,1,1) with a radius of 0.5
		/// Draw.WireCylinder(Vector3.zero, Vector3.one, 0.5f, Color.black);
		/// </code>
		///
		/// [Open online documentation to see images]
		/// </summary>
		public void WireCylinder (float3 bottom, float3 top, float radius) {
			WireCylinder(bottom, top - bottom, math.length(top - bottom), radius);
		}

		/// <summary>
		/// Draws a cylinder.
		///
		/// <code>
		/// // Draw a two meter tall cylinder at the world origin with a radius of 0.5
		/// Draw.WireCylinder(Vector3.zero, Vector3.up, 2, 0.5f, Color.black);
		/// </code>
		///
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="position">The center of the cylinder's "bottom" circle.</param>
		/// <param name="up">The cylinder's main axis. Does not have to be normalized. If zero, nothing will be drawn.</param>
		/// <param name="height">The length of the cylinder, as measured along it's main axis.</param>
		/// <param name="radius">The radius of the cylinder.</param>
		public void WireCylinder (float3 position, float3 up, float height, float radius) {
			up = math.normalizesafe(up);
			if (math.all(up == 0) || math.any(math.isnan(up)) || math.isnan(height) || math.isnan(radius)) return;

			OrthonormalBasis(up, out var basis1, out var basis2);

			PushMatrix(new float4x4(
				new float4(basis1 * radius, 0),
				new float4(up * height, 0),
				new float4(basis2 * radius, 0),
				new float4(position, 1)
				));

			CircleXZInternal(float3.zero, 1);
			if (height > 0) {
				CircleXZInternal(new float3(0, 1, 0), 1);
				Line(new float3(1, 0, 0), new float3(1, 1, 0));
				Line(new float3(-1, 0, 0), new float3(-1, 1, 0));
				Line(new float3(0, 0, 1), new float3(0, 1, 1));
				Line(new float3(0, 0, -1), new float3(0, 1, -1));
			}
			PopMatrix();
		}

		/// <summary>
		/// Constructs an orthonormal basis from a single normal vector.
		///
		/// This is similar to math.orthonormal_basis, but it tries harder to be continuous in its input.
		/// In contrast, math.orthonormal_basis has a tendency to jump around even with small changes to the normal.
		///
		/// It's not as fast as math.orthonormal_basis, though.
		/// </summary>
		static void OrthonormalBasis (float3 normal, out float3 basis1, out float3 basis2) {
			basis1 = math.cross(normal, new float3(1, 1, 1));
			if (math.all(basis1 == 0)) basis1 = math.cross(normal, new float3(-1, 1, 1));
			basis1 = math.normalizesafe(basis1);
			basis2 = math.cross(normal, basis1);
		}

		/// <summary>
		/// Draws a capsule with a (start,end) parameterization.
		///
		/// The behavior of this method matches common Unity APIs such as Physics.CheckCapsule.
		///
		/// <code>
		/// // Draw a tilted capsule between the points (0,0,0) and (1,1,1) with a radius of 0.5
		/// Draw.WireCapsule(Vector3.zero, Vector3.one, 0.5f, Color.black);
		/// </code>
		///
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="start">Center of the start hemisphere of the capsule.</param>
		/// <param name="end">Center of the end hemisphere of the capsule.</param>
		/// <param name="radius">Radius of the capsule.</param>
		public void WireCapsule (float3 start, float3 end, float radius) {
			var dir = end - start;
			var length = math.length(dir);

			if (length < 0.0001) {
				// The endpoints are the same, we can't draw a capsule from this because we don't know its orientation.
				// Draw a sphere as a fallback
				WireSphere(start, radius);
			} else {
				var normalized_dir = dir / length;

				WireCapsule(start - normalized_dir*radius, normalized_dir, length + 2*radius, radius);
			}
		}

		// TODO: Change to center, up, height parameterization
		/// <summary>
		/// Draws a capsule with a (position,direction/length) parameterization.
		///
		/// <code>
		/// // Draw a capsule that touches the y=0 plane, is 2 meters tall and has a radius of 0.5
		/// Draw.WireCapsule(Vector3.zero, Vector3.up, 2.0f, 0.5f, Color.black);
		/// </code>
		///
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="position">One endpoint of the capsule. This is at the edge of the capsule, not at the center of one of the hemispheres.</param>
		/// <param name="direction">The main axis of the capsule. Does not have to be normalized. If zero, nothing will be drawn.</param>
		/// <param name="length">Distance between the two endpoints of the capsule. The length will be clamped to be at least 2*radius.</param>
		/// <param name="radius">The radius of the capsule.</param>
		public void WireCapsule (float3 position, float3 direction, float length, float radius) {
			direction = math.normalizesafe(direction);
			if (math.all(direction == 0) || math.any(math.isnan(direction)) || math.isnan(length) || math.isnan(radius)) return;

			if (radius <= 0) {
				Line(position, position + direction * length);
			} else {
				length = math.max(length, radius*2);
				OrthonormalBasis(direction, out var basis1, out var basis2);

				PushMatrix(new float4x4(
					new float4(basis1, 0),
					new float4(direction, 0),
					new float4(basis2, 0),
					new float4(position, 1)
					));
				CircleXZInternal(new float3(0, radius, 0), radius);
				PushMatrix(XZtoXYPlaneMatrix);
				CircleXZInternal(new float3(0, 0, radius), radius, Mathf.PI, 2 * Mathf.PI);
				PopMatrix();
				PushMatrix(XZtoYZPlaneMatrix);
				CircleXZInternal(new float3(radius, 0, 0), radius, Mathf.PI*0.5f, Mathf.PI*1.5f);
				PopMatrix();
				if (length > 0) {
					var upperY = length - radius;
					var lowerY = radius;
					CircleXZInternal(new float3(0, upperY, 0), radius);
					PushMatrix(XZtoXYPlaneMatrix);
					CircleXZInternal(new float3(0, 0, upperY), radius, 0, Mathf.PI);
					PopMatrix();
					PushMatrix(XZtoYZPlaneMatrix);
					CircleXZInternal(new float3(upperY, 0, 0), radius, -Mathf.PI*0.5f, Mathf.PI*0.5f);
					PopMatrix();
					Line(new float3(radius, lowerY, 0), new float3(radius, upperY, 0));
					Line(new float3(-radius, lowerY, 0), new float3(-radius, upperY, 0));
					Line(new float3(0, lowerY, radius), new float3(0, upperY, radius));
					Line(new float3(0, lowerY, -radius), new float3(0, upperY, -radius));
				}
				PopMatrix();
			}
		}

		/// <summary>
		/// Draws a wire sphere.
		///
		/// [Open online documentation to see images]
		///
		/// <code>
		/// // Draw a wire sphere at the origin with a radius of 0.5
		/// Draw.WireSphere(Vector3.zero, 0.5f, Color.black);
		/// </code>
		///
		/// See: <see cref="Circle"/>
		/// </summary>
		public void WireSphere (float3 position, float radius) {
			SphereOutline(position, radius);
			Circle(position, new float3(1, 0, 0), radius);
			Circle(position, new float3(0, 1, 0), radius);
			Circle(position, new float3(0, 0, 1), radius);
		}

		/// <summary>
		/// Draws lines through a sequence of points.
		///
		/// [Open online documentation to see images]
		/// <code>
		/// // Draw a square
		/// Draw.Polyline(new [] { new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 0) }, true);
		/// </code>
		/// </summary>
		/// <param name="points">Sequence of points to draw lines through</param>
		/// <param name="cycle">If true a line will be drawn from the last point in the sequence back to the first point.</param>
		[BurstDiscard]
		public void Polyline (List<Vector3> points, bool cycle = false) {
			for (int i = 0; i < points.Count - 1; i++) {
				Line(points[i], points[i+1]);
			}
			if (cycle && points.Count > 1) Line(points[points.Count - 1], points[0]);
		}

		/// <summary>
		/// Draws lines through a sequence of points.
		///
		/// [Open online documentation to see images]
		/// <code>
		/// // Draw a square
		/// Draw.Polyline(new [] { new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 0) }, true);
		/// </code>
		/// </summary>
		/// <param name="points">Sequence of points to draw lines through</param>
		/// <param name="cycle">If true a line will be drawn from the last point in the sequence back to the first point.</param>
		public void Polyline<T>(T points, bool cycle = false) where T : IReadOnlyList<float3> {
			for (int i = 0; i < points.Count - 1; i++) {
				Line(points[i], points[i+1]);
			}
			if (cycle && points.Count > 1) Line(points[points.Count - 1], points[0]);
		}

		/// <summary>
		/// Draws lines through a sequence of points.
		///
		/// [Open online documentation to see images]
		/// <code>
		/// // Draw a square
		/// Draw.Polyline(new [] { new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 0) }, true);
		/// </code>
		/// </summary>
		/// <param name="points">Sequence of points to draw lines through</param>
		/// <param name="cycle">If true a line will be drawn from the last point in the sequence back to the first point.</param>
		[BurstDiscard]
		public void Polyline (Vector3[] points, bool cycle = false) {
			for (int i = 0; i < points.Length - 1; i++) {
				Line(points[i], points[i+1]);
			}
			if (cycle && points.Length > 1) Line(points[points.Length - 1], points[0]);
		}

		/// <summary>
		/// Draws lines through a sequence of points.
		///
		/// [Open online documentation to see images]
		/// <code>
		/// // Draw a square
		/// Draw.Polyline(new [] { new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 0) }, true);
		/// </code>
		/// </summary>
		/// <param name="points">Sequence of points to draw lines through</param>
		/// <param name="cycle">If true a line will be drawn from the last point in the sequence back to the first point.</param>
		[BurstDiscard]
		public void Polyline (float3[] points, bool cycle = false) {
			for (int i = 0; i < points.Length - 1; i++) {
				Line(points[i], points[i+1]);
			}
			if (cycle && points.Length > 1) Line(points[points.Length - 1], points[0]);
		}

		/// <summary>
		/// Draws lines through a sequence of points.
		///
		/// [Open online documentation to see images]
		/// <code>
		/// // Draw a square
		/// Draw.Polyline(new [] { new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 0) }, true);
		/// </code>
		/// </summary>
		/// <param name="points">Sequence of points to draw lines through</param>
		/// <param name="cycle">If true a line will be drawn from the last point in the sequence back to the first point.</param>
		public void Polyline (NativeArray<float3> points, bool cycle = false) {
			for (int i = 0; i < points.Length - 1; i++) {
				Line(points[i], points[i+1]);
			}
			if (cycle && points.Length > 1) Line(points[points.Length - 1], points[0]);
		}

		/// <summary>Determines the symbol to use for <see cref="PolylineWithSymbol"/></summary>
		public enum SymbolDecoration : byte {
			/// <summary>
			/// No symbol.
			///
			/// Space will still be reserved, but no symbol will be drawn.
			/// Can be used to draw dashed lines.
			///
			/// [Open online documentation to see images]
			/// </summary>
			None,
			/// <summary>
			/// An arrowhead symbol.
			///
			/// [Open online documentation to see images]
			/// </summary>
			ArrowHead,
			/// <summary>
			/// A circle symbol.
			///
			/// [Open online documentation to see images]
			/// </summary>
			Circle,
		}

		/// <summary>
		/// Draws a dashed line between two points.
		///
		/// <code>
		/// Draw.DashedPolyline(points, 0.1f, 0.1f, color);
		/// </code>
		///
		/// [Open online documentation to see images]
		///
		/// Warning: An individual line segment is drawn for each dash. This means that performance may suffer if you make the dash + gap distance too small.
		/// But for most use cases the performance is nothing to worry about.
		///
		/// See: <see cref="DashedPolyline"/>
		/// See: <see cref="PolylineWithSymbol"/>
		/// </summary>
		public void DashedLine (float3 a, float3 b, float dash, float gap) {
			var p = new PolylineWithSymbol(SymbolDecoration.None, gap, 0, dash + gap);
			p.MoveTo(ref this, a);
			p.MoveTo(ref this, b);
		}

		/// <summary>
		/// Draws a dashed line through a sequence of points.
		///
		/// <code>
		/// Draw.DashedPolyline(points, 0.1f, 0.1f, color);
		/// </code>
		///
		/// [Open online documentation to see images]
		///
		/// Warning: An individual line segment is drawn for each dash. This means that performance may suffer if you make the dash + gap distance too small.
		/// But for most use cases the performance is nothing to worry about.
		///
		/// If you have a different collection type, or you do not have the points in a collection at all, then you can use the <see cref="PolylineWithSymbol"/> struct directly.
		///
		/// <code>
		/// using (Draw.WithColor(color)) {
		///     var dash = 0.1f;
		///     var gap = 0.1f;
		///     var p = new CommandBuilder.PolylineWithSymbol(CommandBuilder.SymbolDecoration.None, gap, 0, dash + gap);
		///     for (int i = 0; i < points.Count; i++) {
		///         p.MoveTo(ref Draw.editor, points[i]);
		///     }
		/// }
		/// </code>
		///
		/// See: <see cref="DashedLine"/>
		/// See: <see cref="PolylineWithSymbol"/>
		/// </summary>
		public void DashedPolyline (List<Vector3> points, float dash, float gap) {
			var p = new PolylineWithSymbol(SymbolDecoration.None, gap, 0, dash + gap);
			for (int i = 0; i < points.Count; i++) {
				p.MoveTo(ref this, points[i]);
			}
		}

		/// <summary>
		/// Helper for drawing a polyline with symbols at regular intervals.
		///
		/// <code>
		/// var generator = new CommandBuilder.PolylineWithSymbol(CommandBuilder.SymbolDecoration.Circle, 0.2f, 0.0f, 0.47f);
		/// generator.MoveTo(ref Draw.editor, new float3(-0.5f, 0, -0.5f));
		/// generator.MoveTo(ref Draw.editor, new float3(0.5f, 0, 0.5f));
		/// </code>
		///
		/// [Open online documentation to see images]
		///
		/// [Open online documentation to see images]
		///
		/// You can also draw a dashed line using this struct, but for common cases you can use the <see cref="DashedPolyline"/> helper function instead.
		///
		/// <code>
		/// using (Draw.WithColor(color)) {
		///     var dash = 0.1f;
		///     var gap = 0.1f;
		///     var p = new CommandBuilder.PolylineWithSymbol(CommandBuilder.SymbolDecoration.None, gap, 0, dash + gap);
		///     for (int i = 0; i < points.Count; i++) {
		///         p.MoveTo(ref Draw.editor, points[i]);
		///     }
		/// }
		/// </code>
		///
		/// [Open online documentation to see images]
		/// </summary>
		public struct PolylineWithSymbol {
			float3 prev;
			float offset;
			readonly float symbolSize;
			readonly float connectingSegmentLength;
			readonly float symbolPadding;
			readonly float symbolOffset;

			/// <summary>
			/// The up direction of the symbols.
			///
			/// This is used to determine the orientation of the symbols.
			/// By default this is set to (0,1,0).
			/// </summary>
			public float3 up;

			readonly SymbolDecoration symbol;
			State state;
			readonly bool reverseSymbols;

			enum State : byte {
				NotStarted,
				ConnectingSegment,
				PreSymbolPadding,
				Symbol,
				PostSymbolPadding,
			}

			/// <summary>
			/// Create a new polyline with symbol generator.
			///
			/// Note: If symbolSize + 2*symbolPadding > symbolSpacing, the symbolSpacing parameter will be increased to accommodate the symbol and its padding.
			/// There will be no connecting lines between the symbols in this case, as there's no space for them.
			/// </summary>
			/// <param name="symbol">The symbol to use</param>
			/// <param name="symbolSize">The size of the symbol. In case of a circle, this is the diameter.</param>
			/// <param name="symbolPadding">The padding on both sides of the symbol between the symbol and the line.</param>
			/// <param name="symbolSpacing">The spacing between symbols. This is the distance between the centers of the symbols.</param>
			/// <param name="reverseSymbols">If true, the symbols will be reversed. For cicles this has no effect, but arrowhead symbols will be reversed.</param>
			/// <param name="offset">Distance to shift all symbols forward along the line. Useful for animations. If offset=0, the first symbol's center is at symbolSpacing/2.</param>
			public PolylineWithSymbol(SymbolDecoration symbol, float symbolSize, float symbolPadding, float symbolSpacing, bool reverseSymbols = false, float offset = 0) {
				if (symbolSpacing <= math.FLT_MIN_NORMAL) throw new System.ArgumentOutOfRangeException(nameof(symbolSpacing), "Symbol spacing must be greater than zero");
				if (symbolSize <= math.FLT_MIN_NORMAL) throw new System.ArgumentOutOfRangeException(nameof(symbolSize), "Symbol size must be greater than zero");
				if (symbolPadding < 0) throw new System.ArgumentOutOfRangeException(nameof(symbolPadding), "Symbol padding must non-negative");

				this.prev = float3.zero;
				this.symbol = symbol;
				this.symbolSize = symbolSize;
				this.symbolPadding = symbolPadding;
				this.connectingSegmentLength = math.max(0, symbolSpacing - symbolPadding * 2f - symbolSize);
				// Calculate actual value, after clamping to a valid range
				symbolSpacing = symbolPadding * 2 + symbolSize + connectingSegmentLength;
				this.reverseSymbols = reverseSymbols;
				this.up = new float3(0, 1, 0);
				symbolOffset = symbol == SymbolDecoration.ArrowHead ? -0.25f * symbolSize : 0;
				if (reverseSymbols) {
					symbolOffset = -symbolOffset;
				}
				symbolOffset += 0.5f * symbolSize;
				this.offset = (this.connectingSegmentLength * 0.5f + offset) % symbolSpacing;
				// Ensure the initial offset is always negative. This makes the state machine start in the correct state when the offset turns positive.
				if (this.offset > 0) this.offset -= symbolSpacing;
				this.state = State.NotStarted;
			}

			/// <summary>
			/// Move to a new point.
			///
			/// This will draw the symbols and line segments between the previous point and the new point.
			/// </summary>
			/// <param name="draw">The command builder to draw to. You can use a built-in builder like \reflink{Draw.editor} or \reflink{Draw.ingame}, or use a custom one.</param>
			/// <param name="next">The next point in the polyline to move to.</param>
			public void MoveTo (ref CommandBuilder draw, float3 next) {
				if (state == State.NotStarted) {
					prev = next;
					state = State.ConnectingSegment;
					return;
				}

				var len = math.length(next - prev);
				var invLen = math.rcp(len);
				var dir = next - prev;
				float3 up = default;
				if (symbol != SymbolDecoration.None) {
					up = math.normalizesafe(math.cross(dir, math.cross(dir, this.up)));
					if (math.all(up == 0f)) {
						up = new float3(0, 0, 1);
					}
					if (reverseSymbols) dir = -dir;
				}

				var currentPositionOnSegment = 0f;
				while (true) {
					if (state == State.ConnectingSegment) {
						if (offset >= 0 && offset != currentPositionOnSegment) {
							currentPositionOnSegment = math.max(0, currentPositionOnSegment);
							var pLast = math.lerp(prev, next, currentPositionOnSegment * invLen);
							var p = math.lerp(prev, next, math.min(offset * invLen, 1));
							draw.Line(pLast, p);
						}

						if (offset < len) {
							state = State.PreSymbolPadding;
							currentPositionOnSegment = offset;
							offset += symbolPadding;
						} else {
							break;
						}
					} else if (state == State.PreSymbolPadding) {
						if (offset >= len) break;

						state = State.Symbol;
						currentPositionOnSegment = offset;
						offset += symbolOffset;
					} else if (state == State.Symbol) {
						if (offset >= len) break;

						if (offset >= 0) {
							var p = math.lerp(prev, next, offset * invLen);
							switch (symbol) {
							case SymbolDecoration.None:
								break;
							case SymbolDecoration.ArrowHead:
								draw.Arrowhead(p, dir, up, symbolSize);
								break;
							case SymbolDecoration.Circle:
							default:
								draw.Circle(p, up, symbolSize * 0.5f);
								break;
							}
						}

						state = State.PostSymbolPadding;
						currentPositionOnSegment = offset;
						offset += -symbolOffset + symbolSize + symbolPadding;
					} else if (state == State.PostSymbolPadding) {
						if (offset >= len) break;

						state = State.ConnectingSegment;
						currentPositionOnSegment = offset;
						offset += connectingSegmentLength;
					} else {
						throw new System.Exception("Invalid state");
					}
				}
				offset -= len;
				prev = next;
			}
		}

		/// <summary>
		/// Draws the outline of a box which is axis-aligned.
		///
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="center">Center of the box</param>
		/// <param name="size">Width of the box along all dimensions</param>
		public void WireBox (float3 center, float3 size) {
			Reserve<BoxData>();
			Add(Command.WireBox);
			Add(new BoxData { center = center, size = size });
		}

		/// <summary>
		/// Draws the outline of a box.
		///
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="center">Center of the box</param>
		/// <param name="rotation">Rotation of the box</param>
		/// <param name="size">Width of the box along all dimensions</param>
		public void WireBox (float3 center, quaternion rotation, float3 size) {
			PushMatrix(float4x4.TRS(center, rotation, size));
			WireBox(float3.zero, new float3(1, 1, 1));
			PopMatrix();
		}

		/// <summary>
		/// Draws the outline of a box.
		///
		/// [Open online documentation to see images]
		/// </summary>
		public void WireBox (Bounds bounds) {
			WireBox(bounds.center, bounds.size);
		}

		/// <summary>
		/// Draws a wire mesh.
		/// Every single edge of the mesh will be drawn using a <see cref="Line"/> command.
		///
		/// <code>
		/// var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		/// go.transform.position = new Vector3(0, 0, 0);
		/// using (Draw.InLocalSpace(go.transform)) {
		///     Draw.WireMesh(go.GetComponent<MeshFilter>().sharedMesh, color);
		/// }
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: <see cref="SolidMesh(Mesh)"/>
		///
		/// Version: Supported in Unity 2020.1 or later.
		/// </summary>
		public void WireMesh (Mesh mesh) {
#if UNITY_2020_1_OR_NEWER
if (mesh == null) throw new System.ArgumentNullException();

			// Use a burst compiled function to draw the lines
			// This is significantly faster than pure C# (about 5x).
			var meshDataArray = Mesh.AcquireReadOnlyMeshData(mesh);
			var meshData = meshDataArray[0];

			JobWireMesh.JobWireMeshFunctionPointer(ref meshData, ref this);
			meshDataArray.Dispose();
#else
Debug.LogError("The WireMesh method is only suppored in Unity 2020.1 or later");
#endif
}

		/// <summary>
		/// Draws a wire mesh.
		/// Every single edge of the mesh will be drawn using a <see cref="Line"/> command.
		///
		/// <code>
		/// var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		/// go.transform.position = new Vector3(0, 0, 0);
		/// using (Draw.InLocalSpace(go.transform)) {
		///     Draw.WireMesh(go.GetComponent<MeshFilter>().sharedMesh, color);
		/// }
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: <see cref="SolidMesh(Mesh)"/>
		///
		/// Version: Supported in Unity 2020.1 or later.
		/// </summary>
		public void WireMesh (NativeArray<float3> vertices, NativeArray<int> triangles) {
#if UNITY_2020_1_OR_NEWER
unsafe {
JobWireMesh.WireMesh((float3*)vertices.GetUnsafeReadOnlyPtr(), (int*)triangles.GetUnsafeReadOnlyPtr(), vertices.Length, triangles.Length, ref this);
}
#else
Debug.LogError("The WireMesh method is only suppored in Unity 2020.1 or later");
#endif
}

#if UNITY_2020_1_OR_NEWER
/// <summary>Helper job for <see cref="WireMesh"/></summary>
[BurstCompile]
class JobWireMesh {
public delegate void JobWireMeshDelegate(ref Mesh.MeshData rawMeshData, ref CommandBuilder draw);

			public static readonly JobWireMeshDelegate JobWireMeshFunctionPointer = BurstCompiler.CompileFunctionPointer<JobWireMeshDelegate>(Execute).Invoke;

			[BurstCompile]
			public static unsafe void WireMesh (float3* verts, int* indices, int vertexCount, int indexCount, ref CommandBuilder draw) {
				if (indexCount % 3 != 0) {
					throw new System.ArgumentException("Invalid index count. Must be a multiple of 3");
				}
				// Ignore warning about NativeHashMap being obsolete in early versions of the collections package.
				// It works just fine, and in later versions the NativeHashMap is not obsolete.
				#pragma warning disable 618
				var seenEdges = new NativeHashMap<int2, bool>(indexCount, Allocator.Temp);
				#pragma warning restore 618
				for (int i = 0; i < indexCount; i += 3) {
					var a = indices[i];
					var b = indices[i+1];
					var c = indices[i+2];
					if (a < 0 || b < 0 || c < 0 || a >= vertexCount || b >= vertexCount || c >= vertexCount) {
						throw new Exception("Invalid vertex index. Index out of bounds");
					}
					int v1, v2;

					// Draw each edge of the triangle.
					// Check so that we do not draw an edge twice.
					v1 = math.min(a, b);
					v2 = math.max(a, b);
					if (!seenEdges.ContainsKey(new int2(v1, v2))) {
						seenEdges.Add(new int2(v1, v2), true);
						draw.Line(verts[v1], verts[v2]);
					}

					v1 = math.min(b, c);
					v2 = math.max(b, c);
					if (!seenEdges.ContainsKey(new int2(v1, v2))) {
						seenEdges.Add(new int2(v1, v2), true);
						draw.Line(verts[v1], verts[v2]);
					}

					v1 = math.min(c, a);
					v2 = math.max(c, a);
					if (!seenEdges.ContainsKey(new int2(v1, v2))) {
						seenEdges.Add(new int2(v1, v2), true);
						draw.Line(verts[v1], verts[v2]);
					}
				}
			}

			[BurstCompile]
			[AOT.MonoPInvokeCallback(typeof(JobWireMeshDelegate))]
			static void Execute (ref Mesh.MeshData rawMeshData, ref CommandBuilder draw) {
				int maxIndices = 0;
				for (int subMeshIndex = 0; subMeshIndex < rawMeshData.subMeshCount; subMeshIndex++) {
					maxIndices = math.max(maxIndices, rawMeshData.GetSubMesh(subMeshIndex).indexCount);
				}
				var tris = new NativeArray<int>(maxIndices, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
				var verts = new NativeArray<Vector3>(rawMeshData.vertexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
				rawMeshData.GetVertices(verts);

				for (int subMeshIndex = 0; subMeshIndex < rawMeshData.subMeshCount; subMeshIndex++) {
					var submesh = rawMeshData.GetSubMesh(subMeshIndex);
					rawMeshData.GetIndices(tris, subMeshIndex);
					unsafe {
						WireMesh((float3*)verts.GetUnsafeReadOnlyPtr(), (int*)tris.GetUnsafeReadOnlyPtr(), verts.Length, submesh.indexCount, ref draw);
					}
				}
			}
		}
#endif

		/// <summary>
		/// Draws a solid mesh.
		/// The mesh will be drawn with a solid color.
		///
		/// <code>
		/// var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		/// go.transform.position = new Vector3(0, 0, 0);
		/// using (Draw.InLocalSpace(go.transform)) {
		///     Draw.SolidMesh(go.GetComponent<MeshFilter>().sharedMesh, color);
		/// }
		/// </code>
		/// [Open online documentation to see images]
		///
		/// Note: This method is not thread safe and must not be used from the Unity Job System.
		/// TODO: Are matrices handled?
		///
		/// See: <see cref="WireMesh(Mesh)"/>
		/// </summary>
		public void SolidMesh (Mesh mesh) {
			SolidMeshInternal(mesh, false);
		}

		void SolidMeshInternal (Mesh mesh, bool temporary, Color color) {
			PushColor(color);
			SolidMeshInternal(mesh, temporary);
			PopColor();
		}


		void SolidMeshInternal (Mesh mesh, bool temporary) {
			var g = gizmos.Target as DrawingData;

			g.data.Get(uniqueID).meshes.Add(new SubmittedMesh {
				mesh = mesh,
				temporary = temporary,
			});
			// Internally we need to make sure to capture the current state
			// (which includes the current matrix and color) so that it
			// can be applied to the mesh.
			Reserve(4);
			Add(Command.CaptureState);
		}

		/// <summary>
		/// Draws a solid mesh with the given vertices.
		///
		/// [Open online documentation to see images]
		///
		/// Note: This method is not thread safe and must not be used from the Unity Job System.
		/// TODO: Are matrices handled?
		/// </summary>
		[BurstDiscard]
		public void SolidMesh (List<Vector3> vertices, List<int> triangles, List<Color> colors) {
			if (vertices.Count != colors.Count) throw new System.ArgumentException("Number of colors must be the same as the number of vertices");

			// TODO: Is this mesh getting recycled at all?
			var g = gizmos.Target as DrawingData;
			var mesh = g.GetMesh(vertices.Count);

			// Set all data on the mesh
			mesh.Clear();
			mesh.SetVertices(vertices);
			mesh.SetTriangles(triangles, 0);
			mesh.SetColors(colors);
			// Upload all data
			mesh.UploadMeshData(false);
			SolidMeshInternal(mesh, true);
		}

		/// <summary>
		/// Draws a solid mesh with the given vertices.
		///
		/// [Open online documentation to see images]
		///
		/// Note: This method is not thread safe and must not be used from the Unity Job System.
		/// TODO: Are matrices handled?
		/// </summary>
		[BurstDiscard]
		public void SolidMesh (Vector3[] vertices, int[] triangles, Color[] colors, int vertexCount, int indexCount) {
			if (vertices.Length != colors.Length) throw new System.ArgumentException("Number of colors must be the same as the number of vertices");

			// TODO: Is this mesh getting recycled at all?
			var g = gizmos.Target as DrawingData;
			var mesh = g.GetMesh(vertices.Length);

			// Set all data on the mesh
			mesh.Clear();
			mesh.SetVertices(vertices, 0, vertexCount);
			mesh.SetTriangles(triangles, 0, indexCount, 0);
			mesh.SetColors(colors, 0, vertexCount);
			// Upload all data
			mesh.UploadMeshData(false);
			SolidMeshInternal(mesh, true);
		}

		/// <summary>
		/// Draws a 3D cross.
		///
		/// [Open online documentation to see images]
		/// </summary>
		public void Cross (float3 position, float size = 1) {
			size *= 0.5f;
			Line(position - new float3(size, 0, 0), position + new float3(size, 0, 0));
			Line(position - new float3(0, size, 0), position + new float3(0, size, 0));
			Line(position - new float3(0, 0, size), position + new float3(0, 0, size));
		}

		/// <summary>
		/// Draws a cross in the XZ plane.
		///
		/// [Open online documentation to see images]
		/// </summary>
		[System.Obsolete("Use Draw.xz.Cross instead")]
		public void CrossXZ (float3 position, float size = 1) {
			size *= 0.5f;
			Line(position - new float3(size, 0, 0), position + new float3(size, 0, 0));
			Line(position - new float3(0, 0, size), position + new float3(0, 0, size));
		}

		/// <summary>
		/// Draws a cross in the XY plane.
		///
		/// [Open online documentation to see images]
		/// </summary>
		[System.Obsolete("Use Draw.xy.Cross instead")]
		public void CrossXY (float3 position, float size = 1) {
			size *= 0.5f;
			Line(position - new float3(size, 0, 0), position + new float3(size, 0, 0));
			Line(position - new float3(0, size, 0), position + new float3(0, size, 0));
		}

		/// <summary>Returns a point on a cubic bezier curve. t is clamped between 0 and 1</summary>
		public static float3 EvaluateCubicBezier (float3 p0, float3 p1, float3 p2, float3 p3, float t) {
			t = math.clamp(t, 0, 1);
			float tr = 1-t;
			return tr*tr*tr * p0 + 3 * tr*tr * t * p1 + 3 * tr * t*t * p2 + t*t*t * p3;
		}

		/// <summary>
		/// Draws a cubic bezier curve.
		///
		/// [Open online documentation to see images]
		///
		/// [Open online documentation to see images]
		///
		/// TODO: Currently uses a fixed resolution of 20 segments. Resolution should depend on the distance to the camera.
		///
		/// See: https://en.wikipedia.org/wiki/Bezier_curve
		/// </summary>
		/// <param name="p0">Start point</param>
		/// <param name="p1">First control point</param>
		/// <param name="p2">Second control point</param>
		/// <param name="p3">End point</param>
		public void Bezier (float3 p0, float3 p1, float3 p2, float3 p3) {
			float3 prev = p0;

			for (int i = 1; i <= 20; i++) {
				float t = i/20.0f;
				float3 p = EvaluateCubicBezier(p0, p1, p2, p3, t);
				Line(prev, p);
				prev = p;
			}
		}

		/// <summary>
		/// Draws a smooth curve through a list of points.
		///
		/// A catmull-rom spline is equivalent to a bezier curve with control points determined by an algorithm.
		/// In fact, this package displays catmull-rom splines by first converting them to bezier curves.
		///
		/// [Open online documentation to see images]
		///
		/// See: https://en.wikipedia.org/wiki/Centripetal_Catmull%E2%80%93Rom_spline
		/// See: <see cref="CatmullRom(float3,float3,float3,float3)"/>
		/// </summary>
		/// <param name="points">The curve will smoothly pass through each point in the list in order.</param>
		public void CatmullRom (List<Vector3> points) {
			if (points.Count < 2) return;

			if (points.Count == 2) {
				Line(points[0], points[1]);
			} else {
				// count >= 3
				var count = points.Count;
				// Draw first curve, this is special because the first two control points are the same
				CatmullRom(points[0], points[0], points[1], points[2]);
				for (int i = 0; i + 3 < count; i++) {
					CatmullRom(points[i], points[i+1], points[i+2], points[i+3]);
				}
				// Draw last curve
				CatmullRom(points[count-3], points[count-2], points[count-1], points[count-1]);
			}
		}

		/// <summary>
		/// Draws a centripetal catmull rom spline.
		///
		/// The curve starts at p1 and ends at p2.
		///
		/// [Open online documentation to see images]
		/// [Open online documentation to see images]
		///
		/// See: <see cref="CatmullRom(List<Vector3>)"/>
		/// </summary>
		/// <param name="p0">First control point</param>
		/// <param name="p1">Second control point. Start of the curve.</param>
		/// <param name="p2">Third control point. End of the curve.</param>
		/// <param name="p3">Fourth control point.</param>
		public void CatmullRom (float3 p0, float3 p1, float3 p2, float3 p3) {
			// References used:
			// p.266 GemsV1
			//
			// tension is often set to 0.5 but you can use any reasonable value:
			// http://www.cs.cmu.edu/~462/projects/assn2/assn2/catmullRom.pdf
			//
			// bias and tension controls:
			// http://local.wasp.uwa.edu.au/~pbourke/miscellaneous/interpolation/

			// We will convert the catmull rom spline to a bezier curve for simplicity.
			// The end result of this will be a conversion matrix where we transform catmull rom control points
			// into the equivalent bezier curve control points.

			// Conversion matrix
			// =================

			// A centripetal catmull rom spline can be separated into the following terms:
			// 1 * p1 +
			// t * (-0.5 * p0 + 0.5*p2) +
			// t*t * (p0 - 2.5*p1  + 2.0*p2 + 0.5*t2) +
			// t*t*t * (-0.5*p0 + 1.5*p1 - 1.5*p2 + 0.5*p3)
			//
			// Matrix form:
			// 1     t   t^2 t^3
			// {0, -1/2, 1, -1/2}
			// {1, 0, -5/2, 3/2}
			// {0, 1/2, 2, -3/2}
			// {0, 0, -1/2, 1/2}

			// Transposed matrix:
			// M_1 = {{0, 1, 0, 0}, {-1/2, 0, 1/2, 0}, {1, -5/2, 2, -1/2}, {-1/2, 3/2, -3/2, 1/2}}

			// A bezier spline can be separated into the following terms:
			// (-t^3 + 3 t^2 - 3 t + 1) * c0 +
			// (3t^3 - 6*t^2 + 3t) * c1 +
			// (3t^2 - 3t^3) * c2 +
			// t^3 * c3
			//
			// Matrix form:
			// 1  t  t^2  t^3
			// {1, -3, 3, -1}
			// {0, 3, -6, 3}
			// {0, 0, 3, -3}
			// {0, 0, 0, 1}

			// Transposed matrix:
			// M_2 = {{1, 0, 0, 0}, {-3, 3, 0, 0}, {3, -6, 3, 0}, {-1, 3, -3, 1}}

			// Thus a bezier curve can be evaluated using the expression
			// output1 = T * M_1 * c
			// where T = [1, t, t^2, t^3] and c being the control points c = [c0, c1, c2, c3]^T
			//
			// and a catmull rom spline can be evaluated using
			//
			// output2 = T * M_2 * p
			// where T = same as before and p = [p0, p1, p2, p3]^T
			//
			// We can solve for c in output1 = output2
			// T * M_1 * c = T * M_2 * p
			// M_1 * c = M_2 * p
			// c = M_1^(-1) * M_2 * p
			// Thus a conversion matrix from p to c is M_1^(-1) * M_2
			// This can be calculated and the result is the following matrix:
			//
			// {0, 1, 0, 0}
			// {-1/6, 1, 1/6, 0}
			// {0, 1/6, 1, -1/6}
			// {0, 0, 1, 0}
			// ------------------------------------------------------------------
			//
			// Using this we can calculate c = M_1^(-1) * M_2 * p
			var c0 = p1;
			var c1 = (-p0 + 6*p1 + 1*p2)*(1/6.0f);
			var c2 = (p1 + 6*p2 - p3)*(1/6.0f);
			var c3 = p2;

			// And finally draw the bezier curve which is equivalent to the desired catmull-rom spline
			Bezier(c0, c1, c2, c3);
		}

		/// <summary>
		/// Draws an arrow between two points.
		///
		/// The size of the head defaults to 20% of the length of the arrow.
		///
		/// [Open online documentation to see images]
		///
		/// See: <see cref="ArrowheadArc"/>
		/// See: <see cref="Arrow(float3,float3,float3,float)"/>
		/// See: <see cref="ArrowRelativeSizeHead"/>
		/// </summary>
		/// <param name="from">Base of the arrow.</param>
		/// <param name="to">Head of the arrow.</param>
		public void Arrow (float3 from, float3 to) {
			ArrowRelativeSizeHead(from, to, DEFAULT_UP, 0.2f);
		}

		/// <summary>
		/// Draws an arrow between two points.
		///
		/// [Open online documentation to see images]
		///
		/// See: <see cref="ArrowRelativeSizeHead"/>
		/// See: <see cref="ArrowheadArc"/>
		/// </summary>
		/// <param name="from">Base of the arrow.</param>
		/// <param name="to">Head of the arrow.</param>
		/// <param name="up">Up direction of the world, the arrowhead plane will be as perpendicular as possible to this direction. Defaults to Vector3.up.</param>
		/// <param name="headSize">The size of the arrowhead in world units.</param>
		public void Arrow (float3 from, float3 to, float3 up, float headSize) {
			var length_sq = math.lengthsq(to - from);

			if (length_sq > 0.000001f) {
				ArrowRelativeSizeHead(from, to, up, headSize * math.rsqrt(length_sq));
			}
		}

		/// <summary>
		/// Draws an arrow between two points with a head that varies with the length of the arrow.
		///
		/// [Open online documentation to see images]
		///
		/// See: <see cref="ArrowheadArc"/>
		/// See: <see cref="Arrow"/>
		/// </summary>
		/// <param name="from">Base of the arrow.</param>
		/// <param name="to">Head of the arrow.</param>
		/// <param name="up">Up direction of the world, the arrowhead plane will be as perpendicular as possible to this direction.</param>
		/// <param name="headFraction">The length of the arrowhead is the distance between from and to multiplied by this fraction. Should be between 0 and 1.</param>
		public void ArrowRelativeSizeHead (float3 from, float3 to, float3 up, float headFraction) {
			Line(from, to);
			var dir = to - from;

			var normal = math.cross(dir, up);
			// Pick a different up direction if the direction happened to be colinear with that one.
			if (math.all(normal == 0)) normal = math.cross(new float3(1, 0, 0), dir);
			// Pick a different up direction if up=(1,0,0) and thus the above check would have generated a zero vector again
			if (math.all(normal == 0)) normal = math.cross(new float3(0, 1, 0), dir);
			normal = math.normalizesafe(normal) * math.length(dir);

			Line(to, to - (dir + normal) * headFraction);
			Line(to, to - (dir - normal) * headFraction);
		}

		/// <summary>
		/// Draws an arrowhead at a point.
		///
		/// <code>
		/// Draw.Arrowhead(Vector3.zero, Vector3.forward, 0.75f, color);
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: <see cref="Arrow"/>
		/// See: <see cref="ArrowRelativeSizeHead"/>
		/// </summary>
		/// <param name="center">Center of the arrowhead.</param>
		/// <param name="direction">Direction the arrow is pointing.</param>
		/// <param name="radius">Distance from the center to each corner of the arrowhead.</param>
		public void Arrowhead (float3 center, float3 direction, float radius) {
			Arrowhead(center, direction, DEFAULT_UP, radius);
		}

		/// <summary>
		/// Draws an arrowhead at a point.
		///
		/// <code>
		/// Draw.Arrowhead(Vector3.zero, Vector3.forward, 0.75f, color);
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: <see cref="Arrow"/>
		/// See: <see cref="ArrowRelativeSizeHead"/>
		/// </summary>
		/// <param name="center">Center of the arrowhead.</param>
		/// <param name="direction">Direction the arrow is pointing.</param>
		/// <param name="up">Up direction of the world, the arrowhead plane will be as perpendicular as possible to this direction. Defaults to Vector3.up. Must be normalized.</param>
		/// <param name="radius">Distance from the center to each corner of the arrowhead.</param>
		public void Arrowhead (float3 center, float3 direction, float3 up, float radius) {
			if (math.all(direction == 0)) return;
			direction = math.normalizesafe(direction);
			var normal = math.cross(direction, up);
			const float SinPiOver3 = 0.866025f;
			const float CosPiOver3 = 0.5f;
			var circleCenter = center - radius * (1 - CosPiOver3)*0.5f * direction;
			var p1 = circleCenter + radius * direction;
			var p2 = circleCenter - radius * CosPiOver3 * direction + radius * SinPiOver3 * normal;
			var p3 = circleCenter - radius * CosPiOver3 * direction - radius * SinPiOver3 * normal;
			Line(p1, p2);
			Line(p2, circleCenter);
			Line(circleCenter, p3);
			Line(p3, p1);
		}

		/// <summary>
		/// Draws an arrowhead centered around a circle.
		///
		/// This can be used to for example show the direction a character is moving in.
		///
		/// [Open online documentation to see images]
		///
		/// Note: In the image above the arrowhead is the only part that is drawn by this method. The cylinder is only included for context.
		///
		/// See: <see cref="Arrow"/>
		/// </summary>
		/// <param name="origin">Point around which the arc is centered</param>
		/// <param name="direction">Direction the arrow is pointing</param>
		/// <param name="offset">Distance from origin that the arrow starts.</param>
		/// <param name="width">Width of the arrowhead in degrees (defaults to 60). Should be between 0 and 90.</param>
		public void ArrowheadArc (float3 origin, float3 direction, float offset, float width = 60) {
			if (!math.any(direction)) return;
			if (offset < 0) throw new System.ArgumentOutOfRangeException(nameof(offset));
			if (offset == 0) return;

			var rot = Quaternion.LookRotation(direction, DEFAULT_UP);
			PushMatrix(Matrix4x4.TRS(origin, rot, Vector3.one));
			var a1 = math.PI * 0.5f - width * (0.5f * Mathf.Deg2Rad);
			var a2 = math.PI * 0.5f + width * (0.5f * Mathf.Deg2Rad);
			CircleXZInternal(float3.zero, offset, a1, a2);
			var p1 = new float3(math.cos(a1), 0, math.sin(a1)) * offset;
			var p2 = new float3(math.cos(a2), 0, math.sin(a2)) * offset;
			const float sqrt2 = 1.4142f;
			var p3 = new float3(0, 0, sqrt2 * offset);
			Line(p1, p3);
			Line(p3, p2);
			PopMatrix();
		}

		/// <summary>
		/// Draws a grid of lines.
		///
		/// <code>
		/// Draw.xz.WireGrid(Vector3.zero, new int2(3, 3), new float2(1, 1), color);
		/// </code>
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="center">Center of the grid</param>
		/// <param name="rotation">Rotation of the grid. The grid will be aligned to the X and Z axes of the rotation.</param>
		/// <param name="cells">Number of cells of the grid. Should be greater than 0.</param>
		/// <param name="totalSize">Total size of the grid along the X and Z axes.</param>
		public void WireGrid (float3 center, quaternion rotation, int2 cells, float2 totalSize) {
			cells = math.max(cells, new int2(1, 1));
			PushMatrix(float4x4.TRS(center, rotation, new Vector3(totalSize.x, 0, totalSize.y)));
			int w = cells.x;
			int h = cells.y;
			for (int i = 0; i <= w; i++) Line(new float3(i/(float)w - 0.5f, 0, -0.5f), new float3(i/(float)w - 0.5f, 0, 0.5f));
			for (int i = 0; i <= h; i++) Line(new float3(-0.5f, 0, i/(float)h - 0.5f), new float3(0.5f, 0, i/(float)h - 0.5f));
			PopMatrix();
		}

		/// <summary>
		/// Draws a triangle outline.
		///
		/// <code>
		/// Draw.WireTriangle(new Vector3(-0.5f, 0, 0), new Vector3(0, 1, 0), new Vector3(0.5f, 0, 0), Color.black);
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: <see cref="Draw.WirePlane(float3,quaternion,float2)"/>
		/// See: <see cref="WirePolygon"/>
		/// See: <see cref="SolidTriangle"/>
		/// </summary>
		/// <param name="a">First corner of the triangle</param>
		/// <param name="b">Second corner of the triangle</param>
		/// <param name="c">Third corner of the triangle</param>
		public void WireTriangle (float3 a, float3 b, float3 c) {
			Line(a, b);
			Line(b, c);
			Line(c, a);
		}

		/// <summary>
		/// Draws a rectangle outline.
		/// The rectangle will be aligned to the X and Z axes.
		///
		/// <code>
		/// Draw.xz.WireRectangle(new Vector3(0f, 0, 0), new Vector2(1, 1), Color.black);
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: <see cref="WirePolygon"/>
		/// </summary>
		[System.Obsolete("Use Draw.xz.WireRectangle instead")]
		public void WireRectangleXZ (float3 center, float2 size) {
			WireRectangle(center, quaternion.identity, size);
		}

		/// <summary>
		/// Draws a rectangle outline.
		/// The rectangle will be oriented along the rotation's X and Z axes.
		///
		/// <code>
		/// Draw.WireRectangle(new Vector3(0f, 0, 0), Quaternion.identity, new Vector2(1, 1), Color.black);
		/// </code>
		/// [Open online documentation to see images]
		///
		/// This is identical to <see cref="Draw.WirePlane(float3,quaternion,float2)"/>, but this name is added for consistency.
		///
		/// See: <see cref="WirePolygon"/>
		/// </summary>
		public void WireRectangle (float3 center, quaternion rotation, float2 size) {
			WirePlane(center, rotation, size);
		}

		/// <summary>
		/// Draws a rectangle outline.
		/// The rectangle corners are assumed to be in XY space.
		/// This is particularly useful when combined with <see cref="InScreenSpace"/>.
		///
		/// <code>
		/// using (Draw.InScreenSpace(Camera.main)) {
		///     Draw.xy.WireRectangle(new Rect(10, 10, 100, 100), Color.black);
		/// }
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: <see cref="WireRectangleXZ"/>
		/// See: <see cref="WireRectangle(float3,quaternion,float2)"/>
		/// See: <see cref="WirePolygon"/>
		/// </summary>
		[System.Obsolete("Use Draw.xy.WireRectangle instead")]
		public void WireRectangle (Rect rect) {
			xy.WireRectangle(rect);
		}


		/// <summary>
		/// Draws a triangle outline.
		///
		/// <code>
		/// Draw.WireTriangle(Vector3.zero, Quaternion.identity, 0.5f, color);
		/// </code>
		/// [Open online documentation to see images]
		///
		/// Note: This is a convenience wrapper for <see cref="WirePolygon(float3,int,quaternion,float)"/>
		///
		/// See: <see cref="WireTriangle(float3,float3,float3)"/>
		/// </summary>
		/// <param name="center">Center of the triangle.</param>
		/// <param name="rotation">Rotation of the triangle. The first vertex will be radius units in front of center as seen from the rotation's point of view.</param>
		/// <param name="radius">Distance from the center to each vertex.</param>
		public void WireTriangle (float3 center, quaternion rotation, float radius) {
			WirePolygon(center, 3, rotation, radius);
		}

		/// <summary>
		/// Draws a pentagon outline.
		///
		/// <code>
		/// Draw.WirePentagon(Vector3.zero, Quaternion.identity, 0.5f, color);
		/// </code>
		/// [Open online documentation to see images]
		///
		/// Note: This is a convenience wrapper for <see cref="WirePolygon(float3,int,quaternion,float)"/>
		/// </summary>
		/// <param name="center">Center of the polygon.</param>
		/// <param name="rotation">Rotation of the polygon. The first vertex will be radius units in front of center as seen from the rotation's point of view.</param>
		/// <param name="radius">Distance from the center to each vertex.</param>
		public void WirePentagon (float3 center, quaternion rotation, float radius) {
			WirePolygon(center, 5, rotation, radius);
		}

		/// <summary>
		/// Draws a hexagon outline.
		///
		/// <code>
		/// Draw.WireHexagon(Vector3.zero, Quaternion.identity, 0.5f, color);
		/// </code>
		/// [Open online documentation to see images]
		///
		/// Note: This is a convenience wrapper for <see cref="WirePolygon(float3,int,quaternion,float)"/>
		/// </summary>
		/// <param name="center">Center of the polygon.</param>
		/// <param name="rotation">Rotation of the polygon. The first vertex will be radius units in front of center as seen from the rotation's point of view.</param>
		/// <param name="radius">Distance from the center to each vertex.</param>
		public void WireHexagon (float3 center, quaternion rotation, float radius) {
			WirePolygon(center, 6, rotation, radius);
		}

		/// <summary>
		/// Draws a regular polygon outline.
		///
		/// <code>
		/// Draw.WirePolygon(new Vector3(-0.5f, 0, +0.5f), 3, Quaternion.identity, 0.4f, color);
		/// Draw.WirePolygon(new Vector3(+0.5f, 0, +0.5f), 4, Quaternion.identity, 0.4f, color);
		/// Draw.WirePolygon(new Vector3(-0.5f, 0, -0.5f), 5, Quaternion.identity, 0.4f, color);
		/// Draw.WirePolygon(new Vector3(+0.5f, 0, -0.5f), 6, Quaternion.identity, 0.4f, color);
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: <see cref="WireTriangle"/>
		/// See: <see cref="WirePentagon"/>
		/// See: <see cref="WireHexagon"/>
		/// </summary>
		/// <param name="center">Center of the polygon.</param>
		/// <param name="vertices">Number of corners (and sides) of the polygon.</param>
		/// <param name="rotation">Rotation of the polygon. The first vertex will be radius units in front of center as seen from the rotation's point of view.</param>
		/// <param name="radius">Distance from the center to each vertex.</param>
		public void WirePolygon (float3 center, int vertices, quaternion rotation, float radius) {
			PushMatrix(float4x4.TRS(center, rotation, new float3(radius, radius, radius)));
			float3 prev = new float3(0, 0, 1);
			for (int i = 1; i <= vertices; i++) {
				float a = 2 * math.PI * (i / (float)vertices);
				var p = new float3(math.sin(a), 0, math.cos(a));
				Line(prev, p);
				prev = p;
			}
			PopMatrix();
		}

		/// <summary>
		/// Draws a solid rectangle.
		/// The rectangle corners are assumed to be in XY space.
		/// This is particularly useful when combined with <see cref="InScreenSpace"/>.
		///
		/// Behind the scenes this is implemented using <see cref="SolidPlane"/>.
		///
		/// <code>
		/// using (Draw.InScreenSpace(Camera.main)) {
		///     Draw.xy.SolidRectangle(new Rect(10, 10, 100, 100), Color.black);
		/// }
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: <see cref="WireRectangleXZ"/>
		/// See: <see cref="WireRectangle(float3,quaternion,float2)"/>
		/// See: <see cref="SolidBox"/>
		/// </summary>
		[System.Obsolete("Use Draw.xy.SolidRectangle instead")]
		public void SolidRectangle (Rect rect) {
			xy.SolidRectangle(rect);
		}

		/// <summary>
		/// Draws a solid plane.
		///
		/// <code>
		/// Draw.SolidPlane(new float3(0, 0, 0), new float3(0, 1, 0), 1.0f, color);
		/// </code>
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="center">Center of the visualized plane.</param>
		/// <param name="normal">Direction perpendicular to the plane. If this is (0,0,0) then nothing will be rendered.</param>
		/// <param name="size">Width and height of the visualized plane.</param>
		public void SolidPlane (float3 center, float3 normal, float2 size) {
			if (math.any(normal)) {
				SolidPlane(center, Quaternion.LookRotation(calculateTangent(normal), normal), size);
			}
		}

		/// <summary>
		/// Draws a solid plane.
		///
		/// The plane will lie in the XZ plane with respect to the rotation.
		///
		/// <code>
		/// Draw.SolidPlane(new float3(0, 0, 0), new float3(0, 1, 0), 1.0f, color);
		/// </code>
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="center">Center of the visualized plane.</param>
		/// <param name="size">Width and height of the visualized plane.</param>
		public void SolidPlane (float3 center, quaternion rotation, float2 size) {
			PushMatrix(float4x4.TRS(center, rotation, new float3(size.x, 0, size.y)));
			Reserve<BoxData>();
			Add(Command.Box);
			Add(new BoxData { center = 0, size = 1 });
			PopMatrix();
		}

		/// <summary>Returns an arbitrary vector which is orthogonal to the given one</summary>
		private static float3 calculateTangent (float3 normal) {
			var tangent = math.cross(new float3(0, 1, 0), normal);

			if (math.all(tangent == 0)) tangent = math.cross(new float3(1, 0, 0), normal);
			return tangent;
		}

		/// <summary>
		/// Draws a wire plane.
		///
		/// <code>
		/// Draw.WirePlane(new float3(0, 0, 0), new float3(0, 1, 0), 1.0f, color);
		/// </code>
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="center">Center of the visualized plane.</param>
		/// <param name="normal">Direction perpendicular to the plane. If this is (0,0,0) then nothing will be rendered.</param>
		/// <param name="size">Width and height of the visualized plane.</param>
		public void WirePlane (float3 center, float3 normal, float2 size) {
			if (math.any(normal)) {
				WirePlane(center, Quaternion.LookRotation(calculateTangent(normal), normal), size);
			}
		}

		/// <summary>
		/// Draws a wire plane.
		///
		/// This is identical to <see cref="WireRectangle(float3,quaternion,float2)"/>, but it is included for consistency.
		///
		/// <code>
		/// Draw.WirePlane(new float3(0, 0, 0), new float3(0, 1, 0), 1.0f, color);
		/// </code>
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="center">Center of the visualized plane.</param>
		/// <param name="rotation">Rotation of the plane. The plane will lie in the XZ plane with respect to the rotation.</param>
		/// <param name="size">Width and height of the visualized plane.</param>
		public void WirePlane (float3 center, quaternion rotation, float2 size) {
			Reserve<PlaneData>();
			Add(Command.WirePlane);
			Add(new PlaneData { center = center, rotation = rotation, size = size });
		}

		/// <summary>
		/// Draws a plane and a visualization of its normal.
		///
		/// <code>
		/// Draw.PlaneWithNormal(new float3(0, 0, 0), new float3(0, 1, 0), 1.0f, color);
		/// </code>
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="center">Center of the visualized plane.</param>
		/// <param name="normal">Direction perpendicular to the plane. If this is (0,0,0) then nothing will be rendered.</param>
		/// <param name="size">Width and height of the visualized plane.</param>
		public void PlaneWithNormal (float3 center, float3 normal, float2 size) {
			if (math.any(normal)) {
				PlaneWithNormal(center, Quaternion.LookRotation(calculateTangent(normal), normal), size);
			}
		}

		/// <summary>
		/// Draws a plane and a visualization of its normal.
		///
		/// <code>
		/// Draw.PlaneWithNormal(new float3(0, 0, 0), new float3(0, 1, 0), 1.0f, color);
		/// </code>
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="center">Center of the visualized plane.</param>
		/// <param name="rotation">Rotation of the plane. The plane will lie in the XZ plane with respect to the rotation.</param>
		/// <param name="size">Width and height of the visualized plane.</param>
		public void PlaneWithNormal (float3 center, quaternion rotation, float2 size) {
			SolidPlane(center, rotation, size);
			WirePlane(center, rotation, size);
			ArrowRelativeSizeHead(center, center + math.mul(rotation, new float3(0, 1, 0)) * 0.5f, math.mul(rotation, new float3(0, 0, 1)), 0.2f);
		}

		/// <summary>
		/// Draws a solid triangle.
		///
		/// <code>
		/// Draw.xy.SolidTriangle(new float2(-0.43f, -0.25f), new float2(0, 0.5f), new float2(0.43f, -0.25f), color);
		/// </code>
		/// [Open online documentation to see images]
		///
		/// Note: If you are going to be drawing lots of triangles it's better to use <see cref="Draw.SolidMesh"/> instead as it will be more efficient.
		///
		/// See: <see cref="Draw.SolidMesh"/>
		/// See: <see cref="Draw.WireTriangle"/>
		/// </summary>
		/// <param name="a">First corner of the triangle.</param>
		/// <param name="b">Second corner of the triangle.</param>
		/// <param name="c">Third corner of the triangle.</param>
		public void SolidTriangle (float3 a, float3 b, float3 c) {
			Reserve<TriangleData>();
			Add(Command.SolidTriangle);
			Add(new TriangleData { a = a, b = b, c = c });
		}

		/// <summary>
		/// Draws a solid box.
		///
		/// <code>
		/// Draw.SolidBox(new float3(0, 0, 0), new float3(1, 1, 1), color);
		/// </code>
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="center">Center of the box</param>
		/// <param name="size">Width of the box along all dimensions</param>
		public void SolidBox (float3 center, float3 size) {
			Reserve<BoxData>();
			Add(Command.Box);
			Add(new BoxData { center = center, size = size });
		}

		/// <summary>
		/// Draws a solid box.
		///
		/// <code>
		/// Draw.SolidBox(new float3(0, 0, 0), new float3(1, 1, 1), color);
		/// </code>
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="bounds">Bounding box of the box</param>
		public void SolidBox (Bounds bounds) {
			SolidBox(bounds.center, bounds.size);
		}

		/// <summary>
		/// Draws a solid box.
		///
		/// <code>
		/// Draw.SolidBox(new float3(0, 0, 0), new float3(1, 1, 1), color);
		/// </code>
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="center">Center of the box</param>
		/// <param name="rotation">Rotation of the box</param>
		/// <param name="size">Width of the box along all dimensions</param>
		public void SolidBox (float3 center, quaternion rotation, float3 size) {
			PushMatrix(float4x4.TRS(center, rotation, size));
			SolidBox(float3.zero, Vector3.one);
			PopMatrix();
		}

		/// <summary>
		/// Draws a label in 3D space.
		///
		/// The default alignment is <see cref="Drawing.LabelAlignment.MiddleLeft"/>.
		///
		/// <code>
		/// Draw.Label3D(new float3(0.2f, -1f, 0.2f), Quaternion.Euler(45, -110, -90), "Label", 1, LabelAlignment.Center, color);
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: Label3D(float3,quaternion,string,float,LabelAlignment)
		///
		/// Note: Only ASCII is supported since the built-in font texture only includes ASCII. Other characters will be rendered as question marks (?).
		/// </summary>
		/// <param name="position">Position in 3D space.</param>
		/// <param name="rotation">Rotation in 3D space.</param>
		/// <param name="text">Text to display.</param>
		/// <param name="size">World size of the text. For large sizes an SDF (signed distance field) font is used and for small sizes a normal font texture is used.</param>
		public void Label3D (float3 position, quaternion rotation, string text, float size) {
			Label3D(position, rotation, text, size, LabelAlignment.MiddleLeft);
		}

		/// <summary>
		/// Draws a label in 3D space.
		///
		/// <code>
		/// Draw.Label3D(new float3(0.2f, -1f, 0.2f), Quaternion.Euler(45, -110, -90), "Label", 1, LabelAlignment.Center, color);
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: Label3D(float3,quaternion,string,float)
		///
		/// Note: Only ASCII is supported since the built-in font texture only includes ASCII. Other characters will be rendered as question marks (?).
		///
		/// Note: This method cannot be used in burst since managed strings are not suppported in burst. However, you can use the separate Label3D overload which takes a FixedString.
		/// </summary>
		/// <param name="position">Position in 3D space.</param>
		/// <param name="rotation">Rotation in 3D space.</param>
		/// <param name="text">Text to display.</param>
		/// <param name="size">World size of the text. For large sizes an SDF (signed distance field) font is used and for small sizes a normal font texture is used.</param>
		/// <param name="alignment">How to align the text relative to the given position.</param>
		public void Label3D (float3 position, quaternion rotation, string text, float size, LabelAlignment alignment) {
			AssertBufferExists();
			Reserve<TextData3D>();
			Add(Command.Text3D);
			Add(new TextData3D { center = position, rotation = rotation, numCharacters = text.Length, size = size, alignment = alignment });
			AddText(text);
		}

		/// <summary>
		/// Draws a label in 3D space aligned with the camera.
		///
		/// The default alignment is <see cref="Drawing.LabelAlignment.MiddleLeft"/>.
		///
		/// <code>
		/// Draw.Label2D(Vector3.zero, "Label", 48, LabelAlignment.Center, color);
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: Label2D(float3,string,float,LabelAlignment)
		///
		/// Note: Only ASCII is supported since the built-in font texture only includes ASCII. Other characters will be rendered as question marks (?).
		/// </summary>
		/// <param name="position">Position in 3D space.</param>
		/// <param name="text">Text to display.</param>
		/// <param name="sizeInPixels">Size of the text in screen pixels. For large sizes an SDF (signed distance field) font is used and for small sizes a normal font texture is used.</param>
		public void Label2D (float3 position, string text, float sizeInPixels = 14) {
			Label2D(position, text, sizeInPixels, LabelAlignment.MiddleLeft);
		}

		/// <summary>
		/// Draws a label in 3D space aligned with the camera.
		///
		/// <code>
		/// Draw.Label2D(Vector3.zero, "Label", 48, LabelAlignment.Center, color);
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: Label2D(float3,string,float)
		///
		/// Note: Only ASCII is supported since the built-in font texture only includes ASCII. Other characters will be rendered as question marks (?).
		///
		/// Note: This method cannot be used in burst since managed strings are not suppported in burst. However, you can use the separate Label2D overload which takes a FixedString.
		/// </summary>
		/// <param name="position">Position in 3D space.</param>
		/// <param name="text">Text to display.</param>
		/// <param name="sizeInPixels">Size of the text in screen pixels. For large sizes an SDF (signed distance field) font is used and for small sizes a normal font texture is used.</param>
		/// <param name="alignment">How to align the text relative to the given position.</param>
		public void Label2D (float3 position, string text, float sizeInPixels, LabelAlignment alignment) {
			AssertBufferExists();
			Reserve<TextData>();
			Add(Command.Text);
			Add(new TextData { center = position, numCharacters = text.Length, sizeInPixels = sizeInPixels, alignment = alignment });
			AddText(text);
		}

		void AddText (string text) {
			var g = gizmos.Target as DrawingData;
			Reserve(UnsafeUtility.SizeOf<System.UInt16>() * text.Length);
			for (int i = 0; i < text.Length; i++) {
				char c = text[i];
				System.UInt16 index = (System.UInt16)g.fontData.GetIndex(c);
				Add(index);
			}
		}

		#region Label2DFixedString
		/// <summary>
		/// Draws a label in 3D space aligned with the camera.
		///
		/// <code>
		/// // This part can be inside a burst job
		/// for (int i = 0; i < 10; i++) {
		///     Unity.Collections.FixedString32Bytes text = $"X = {i}";
		///     builder.Label2D(new float3(i, 0, 0), ref text, 12, LabelAlignment.Center);
		/// }
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: Label2D(float3,string,float)
		///
		/// Note: Only ASCII is supported since the built-in font texture only includes ASCII. Other characters will be rendered as question marks (?).
		///
		/// Note: This method requires the Unity.Collections package version 0.8 or later.
		/// </summary>
		/// <param name="position">Position in 3D space.</param>
		/// <param name="text">Text to display.</param>
		/// <param name="sizeInPixels">Size of the text in screen pixels. For large sizes an SDF (signed distance field) font is used and for small sizes a normal font texture is used.</param>
		public void Label2D (float3 position, ref FixedString32Bytes text, float sizeInPixels = 14) {
			Label2D(position, ref text, sizeInPixels, LabelAlignment.MiddleLeft);
		}

		/// <summary>\copydocref{Label2D(float3,FixedString32Bytes,float)}</summary>
		public void Label2D (float3 position, ref FixedString64Bytes text, float sizeInPixels = 14) {
			Label2D(position, ref text, sizeInPixels, LabelAlignment.MiddleLeft);
		}

		/// <summary>\copydocref{Label2D(float3,FixedString32Bytes,float)}</summary>
		public void Label2D (float3 position, ref FixedString128Bytes text, float sizeInPixels = 14) {
			Label2D(position, ref text, sizeInPixels, LabelAlignment.MiddleLeft);
		}

		/// <summary>\copydocref{Label2D(float3,FixedString32Bytes,float)}</summary>
		public void Label2D (float3 position, ref FixedString512Bytes text, float sizeInPixels = 14) {
			Label2D(position, ref text, sizeInPixels, LabelAlignment.MiddleLeft);
		}

		/// <summary>
		/// Draws a label in 3D space aligned with the camera.
		///
		/// <code>
		/// // This part can be inside a burst job
		/// for (int i = 0; i < 10; i++) {
		///     Unity.Collections.FixedString32Bytes text = $"X = {i}";
		///     builder.Label2D(new float3(i, 0, 0), ref text, 12, LabelAlignment.Center);
		/// }
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: Label2D(float3,string,float)
		///
		/// Note: Only ASCII is supported since the built-in font texture only includes ASCII. Other characters will be rendered as question marks (?).
		///
		/// Note: This method requires the Unity.Collections package version 0.8 or later.
		/// </summary>
		/// <param name="position">Position in 3D space.</param>
		/// <param name="text">Text to display.</param>
		/// <param name="sizeInPixels">Size of the text in screen pixels. For large sizes an SDF (signed distance field) font is used and for small sizes a normal font texture is used.</param>
		/// <param name="alignment">How to align the text relative to the given position.</param>
		public void Label2D (float3 position, ref FixedString32Bytes text, float sizeInPixels, LabelAlignment alignment) {
#if MODULE_COLLECTIONS_0_12_0_OR_NEWER
unsafe {
Label2D(position, text.GetUnsafePtr(), text.Length, sizeInPixels, alignment);
}
#else
Debug.LogError("The Label2D method which takes FixedStrings requires the Unity.Collections package version 0.12 or newer");
#endif
}

		/// <summary>\copydocref{Label2D(float3,FixedString32Bytes,float,LabelAlignment)}</summary>
		public void Label2D (float3 position, ref FixedString64Bytes text, float sizeInPixels, LabelAlignment alignment) {
#if MODULE_COLLECTIONS_0_12_0_OR_NEWER
unsafe {
Label2D(position, text.GetUnsafePtr(), text.Length, sizeInPixels, alignment);
}
#else
Debug.LogError("The Label2D method which takes FixedStrings requires the Unity.Collections package version 0.12 or newer");
#endif
}

		/// <summary>\copydocref{Label2D(float3,FixedString32Bytes,float,LabelAlignment)}</summary>
		public void Label2D (float3 position, ref FixedString128Bytes text, float sizeInPixels, LabelAlignment alignment) {
#if MODULE_COLLECTIONS_0_12_0_OR_NEWER
unsafe {
Label2D(position, text.GetUnsafePtr(), text.Length, sizeInPixels, alignment);
}
#else
Debug.LogError("The Label2D method which takes FixedStrings requires the Unity.Collections package version 0.12 or newer");
#endif
}

		/// <summary>\copydocref{Label2D(float3,FixedString32Bytes,float,LabelAlignment)}</summary>
		public void Label2D (float3 position, ref FixedString512Bytes text, float sizeInPixels, LabelAlignment alignment) {
#if MODULE_COLLECTIONS_0_12_0_OR_NEWER
unsafe {
Label2D(position, text.GetUnsafePtr(), text.Length, sizeInPixels, alignment);
}
#else
Debug.LogError("The Label2D method which takes FixedStrings requires the Unity.Collections package version 0.12 or newer");
#endif
}

		/// <summary>\copydocref{Label2D(float3,FixedString32Bytes,float,LabelAlignment)}</summary>
		internal unsafe void Label2D (float3 position, byte* text, int byteCount, float sizeInPixels, LabelAlignment alignment) {
#if MODULE_COLLECTIONS_0_12_0_OR_NEWER
AssertBufferExists();
Reserve<TextData>();
Add(Command.Text);
Add(new TextData { center = position, numCharacters = byteCount, sizeInPixels = sizeInPixels, alignment = alignment });

			Reserve(UnsafeUtility.SizeOf<System.UInt16>() * byteCount);
			for (int i = 0; i < byteCount; i++) {
				// The first 128 elements in the font data are guaranteed to be laid out as ascii.
				// We use this since we cannot use the dynamic font lookup.
				System.UInt16 c = *(text + i);
				if (c >= 128) c = (System.UInt16) '?';
				if (c == (byte)'\n') c = SDFLookupData.Newline;
				// Ignore carriage return instead of printing them as '?'. Windows encodes newlines as \r\n.
				if (c == (byte)'\r') continue;
				Add(c);
			}
#endif
}
#endregion

		#region Label3DFixedString
		/// <summary>
		/// Draws a label in 3D space.
		///
		/// <code>
		/// // This part can be inside a burst job
		/// for (int i = 0; i < 10; i++) {
		///     Unity.Collections.FixedString32Bytes text = $"X = {i}";
		///     builder.Label3D(new float3(i, 0, 0), quaternion.identity, ref text, 1, LabelAlignment.Center);
		/// }
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: Label3D(float3,quaternion,string,float)
		///
		/// Note: Only ASCII is supported since the built-in font texture only includes ASCII. Other characters will be rendered as question marks (?).
		///
		/// Note: This method requires the Unity.Collections package version 0.8 or later.
		/// </summary>
		/// <param name="position">Position in 3D space.</param>
		/// <param name="rotation">Rotation in 3D space.</param>
		/// <param name="text">Text to display.</param>
		/// <param name="size">World size of the text. For large sizes an SDF (signed distance field) font is used and for small sizes a normal font texture is used.</param>
		public void Label3D (float3 position, quaternion rotation, ref FixedString32Bytes text, float size) {
			Label3D(position, rotation, ref text, size, LabelAlignment.MiddleLeft);
		}

		/// <summary>\copydocref{Label3D(float3,quaternion,FixedString32Bytes,float)}</summary>
		public void Label3D (float3 position, quaternion rotation, ref FixedString64Bytes text, float size) {
			Label3D(position, rotation, ref text, size, LabelAlignment.MiddleLeft);
		}

		/// <summary>\copydocref{Label3D(float3,quaternion,FixedString32Bytes,float)}</summary>
		public void Label3D (float3 position, quaternion rotation, ref FixedString128Bytes text, float size) {
			Label3D(position, rotation, ref text, size, LabelAlignment.MiddleLeft);
		}

		/// <summary>\copydocref{Label3D(float3,quaternion,FixedString32Bytes,float)}</summary>
		public void Label3D (float3 position, quaternion rotation, ref FixedString512Bytes text, float size) {
			Label3D(position, rotation, ref text, size, LabelAlignment.MiddleLeft);
		}

		/// <summary>
		/// Draws a label in 3D space.
		///
		/// <code>
		/// // This part can be inside a burst job
		/// for (int i = 0; i < 10; i++) {
		///     Unity.Collections.FixedString32Bytes text = $"X = {i}";
		///     builder.Label3D(new float3(i, 0, 0), quaternion.identity, ref text, 1, LabelAlignment.Center);
		/// }
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: Label3D(float3,quaternion,string,float)
		///
		/// Note: Only ASCII is supported since the built-in font texture only includes ASCII. Other characters will be rendered as question marks (?).
		///
		/// Note: This method requires the Unity.Collections package version 0.8 or later.
		/// </summary>
		/// <param name="position">Position in 3D space.</param>
		/// <param name="rotation">Rotation in 3D space.</param>
		/// <param name="text">Text to display.</param>
		/// <param name="size">World size of the text. For large sizes an SDF (signed distance field) font is used and for small sizes a normal font texture is used.</param>
		/// <param name="alignment">How to align the text relative to the given position.</param>
		public void Label3D (float3 position, quaternion rotation, ref FixedString32Bytes text, float size, LabelAlignment alignment) {
#if MODULE_COLLECTIONS_0_12_0_OR_NEWER
unsafe {
Label3D(position, rotation, text.GetUnsafePtr(), text.Length, size, alignment);
}
#else
Debug.LogError("The Label3D method which takes FixedStrings requires the Unity.Collections package version 0.12 or newer");
#endif
}

		/// <summary>\copydocref{Label3D(float3,quaternion,FixedString32Bytes,float,LabelAlignment)}</summary>
		public void Label3D (float3 position, quaternion rotation, ref FixedString64Bytes text, float size, LabelAlignment alignment) {
#if MODULE_COLLECTIONS_0_12_0_OR_NEWER
unsafe {
Label3D(position, rotation, text.GetUnsafePtr(), text.Length, size, alignment);
}
#else
Debug.LogError("The Label3D method which takes FixedStrings requires the Unity.Collections package version 0.12 or newer");
#endif
}

		/// <summary>\copydocref{Label3D(float3,quaternion,FixedString32Bytes,float,LabelAlignment)}</summary>
		public void Label3D (float3 position, quaternion rotation, ref FixedString128Bytes text, float size, LabelAlignment alignment) {
#if MODULE_COLLECTIONS_0_12_0_OR_NEWER
unsafe {
Label3D(position, rotation, text.GetUnsafePtr(), text.Length, size, alignment);
}
#else
Debug.LogError("The Label3D method which takes FixedStrings requires the Unity.Collections package version 0.12 or newer");
#endif
}

		/// <summary>\copydocref{Label3D(float3,quaternion,FixedString32Bytes,float,LabelAlignment)}</summary>
		public void Label3D (float3 position, quaternion rotation, ref FixedString512Bytes text, float size, LabelAlignment alignment) {
#if MODULE_COLLECTIONS_0_12_0_OR_NEWER
unsafe {
Label3D(position, rotation, text.GetUnsafePtr(), text.Length, size, alignment);
}
#else
Debug.LogError("The Label3D method which takes FixedStrings requires the Unity.Collections package version 0.12 or newer");
#endif
}

		/// <summary>\copydocref{Label3D(float3,quaternion,FixedString32Bytes,float,LabelAlignment)}</summary>
		internal unsafe void Label3D (float3 position, quaternion rotation, byte* text, int byteCount, float size, LabelAlignment alignment) {
#if MODULE_COLLECTIONS_0_12_0_OR_NEWER
AssertBufferExists();
Reserve<TextData3D>();
Add(Command.Text3D);
Add(new TextData3D { center = position, rotation = rotation, numCharacters = byteCount, size = size, alignment = alignment });

			Reserve(UnsafeUtility.SizeOf<System.UInt16>() * byteCount);
			for (int i = 0; i < byteCount; i++) {
				// The first 128 elements in the font data are guaranteed to be laid out as ascii.
				// We use this since we cannot use the dynamic font lookup.
				System.UInt16 c = *(text + i);
				if (c >= 128) c = (System.UInt16) '?';
				if (c == (byte)'\n') c = SDFLookupData.Newline;
				Add(c);
			}
#endif
}
#endregion
}
}
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;
using Unity.Burst;
using UnityEngine.Profiling;
using Unity.Collections;
using Unity.Jobs;

namespace Drawing {
using static CommandBuilder;

	[BurstCompile]
	internal struct StreamSplitter : IJob {
		public NativeArray<UnsafeAppendBuffer> inputBuffers;
		[NativeDisableUnsafePtrRestriction]
		public unsafe UnsafeAppendBuffer* staticBuffer, dynamicBuffer, persistentBuffer;

		internal static readonly int PushCommands = (1 << (int)Command.PushColor) | (1 << (int)Command.PushMatrix) | (1 << (int)Command.PushSetMatrix) | (1 << (int)Command.PushPersist) | (1 << (int)Command.PushLineWidth);
		internal static readonly int PopCommands = (1 << (int)Command.PopColor) | (1 << (int)Command.PopMatrix) | (1 << (int)Command.PopPersist) | (1 << (int)Command.PopLineWidth);
		internal static readonly int MetaCommands = PushCommands | PopCommands;
		internal static readonly int DynamicCommands = (1 << (int)Command.SphereOutline) | (1 << (int)Command.CircleXZ) | (1 << (int)Command.Circle) | (1 << (int)Command.DiscXZ) | (1 << (int)Command.Disc) | (1 << (int)Command.Text) | (1 << (int)Command.Text3D) | (1 << (int)Command.CaptureState) | MetaCommands;
		internal static readonly int StaticCommands = (1 << (int)Command.Line) | (1 << (int)Command.Box) | (1 << (int)Command.WirePlane) | (1 << (int)Command.WireBox) | (1 << (int)Command.SolidTriangle) | MetaCommands;

		internal static readonly int[] CommandSizes;
		static StreamSplitter() {
			// Size of all commands in bytes
			CommandSizes = new int[22];
			CommandSizes[(int)Command.PushColor] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<Color32>();
			CommandSizes[(int)Command.PopColor] = UnsafeUtility.SizeOf<Command>() + 0;
			CommandSizes[(int)Command.PushMatrix] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<float4x4>();
			CommandSizes[(int)Command.PushSetMatrix] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<float4x4>();
			CommandSizes[(int)Command.PopMatrix] = UnsafeUtility.SizeOf<Command>() + 0;
			CommandSizes[(int)Command.Line] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<LineData>();
			CommandSizes[(int)Command.CircleXZ] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<CircleXZData>();
			CommandSizes[(int)Command.SphereOutline] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<SphereData>();
			CommandSizes[(int)Command.Circle] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<CircleData>();
			CommandSizes[(int)Command.Disc] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<CircleData>();
			CommandSizes[(int)Command.DiscXZ] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<CircleXZData>();
			CommandSizes[(int)Command.Box] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<BoxData>();
			CommandSizes[(int)Command.WirePlane] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<PlaneData>();
			CommandSizes[(int)Command.WireBox] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<BoxData>();
			CommandSizes[(int)Command.SolidTriangle] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<TriangleData>();
			CommandSizes[(int)Command.PushPersist] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<PersistData>();
			CommandSizes[(int)Command.PopPersist] = UnsafeUtility.SizeOf<Command>();
			CommandSizes[(int)Command.Text] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<TextData>(); // Dynamically sized
			CommandSizes[(int)Command.Text3D] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<TextData3D>(); // Dynamically sized
			CommandSizes[(int)Command.PushLineWidth] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<LineWidthData>();
			CommandSizes[(int)Command.PopLineWidth] = UnsafeUtility.SizeOf<Command>();
			CommandSizes[(int)Command.CaptureState] = UnsafeUtility.SizeOf<Command>();
		}

		public void Execute () {
			var lastWriteStatic = -1;
			var lastWriteDynamic = -1;
			var lastWritePersist = -1;
			var stackStatic = new NativeArray<int>(GeometryBuilderJob.MaxStackSize, Allocator.Temp, NativeArrayOptions.ClearMemory);
			var stackDynamic = new NativeArray<int>(GeometryBuilderJob.MaxStackSize, Allocator.Temp, NativeArrayOptions.ClearMemory);
			var stackPersist = new NativeArray<int>(GeometryBuilderJob.MaxStackSize, Allocator.Temp, NativeArrayOptions.ClearMemory);

			unsafe {
				// Store in local variables for performance (makes it possible to use registers for a lot of fields)
				var bufferStatic = *staticBuffer;
				var bufferDynamic = *dynamicBuffer;
				var bufferPersist = *persistentBuffer;

				bufferStatic.Reset();
				bufferDynamic.Reset();
				bufferPersist.Reset();

				for (int i = 0; i < inputBuffers.Length; i++) {
					int stackSize = 0;
					int persist = 0;
					var reader = inputBuffers[i].AsReader();

					// Guarantee we have enough space for copying the whole buffer
					if (bufferStatic.Capacity < bufferStatic.Length + reader.Size) bufferStatic.SetCapacity(math.ceilpow2(bufferStatic.Length + reader.Size));
					if (bufferDynamic.Capacity < bufferDynamic.Length + reader.Size) bufferDynamic.SetCapacity(math.ceilpow2(bufferDynamic.Length + reader.Size));
					if (bufferPersist.Capacity < bufferPersist.Length + reader.Size) bufferPersist.SetCapacity(math.ceilpow2(bufferPersist.Length + reader.Size));

					// To ensure that even if exceptions are thrown the output buffers still point to valid memory regions
					*staticBuffer = bufferStatic;
					*dynamicBuffer = bufferDynamic;
					*persistentBuffer = bufferPersist;

					while (reader.Offset < reader.Size) {
						var cmd = *(Command*)((byte*)reader.Ptr + reader.Offset);
						var cmdBit = 1 << ((int)cmd & 0xFF);
						int size = CommandSizes[(int)cmd & 0xFF] + ((cmd & Command.PushColorInline) != 0 ? UnsafeUtility.SizeOf<Color32>() : 0);
						bool isMeta = (cmdBit & MetaCommands) != 0;

						if ((cmd & (Command)0xFF) == Command.Text) {
							// Very pretty way of reading the TextData struct right after the command label and optional Color32
							var data = *((TextData*)((byte*)reader.Ptr + reader.Offset + size) - 1);
							// Add the size of the embedded string in the buffer
							// TODO: Unaligned memory access performance penalties?? Update: Doesn't seem to be so bad on Intel at least.
							size += data.numCharacters * UnsafeUtility.SizeOf<System.UInt16>();
						} else if ((cmd & (Command)0xFF) == Command.Text3D) {
							// Very pretty way of reading the TextData struct right after the command label and optional Color32
							var data = *((TextData3D*)((byte*)reader.Ptr + reader.Offset + size) - 1);
							// Add the size of the embedded string in the buffer
							// TODO: Unaligned memory access performance penalties?? Update: Doesn't seem to be so bad on Intel at least.
							size += data.numCharacters * UnsafeUtility.SizeOf<System.UInt16>();
						}

#if ENABLE_UNITY_COLLECTIONS_CHECKS
UnityEngine.Assertions.Assert.IsTrue(reader.Offset + size <= reader.Size);
#endif

						if ((cmdBit & DynamicCommands) != 0 && persist == 0) {
							if (!isMeta) lastWriteDynamic = bufferDynamic.Length;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
UnityEngine.Assertions.Assert.IsTrue(bufferDynamic.Length + size <= bufferDynamic.Capacity);
#endif
UnsafeUtility.MemCpy((byte*)bufferDynamic.Ptr + bufferDynamic.Length, (byte*)reader.Ptr + reader.Offset, size);
bufferDynamic.Length = bufferDynamic.Length + size;
}

						if ((cmdBit & StaticCommands) != 0 && persist == 0) {
							if (!isMeta) lastWriteStatic = bufferStatic.Length;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
UnityEngine.Assertions.Assert.IsTrue(bufferStatic.Length + size <= bufferStatic.Capacity);
#endif
UnsafeUtility.MemCpy((byte*)bufferStatic.Ptr + bufferStatic.Length, (byte*)reader.Ptr + reader.Offset, size);
bufferStatic.Length = bufferStatic.Length + size;
}

						if ((cmdBit & MetaCommands) != 0 || persist > 0) {
							if (persist > 0 && !isMeta) lastWritePersist = bufferPersist.Length;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
UnityEngine.Assertions.Assert.IsTrue(bufferPersist.Length + size <= bufferPersist.Capacity);
#endif
UnsafeUtility.MemCpy((byte*)bufferPersist.Ptr + bufferPersist.Length, (byte*)reader.Ptr + reader.Offset, size);
bufferPersist.Length = bufferPersist.Length + size;
}

						if ((cmdBit & PushCommands) != 0) {
							stackStatic[stackSize] = bufferStatic.Length - size;
							stackDynamic[stackSize] = bufferDynamic.Length - size;
							stackPersist[stackSize] = bufferPersist.Length - size;
							stackSize++;
							if ((cmd & (Command)0xFF) == Command.PushPersist) {
								persist++;
							}
#if ENABLE_UNITY_COLLECTIONS_CHECKS
if (stackSize >= GeometryBuilderJob.MaxStackSize) throw new System.Exception("Push commands are too deeply nested. This can happen if you have deeply nested WithMatrix or WithColor scopes.");
#else
if (stackSize >= GeometryBuilderJob.MaxStackSize) {
return;
}
#endif
} else if ((cmdBit & PopCommands) != 0) {
stackSize--;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
if (stackSize < 0) throw new System.Exception("Trying to issue a pop command but there is no corresponding push command");
#else
if (stackSize < 0) return;
#endif
// If a scope was pushed and later popped, but no actual draw commands were written to the buffers
// inside that scope then we erase the whole scope.
if (lastWriteStatic < stackStatic[stackSize]) {
bufferStatic.Length = stackStatic[stackSize];
}
if (lastWriteDynamic < stackDynamic[stackSize]) {
bufferDynamic.Length = stackDynamic[stackSize];
}
if (lastWritePersist < stackPersist[stackSize]) {
bufferPersist.Length = stackPersist[stackSize];
}
if ((cmd & (Command)0xFF) == Command.PopPersist) {
persist--;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
if (persist < 0) throw new System.Exception("Too many PopPersist commands. Are your PushPersist/PopPersist calls matched?");
#else
if (persist < 0) return;
#endif
}
}

						reader.Offset += size;
					}

#if ENABLE_UNITY_COLLECTIONS_CHECKS
if (stackSize != 0) throw new System.Exception("Too few pop commands and too many push commands. Are your push and pop commands properly matched?");
if (reader.Offset != reader.Size) throw new System.Exception("Did not end up at the end of the buffer. This is a bug.");
#else
if (stackSize != 0) return;
if (reader.Offset != reader.Size) return;
#endif
}

#if ENABLE_UNITY_COLLECTIONS_CHECKS
if (bufferStatic.Length > bufferStatic.Capacity) throw new System.Exception("Buffer overrun. This is a bug");
if (bufferDynamic.Length > bufferDynamic.Capacity) throw new System.Exception("Buffer overrun. This is a bug");
if (bufferPersist.Length > bufferPersist.Capacity) throw new System.Exception("Buffer overrun. This is a bug");
#endif

				*staticBuffer = bufferStatic;
				*dynamicBuffer = bufferDynamic;
				*persistentBuffer = bufferPersist;
			}
		}
	}
}
#if MODULE_RENDER_PIPELINES_UNIVERSAL
using UnityEngine;
using UnityEngine.Rendering;
#if MODULE_RENDER_PIPELINES_UNIVERSAL_17_0_0_OR_NEWER
using UnityEngine.Rendering.RenderGraphModule;
#endif
using UnityEngine.Rendering.Universal;

namespace Drawing {
/// <summary>Custom Universal Render Pipeline Render Pass for ALINE</summary>
public class AlineURPRenderPassFeature : ScriptableRendererFeature {
/// <summary>Custom Universal Render Pipeline Render Pass for ALINE</summary>
public class AlineURPRenderPass : ScriptableRenderPass {
/// <summary>This method is called before executing the render pass</summary>
#if MODULE_RENDER_PIPELINES_UNIVERSAL_17_0_0_OR_NEWER
[System.Obsolete]
#endif
public override void Configure (CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) {
}

#if MODULE_RENDER_PIPELINES_UNIVERSAL_17_0_0_OR_NEWER
[System.Obsolete]
#endif
public override void Execute (ScriptableRenderContext context, ref RenderingData renderingData) {
DrawingManager.instance.ExecuteCustomRenderPass(context, renderingData.cameraData.camera);
}

			public AlineURPRenderPass() : base() {
				profilingSampler = new ProfilingSampler("ALINE");
			}

#if MODULE_RENDER_PIPELINES_UNIVERSAL_17_0_0_OR_NEWER
private class PassData {
public Camera camera;
public bool allowDisablingWireframe;
}

			public override void RecordRenderGraph (RenderGraph renderGraph, ContextContainer frameData) {
				var cameraData = frameData.Get<UniversalCameraData>();
				var resourceData = frameData.Get<UniversalResourceData>();

				// This could happen if the camera does not have a color target or depth target set.
				// In that case we are probably rendering some kind of special effect. Skip ALINE rendering in that case.
				if (!resourceData.activeColorTexture.IsValid() || !resourceData.activeDepthTexture.IsValid()) {
					return;
				}

				using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass<PassData>("ALINE", out PassData passData, profilingSampler)) {
					passData.allowDisablingWireframe = false;

					if (Application.isEditor && (cameraData.cameraType & (CameraType.SceneView | CameraType.Preview)) != 0) {
						// We need this to be able to disable wireframe rendering in the scene view
						builder.AllowGlobalStateModification(true);
						passData.allowDisablingWireframe = true;
					}

					builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
					builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture);
					passData.camera = cameraData.camera;

					builder.SetRenderFunc<PassData>(
						(PassData data, RasterGraphContext context) => {
						DrawingManager.instance.ExecuteCustomRenderGraphPass(new DrawingData.CommandBufferWrapper { cmd2 = context.cmd, allowDisablingWireframe = data.allowDisablingWireframe }, data.camera);
					}
						);
				}
			}
#endif

			public override void FrameCleanup (CommandBuffer cmd) {
			}
		}

		AlineURPRenderPass m_ScriptablePass;

		public override void Create () {
			m_ScriptablePass = new AlineURPRenderPass();

			// Configures where the render pass should be injected.
			// URP's post processing actually happens in BeforeRenderingPostProcessing, not after BeforeRenderingPostProcessing as one would expect.
			// Use BeforeRenderingPostProcessing-1 to ensure this pass gets executed before post processing effects.
			m_ScriptablePass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing-1;
		}

		/// <summary>This method is called when setting up the renderer once per-camera</summary>
		public override void AddRenderPasses (ScriptableRenderer renderer, ref RenderingData renderingData) {
			AddRenderPasses(renderer);
		}

		public void AddRenderPasses (ScriptableRenderer renderer) {
			renderer.EnqueuePass(m_ScriptablePass);
		}
	}
}
#endif
// This file is automatically generated by a script based on the CommandBuilder API.
// This file adds additional overloads to the CommandBuilder API with convenience parameters like colors.
using Unity.Burst;
using UnityEngine;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Drawing {
public partial struct CommandBuilder {
/// <summary>\copydocref{Line(float3,float3)}</summary>
public void Line (float3 a, float3 b, Color color) {
Reserve<Color32, LineData>();
Add(Command.Line | Command.PushColorInline);
Add(ConvertColor(color));
Add(new LineData { a = a, b = b });
}
/// <summary>\copydocref{Ray(float3,float3)}</summary>
public void Ray (float3 origin, float3 direction, Color color) {
Line(origin, origin + direction, color);
}
/// <summary>\copydocref{Ray(Ray,float)}</summary>
public void Ray (Ray ray, float length, Color color) {
Line(ray.origin, ray.origin + ray.direction * length, color);
}
/// <summary>\copydocref{Arc(float3,float3,float3)}</summary>
/// <param name="color">Color of the object</param>
public void Arc (float3 center, float3 start, float3 end, Color color) {
PushColor(color);
var d1 = start - center;
var d2 = end - center;
var normal = math.cross(d2, d1);

			if (math.any(normal != 0) && math.all(math.isfinite(normal))) {
				var m = Matrix4x4.TRS(center, Quaternion.LookRotation(d1, normal), Vector3.one);
				var angle = Vector3.SignedAngle(d1, d2, normal) * Mathf.Deg2Rad;
				PushMatrix(m);
				CircleXZInternal(float3.zero, math.length(d1), 90 * Mathf.Deg2Rad, 90 * Mathf.Deg2Rad - angle);
				PopMatrix();
			}
			PopColor();
		}
		/// <summary>\copydocref{CircleXZ(float3,float,float,float)}</summary>
		/// <param name="color">Color of the object</param>
		[System.Obsolete("Use Draw.xz.Circle instead")]
		public void CircleXZ (float3 center, float radius, float startAngle, float endAngle, Color color) {
			CircleXZInternal(center, radius, startAngle, endAngle, color);
		}
		/// <summary>\copydocref{CircleXZ(float3,float,float,float)}</summary>
		/// <param name="color">Color of the object</param>
		[System.Obsolete("Use Draw.xz.Circle instead")]
		public void CircleXZ (float3 center, float radius, Color color) {
			CircleXZ(center, radius, 0f, 2 * Mathf.PI, color);
		}
		/// <summary>\copydocref{CircleXY(float3,float,float,float)}</summary>
		/// <param name="color">Color of the object</param>
		[System.Obsolete("Use Draw.xy.Circle instead")]
		public void CircleXY (float3 center, float radius, float startAngle, float endAngle, Color color) {
			PushColor(color);
			PushMatrix(XZtoXYPlaneMatrix);
			CircleXZ(new float3(center.x, -center.z, center.y), radius, startAngle, endAngle);
			PopMatrix();
			PopColor();
		}

		/// <summary>\copydocref{CircleXY(float3,float,float,float)}</summary>
		/// <param name="color">Color of the object</param>
		[System.Obsolete("Use Draw.xy.Circle instead")]
		public void CircleXY (float3 center, float radius, Color color) {
			CircleXY(center, radius, 0f, 2 * Mathf.PI, color);
		}

		/// <summary>\copydocref{Circle(float3,float3,float)}</summary>
		public void Circle (float3 center, float3 normal, float radius, Color color) {
			Reserve<Color32, CircleData>();
			Add(Command.Circle | Command.PushColorInline);
			Add(ConvertColor(color));
			Add(new CircleData { center = center, normal = normal, radius = radius });
		}
		/// <summary>\copydocref{SolidArc(float3,float3,float3)}</summary>
		/// <param name="color">Color of the object</param>
		public void SolidArc (float3 center, float3 start, float3 end, Color color) {
			PushColor(color);
			var d1 = start - center;
			var d2 = end - center;
			var normal = math.cross(d2, d1);

			if (math.any(normal)) {
				var m = Matrix4x4.TRS(center, Quaternion.LookRotation(d1, normal), Vector3.one);
				var angle = Vector3.SignedAngle(d1, d2, normal) * Mathf.Deg2Rad;
				PushMatrix(m);
				SolidCircleXZInternal(float3.zero, math.length(d1), 90 * Mathf.Deg2Rad, 90 * Mathf.Deg2Rad - angle);
				PopMatrix();
			}
			PopColor();
		}

		/// <summary>\copydocref{SolidCircleXZ(float3,float,float,float)}</summary>
		/// <param name="color">Color of the object</param>
		[System.Obsolete("Use Draw.xz.SolidCircle instead")]
		public void SolidCircleXZ (float3 center, float radius, float startAngle, float endAngle, Color color) {
			SolidCircleXZInternal(center, radius, startAngle, endAngle, color);
		}

		/// <summary>\copydocref{SolidCircleXZ(float3,float,float,float)}</summary>
		/// <param name="color">Color of the object</param>
		[System.Obsolete("Use Draw.xz.SolidCircle instead")]
		public void SolidCircleXZ (float3 center, float radius, Color color) {
			SolidCircleXZ(center, radius, 0f, 2 * Mathf.PI, color);
		}

		/// <summary>\copydocref{SolidCircleXY(float3,float,float,float)}</summary>
		/// <param name="color">Color of the object</param>
		[System.Obsolete("Use Draw.xy.SolidCircle instead")]
		public void SolidCircleXY (float3 center, float radius, float startAngle, float endAngle, Color color) {
			PushColor(color);
			PushMatrix(XZtoXYPlaneMatrix);
			SolidCircleXZInternal(new float3(center.x, -center.z, center.y), radius, startAngle, endAngle);
			PopMatrix();
			PopColor();
		}

		/// <summary>\copydocref{SolidCircleXY(float3,float,float,float)}</summary>
		/// <param name="color">Color of the object</param>
		[System.Obsolete("Use Draw.xy.SolidCircle instead")]
		public void SolidCircleXY (float3 center, float radius, Color color) {
			SolidCircleXY(center, radius, 0f, 2 * Mathf.PI, color);
		}

		/// <summary>\copydocref{SolidCircle(float3,float3,float)}</summary>
		public void SolidCircle (float3 center, float3 normal, float radius, Color color) {
			Reserve<Color32, CircleData>();
			Add(Command.Disc | Command.PushColorInline);
			Add(ConvertColor(color));
			Add(new CircleData { center = center, normal = normal, radius = radius });
		}

		/// <summary>\copydocref{SphereOutline(float3,float)}</summary>
		public void SphereOutline (float3 center, float radius, Color color) {
			Reserve<Color32, SphereData>();
			Add(Command.SphereOutline | Command.PushColorInline);
			Add(ConvertColor(color));
			Add(new SphereData { center = center, radius = radius });
		}

		/// <summary>\copydocref{WireCylinder(float3,float3,float)}</summary>
		public void WireCylinder (float3 bottom, float3 top, float radius, Color color) {
			WireCylinder(bottom, top - bottom, math.length(top - bottom), radius, color);
		}
		/// <summary>\copydocref{WireCylinder(float3,float3,float,float)}</summary>
		/// <param name="color">Color of the object</param>
		public void WireCylinder (float3 position, float3 up, float height, float radius, Color color) {
			up = math.normalizesafe(up);
			if (math.all(up == 0) || math.any(math.isnan(up)) || math.isnan(height) || math.isnan(radius)) return;
			PushColor(color);

			OrthonormalBasis(up, out var basis1, out var basis2);

			PushMatrix(new float4x4(
				new float4(basis1 * radius, 0),
				new float4(up * height, 0),
				new float4(basis2 * radius, 0),
				new float4(position, 1)
				));

			CircleXZInternal(float3.zero, 1);
			if (height > 0) {
				CircleXZInternal(new float3(0, 1, 0), 1);
				Line(new float3(1, 0, 0), new float3(1, 1, 0));
				Line(new float3(-1, 0, 0), new float3(-1, 1, 0));
				Line(new float3(0, 0, 1), new float3(0, 1, 1));
				Line(new float3(0, 0, -1), new float3(0, 1, -1));
			}
			PopMatrix();
			PopColor();
		}
		/// <summary>\copydocref{WireCapsule(float3,float3,float)}</summary>
		/// <param name="color">Color of the object</param>
		public void WireCapsule (float3 start, float3 end, float radius, Color color) {
			PushColor(color);
			var dir = end - start;
			var length = math.length(dir);

			if (length < 0.0001) {
				// The endpoints are the same, we can't draw a capsule from this because we don't know its orientation.
				// Draw a sphere as a fallback
				WireSphere(start, radius);
			} else {
				var normalized_dir = dir / length;

				WireCapsule(start - normalized_dir*radius, normalized_dir, length + 2*radius, radius);
			}
			PopColor();
		}
		/// <summary>\copydocref{WireCapsule(float3,float3,float,float)}</summary>
		/// <param name="color">Color of the object</param>
		public void WireCapsule (float3 position, float3 direction, float length, float radius, Color color) {
			direction = math.normalizesafe(direction);
			if (math.all(direction == 0) || math.any(math.isnan(direction)) || math.isnan(length) || math.isnan(radius)) return;
			PushColor(color);

			if (radius <= 0) {
				Line(position, position + direction * length);
			} else {
				length = math.max(length, radius*2);
				OrthonormalBasis(direction, out var basis1, out var basis2);

				PushMatrix(new float4x4(
					new float4(basis1, 0),
					new float4(direction, 0),
					new float4(basis2, 0),
					new float4(position, 1)
					));
				CircleXZInternal(new float3(0, radius, 0), radius);
				PushMatrix(XZtoXYPlaneMatrix);
				CircleXZInternal(new float3(0, 0, radius), radius, Mathf.PI, 2 * Mathf.PI);
				PopMatrix();
				PushMatrix(XZtoYZPlaneMatrix);
				CircleXZInternal(new float3(radius, 0, 0), radius, Mathf.PI*0.5f, Mathf.PI*1.5f);
				PopMatrix();
				if (length > 0) {
					var upperY = length - radius;
					var lowerY = radius;
					CircleXZInternal(new float3(0, upperY, 0), radius);
					PushMatrix(XZtoXYPlaneMatrix);
					CircleXZInternal(new float3(0, 0, upperY), radius, 0, Mathf.PI);
					PopMatrix();
					PushMatrix(XZtoYZPlaneMatrix);
					CircleXZInternal(new float3(upperY, 0, 0), radius, -Mathf.PI*0.5f, Mathf.PI*0.5f);
					PopMatrix();
					Line(new float3(radius, lowerY, 0), new float3(radius, upperY, 0));
					Line(new float3(-radius, lowerY, 0), new float3(-radius, upperY, 0));
					Line(new float3(0, lowerY, radius), new float3(0, upperY, radius));
					Line(new float3(0, lowerY, -radius), new float3(0, upperY, -radius));
				}
				PopMatrix();
			}
			PopColor();
		}
		/// <summary>\copydocref{WireSphere(float3,float)}</summary>
		public void WireSphere (float3 position, float radius, Color color) {
			PushColor(color);
			SphereOutline(position, radius);
			Circle(position, new float3(1, 0, 0), radius);
			Circle(position, new float3(0, 1, 0), radius);
			Circle(position, new float3(0, 0, 1), radius);
			PopColor();
		}
		/// <summary>\copydocref{Polyline(List<Vector3>,bool)}</summary>
		/// <param name="color">Color of the object</param>
		[BurstDiscard]
		public void Polyline (List<Vector3> points, bool cycle, Color color) {
			PushColor(color);
			for (int i = 0; i < points.Count - 1; i++) {
				Line(points[i], points[i+1]);
			}
			if (cycle && points.Count > 1) Line(points[points.Count - 1], points[0]);
			PopColor();
		}
		/// <summary>\copydocref{Polyline(List<Vector3>,bool)}</summary>
		/// <param name="color">Color of the object</param>
		[BurstDiscard]
		public void Polyline (List<Vector3> points, Color color) {
			Polyline(points, false, color);
		}
		/// <summary>\copydocref{Polyline(Vector3[],bool)}</summary>
		/// <param name="color">Color of the object</param>
		[BurstDiscard]
		public void Polyline (Vector3[] points, bool cycle, Color color) {
			PushColor(color);
			for (int i = 0; i < points.Length - 1; i++) {
				Line(points[i], points[i+1]);
			}
			if (cycle && points.Length > 1) Line(points[points.Length - 1], points[0]);
			PopColor();
		}
		/// <summary>\copydocref{Polyline(Vector3[],bool)}</summary>
		/// <param name="color">Color of the object</param>
		[BurstDiscard]
		public void Polyline (Vector3[] points, Color color) {
			Polyline(points, false, color);
		}
		/// <summary>\copydocref{Polyline(float3[],bool)}</summary>
		/// <param name="color">Color of the object</param>
		[BurstDiscard]
		public void Polyline (float3[] points, bool cycle, Color color) {
			PushColor(color);
			for (int i = 0; i < points.Length - 1; i++) {
				Line(points[i], points[i+1]);
			}
			if (cycle && points.Length > 1) Line(points[points.Length - 1], points[0]);
			PopColor();
		}
		/// <summary>\copydocref{Polyline(float3[],bool)}</summary>
		/// <param name="color">Color of the object</param>
		[BurstDiscard]
		public void Polyline (float3[] points, Color color) {
			Polyline(points, false, color);
		}
		/// <summary>\copydocref{Polyline(NativeArray<float3>,bool)}</summary>
		/// <param name="color">Color of the object</param>
		public void Polyline (NativeArray<float3> points, bool cycle, Color color) {
			PushColor(color);
			for (int i = 0; i < points.Length - 1; i++) {
				Line(points[i], points[i+1]);
			}
			if (cycle && points.Length > 1) Line(points[points.Length - 1], points[0]);
			PopColor();
		}
		/// <summary>\copydocref{Polyline(NativeArray<float3>,bool)}</summary>
		/// <param name="color">Color of the object</param>
		public void Polyline (NativeArray<float3> points, Color color) {
			Polyline(points, false, color);
		}
		/// <summary>\copydocref{DashedLine(float3,float3,float,float)}</summary>
		public void DashedLine (float3 a, float3 b, float dash, float gap, Color color) {
			PushColor(color);
			var p = new PolylineWithSymbol(SymbolDecoration.None, gap, 0, dash + gap);
			p.MoveTo(ref this, a);
			p.MoveTo(ref this, b);
			PopColor();
		}

		/// <summary>\copydocref{DashedPolyline(List<Vector3>,float,float)}</summary>
		public void DashedPolyline (List<Vector3> points, float dash, float gap, Color color) {
			PushColor(color);
			var p = new PolylineWithSymbol(SymbolDecoration.None, gap, 0, dash + gap);
			for (int i = 0; i < points.Count; i++) {
				p.MoveTo(ref this, points[i]);
			}
			PopColor();
		}

		/// <summary>\copydocref{WireBox(float3,float3)}</summary>
		/// <param name="color">Color of the object</param>
		public void WireBox (float3 center, float3 size, Color color) {
			Reserve<Color32, BoxData>();
			Add(Command.WireBox | Command.PushColorInline);
			Add(ConvertColor(color));
			Add(new BoxData { center = center, size = size });
		}
		/// <summary>\copydocref{WireBox(float3,quaternion,float3)}</summary>
		/// <param name="color">Color of the object</param>
		public void WireBox (float3 center, quaternion rotation, float3 size, Color color) {
			PushColor(color);
			PushMatrix(float4x4.TRS(center, rotation, size));
			WireBox(float3.zero, new float3(1, 1, 1));
			PopMatrix();
			PopColor();
		}
		/// <summary>\copydocref{WireBox(Bounds)}</summary>
		public void WireBox (Bounds bounds, Color color) {
			WireBox(bounds.center, bounds.size, color);
		}
		/// <summary>\copydocref{WireMesh(Mesh)}</summary>
		public void WireMesh (Mesh mesh, Color color) {
#if UNITY_2020_1_OR_NEWER
if (mesh == null) throw new System.ArgumentNullException();
PushColor(color);

			// Use a burst compiled function to draw the lines
			// This is significantly faster than pure C# (about 5x).
			var meshDataArray = Mesh.AcquireReadOnlyMeshData(mesh);
			var meshData = meshDataArray[0];

			JobWireMesh.JobWireMeshFunctionPointer(ref meshData, ref this);
			meshDataArray.Dispose();
#else
Debug.LogError("The WireMesh method is only suppored in Unity 2020.1 or later");
#endif
PopColor();
}
/// <summary>\copydocref{WireMesh(NativeArray<float3>,NativeArray<int>)}</summary>
public void WireMesh (NativeArray<float3> vertices, NativeArray<int> triangles, Color color) {
PushColor(color);
#if UNITY_2020_1_OR_NEWER
unsafe {
JobWireMesh.WireMesh((float3*)vertices.GetUnsafeReadOnlyPtr(), (int*)triangles.GetUnsafeReadOnlyPtr(), vertices.Length, triangles.Length, ref this);
}
#else
Debug.LogError("The WireMesh method is only suppored in Unity 2020.1 or later");
#endif
PopColor();
}
/// <summary>\copydocref{SolidMesh(Mesh)}</summary>
public void SolidMesh (Mesh mesh, Color color) {
SolidMeshInternal(mesh, false, color);
}

		/// <summary>\copydocref{Cross(float3,float)}</summary>
		public void Cross (float3 position, float size, Color color) {
			PushColor(color);
			size *= 0.5f;
			Line(position - new float3(size, 0, 0), position + new float3(size, 0, 0));
			Line(position - new float3(0, size, 0), position + new float3(0, size, 0));
			Line(position - new float3(0, 0, size), position + new float3(0, 0, size));
			PopColor();
		}
		/// <summary>\copydocref{Cross(float3,float)}</summary>
		public void Cross (float3 position, Color color) {
			Cross(position, 1, color);
		}
		/// <summary>\copydocref{CrossXZ(float3,float)}</summary>
		[System.Obsolete("Use Draw.xz.Cross instead")]
		public void CrossXZ (float3 position, float size, Color color) {
			PushColor(color);
			size *= 0.5f;
			Line(position - new float3(size, 0, 0), position + new float3(size, 0, 0));
			Line(position - new float3(0, 0, size), position + new float3(0, 0, size));
			PopColor();
		}
		/// <summary>\copydocref{CrossXZ(float3,float)}</summary>
		[System.Obsolete("Use Draw.xz.Cross instead")]
		public void CrossXZ (float3 position, Color color) {
			CrossXZ(position, 1, color);
		}
		/// <summary>\copydocref{CrossXY(float3,float)}</summary>
		[System.Obsolete("Use Draw.xy.Cross instead")]
		public void CrossXY (float3 position, float size, Color color) {
			PushColor(color);
			size *= 0.5f;
			Line(position - new float3(size, 0, 0), position + new float3(size, 0, 0));
			Line(position - new float3(0, size, 0), position + new float3(0, size, 0));
			PopColor();
		}
		/// <summary>\copydocref{CrossXY(float3,float)}</summary>
		[System.Obsolete("Use Draw.xy.Cross instead")]
		public void CrossXY (float3 position, Color color) {
			CrossXY(position, 1, color);
		}
		/// <summary>\copydocref{Bezier(float3,float3,float3,float3)}</summary>
		/// <param name="color">Color of the object</param>
		public void Bezier (float3 p0, float3 p1, float3 p2, float3 p3, Color color) {
			PushColor(color);
			float3 prev = p0;

			for (int i = 1; i <= 20; i++) {
				float t = i/20.0f;
				float3 p = EvaluateCubicBezier(p0, p1, p2, p3, t);
				Line(prev, p);
				prev = p;
			}
			PopColor();
		}
		/// <summary>\copydocref{CatmullRom(List<Vector3>)}</summary>
		/// <param name="color">Color of the object</param>
		public void CatmullRom (List<Vector3> points, Color color) {
			if (points.Count < 2) return;
			PushColor(color);

			if (points.Count == 2) {
				Line(points[0], points[1]);
			} else {
				// count >= 3
				var count = points.Count;
				// Draw first curve, this is special because the first two control points are the same
				CatmullRom(points[0], points[0], points[1], points[2]);
				for (int i = 0; i + 3 < count; i++) {
					CatmullRom(points[i], points[i+1], points[i+2], points[i+3]);
				}
				// Draw last curve
				CatmullRom(points[count-3], points[count-2], points[count-1], points[count-1]);
			}
			PopColor();
		}

		/// <summary>\copydocref{CatmullRom(float3,float3,float3,float3)}</summary>
		/// <param name="color">Color of the object</param>
		public void CatmullRom (float3 p0, float3 p1, float3 p2, float3 p3, Color color) {
			PushColor(color);
			// References used:
			// p.266 GemsV1
			//
			// tension is often set to 0.5 but you can use any reasonable value:
			// http://www.cs.cmu.edu/~462/projects/assn2/assn2/catmullRom.pdf
			//
			// bias and tension controls:
			// http://local.wasp.uwa.edu.au/~pbourke/miscellaneous/interpolation/

			// We will convert the catmull rom spline to a bezier curve for simplicity.
			// The end result of this will be a conversion matrix where we transform catmull rom control points
			// into the equivalent bezier curve control points.

			// Conversion matrix
			// =================

			// A centripetal catmull rom spline can be separated into the following terms:
			// 1 * p1 +
			// t * (-0.5 * p0 + 0.5*p2) +
			// t*t * (p0 - 2.5*p1  + 2.0*p2 + 0.5*t2) +
			// t*t*t * (-0.5*p0 + 1.5*p1 - 1.5*p2 + 0.5*p3)
			//
			// Matrix form:
			// 1     t   t^2 t^3
			// {0, -1/2, 1, -1/2}
			// {1, 0, -5/2, 3/2}
			// {0, 1/2, 2, -3/2}
			// {0, 0, -1/2, 1/2}

			// Transposed matrix:
			// M_1 = {{0, 1, 0, 0}, {-1/2, 0, 1/2, 0}, {1, -5/2, 2, -1/2}, {-1/2, 3/2, -3/2, 1/2}}

			// A bezier spline can be separated into the following terms:
			// (-t^3 + 3 t^2 - 3 t + 1) * c0 +
			// (3t^3 - 6*t^2 + 3t) * c1 +
			// (3t^2 - 3t^3) * c2 +
			// t^3 * c3
			//
			// Matrix form:
			// 1  t  t^2  t^3
			// {1, -3, 3, -1}
			// {0, 3, -6, 3}
			// {0, 0, 3, -3}
			// {0, 0, 0, 1}

			// Transposed matrix:
			// M_2 = {{1, 0, 0, 0}, {-3, 3, 0, 0}, {3, -6, 3, 0}, {-1, 3, -3, 1}}

			// Thus a bezier curve can be evaluated using the expression
			// output1 = T * M_1 * c
			// where T = [1, t, t^2, t^3] and c being the control points c = [c0, c1, c2, c3]^T
			//
			// and a catmull rom spline can be evaluated using
			//
			// output2 = T * M_2 * p
			// where T = same as before and p = [p0, p1, p2, p3]^T
			//
			// We can solve for c in output1 = output2
			// T * M_1 * c = T * M_2 * p
			// M_1 * c = M_2 * p
			// c = M_1^(-1) * M_2 * p
			// Thus a conversion matrix from p to c is M_1^(-1) * M_2
			// This can be calculated and the result is the following matrix:
			//
			// {0, 1, 0, 0}
			// {-1/6, 1, 1/6, 0}
			// {0, 1/6, 1, -1/6}
			// {0, 0, 1, 0}
			// ------------------------------------------------------------------
			//
			// Using this we can calculate c = M_1^(-1) * M_2 * p
			var c0 = p1;
			var c1 = (-p0 + 6*p1 + 1*p2)*(1/6.0f);
			var c2 = (p1 + 6*p2 - p3)*(1/6.0f);
			var c3 = p2;

			// And finally draw the bezier curve which is equivalent to the desired catmull-rom spline
			Bezier(c0, c1, c2, c3);
			PopColor();
		}

		/// <summary>\copydocref{Arrow(float3,float3)}</summary>
		/// <param name="color">Color of the object</param>
		public void Arrow (float3 from, float3 to, Color color) {
			ArrowRelativeSizeHead(from, to, DEFAULT_UP, 0.2f, color);
		}
		/// <summary>\copydocref{Arrow(float3,float3,float3,float)}</summary>
		/// <param name="color">Color of the object</param>
		public void Arrow (float3 from, float3 to, float3 up, float headSize, Color color) {
			PushColor(color);
			var length_sq = math.lengthsq(to - from);

			if (length_sq > 0.000001f) {
				ArrowRelativeSizeHead(from, to, up, headSize * math.rsqrt(length_sq));
			}
			PopColor();
		}
		/// <summary>\copydocref{ArrowRelativeSizeHead(float3,float3,float3,float)}</summary>
		/// <param name="color">Color of the object</param>
		public void ArrowRelativeSizeHead (float3 from, float3 to, float3 up, float headFraction, Color color) {
			PushColor(color);
			Line(from, to);
			var dir = to - from;

			var normal = math.cross(dir, up);
			// Pick a different up direction if the direction happened to be colinear with that one.
			if (math.all(normal == 0)) normal = math.cross(new float3(1, 0, 0), dir);
			// Pick a different up direction if up=(1,0,0) and thus the above check would have generated a zero vector again
			if (math.all(normal == 0)) normal = math.cross(new float3(0, 1, 0), dir);
			normal = math.normalizesafe(normal) * math.length(dir);

			Line(to, to - (dir + normal) * headFraction);
			Line(to, to - (dir - normal) * headFraction);
			PopColor();
		}
		/// <summary>\copydocref{Arrowhead(float3,float3,float)}</summary>
		/// <param name="color">Color of the object</param>
		public void Arrowhead (float3 center, float3 direction, float radius, Color color) {
			Arrowhead(center, direction, DEFAULT_UP, radius, color);
		}

		/// <summary>\copydocref{Arrowhead(float3,float3,float3,float)}</summary>
		/// <param name="color">Color of the object</param>
		public void Arrowhead (float3 center, float3 direction, float3 up, float radius, Color color) {
			if (math.all(direction == 0)) return;
			PushColor(color);
			direction = math.normalizesafe(direction);
			var normal = math.cross(direction, up);
			const float SinPiOver3 = 0.866025f;
			const float CosPiOver3 = 0.5f;
			var circleCenter = center - radius * (1 - CosPiOver3)*0.5f * direction;
			var p1 = circleCenter + radius * direction;
			var p2 = circleCenter - radius * CosPiOver3 * direction + radius * SinPiOver3 * normal;
			var p3 = circleCenter - radius * CosPiOver3 * direction - radius * SinPiOver3 * normal;
			Line(p1, p2);
			Line(p2, circleCenter);
			Line(circleCenter, p3);
			Line(p3, p1);
			PopColor();
		}

		/// <summary>\copydocref{ArrowheadArc(float3,float3,float,float)}</summary>
		/// <param name="color">Color of the object</param>
		public void ArrowheadArc (float3 origin, float3 direction, float offset, float width, Color color) {
			if (!math.any(direction)) return;
			if (offset < 0) throw new System.ArgumentOutOfRangeException(nameof(offset));
			if (offset == 0) return;
			PushColor(color);

			var rot = Quaternion.LookRotation(direction, DEFAULT_UP);
			PushMatrix(Matrix4x4.TRS(origin, rot, Vector3.one));
			var a1 = math.PI * 0.5f - width * (0.5f * Mathf.Deg2Rad);
			var a2 = math.PI * 0.5f + width * (0.5f * Mathf.Deg2Rad);
			CircleXZInternal(float3.zero, offset, a1, a2);
			var p1 = new float3(math.cos(a1), 0, math.sin(a1)) * offset;
			var p2 = new float3(math.cos(a2), 0, math.sin(a2)) * offset;
			const float sqrt2 = 1.4142f;
			var p3 = new float3(0, 0, sqrt2 * offset);
			Line(p1, p3);
			Line(p3, p2);
			PopMatrix();
			PopColor();
		}
		/// <summary>\copydocref{ArrowheadArc(float3,float3,float,float)}</summary>
		/// <param name="color">Color of the object</param>
		public void ArrowheadArc (float3 origin, float3 direction, float offset, Color color) {
			ArrowheadArc(origin, direction, offset, 60, color);
		}
		/// <summary>\copydocref{WireGrid(float3,quaternion,int2,float2)}</summary>
		/// <param name="color">Color of the object</param>
		public void WireGrid (float3 center, quaternion rotation, int2 cells, float2 totalSize, Color color) {
			PushColor(color);
			cells = math.max(cells, new int2(1, 1));
			PushMatrix(float4x4.TRS(center, rotation, new Vector3(totalSize.x, 0, totalSize.y)));
			int w = cells.x;
			int h = cells.y;
			for (int i = 0; i <= w; i++) Line(new float3(i/(float)w - 0.5f, 0, -0.5f), new float3(i/(float)w - 0.5f, 0, 0.5f));
			for (int i = 0; i <= h; i++) Line(new float3(-0.5f, 0, i/(float)h - 0.5f), new float3(0.5f, 0, i/(float)h - 0.5f));
			PopMatrix();
			PopColor();
		}
		/// <summary>\copydocref{WireTriangle(float3,float3,float3)}</summary>
		/// <param name="color">Color of the object</param>
		public void WireTriangle (float3 a, float3 b, float3 c, Color color) {
			PushColor(color);
			Line(a, b);
			Line(b, c);
			Line(c, a);
			PopColor();
		}

		/// <summary>\copydocref{WireRectangleXZ(float3,float2)}</summary>
		[System.Obsolete("Use Draw.xz.WireRectangle instead")]
		public void WireRectangleXZ (float3 center, float2 size, Color color) {
			WireRectangle(center, quaternion.identity, size, color);
		}

		/// <summary>\copydocref{WireRectangle(float3,quaternion,float2)}</summary>
		public void WireRectangle (float3 center, quaternion rotation, float2 size, Color color) {
			WirePlane(center, rotation, size, color);
		}
		/// <summary>\copydocref{WireRectangle(Rect)}</summary>
		[System.Obsolete("Use Draw.xy.WireRectangle instead")]
		public void WireRectangle (Rect rect, Color color) {
			xy.WireRectangle(rect, color);
		}
		/// <summary>\copydocref{WireTriangle(float3,quaternion,float)}</summary>
		/// <param name="color">Color of the object</param>
		public void WireTriangle (float3 center, quaternion rotation, float radius, Color color) {
			WirePolygon(center, 3, rotation, radius, color);
		}

		/// <summary>\copydocref{WirePentagon(float3,quaternion,float)}</summary>
		/// <param name="color">Color of the object</param>
		public void WirePentagon (float3 center, quaternion rotation, float radius, Color color) {
			WirePolygon(center, 5, rotation, radius, color);
		}

		/// <summary>\copydocref{WireHexagon(float3,quaternion,float)}</summary>
		/// <param name="color">Color of the object</param>
		public void WireHexagon (float3 center, quaternion rotation, float radius, Color color) {
			WirePolygon(center, 6, rotation, radius, color);
		}

		/// <summary>\copydocref{WirePolygon(float3,int,quaternion,float)}</summary>
		/// <param name="color">Color of the object</param>
		public void WirePolygon (float3 center, int vertices, quaternion rotation, float radius, Color color) {
			PushColor(color);
			PushMatrix(float4x4.TRS(center, rotation, new float3(radius, radius, radius)));
			float3 prev = new float3(0, 0, 1);
			for (int i = 1; i <= vertices; i++) {
				float a = 2 * math.PI * (i / (float)vertices);
				var p = new float3(math.sin(a), 0, math.cos(a));
				Line(prev, p);
				prev = p;
			}
			PopMatrix();
			PopColor();
		}

		/// <summary>\copydocref{SolidRectangle(Rect)}</summary>
		[System.Obsolete("Use Draw.xy.SolidRectangle instead")]
		public void SolidRectangle (Rect rect, Color color) {
			xy.SolidRectangle(rect, color);
		}

		/// <summary>\copydocref{SolidPlane(float3,float3,float2)}</summary>
		/// <param name="color">Color of the object</param>
		public void SolidPlane (float3 center, float3 normal, float2 size, Color color) {
			PushColor(color);
			if (math.any(normal)) {
				SolidPlane(center, Quaternion.LookRotation(calculateTangent(normal), normal), size);
			}
			PopColor();
		}

		/// <summary>\copydocref{SolidPlane(float3,quaternion,float2)}</summary>
		/// <param name="color">Color of the object</param>
		public void SolidPlane (float3 center, quaternion rotation, float2 size, Color color) {
			PushMatrix(float4x4.TRS(center, rotation, new float3(size.x, 0, size.y)));
			Reserve<Color32, BoxData>();
			Add(Command.Box | Command.PushColorInline);
			Add(ConvertColor(color));
			Add(new BoxData { center = 0, size = 1 });
			PopMatrix();
		}

		/// <summary>\copydocref{WirePlane(float3,float3,float2)}</summary>
		/// <param name="color">Color of the object</param>
		public void WirePlane (float3 center, float3 normal, float2 size, Color color) {
			PushColor(color);
			if (math.any(normal)) {
				WirePlane(center, Quaternion.LookRotation(calculateTangent(normal), normal), size);
			}
			PopColor();
		}
		/// <summary>\copydocref{WirePlane(float3,quaternion,float2)}</summary>
		/// <param name="color">Color of the object</param>
		public void WirePlane (float3 center, quaternion rotation, float2 size, Color color) {
			Reserve<Color32, PlaneData>();
			Add(Command.WirePlane | Command.PushColorInline);
			Add(ConvertColor(color));
			Add(new PlaneData { center = center, rotation = rotation, size = size });
		}
		/// <summary>\copydocref{PlaneWithNormal(float3,float3,float2)}</summary>
		/// <param name="color">Color of the object</param>
		public void PlaneWithNormal (float3 center, float3 normal, float2 size, Color color) {
			PushColor(color);
			if (math.any(normal)) {
				PlaneWithNormal(center, Quaternion.LookRotation(calculateTangent(normal), normal), size);
			}
			PopColor();
		}

		/// <summary>\copydocref{PlaneWithNormal(float3,quaternion,float2)}</summary>
		/// <param name="color">Color of the object</param>
		public void PlaneWithNormal (float3 center, quaternion rotation, float2 size, Color color) {
			PushColor(color);
			SolidPlane(center, rotation, size);
			WirePlane(center, rotation, size);
			ArrowRelativeSizeHead(center, center + math.mul(rotation, new float3(0, 1, 0)) * 0.5f, math.mul(rotation, new float3(0, 0, 1)), 0.2f);
			PopColor();
		}

		/// <summary>\copydocref{SolidTriangle(float3,float3,float3)}</summary>
		/// <param name="color">Color of the object</param>
		public void SolidTriangle (float3 a, float3 b, float3 c, Color color) {
			Reserve<Color32, TriangleData>();
			Add(Command.SolidTriangle | Command.PushColorInline);
			Add(ConvertColor(color));
			Add(new TriangleData { a = a, b = b, c = c });
		}

		/// <summary>\copydocref{SolidBox(float3,float3)}</summary>
		/// <param name="color">Color of the object</param>
		public void SolidBox (float3 center, float3 size, Color color) {
			Reserve<Color32, BoxData>();
			Add(Command.Box | Command.PushColorInline);
			Add(ConvertColor(color));
			Add(new BoxData { center = center, size = size });
		}
		/// <summary>\copydocref{SolidBox(Bounds)}</summary>
		/// <param name="color">Color of the object</param>
		public void SolidBox (Bounds bounds, Color color) {
			SolidBox(bounds.center, bounds.size, color);
		}
		/// <summary>\copydocref{SolidBox(float3,quaternion,float3)}</summary>
		/// <param name="color">Color of the object</param>
		public void SolidBox (float3 center, quaternion rotation, float3 size, Color color) {
			PushColor(color);
			PushMatrix(float4x4.TRS(center, rotation, size));
			SolidBox(float3.zero, Vector3.one);
			PopMatrix();
			PopColor();
		}
		/// <summary>\copydocref{Label3D(float3,quaternion,string,float)}</summary>
		/// <param name="color">Color of the object</param>
		public void Label3D (float3 position, quaternion rotation, string text, float size, Color color) {
			Label3D(position, rotation, text, size, LabelAlignment.MiddleLeft, color);
		}

		/// <summary>\copydocref{Label3D(float3,quaternion,string,float,LabelAlignment)}</summary>
		/// <param name="color">Color of the object</param>
		public void Label3D (float3 position, quaternion rotation, string text, float size, LabelAlignment alignment, Color color) {
			AssertBufferExists();
			Reserve<Color32, TextData3D>();
			Add(Command.Text3D | Command.PushColorInline);
			Add(ConvertColor(color));
			Add(new TextData3D { center = position, rotation = rotation, numCharacters = text.Length, size = size, alignment = alignment });
			AddText(text);
		}

		/// <summary>\copydocref{Label2D(float3,string,float)}</summary>
		/// <param name="color">Color of the object</param>
		public void Label2D (float3 position, string text, float sizeInPixels, Color color) {
			Label2D(position, text, sizeInPixels, LabelAlignment.MiddleLeft, color);
		}

		/// <summary>\copydocref{Label2D(float3,string,float)}</summary>
		/// <param name="color">Color of the object</param>
		public void Label2D (float3 position, string text, Color color) {
			Label2D(position, text, 14, color);
		}

		/// <summary>\copydocref{Label2D(float3,string,float,LabelAlignment)}</summary>
		/// <param name="color">Color of the object</param>
		public void Label2D (float3 position, string text, float sizeInPixels, LabelAlignment alignment, Color color) {
			AssertBufferExists();
			Reserve<Color32, TextData>();
			Add(Command.Text | Command.PushColorInline);
			Add(ConvertColor(color));
			Add(new TextData { center = position, numCharacters = text.Length, sizeInPixels = sizeInPixels, alignment = alignment });
			AddText(text);
		}

		/// <summary>\copydocref{Label2D(float3,FixedString32Bytes,float)}</summary>
		/// <param name="color">Color of the object</param>
		public void Label2D (float3 position, ref FixedString32Bytes text, float sizeInPixels, Color color) {
			Label2D(position, ref text, sizeInPixels, LabelAlignment.MiddleLeft, color);
		}

		/// <summary>\copydocref{Label2D(float3,FixedString32Bytes,float)}</summary>
		/// <param name="color">Color of the object</param>
		public void Label2D (float3 position, ref FixedString32Bytes text, Color color) {
			Label2D(position, ref text, 14, color);
		}

		/// <summary>\copydocref{Label2D(float3,FixedString64Bytes,float)}</summary>
		public void Label2D (float3 position, ref FixedString64Bytes text, float sizeInPixels, Color color) {
			Label2D(position, ref text, sizeInPixels, LabelAlignment.MiddleLeft, color);
		}

		/// <summary>\copydocref{Label2D(float3,FixedString64Bytes,float)}</summary>
		public void Label2D (float3 position, ref FixedString64Bytes text, Color color) {
			Label2D(position, ref text, 14, color);
		}

		/// <summary>\copydocref{Label2D(float3,FixedString128Bytes,float)}</summary>
		public void Label2D (float3 position, ref FixedString128Bytes text, float sizeInPixels, Color color) {
			Label2D(position, ref text, sizeInPixels, LabelAlignment.MiddleLeft, color);
		}

		/// <summary>\copydocref{Label2D(float3,FixedString128Bytes,float)}</summary>
		public void Label2D (float3 position, ref FixedString128Bytes text, Color color) {
			Label2D(position, ref text, 14, color);
		}

		/// <summary>\copydocref{Label2D(float3,FixedString512Bytes,float)}</summary>
		public void Label2D (float3 position, ref FixedString512Bytes text, float sizeInPixels, Color color) {
			Label2D(position, ref text, sizeInPixels, LabelAlignment.MiddleLeft, color);
		}

		/// <summary>\copydocref{Label2D(float3,FixedString512Bytes,float)}</summary>
		public void Label2D (float3 position, ref FixedString512Bytes text, Color color) {
			Label2D(position, ref text, 14, color);
		}

		/// <summary>\copydocref{Label2D(float3,FixedString32Bytes,float,LabelAlignment)}</summary>
		/// <param name="color">Color of the object</param>
		public void Label2D (float3 position, ref FixedString32Bytes text, float sizeInPixels, LabelAlignment alignment, Color color) {
			PushColor(color);
#if MODULE_COLLECTIONS_0_12_0_OR_NEWER
unsafe {
Label2D(position, text.GetUnsafePtr(), text.Length, sizeInPixels, alignment);
}
#else
Debug.LogError("The Label2D method which takes FixedStrings requires the Unity.Collections package version 0.12 or newer");
#endif
PopColor();
}

		/// <summary>\copydocref{Label2D(float3,FixedString64Bytes,float,LabelAlignment)}</summary>
		public void Label2D (float3 position, ref FixedString64Bytes text, float sizeInPixels, LabelAlignment alignment, Color color) {
			PushColor(color);
#if MODULE_COLLECTIONS_0_12_0_OR_NEWER
unsafe {
Label2D(position, text.GetUnsafePtr(), text.Length, sizeInPixels, alignment);
}
#else
Debug.LogError("The Label2D method which takes FixedStrings requires the Unity.Collections package version 0.12 or newer");
#endif
PopColor();
}

		/// <summary>\copydocref{Label2D(float3,FixedString128Bytes,float,LabelAlignment)}</summary>
		public void Label2D (float3 position, ref FixedString128Bytes text, float sizeInPixels, LabelAlignment alignment, Color color) {
			PushColor(color);
#if MODULE_COLLECTIONS_0_12_0_OR_NEWER
unsafe {
Label2D(position, text.GetUnsafePtr(), text.Length, sizeInPixels, alignment);
}
#else
Debug.LogError("The Label2D method which takes FixedStrings requires the Unity.Collections package version 0.12 or newer");
#endif
PopColor();
}

		/// <summary>\copydocref{Label2D(float3,FixedString512Bytes,float,LabelAlignment)}</summary>
		public void Label2D (float3 position, ref FixedString512Bytes text, float sizeInPixels, LabelAlignment alignment, Color color) {
			PushColor(color);
#if MODULE_COLLECTIONS_0_12_0_OR_NEWER
unsafe {
Label2D(position, text.GetUnsafePtr(), text.Length, sizeInPixels, alignment);
}
#else
Debug.LogError("The Label2D method which takes FixedStrings requires the Unity.Collections package version 0.12 or newer");
#endif
PopColor();
}

		/// <summary>\copydocref{Label3D(float3,quaternion,FixedString32Bytes,float)}</summary>
		/// <param name="color">Color of the object</param>
		public void Label3D (float3 position, quaternion rotation, ref FixedString32Bytes text, float size, Color color) {
			Label3D(position, rotation, ref text, size, LabelAlignment.MiddleLeft, color);
		}

		/// <summary>\copydocref{Label3D(float3,quaternion,FixedString64Bytes,float)}</summary>
		public void Label3D (float3 position, quaternion rotation, ref FixedString64Bytes text, float size, Color color) {
			Label3D(position, rotation, ref text, size, LabelAlignment.MiddleLeft, color);
		}

		/// <summary>\copydocref{Label3D(float3,quaternion,FixedString128Bytes,float)}</summary>
		public void Label3D (float3 position, quaternion rotation, ref FixedString128Bytes text, float size, Color color) {
			Label3D(position, rotation, ref text, size, LabelAlignment.MiddleLeft, color);
		}

		/// <summary>\copydocref{Label3D(float3,quaternion,FixedString512Bytes,float)}</summary>
		public void Label3D (float3 position, quaternion rotation, ref FixedString512Bytes text, float size, Color color) {
			Label3D(position, rotation, ref text, size, LabelAlignment.MiddleLeft, color);
		}

		/// <summary>\copydocref{Label3D(float3,quaternion,FixedString32Bytes,float,LabelAlignment)}</summary>
		/// <param name="color">Color of the object</param>
		public void Label3D (float3 position, quaternion rotation, ref FixedString32Bytes text, float size, LabelAlignment alignment, Color color) {
			PushColor(color);
#if MODULE_COLLECTIONS_0_12_0_OR_NEWER
unsafe {
Label3D(position, rotation, text.GetUnsafePtr(), text.Length, size, alignment);
}
#else
Debug.LogError("The Label3D method which takes FixedStrings requires the Unity.Collections package version 0.12 or newer");
#endif
PopColor();
}

		/// <summary>\copydocref{Label3D(float3,quaternion,FixedString64Bytes,float,LabelAlignment)}</summary>
		public void Label3D (float3 position, quaternion rotation, ref FixedString64Bytes text, float size, LabelAlignment alignment, Color color) {
			PushColor(color);
#if MODULE_COLLECTIONS_0_12_0_OR_NEWER
unsafe {
Label3D(position, rotation, text.GetUnsafePtr(), text.Length, size, alignment);
}
#else
Debug.LogError("The Label3D method which takes FixedStrings requires the Unity.Collections package version 0.12 or newer");
#endif
PopColor();
}

		/// <summary>\copydocref{Label3D(float3,quaternion,FixedString128Bytes,float,LabelAlignment)}</summary>
		public void Label3D (float3 position, quaternion rotation, ref FixedString128Bytes text, float size, LabelAlignment alignment, Color color) {
			PushColor(color);
#if MODULE_COLLECTIONS_0_12_0_OR_NEWER
unsafe {
Label3D(position, rotation, text.GetUnsafePtr(), text.Length, size, alignment);
}
#else
Debug.LogError("The Label3D method which takes FixedStrings requires the Unity.Collections package version 0.12 or newer");
#endif
PopColor();
}

		/// <summary>\copydocref{Label3D(float3,quaternion,FixedString512Bytes,float,LabelAlignment)}</summary>
		public void Label3D (float3 position, quaternion rotation, ref FixedString512Bytes text, float size, LabelAlignment alignment, Color color) {
			PushColor(color);
#if MODULE_COLLECTIONS_0_12_0_OR_NEWER
unsafe {
Label3D(position, rotation, text.GetUnsafePtr(), text.Length, size, alignment);
}
#else
Debug.LogError("The Label3D method which takes FixedStrings requires the Unity.Collections package version 0.12 or newer");
#endif
PopColor();
}
}
}
using UnityEngine;

namespace Drawing {
/// <summary>
/// Collections of colors.
///
/// The easiest way to use this class is to import it with a "using" statement:
///
/// <code>
/// using Palette = Pathfinding.Drawing.Palette.Colorbrewer.Set1;
///
/// class PaletteTest : MonoBehaviour {
///     public void Update () {
///         Draw.Line(new Vector3(0, 0, 0), new Vector3(1, 1, 1), Palette.Orange);
///     }
/// }
/// </code>
///
/// Note: This class has relatively few color collections at the moment. More will be added in the future.
/// </summary>
public static class Palette {
/// <summary>Pure colors</summary>
public static class Pure {
public static readonly Color Yellow = new Color(1, 1, 0, 1);
public static readonly Color Clear = new Color(0, 0, 0, 0);
public static readonly Color Grey = new Color(0.5f, 0.5f, 0.5f, 1);
public static readonly Color Magenta = new Color(1, 0, 1, 1);
public static readonly Color Cyan = new Color(0, 1, 1, 1);
public static readonly Color Red = new Color(1, 0, 0, 1);
public static readonly Color Black = new Color(0, 0, 0, 1);
public static readonly Color White = new Color(1, 1, 1, 1);
public static readonly Color Blue = new Color(0, 0, 1, 1);
public static readonly Color Green = new Color(0, 1, 0, 1);
}

		/// <summary>
		/// Colorbrewer colors.
		/// See: http://colorbrewer2.org/
		/// </summary>
		public static class Colorbrewer {
			/// <summary>Set 1 - Qualitative</summary>
			public static class Set1 {
				public static readonly Color Red = new Color(228/255f, 26/255f, 28/255f, 1);
				public static readonly Color Blue = new Color(55/255f, 126/255f, 184/255f, 1);
				public static readonly Color Green = new Color(77/255f, 175/255f, 74/255f, 1);
				public static readonly Color Purple = new Color(152/255f, 78/255f, 163/255f, 1);
				public static readonly Color Orange = new Color(255/255f, 127/255f, 0/255f, 1);
				public static readonly Color Yellow = new Color(255/255f, 255/255f, 51/255f, 1);
				public static readonly Color Brown = new Color(166/255f, 86/255f, 40/255f, 1);
				public static readonly Color Pink = new Color(247/255f, 129/255f, 191/255f, 1);
				public static readonly Color Grey = new Color(153/255f, 153/255f, 153/255f, 1);
			}

			/// <summary>Blues - Sequential</summary>
			public static class Blues {
				static readonly Color[] Colors = new Color[] {
					new Color(43/255f, 140/255f, 190/255f),

					new Color(166/255f, 189/255f, 219/255f),
					new Color(43/255f, 140/255f, 190/255f),

					new Color(236/255f, 231/255f, 242/255f),
					new Color(166/255f, 189/255f, 219/255f),
					new Color(43/255f, 140/255f, 190/255f),

					new Color(241/255f, 238/255f, 246/255f),
					new Color(189/255f, 201/255f, 225/255f),
					new Color(116/255f, 169/255f, 207/255f),
					new Color(5/255f, 112/255f, 176/255f),

					new Color(241/255f, 238/255f, 246/255f),
					new Color(189/255f, 201/255f, 225/255f),
					new Color(116/255f, 169/255f, 207/255f),
					new Color(43/255f, 140/255f, 190/255f),
					new Color(4/255f, 90/255f, 141/255f),

					new Color(241/255f, 238/255f, 246/255f),
					new Color(208/255f, 209/255f, 230/255f),
					new Color(166/255f, 189/255f, 219/255f),
					new Color(116/255f, 169/255f, 207/255f),
					new Color(43/255f, 140/255f, 190/255f),
					new Color(4/255f, 90/255f, 141/255f),

					new Color(241/255f, 238/255f, 246/255f),
					new Color(208/255f, 209/255f, 230/255f),
					new Color(166/255f, 189/255f, 219/255f),
					new Color(116/255f, 169/255f, 207/255f),
					new Color(54/255f, 144/255f, 192/255f),
					new Color(5/255f, 112/255f, 176/255f),
					new Color(3/255f, 78/255f, 123/255f),

					new Color(255/255f, 247/255f, 251/255f),
					new Color(236/255f, 231/255f, 242/255f),
					new Color(208/255f, 209/255f, 230/255f),
					new Color(166/255f, 189/255f, 219/255f),
					new Color(116/255f, 169/255f, 207/255f),
					new Color(54/255f, 144/255f, 192/255f),
					new Color(5/255f, 112/255f, 176/255f),
					new Color(3/255f, 78/255f, 123/255f),

					new Color(255/255f, 247/255f, 251/255f),
					new Color(236/255f, 231/255f, 242/255f),
					new Color(208/255f, 209/255f, 230/255f),
					new Color(166/255f, 189/255f, 219/255f),
					new Color(116/255f, 169/255f, 207/255f),
					new Color(54/255f, 144/255f, 192/255f),
					new Color(5/255f, 112/255f, 176/255f),
					new Color(4/255f, 90/255f, 141/255f),
					new Color(2/255f, 56/255f, 88/255f),
				};

				/// <summary>Returns a color for the specified class.</summary>
				/// <param name="classes">Number of classes. Must be between 1 and 9.</param>
				/// <param name="index">Index of the color class. Must be between 0 and classes-1.</param>
				public static Color GetColor (int classes, int index) {
					if (index < 0 || index >= classes) throw new System.ArgumentOutOfRangeException("index", "Index must be less than classes and at least 0");
					if (classes <= 0 || classes > 9) throw new System.ArgumentOutOfRangeException("classes", "Only up to 9 classes are supported");

					return Colors[(classes - 1)*classes/2 + index];
				}
			}
		}
	}
}
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using System.Collections.Generic;

namespace Drawing {
/// <summary>Various high-level utilities that are useful when drawing things</summary>
public static class DrawingUtilities {
private static List<Component> componentBuffer = new List<Component>();

		/// <summary>
		/// Bounding box of a GameObject.
		/// Sometimes you want to quickly draw the bounding box of an object. This is not always trivial as the object may have any number of children with colliders and renderers.
		/// You can use this method to calculate the bounding box easily.
		///
		/// The bounding box is calculated based on the colliders and renderers on this object and all its children.
		///
		/// [Open online documentation to see images]
		/// <code>
		/// Draw.WireBox(DrawingUtilities.BoundsFrom(transform), Color.black);
		/// </code>
		///
		/// See: <see cref="BoundsFrom(Transform)"/>
		/// </summary>
		public static Bounds BoundsFrom (GameObject gameObject) {
			return BoundsFrom(gameObject.transform);
		}

		/// <summary>
		/// Bounding box of a Transform.
		/// Sometimes you want to quickly draw the bounding box of an object. This is not always trivial as the object may have any number of children with colliders and renderers.
		/// You can use this method to calculate the bounding box easily.
		///
		/// The bounding box is calculated based on the colliders and renderers on this object and all its children.
		///
		/// [Open online documentation to see images]
		/// <code>
		/// Draw.WireBox(DrawingUtilities.BoundsFrom(transform), Color.black);
		/// </code>
		///
		/// See: <see cref="BoundsFrom(GameObject)"/>
		/// </summary>
		public static Bounds BoundsFrom (Transform transform) {
			transform.gameObject.GetComponents(componentBuffer);
			Bounds bounds = new Bounds(transform.position, Vector3.zero);
			for (int i = 0; i < componentBuffer.Count; i++) {
				var component = componentBuffer[i];
				if (component is Collider coll) bounds.Encapsulate(coll.bounds);
				else if (component is Collider2D coll2D) bounds.Encapsulate(coll2D.bounds);
				else if (component is MeshRenderer rend) bounds.Encapsulate(rend.bounds);
				else if (component is SpriteRenderer rendSprite) bounds.Encapsulate(rendSprite.bounds);
			}
			componentBuffer.Clear();
			var children = transform.childCount;
			for (int i = 0; i < children; i++) bounds.Encapsulate(BoundsFrom(transform.GetChild(i)));
			return bounds;
		}

		/// <summary>
		/// Bounding box which contains all points in the list.
		/// <code>
		/// List<Vector3> points = new List<Vector3> { new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 1) };
		/// Draw.WireBox(DrawingUtilities.BoundsFrom(points), Color.black);
		/// </code>
		///
		/// See: <see cref="BoundsFrom(Vector3"/>[])
		/// See: <see cref="BoundsFrom(NativeArray<float3>)"/>
		/// </summary>
		public static Bounds BoundsFrom (List<Vector3> points) {
			if (points.Count == 0) throw new System.ArgumentException("At least 1 point is required");
			Vector3 mn = points[0];
			Vector3 mx = points[0];
			for (int i = 0; i < points.Count; i++) {
				mn = Vector3.Min(mn, points[i]);
				mx = Vector3.Max(mx, points[i]);
			}
			return new Bounds((mx + mn) * 0.5f, (mx - mn) * 0.5f);
		}

		/// <summary>
		/// Bounding box which contains all points in the array.
		/// <code>
		/// List<Vector3> points = new List<Vector3> { new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 1) };
		/// Draw.WireBox(DrawingUtilities.BoundsFrom(points), Color.black);
		/// </code>
		///
		/// See: <see cref="BoundsFrom(List<Vector3>)"/>
		/// See: <see cref="BoundsFrom(NativeArray<float3>)"/>
		/// </summary>
		public static Bounds BoundsFrom (Vector3[] points) {
			if (points.Length == 0) throw new System.ArgumentException("At least 1 point is required");
			Vector3 mn = points[0];
			Vector3 mx = points[0];
			for (int i = 0; i < points.Length; i++) {
				mn = Vector3.Min(mn, points[i]);
				mx = Vector3.Max(mx, points[i]);
			}
			return new Bounds((mx + mn) * 0.5f, (mx - mn) * 0.5f);
		}

		/// <summary>
		/// Bounding box which contains all points in the array.
		/// <code>
		/// List<Vector3> points = new List<Vector3> { new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 1) };
		/// Draw.WireBox(DrawingUtilities.BoundsFrom(points), Color.black);
		/// </code>
		///
		/// See: <see cref="BoundsFrom(List<Vector3>)"/>
		/// See: <see cref="BoundsFrom(Vector3"/>[])
		/// </summary>
		public static Bounds BoundsFrom (NativeArray<float3> points) {
			if (points.Length == 0) throw new System.ArgumentException("At least 1 point is required");
			float3 mn = points[0];
			float3 mx = points[0];
			for (int i = 0; i < points.Length; i++) {
				mn = math.min(mn, points[i]);
				mx = math.max(mx, points[i]);
			}
			return new Bounds((mx + mn) * 0.5f, (mx - mn) * 0.5f);
		}
	}
}
// This file is automatically generated by a script based on the CommandBuilder API
using Unity.Burst;
using UnityEngine;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Drawing {
/// <summary>
/// Methods for easily drawing things in the editor and in standalone games.
///
/// See: getstarted (view in online documentation for working links)
/// </summary>
public static class Draw {
internal static CommandBuilder builder;
internal static CommandBuilder ingame_builder;

		/// <summary>
		/// Draws items in the editor and in standalone games, even if gizmos are disabled.
		/// See: ingame (view in online documentation for working links)
		/// </summary>
		public static ref CommandBuilder ingame {
			get {
				DrawingManager.Init();
				return ref ingame_builder;
			}
		}

		/// <summary>
		/// Draws items in the editor if gizmos are enabled.
		/// All drawing methods on the static Draw class are forwarded to this command builder.
		///
		/// See: ingame (view in online documentation for working links)
		/// </summary>
		public static ref CommandBuilder editor {
			get {
				DrawingManager.Init();
				return ref builder;
			}
		}

		/// <summary>\copydocref{CommandBuilder.xy}</summary>
		public static CommandBuilder2D xy {
			get {
				DrawingManager.Init();
				return new CommandBuilder2D(builder, true);
			}
		}

		/// <summary>\copydocref{CommandBuilder.xz}</summary>
		public static CommandBuilder2D xz {
			get {
				DrawingManager.Init();
				return new CommandBuilder2D(builder, false);
			}
		}


#if UNITY_EDITOR
/// <summary>
/// \copydocref{Drawing::CommandBuilder::WithMatrix(Matrix4x4)}
/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
/// </summary>
[BurstDiscard]
public static CommandBuilder.ScopeMatrix WithMatrix (Matrix4x4 matrix) {
DrawingManager.Init();
return builder.WithMatrix(matrix);
}
#else
[BurstDiscard]
public static CommandBuilder.ScopeEmpty WithMatrix (Matrix4x4 matrix) {
// Do nothing in standlone builds
return new CommandBuilder.ScopeEmpty();
}
#endif


#if UNITY_EDITOR
/// <summary>
/// \copydocref{Drawing::CommandBuilder::WithMatrix(float3x3)}
/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
/// </summary>
[BurstDiscard]
public static CommandBuilder.ScopeMatrix WithMatrix (float3x3 matrix) {
DrawingManager.Init();
return builder.WithMatrix(matrix);
}
#else
[BurstDiscard]
public static CommandBuilder.ScopeEmpty WithMatrix (float3x3 matrix) {
// Do nothing in standlone builds
return new CommandBuilder.ScopeEmpty();
}
#endif


#if UNITY_EDITOR
/// <summary>
/// \copydocref{Drawing::CommandBuilder::WithColor(Color)}
/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
/// </summary>
[BurstDiscard]
public static CommandBuilder.ScopeColor WithColor (Color color) {
DrawingManager.Init();
return builder.WithColor(color);
}
#else
[BurstDiscard]
public static CommandBuilder.ScopeEmpty WithColor (Color color) {
// Do nothing in standlone builds
return new CommandBuilder.ScopeEmpty();
}
#endif


#if UNITY_EDITOR
/// <summary>
/// \copydocref{Drawing::CommandBuilder::WithDuration(float)}
/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
/// </summary>
[BurstDiscard]
public static CommandBuilder.ScopePersist WithDuration (float duration) {
DrawingManager.Init();
return builder.WithDuration(duration);
}
#else
[BurstDiscard]
public static CommandBuilder.ScopeEmpty WithDuration (float duration) {
// Do nothing in standlone builds
return new CommandBuilder.ScopeEmpty();
}
#endif


#if UNITY_EDITOR
/// <summary>
/// \copydocref{Drawing::CommandBuilder::WithLineWidth(float,bool)}
/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
/// </summary>
[BurstDiscard]
public static CommandBuilder.ScopeLineWidth WithLineWidth (float pixels, bool automaticJoins = true) {
DrawingManager.Init();
return builder.WithLineWidth(pixels, automaticJoins);
}
#else
[BurstDiscard]
public static CommandBuilder.ScopeEmpty WithLineWidth (float pixels, bool automaticJoins = true) {
// Do nothing in standlone builds
return new CommandBuilder.ScopeEmpty();
}
#endif


#if UNITY_EDITOR
/// <summary>
/// \copydocref{Drawing::CommandBuilder::InLocalSpace(Transform)}
/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
/// </summary>
[BurstDiscard]
public static CommandBuilder.ScopeMatrix InLocalSpace (Transform transform) {
DrawingManager.Init();
return builder.InLocalSpace(transform);
}
#else
[BurstDiscard]
public static CommandBuilder.ScopeEmpty InLocalSpace (Transform transform) {
// Do nothing in standlone builds
return new CommandBuilder.ScopeEmpty();
}
#endif


#if UNITY_EDITOR
/// <summary>
/// \copydocref{Drawing::CommandBuilder::InScreenSpace(Camera)}
/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
/// </summary>
[BurstDiscard]
public static CommandBuilder.ScopeMatrix InScreenSpace (Camera camera) {
DrawingManager.Init();
return builder.InScreenSpace(camera);
}
#else
[BurstDiscard]
public static CommandBuilder.ScopeEmpty InScreenSpace (Camera camera) {
// Do nothing in standlone builds
return new CommandBuilder.ScopeEmpty();
}
#endif


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::PushMatrix(Matrix4x4)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void PushMatrix (Matrix4x4 matrix) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.PushMatrix(matrix);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::PushMatrix(float4x4)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void PushMatrix (float4x4 matrix) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.PushMatrix(matrix);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::PushSetMatrix(Matrix4x4)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void PushSetMatrix (Matrix4x4 matrix) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.PushSetMatrix(matrix);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::PushSetMatrix(float4x4)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void PushSetMatrix (float4x4 matrix) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.PushSetMatrix(matrix);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::PopMatrix()}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void PopMatrix () {
#if UNITY_EDITOR
DrawingManager.Init();
builder.PopMatrix();
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::PushColor(Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void PushColor (Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.PushColor(color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::PopColor()}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void PopColor () {
#if UNITY_EDITOR
DrawingManager.Init();
builder.PopColor();
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::PushDuration(float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void PushDuration (float duration) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.PushDuration(duration);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::PopDuration()}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void PopDuration () {
#if UNITY_EDITOR
DrawingManager.Init();
builder.PopDuration();
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::PushPersist(float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		[System.Obsolete("Renamed to PushDuration for consistency")]
		public static void PushPersist (float duration) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.PushPersist(duration);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::PopPersist()}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		[System.Obsolete("Renamed to PopDuration for consistency")]
		public static void PopPersist () {
#if UNITY_EDITOR
DrawingManager.Init();
builder.PopPersist();
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::PushLineWidth(float,bool)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void PushLineWidth (float pixels, bool automaticJoins = true) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.PushLineWidth(pixels, automaticJoins);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::PopLineWidth()}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void PopLineWidth () {
#if UNITY_EDITOR
DrawingManager.Init();
builder.PopLineWidth();
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Line(float3,float3)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Line (float3 a, float3 b) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Line(a, b);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Line(Vector3,Vector3)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Line (Vector3 a, Vector3 b) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Line(a, b);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Line(Vector3,Vector3,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Line (Vector3 a, Vector3 b, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Line(a, b, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Ray(float3,float3)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Ray (float3 origin, float3 direction) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Ray(origin, direction);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Ray(Ray,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Ray (Ray ray, float length) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Ray(ray, length);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Arc(float3,float3,float3)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Arc (float3 center, float3 start, float3 end) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Arc(center, start, end);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::CircleXZ(float3,float,float,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		[System.Obsolete("Use Draw.xz.Circle instead")]
		public static void CircleXZ (float3 center, float radius, float startAngle = 0f, float endAngle = 2 * Mathf.PI) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.CircleXZ(center, radius, startAngle, endAngle);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::CircleXY(float3,float,float,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		[System.Obsolete("Use Draw.xy.Circle instead")]
		public static void CircleXY (float3 center, float radius, float startAngle = 0f, float endAngle = 2 * Mathf.PI) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.CircleXY(center, radius, startAngle, endAngle);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Circle(float3,float3,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Circle (float3 center, float3 normal, float radius) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Circle(center, normal, radius);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::SolidArc(float3,float3,float3)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void SolidArc (float3 center, float3 start, float3 end) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.SolidArc(center, start, end);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::SolidCircleXZ(float3,float,float,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		[System.Obsolete("Use Draw.xz.SolidCircle instead")]
		public static void SolidCircleXZ (float3 center, float radius, float startAngle = 0f, float endAngle = 2 * Mathf.PI) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.SolidCircleXZ(center, radius, startAngle, endAngle);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::SolidCircleXY(float3,float,float,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		[System.Obsolete("Use Draw.xy.SolidCircle instead")]
		public static void SolidCircleXY (float3 center, float radius, float startAngle = 0f, float endAngle = 2 * Mathf.PI) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.SolidCircleXY(center, radius, startAngle, endAngle);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::SolidCircle(float3,float3,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void SolidCircle (float3 center, float3 normal, float radius) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.SolidCircle(center, normal, radius);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::SphereOutline(float3,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void SphereOutline (float3 center, float radius) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.SphereOutline(center, radius);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireCylinder(float3,float3,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireCylinder (float3 bottom, float3 top, float radius) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.WireCylinder(bottom, top, radius);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireCylinder(float3,float3,float,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireCylinder (float3 position, float3 up, float height, float radius) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.WireCylinder(position, up, height, radius);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireCapsule(float3,float3,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireCapsule (float3 start, float3 end, float radius) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.WireCapsule(start, end, radius);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireCapsule(float3,float3,float,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireCapsule (float3 position, float3 direction, float length, float radius) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.WireCapsule(position, direction, length, radius);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireSphere(float3,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireSphere (float3 position, float radius) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.WireSphere(position, radius);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Polyline(List<Vector3>,bool)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Polyline (List<Vector3> points, bool cycle = false) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Polyline(points, cycle);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Polyline(Vector3[],bool)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Polyline (Vector3[] points, bool cycle = false) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Polyline(points, cycle);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Polyline(float3[],bool)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Polyline (float3[] points, bool cycle = false) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Polyline(points, cycle);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Polyline(NativeArray<float3>,bool)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Polyline (NativeArray<float3> points, bool cycle = false) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Polyline(points, cycle);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::DashedLine(float3,float3,float,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void DashedLine (float3 a, float3 b, float dash, float gap) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.DashedLine(a, b, dash, gap);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::DashedPolyline(List<Vector3>,float,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void DashedPolyline (List<Vector3> points, float dash, float gap) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.DashedPolyline(points, dash, gap);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireBox(float3,float3)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireBox (float3 center, float3 size) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.WireBox(center, size);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireBox(float3,quaternion,float3)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireBox (float3 center, quaternion rotation, float3 size) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.WireBox(center, rotation, size);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireBox(Bounds)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireBox (Bounds bounds) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.WireBox(bounds);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireMesh(Mesh)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireMesh (Mesh mesh) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.WireMesh(mesh);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireMesh(NativeArray<float3>,NativeArray<int>)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireMesh (NativeArray<float3> vertices, NativeArray<int> triangles) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.WireMesh(vertices, triangles);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::SolidMesh(Mesh)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void SolidMesh (Mesh mesh) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.SolidMesh(mesh);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::SolidMesh(List<Vector3>,List<int>,List<Color>)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void SolidMesh (List<Vector3> vertices, List<int> triangles, List<Color> colors) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.SolidMesh(vertices, triangles, colors);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::SolidMesh(Vector3[],int[],Color[],int,int)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void SolidMesh (Vector3[] vertices, int[] triangles, Color[] colors, int vertexCount, int indexCount) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.SolidMesh(vertices, triangles, colors, vertexCount, indexCount);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Cross(float3,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Cross (float3 position, float size = 1) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Cross(position, size);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::CrossXZ(float3,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		[System.Obsolete("Use Draw.xz.Cross instead")]
		public static void CrossXZ (float3 position, float size = 1) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.CrossXZ(position, size);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::CrossXY(float3,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		[System.Obsolete("Use Draw.xy.Cross instead")]
		public static void CrossXY (float3 position, float size = 1) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.CrossXY(position, size);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Bezier(float3,float3,float3,float3)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Bezier (float3 p0, float3 p1, float3 p2, float3 p3) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Bezier(p0, p1, p2, p3);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::CatmullRom(List<Vector3>)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void CatmullRom (List<Vector3> points) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.CatmullRom(points);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::CatmullRom(float3,float3,float3,float3)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void CatmullRom (float3 p0, float3 p1, float3 p2, float3 p3) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.CatmullRom(p0, p1, p2, p3);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Arrow(float3,float3)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Arrow (float3 from, float3 to) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Arrow(from, to);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Arrow(float3,float3,float3,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Arrow (float3 from, float3 to, float3 up, float headSize) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Arrow(from, to, up, headSize);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::ArrowRelativeSizeHead(float3,float3,float3,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void ArrowRelativeSizeHead (float3 from, float3 to, float3 up, float headFraction) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.ArrowRelativeSizeHead(from, to, up, headFraction);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Arrowhead(float3,float3,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Arrowhead (float3 center, float3 direction, float radius) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Arrowhead(center, direction, radius);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Arrowhead(float3,float3,float3,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Arrowhead (float3 center, float3 direction, float3 up, float radius) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Arrowhead(center, direction, up, radius);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::ArrowheadArc(float3,float3,float,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void ArrowheadArc (float3 origin, float3 direction, float offset, float width = 60) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.ArrowheadArc(origin, direction, offset, width);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireGrid(float3,quaternion,int2,float2)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireGrid (float3 center, quaternion rotation, int2 cells, float2 totalSize) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.WireGrid(center, rotation, cells, totalSize);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireTriangle(float3,float3,float3)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireTriangle (float3 a, float3 b, float3 c) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.WireTriangle(a, b, c);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireRectangleXZ(float3,float2)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		[System.Obsolete("Use Draw.xz.WireRectangle instead")]
		public static void WireRectangleXZ (float3 center, float2 size) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.WireRectangleXZ(center, size);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireRectangle(float3,quaternion,float2)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireRectangle (float3 center, quaternion rotation, float2 size) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.WireRectangle(center, rotation, size);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireRectangle(Rect)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		[System.Obsolete("Use Draw.xy.WireRectangle instead")]
		public static void WireRectangle (Rect rect) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.WireRectangle(rect);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireTriangle(float3,quaternion,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireTriangle (float3 center, quaternion rotation, float radius) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.WireTriangle(center, rotation, radius);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WirePentagon(float3,quaternion,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WirePentagon (float3 center, quaternion rotation, float radius) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.WirePentagon(center, rotation, radius);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireHexagon(float3,quaternion,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireHexagon (float3 center, quaternion rotation, float radius) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.WireHexagon(center, rotation, radius);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WirePolygon(float3,int,quaternion,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WirePolygon (float3 center, int vertices, quaternion rotation, float radius) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.WirePolygon(center, vertices, rotation, radius);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::SolidRectangle(Rect)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		[System.Obsolete("Use Draw.xy.SolidRectangle instead")]
		public static void SolidRectangle (Rect rect) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.SolidRectangle(rect);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::SolidPlane(float3,float3,float2)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void SolidPlane (float3 center, float3 normal, float2 size) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.SolidPlane(center, normal, size);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::SolidPlane(float3,quaternion,float2)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void SolidPlane (float3 center, quaternion rotation, float2 size) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.SolidPlane(center, rotation, size);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WirePlane(float3,float3,float2)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WirePlane (float3 center, float3 normal, float2 size) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.WirePlane(center, normal, size);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WirePlane(float3,quaternion,float2)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WirePlane (float3 center, quaternion rotation, float2 size) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.WirePlane(center, rotation, size);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::PlaneWithNormal(float3,float3,float2)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void PlaneWithNormal (float3 center, float3 normal, float2 size) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.PlaneWithNormal(center, normal, size);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::PlaneWithNormal(float3,quaternion,float2)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void PlaneWithNormal (float3 center, quaternion rotation, float2 size) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.PlaneWithNormal(center, rotation, size);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::SolidTriangle(float3,float3,float3)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void SolidTriangle (float3 a, float3 b, float3 c) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.SolidTriangle(a, b, c);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::SolidBox(float3,float3)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void SolidBox (float3 center, float3 size) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.SolidBox(center, size);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::SolidBox(Bounds)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void SolidBox (Bounds bounds) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.SolidBox(bounds);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::SolidBox(float3,quaternion,float3)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void SolidBox (float3 center, quaternion rotation, float3 size) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.SolidBox(center, rotation, size);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label3D(float3,quaternion,string,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label3D (float3 position, quaternion rotation, string text, float size) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label3D(position, rotation, text, size);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label3D(float3,quaternion,string,float,LabelAlignment)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label3D (float3 position, quaternion rotation, string text, float size, LabelAlignment alignment) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label3D(position, rotation, text, size, alignment);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label2D(float3,string,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label2D (float3 position, string text, float sizeInPixels = 14) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label2D(position, text, sizeInPixels);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label2D(float3,string,float,LabelAlignment)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label2D (float3 position, string text, float sizeInPixels, LabelAlignment alignment) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label2D(position, text, sizeInPixels, alignment);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label2D(float3,FixedString32Bytes,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label2D (float3 position, ref FixedString32Bytes text, float sizeInPixels = 14) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label2D(position, ref text, sizeInPixels);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label2D(float3,FixedString64Bytes,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label2D (float3 position, ref FixedString64Bytes text, float sizeInPixels = 14) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label2D(position, ref text, sizeInPixels);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label2D(float3,FixedString128Bytes,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label2D (float3 position, ref FixedString128Bytes text, float sizeInPixels = 14) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label2D(position, ref text, sizeInPixels);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label2D(float3,FixedString512Bytes,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label2D (float3 position, ref FixedString512Bytes text, float sizeInPixels = 14) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label2D(position, ref text, sizeInPixels);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label2D(float3,FixedString32Bytes,float,LabelAlignment)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label2D (float3 position, ref FixedString32Bytes text, float sizeInPixels, LabelAlignment alignment) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label2D(position, ref text, sizeInPixels, alignment);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label2D(float3,FixedString64Bytes,float,LabelAlignment)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label2D (float3 position, ref FixedString64Bytes text, float sizeInPixels, LabelAlignment alignment) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label2D(position, ref text, sizeInPixels, alignment);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label2D(float3,FixedString128Bytes,float,LabelAlignment)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label2D (float3 position, ref FixedString128Bytes text, float sizeInPixels, LabelAlignment alignment) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label2D(position, ref text, sizeInPixels, alignment);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label2D(float3,FixedString512Bytes,float,LabelAlignment)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label2D (float3 position, ref FixedString512Bytes text, float sizeInPixels, LabelAlignment alignment) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label2D(position, ref text, sizeInPixels, alignment);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label3D(float3,quaternion,FixedString32Bytes,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label3D (float3 position, quaternion rotation, ref FixedString32Bytes text, float size) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label3D(position, rotation, ref text, size);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label3D(float3,quaternion,FixedString64Bytes,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label3D (float3 position, quaternion rotation, ref FixedString64Bytes text, float size) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label3D(position, rotation, ref text, size);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label3D(float3,quaternion,FixedString128Bytes,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label3D (float3 position, quaternion rotation, ref FixedString128Bytes text, float size) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label3D(position, rotation, ref text, size);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label3D(float3,quaternion,FixedString512Bytes,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label3D (float3 position, quaternion rotation, ref FixedString512Bytes text, float size) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label3D(position, rotation, ref text, size);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label3D(float3,quaternion,FixedString32Bytes,float,LabelAlignment)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label3D (float3 position, quaternion rotation, ref FixedString32Bytes text, float size, LabelAlignment alignment) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label3D(position, rotation, ref text, size, alignment);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label3D(float3,quaternion,FixedString64Bytes,float,LabelAlignment)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label3D (float3 position, quaternion rotation, ref FixedString64Bytes text, float size, LabelAlignment alignment) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label3D(position, rotation, ref text, size, alignment);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label3D(float3,quaternion,FixedString128Bytes,float,LabelAlignment)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label3D (float3 position, quaternion rotation, ref FixedString128Bytes text, float size, LabelAlignment alignment) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label3D(position, rotation, ref text, size, alignment);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label3D(float3,quaternion,FixedString512Bytes,float,LabelAlignment)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label3D (float3 position, quaternion rotation, ref FixedString512Bytes text, float size, LabelAlignment alignment) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label3D(position, rotation, ref text, size, alignment);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Line(float3,float3,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Line (float3 a, float3 b, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Line(a, b, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Ray(float3,float3,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Ray (float3 origin, float3 direction, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Ray(origin, direction, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Ray(Ray,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Ray (Ray ray, float length, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Ray(ray, length, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Arc(float3,float3,float3,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Arc (float3 center, float3 start, float3 end, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Arc(center, start, end, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::CircleXZ(float3,float,float,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		[System.Obsolete("Use Draw.xz.Circle instead")]
		public static void CircleXZ (float3 center, float radius, float startAngle, float endAngle, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.CircleXZ(center, radius, startAngle, endAngle, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::CircleXZ(float3,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		[System.Obsolete("Use Draw.xz.Circle instead")]
		public static void CircleXZ (float3 center, float radius, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.CircleXZ(center, radius, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::CircleXY(float3,float,float,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		[System.Obsolete("Use Draw.xy.Circle instead")]
		public static void CircleXY (float3 center, float radius, float startAngle, float endAngle, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.CircleXY(center, radius, startAngle, endAngle, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::CircleXY(float3,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		[System.Obsolete("Use Draw.xy.Circle instead")]
		public static void CircleXY (float3 center, float radius, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.CircleXY(center, radius, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Circle(float3,float3,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Circle (float3 center, float3 normal, float radius, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Circle(center, normal, radius, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::SolidArc(float3,float3,float3,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void SolidArc (float3 center, float3 start, float3 end, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.SolidArc(center, start, end, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::SolidCircleXZ(float3,float,float,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		[System.Obsolete("Use Draw.xz.SolidCircle instead")]
		public static void SolidCircleXZ (float3 center, float radius, float startAngle, float endAngle, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.SolidCircleXZ(center, radius, startAngle, endAngle, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::SolidCircleXZ(float3,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		[System.Obsolete("Use Draw.xz.SolidCircle instead")]
		public static void SolidCircleXZ (float3 center, float radius, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.SolidCircleXZ(center, radius, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::SolidCircleXY(float3,float,float,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		[System.Obsolete("Use Draw.xy.SolidCircle instead")]
		public static void SolidCircleXY (float3 center, float radius, float startAngle, float endAngle, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.SolidCircleXY(center, radius, startAngle, endAngle, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::SolidCircleXY(float3,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		[System.Obsolete("Use Draw.xy.SolidCircle instead")]
		public static void SolidCircleXY (float3 center, float radius, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.SolidCircleXY(center, radius, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::SolidCircle(float3,float3,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void SolidCircle (float3 center, float3 normal, float radius, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.SolidCircle(center, normal, radius, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::SphereOutline(float3,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void SphereOutline (float3 center, float radius, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.SphereOutline(center, radius, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireCylinder(float3,float3,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireCylinder (float3 bottom, float3 top, float radius, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.WireCylinder(bottom, top, radius, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireCylinder(float3,float3,float,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireCylinder (float3 position, float3 up, float height, float radius, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.WireCylinder(position, up, height, radius, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireCapsule(float3,float3,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireCapsule (float3 start, float3 end, float radius, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.WireCapsule(start, end, radius, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireCapsule(float3,float3,float,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireCapsule (float3 position, float3 direction, float length, float radius, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.WireCapsule(position, direction, length, radius, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireSphere(float3,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireSphere (float3 position, float radius, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.WireSphere(position, radius, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Polyline(List<Vector3>,bool,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Polyline (List<Vector3> points, bool cycle, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Polyline(points, cycle, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Polyline(List<Vector3>,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Polyline (List<Vector3> points, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Polyline(points, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Polyline(Vector3[],bool,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Polyline (Vector3[] points, bool cycle, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Polyline(points, cycle, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Polyline(Vector3[],Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Polyline (Vector3[] points, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Polyline(points, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Polyline(float3[],bool,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Polyline (float3[] points, bool cycle, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Polyline(points, cycle, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Polyline(float3[],Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Polyline (float3[] points, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Polyline(points, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Polyline(NativeArray<float3>,bool,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Polyline (NativeArray<float3> points, bool cycle, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Polyline(points, cycle, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Polyline(NativeArray<float3>,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Polyline (NativeArray<float3> points, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Polyline(points, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::DashedLine(float3,float3,float,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void DashedLine (float3 a, float3 b, float dash, float gap, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.DashedLine(a, b, dash, gap, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::DashedPolyline(List<Vector3>,float,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void DashedPolyline (List<Vector3> points, float dash, float gap, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.DashedPolyline(points, dash, gap, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireBox(float3,float3,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireBox (float3 center, float3 size, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.WireBox(center, size, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireBox(float3,quaternion,float3,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireBox (float3 center, quaternion rotation, float3 size, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.WireBox(center, rotation, size, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireBox(Bounds,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireBox (Bounds bounds, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.WireBox(bounds, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireMesh(Mesh,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireMesh (Mesh mesh, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.WireMesh(mesh, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireMesh(NativeArray<float3>,NativeArray<int>,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireMesh (NativeArray<float3> vertices, NativeArray<int> triangles, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.WireMesh(vertices, triangles, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::SolidMesh(Mesh,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void SolidMesh (Mesh mesh, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.SolidMesh(mesh, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Cross(float3,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Cross (float3 position, float size, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Cross(position, size, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Cross(float3,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Cross (float3 position, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Cross(position, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::CrossXZ(float3,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		[System.Obsolete("Use Draw.xz.Cross instead")]
		public static void CrossXZ (float3 position, float size, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.CrossXZ(position, size, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::CrossXZ(float3,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		[System.Obsolete("Use Draw.xz.Cross instead")]
		public static void CrossXZ (float3 position, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.CrossXZ(position, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::CrossXY(float3,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		[System.Obsolete("Use Draw.xy.Cross instead")]
		public static void CrossXY (float3 position, float size, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.CrossXY(position, size, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::CrossXY(float3,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		[System.Obsolete("Use Draw.xy.Cross instead")]
		public static void CrossXY (float3 position, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.CrossXY(position, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Bezier(float3,float3,float3,float3,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Bezier (float3 p0, float3 p1, float3 p2, float3 p3, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Bezier(p0, p1, p2, p3, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::CatmullRom(List<Vector3>,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void CatmullRom (List<Vector3> points, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.CatmullRom(points, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::CatmullRom(float3,float3,float3,float3,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void CatmullRom (float3 p0, float3 p1, float3 p2, float3 p3, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.CatmullRom(p0, p1, p2, p3, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Arrow(float3,float3,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Arrow (float3 from, float3 to, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Arrow(from, to, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Arrow(float3,float3,float3,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Arrow (float3 from, float3 to, float3 up, float headSize, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Arrow(from, to, up, headSize, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::ArrowRelativeSizeHead(float3,float3,float3,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void ArrowRelativeSizeHead (float3 from, float3 to, float3 up, float headFraction, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.ArrowRelativeSizeHead(from, to, up, headFraction, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Arrowhead(float3,float3,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Arrowhead (float3 center, float3 direction, float radius, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Arrowhead(center, direction, radius, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Arrowhead(float3,float3,float3,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Arrowhead (float3 center, float3 direction, float3 up, float radius, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Arrowhead(center, direction, up, radius, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::ArrowheadArc(float3,float3,float,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void ArrowheadArc (float3 origin, float3 direction, float offset, float width, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.ArrowheadArc(origin, direction, offset, width, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::ArrowheadArc(float3,float3,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void ArrowheadArc (float3 origin, float3 direction, float offset, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.ArrowheadArc(origin, direction, offset, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireGrid(float3,quaternion,int2,float2,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireGrid (float3 center, quaternion rotation, int2 cells, float2 totalSize, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.WireGrid(center, rotation, cells, totalSize, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireTriangle(float3,float3,float3,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireTriangle (float3 a, float3 b, float3 c, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.WireTriangle(a, b, c, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireRectangleXZ(float3,float2,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		[System.Obsolete("Use Draw.xz.WireRectangle instead")]
		public static void WireRectangleXZ (float3 center, float2 size, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.WireRectangleXZ(center, size, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireRectangle(float3,quaternion,float2,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireRectangle (float3 center, quaternion rotation, float2 size, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.WireRectangle(center, rotation, size, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireRectangle(Rect,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		[System.Obsolete("Use Draw.xy.WireRectangle instead")]
		public static void WireRectangle (Rect rect, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.WireRectangle(rect, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireTriangle(float3,quaternion,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireTriangle (float3 center, quaternion rotation, float radius, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.WireTriangle(center, rotation, radius, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WirePentagon(float3,quaternion,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WirePentagon (float3 center, quaternion rotation, float radius, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.WirePentagon(center, rotation, radius, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireHexagon(float3,quaternion,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireHexagon (float3 center, quaternion rotation, float radius, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.WireHexagon(center, rotation, radius, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WirePolygon(float3,int,quaternion,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WirePolygon (float3 center, int vertices, quaternion rotation, float radius, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.WirePolygon(center, vertices, rotation, radius, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::SolidRectangle(Rect,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		[System.Obsolete("Use Draw.xy.SolidRectangle instead")]
		public static void SolidRectangle (Rect rect, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.SolidRectangle(rect, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::SolidPlane(float3,float3,float2,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void SolidPlane (float3 center, float3 normal, float2 size, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.SolidPlane(center, normal, size, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::SolidPlane(float3,quaternion,float2,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void SolidPlane (float3 center, quaternion rotation, float2 size, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.SolidPlane(center, rotation, size, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WirePlane(float3,float3,float2,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WirePlane (float3 center, float3 normal, float2 size, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.WirePlane(center, normal, size, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WirePlane(float3,quaternion,float2,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WirePlane (float3 center, quaternion rotation, float2 size, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.WirePlane(center, rotation, size, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::PlaneWithNormal(float3,float3,float2,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void PlaneWithNormal (float3 center, float3 normal, float2 size, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.PlaneWithNormal(center, normal, size, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::PlaneWithNormal(float3,quaternion,float2,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void PlaneWithNormal (float3 center, quaternion rotation, float2 size, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.PlaneWithNormal(center, rotation, size, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::SolidTriangle(float3,float3,float3,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void SolidTriangle (float3 a, float3 b, float3 c, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.SolidTriangle(a, b, c, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::SolidBox(float3,float3,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void SolidBox (float3 center, float3 size, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.SolidBox(center, size, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::SolidBox(Bounds,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void SolidBox (Bounds bounds, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.SolidBox(bounds, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::SolidBox(float3,quaternion,float3,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void SolidBox (float3 center, quaternion rotation, float3 size, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.SolidBox(center, rotation, size, color);
#endif
}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label3D(float3,quaternion,string,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label3D (float3 position, quaternion rotation, string text, float size, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label3D(position, rotation, text, size, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label3D(float3,quaternion,string,float,LabelAlignment,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label3D (float3 position, quaternion rotation, string text, float size, LabelAlignment alignment, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label3D(position, rotation, text, size, alignment, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label2D(float3,string,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label2D (float3 position, string text, float sizeInPixels, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label2D(position, text, sizeInPixels, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label2D(float3,string,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label2D (float3 position, string text, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label2D(position, text, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label2D(float3,string,float,LabelAlignment,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label2D (float3 position, string text, float sizeInPixels, LabelAlignment alignment, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label2D(position, text, sizeInPixels, alignment, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label2D(float3,FixedString32Bytes,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label2D (float3 position, ref FixedString32Bytes text, float sizeInPixels, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label2D(position, ref text, sizeInPixels, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label2D(float3,FixedString32Bytes,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label2D (float3 position, ref FixedString32Bytes text, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label2D(position, ref text, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label2D(float3,FixedString64Bytes,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label2D (float3 position, ref FixedString64Bytes text, float sizeInPixels, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label2D(position, ref text, sizeInPixels, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label2D(float3,FixedString64Bytes,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label2D (float3 position, ref FixedString64Bytes text, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label2D(position, ref text, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label2D(float3,FixedString128Bytes,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label2D (float3 position, ref FixedString128Bytes text, float sizeInPixels, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label2D(position, ref text, sizeInPixels, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label2D(float3,FixedString128Bytes,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label2D (float3 position, ref FixedString128Bytes text, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label2D(position, ref text, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label2D(float3,FixedString512Bytes,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label2D (float3 position, ref FixedString512Bytes text, float sizeInPixels, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label2D(position, ref text, sizeInPixels, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label2D(float3,FixedString512Bytes,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label2D (float3 position, ref FixedString512Bytes text, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label2D(position, ref text, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label2D(float3,FixedString32Bytes,float,LabelAlignment,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label2D (float3 position, ref FixedString32Bytes text, float sizeInPixels, LabelAlignment alignment, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label2D(position, ref text, sizeInPixels, alignment, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label2D(float3,FixedString64Bytes,float,LabelAlignment,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label2D (float3 position, ref FixedString64Bytes text, float sizeInPixels, LabelAlignment alignment, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label2D(position, ref text, sizeInPixels, alignment, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label2D(float3,FixedString128Bytes,float,LabelAlignment,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label2D (float3 position, ref FixedString128Bytes text, float sizeInPixels, LabelAlignment alignment, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label2D(position, ref text, sizeInPixels, alignment, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label2D(float3,FixedString512Bytes,float,LabelAlignment,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label2D (float3 position, ref FixedString512Bytes text, float sizeInPixels, LabelAlignment alignment, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label2D(position, ref text, sizeInPixels, alignment, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label3D(float3,quaternion,FixedString32Bytes,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label3D (float3 position, quaternion rotation, ref FixedString32Bytes text, float size, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label3D(position, rotation, ref text, size, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label3D(float3,quaternion,FixedString64Bytes,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label3D (float3 position, quaternion rotation, ref FixedString64Bytes text, float size, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label3D(position, rotation, ref text, size, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label3D(float3,quaternion,FixedString128Bytes,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label3D (float3 position, quaternion rotation, ref FixedString128Bytes text, float size, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label3D(position, rotation, ref text, size, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label3D(float3,quaternion,FixedString512Bytes,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label3D (float3 position, quaternion rotation, ref FixedString512Bytes text, float size, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label3D(position, rotation, ref text, size, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label3D(float3,quaternion,FixedString32Bytes,float,LabelAlignment,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label3D (float3 position, quaternion rotation, ref FixedString32Bytes text, float size, LabelAlignment alignment, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label3D(position, rotation, ref text, size, alignment, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label3D(float3,quaternion,FixedString64Bytes,float,LabelAlignment,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label3D (float3 position, quaternion rotation, ref FixedString64Bytes text, float size, LabelAlignment alignment, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label3D(position, rotation, ref text, size, alignment, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label3D(float3,quaternion,FixedString128Bytes,float,LabelAlignment,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label3D (float3 position, quaternion rotation, ref FixedString128Bytes text, float size, LabelAlignment alignment, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label3D(position, rotation, ref text, size, alignment, color);
#endif
}


		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Label3D(float3,quaternion,FixedString512Bytes,float,LabelAlignment,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Label3D (float3 position, quaternion rotation, ref FixedString512Bytes text, float size, LabelAlignment alignment, Color color) {
#if UNITY_EDITOR
DrawingManager.Init();
builder.Label3D(position, rotation, ref text, size, alignment, color);
#endif
}
}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Drawing {
/// <summary>
/// Inherit from this class to draw gizmos.
/// See: getstarted (view in online documentation for working links)
/// </summary>
public abstract class MonoBehaviourGizmos : MonoBehaviour, IDrawGizmos {
public MonoBehaviourGizmos() {
#if UNITY_EDITOR
DrawingManager.Register(this);
#endif
}

		/// <summary>
		/// An empty OnDrawGizmosSelected method.
		/// Why an empty OnDrawGizmosSelected method?
		/// This is because only objects with an OnDrawGizmos/OnDrawGizmosSelected method will show up in Unity's menu for enabling/disabling
		/// the gizmos per object type (upper right corner of the scene view). So we need it here even though we don't use normal gizmos.
		///
		/// By using OnDrawGizmosSelected instead of OnDrawGizmos we minimize the overhead of Unity calling this empty method.
		/// </summary>
		void OnDrawGizmosSelected () {
		}

		/// <summary>
		/// Draw gizmos for this object.
		///
		/// The gizmos will be visible in the scene view, and the game view, if gizmos have been enabled.
		///
		/// This method will only be called in the Unity Editor.
		///
		/// See: <see cref="Draw"/>
		/// </summary>
		public virtual void DrawGizmos () {
		}
	}
}
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Drawing {
[CustomEditor(typeof(DrawingManager))]
public class DrawingManagerEditor : Editor {
// Use this for initialization
void Start () {
}

		// Update is called once per frame
		void Update () {
		}

		void OnSceneGUI () {
		}
	}
}
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Drawing {
/// <summary>Helper for adding project settings</summary>
static class ALINESettingsRegister {
const string PROVIDER_PATH = "Project/ALINE";
const string SETTINGS_LABEL = "ALINE";


		[SettingsProvider]
		public static SettingsProvider CreateMyCustomSettingsProvider () {
			// First parameter is the path in the Settings window.
			// Second parameter is the scope of this setting: it only appears in the Project Settings window.
			var provider = new SettingsProvider(PROVIDER_PATH, SettingsScope.Project) {
				// By default the last token of the path is used as display name if no label is provided.
				label = SETTINGS_LABEL,
				guiHandler = (searchContext) =>
				{
					var settings = new SerializedObject(DrawingSettings.GetSettingsAsset());
					EditorGUILayout.HelpBox("Opacity of lines, solid objects and text drawn using ALINE. When drawing behind other objects, an additional opacity multiplier is applied.", MessageType.None);
					EditorGUILayout.Separator();
					EditorGUILayout.LabelField("Lines", EditorStyles.boldLabel);
					EditorGUILayout.Slider(settings.FindProperty("settings.lineOpacity"), 0, 1, new GUIContent("Opacity", "Opacity of lines when in front of objects"));
					EditorGUILayout.Slider(settings.FindProperty("settings.lineOpacityBehindObjects"), 0, 1, new GUIContent("Opacity (occluded)", "Additional opacity multiplier of lines when behind or inside objects"));
					EditorGUILayout.Separator();
					EditorGUILayout.LabelField("Solids", EditorStyles.boldLabel);
					EditorGUILayout.Slider(settings.FindProperty("settings.solidOpacity"), 0, 1, new GUIContent("Opacity", "Opacity of solid objects when in front of other objects"));
					EditorGUILayout.Slider(settings.FindProperty("settings.solidOpacityBehindObjects"), 0, 1, new GUIContent("Opacity (occluded)", "Additional opacity multiplier of solid objects when behind or inside other objects"));
					EditorGUILayout.Separator();
					EditorGUILayout.LabelField("Text", EditorStyles.boldLabel);
					EditorGUILayout.Slider(settings.FindProperty("settings.textOpacity"), 0, 1, new GUIContent("Opacity", "Opacity of text when in front of other objects"));
					EditorGUILayout.Slider(settings.FindProperty("settings.textOpacityBehindObjects"), 0, 1, new GUIContent("Opacity (occluded)", "Additional opacity multiplier of text when behind or inside other objects"));
					EditorGUILayout.Separator();
					EditorGUILayout.Slider(settings.FindProperty("settings.curveResolution"), 0.1f, 3f, new GUIContent("Curve resolution", "Higher values will make curves smoother, but also a bit slower to draw."));

					settings.ApplyModifiedProperties();
					if (GUILayout.Button("Reset to default")) {
						var def = DrawingSettings.DefaultSettings;
						var current = DrawingSettings.GetSettingsAsset();
						current.settings.lineOpacity = def.lineOpacity;
						current.settings.lineOpacityBehindObjects = def.lineOpacityBehindObjects;
						current.settings.solidOpacity = def.solidOpacity;
						current.settings.solidOpacityBehindObjects = def.solidOpacityBehindObjects;
						current.settings.textOpacity = def.textOpacity;
						current.settings.textOpacityBehindObjects = def.textOpacityBehindObjects;
						current.settings.curveResolution = def.curveResolution;
						EditorUtility.SetDirty(current);
					}
				},

				// Populate the search keywords to enable smart search filtering and label highlighting:
				keywords = new HashSet<string>(new[] { "Drawing", "Wire", "aline", "opacity" })
			};

			return provider;
		}
	}
}
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using static Drawing.CommandBuilder;

namespace Drawing {
/// <summary>
/// 2D wrapper for a <see cref="CommandBuilder"/>.
///
/// <code>
/// var p1 = new Vector2(0, 1);
/// var p2 = new Vector2(5, 7);
///
/// // Draw it in the XY plane
/// Draw.xy.Line(p1, p2);
///
/// // Draw it in the XZ plane
/// Draw.xz.Line(p1, p2);
/// </code>
///
/// See: 2d-drawing (view in online documentation for working links)
/// See: <see cref="Draw.xy"/>
/// See: <see cref="Draw.xz"/>
/// </summary>
public partial struct CommandBuilder2D {
/// <summary>The wrapped command builder</summary>
private CommandBuilder draw;
/// <summary>True if drawing in the XY plane, false if drawing in the XZ plane</summary>
bool xy;

		static readonly float3 XY_UP = new float3(0, 0, 1);
		static readonly float3 XZ_UP = new float3(0, 1, 0);
		static readonly quaternion XY_TO_XZ_ROTATION =  quaternion.RotateX(-math.PI*0.5f);
		static readonly quaternion XZ_TO_XZ_ROTATION =  quaternion.identity;
		static readonly float4x4 XZ_TO_XY_MATRIX = new float4x4(new float4(1, 0, 0, 0), new float4(0, 0, 1, 0), new float4(0, 1, 0, 0), new float4(0, 0, 0, 1));

		public CommandBuilder2D(CommandBuilder draw, bool xy) {
			this.draw = draw;
			this.xy = xy;
		}

		/// <summary>
		/// Draws a line between two points.
		///
		/// [Open online documentation to see images]
		///
		/// <code>
		/// void Update () {
		///     Draw.Line(Vector3.zero, Vector3.up);
		/// }
		/// </code>
		/// </summary>
		public void Line (float2 a, float2 b) {
			draw.Reserve<LineData>();
			// Add(Command.Line);
			// Add(new LineData { a = a, b = b });

			// The code below is equivalent to the commented out code above.
			// But drawing lines is the most common operation so it needs to be really fast.
			// Having this hardcoded improves line rendering performance by about 8%.
			unsafe {
				var buffer = draw.buffer;
				var bufferSize = buffer->Length;
				var newLen = bufferSize + 4 + 24;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
UnityEngine.Assertions.Assert.IsTrue(newLen <= buffer->Capacity);
#endif
var ptr = (byte*)buffer->Ptr + bufferSize;
*(Command*)ptr = Command.Line;
var lineData = (LineData*)(ptr + 4);
if (xy) {
lineData->a = new float3(a, 0);
lineData->b = new float3(b, 0);
} else {
lineData->a = new float3(a.x, 0, a.y);
lineData->b = new float3(b.x, 0, b.y);
}
buffer->Length = newLen;
}
}

		/// <summary>
		/// Draws a line between two points.
		///
		/// [Open online documentation to see images]
		///
		/// <code>
		/// void Update () {
		///     Draw.Line(Vector3.zero, Vector3.up);
		/// }
		/// </code>
		/// </summary>
		public void Line (float2 a, float2 b, Color color) {
			draw.Reserve<Color32, LineData>();
			// Add(Command.Line);
			// Add(new LineData { a = a, b = b });

			// The code below is equivalent to the commented out code above.
			// But drawing lines is the most common operation so it needs to be really fast.
			// Having this hardcoded improves line rendering performance by about 8%.
			unsafe {
				var buffer = draw.buffer;
				var bufferSize = buffer->Length;
				var newLen = bufferSize + 4 + 24 + 4;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
UnityEngine.Assertions.Assert.IsTrue(newLen <= buffer->Capacity);
#endif
var ptr = (byte*)buffer->Ptr + bufferSize;
*(Command*)ptr = Command.Line | Command.PushColorInline;
*(uint*)(ptr + 4) = CommandBuilder.ConvertColor(color);
var lineData = (LineData*)(ptr + 8);
if (xy) {
lineData->a = new float3(a, 0);
lineData->b = new float3(b, 0);
} else {
lineData->a = new float3(a.x, 0, a.y);
lineData->b = new float3(b.x, 0, b.y);
}
buffer->Length = newLen;
}
}

		/// <summary>
		/// Draws a line between two points.
		///
		/// [Open online documentation to see images]
		///
		/// <code>
		/// void Update () {
		///     Draw.Line(Vector3.zero, Vector3.up);
		/// }
		/// </code>
		/// </summary>
		public void Line (float3 a, float3 b) {
			draw.Line(a, b);
		}

		/// <summary>
		/// Draws a circle.
		///
		/// You can draw an arc by supplying the startAngle and endAngle parameters.
		///
		/// [Open online documentation to see images]
		///
		/// See: <see cref="Circle(float3,float,float,float)"/>
		/// See: <see cref="Arc(float3,float3,float3)"/>
		/// </summary>
		/// <param name="center">Center of the circle or arc.</param>
		/// <param name="radius">Radius of the circle or arc.</param>
		/// <param name="startAngle">Starting angle in radians. 0 corrsponds to the positive X axis.</param>
		/// <param name="endAngle">End angle in radians.</param>
		public void Circle (float2 center, float radius, float startAngle = 0f, float endAngle = 2 * math.PI) {
			Circle(xy ? new float3(center, 0) : new float3(center.x, 0, center.y), radius, startAngle, endAngle);
		}

		/// <summary>
		/// Draws a circle.
		///
		/// You can draw an arc by supplying the startAngle and endAngle parameters.
		///
		/// [Open online documentation to see images]
		///
		/// See: <see cref="CommandBuilder.Circle(float3,float3,float)"/>
		/// See: <see cref="Arc(float3,float3,float3)"/>
		/// </summary>
		/// <param name="center">Center of the circle or arc.</param>
		/// <param name="radius">Radius of the circle or arc.</param>
		/// <param name="startAngle">Starting angle in radians. 0 corrsponds to the positive X axis.</param>
		/// <param name="endAngle">End angle in radians.</param>
		public void Circle (float3 center, float radius, float startAngle = 0f, float endAngle = 2 * math.PI) {
			if (xy) {
				draw.PushMatrix(XZ_TO_XY_MATRIX);
				draw.CircleXZInternal(new float3(center.x, center.z, center.y), radius, startAngle, endAngle);
				draw.PopMatrix();
			} else {
				draw.CircleXZInternal(center, radius, startAngle, endAngle);
			}
		}

		/// <summary>\copydocref{SolidCircle(float3,float,float,float)}</summary>
		public void SolidCircle (float2 center, float radius, float startAngle = 0f, float endAngle = 2 * math.PI) {
			SolidCircle(xy ? new float3(center, 0) : new float3(center.x, 0, center.y), radius, startAngle, endAngle);
		}

		/// <summary>
		/// Draws a disc.
		///
		/// You can draw an arc by supplying the startAngle and endAngle parameters.
		///
		/// [Open online documentation to see images]
		///
		/// See: <see cref="Draw.SolidCircle(float3,float3,float)"/>
		/// See: <see cref="SolidArc(float3,float3,float3)"/>
		/// </summary>
		/// <param name="center">Center of the disc or solid arc.</param>
		/// <param name="radius">Radius of the disc or solid arc.</param>
		/// <param name="startAngle">Starting angle in radians. 0 corrsponds to the positive X axis.</param>
		/// <param name="endAngle">End angle in radians.</param>
		public void SolidCircle (float3 center, float radius, float startAngle = 0f, float endAngle = 2 * math.PI) {
			if (xy) draw.PushMatrix(XZ_TO_XY_MATRIX);
			draw.SolidCircleXZInternal(xy ? new float3(center.x, center.z, center.y) : center, radius, startAngle, endAngle);
			if (xy) draw.PopMatrix();
		}

		/// <summary>
		/// Draws a wire pill in 2D.
		///
		/// <code>
		/// Draw.xy.WirePill(new float2(-0.5f, -0.5f), new float2(0.5f, 0.5f), 0.5f, color);
		/// </code>
		///
		/// [Open online documentation to see images]
		///
		/// See: <see cref="WirePill(float2,float2,float,float)"/>
		/// </summary>
		/// <param name="a">Center of the first circle of the capsule.</param>
		/// <param name="b">Center of the second circle of the capsule.</param>
		/// <param name="radius">Radius of the capsule.</param>
		public void WirePill (float2 a, float2 b, float radius) {
			WirePill(a, b - a, math.length(b - a), radius);
		}

		/// <summary>
		/// Draws a wire pill in 2D.
		///
		/// <code>
		/// Draw.xy.WirePill(new float2(-0.5f, -0.5f), new float2(1, 1), 1, 0.5f, color);
		/// </code>
		///
		/// [Open online documentation to see images]
		///
		/// See: <see cref="WirePill(float2,float2,float)"/>
		/// </summary>
		/// <param name="position">Center of the first circle of the capsule.</param>
		/// <param name="direction">The main axis of the capsule. Does not have to be normalized. If zero, a circle will be drawn.</param>
		/// <param name="length">Length of the main axis of the capsule, from circle center to circle center. If zero, a circle will be drawn.</param>
		/// <param name="radius">Radius of the capsule.</param>
		public void WirePill (float2 position, float2 direction, float length, float radius) {
			direction = math.normalizesafe(direction);

			if (radius <= 0) {
				Line(position, position + direction * length);
			} else if (length <= 0 || math.all(direction == 0)) {
				Circle(position, radius);
			} else {
				float4x4 m;
				if (xy) {
					m = new float4x4(
						new float4(direction, 0, 0),
						new float4(math.cross(new float3(direction, 0), XY_UP), 0),
						new float4(0, 0, 1, 0),
						new float4(position, 0, 1)
						);
				} else {
					m = new float4x4(
						new float4(direction.x, 0, direction.y, 0),
						new float4(0, 1, 0, 0),
						new float4(math.cross(new float3(direction.x, 0, direction.y), XZ_UP), 0),
						new float4(position.x, 0, position.y, 1)
						);
				}
				draw.PushMatrix(m);
				Circle(new float2(0, 0), radius, 0.5f * math.PI, 1.5f * math.PI);
				Line(new float2(0, -radius), new float2(length, -radius));
				Circle(new float2(length, 0), radius, -0.5f * math.PI, 0.5f * math.PI);
				Line(new float2(0, radius), new float2(length, radius));
				draw.PopMatrix();
			}
		}

		/// <summary>\copydocref{CommandBuilder.Polyline(List<Vector3>,bool)}</summary>
		[BurstDiscard]
		public void Polyline (List<Vector2> points, bool cycle = false) {
			for (int i = 0; i < points.Count - 1; i++) {
				Line(points[i], points[i+1]);
			}
			if (cycle && points.Count > 1) Line(points[points.Count - 1], points[0]);
		}

		/// <summary>\copydocref{CommandBuilder.Polyline(Vector3[],bool)}</summary>
		[BurstDiscard]
		public void Polyline (Vector2[] points, bool cycle = false) {
			for (int i = 0; i < points.Length - 1; i++) {
				Line(points[i], points[i+1]);
			}
			if (cycle && points.Length > 1) Line(points[points.Length - 1], points[0]);
		}

		/// <summary>\copydocref{CommandBuilder.Polyline(float3[],bool)}</summary>
		[BurstDiscard]
		public void Polyline (float2[] points, bool cycle = false) {
			for (int i = 0; i < points.Length - 1; i++) {
				Line(points[i], points[i+1]);
			}
			if (cycle && points.Length > 1) Line(points[points.Length - 1], points[0]);
		}

		/// <summary>\copydocref{CommandBuilder.Polyline(NativeArray<float3>,bool)}</summary>
		public void Polyline (NativeArray<float2> points, bool cycle = false) {
			for (int i = 0; i < points.Length - 1; i++) {
				Line(points[i], points[i+1]);
			}
			if (cycle && points.Length > 1) Line(points[points.Length - 1], points[0]);
		}

		/// <summary>
		/// Draws a 2D cross.
		///
		/// <code>
		/// Draw.xz.Cross(float3.zero, color);
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: <see cref="Draw.Cross"/>
		/// </summary>
		public void Cross (float2 position, float size = 1) {
			size *= 0.5f;
			Line(position - new float2(size, 0), position + new float2(size, 0));
			Line(position - new float2(0, size), position + new float2(0, size));
		}

		/// <summary>
		/// Draws a rectangle outline.
		/// The rectangle will be oriented along the rotation's X and Z axes.
		///
		/// <code>
		/// Draw.xz.WireRectangle(new Vector3(0f, 0, 0), new Vector2(1, 1), Color.black);
		/// </code>
		/// [Open online documentation to see images]
		///
		/// This is identical to <see cref="Draw.WirePlane(float3,quaternion,float2)"/>, but this name is added for consistency.
		///
		/// See: <see cref="Draw.WirePolygon"/>
		/// </summary>
		public void WireRectangle (float3 center, float2 size) {
			draw.WirePlane(center, xy ? XY_TO_XZ_ROTATION : XZ_TO_XZ_ROTATION, size);
		}

		/// <summary>
		/// Draws a rectangle outline.
		/// This is particularly useful when combined with <see cref="InScreenSpace"/>.
		///
		/// <code>
		/// using (Draw.InScreenSpace(Camera.main)) {
		///     Draw.xy.WireRectangle(new Rect(10, 10, 100, 100), Color.black);
		/// }
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: <see cref="Draw.WireRectangle(float3,quaternion,float2)"/>
		/// See: <see cref="Draw.WirePolygon"/>
		/// </summary>
		public void WireRectangle (Rect rect) {
			float2 min = rect.min;
			float2 max = rect.max;

			Line(new float2(min.x, min.y), new float2(max.x, min.y));
			Line(new float2(max.x, min.y), new float2(max.x, max.y));
			Line(new float2(max.x, max.y), new float2(min.x, max.y));
			Line(new float2(min.x, max.y), new float2(min.x, min.y));
		}

		/// <summary>
		/// Draws a solid rectangle.
		/// This is particularly useful when combined with <see cref="InScreenSpace"/>.
		///
		/// Behind the scenes this is implemented using <see cref="Draw.SolidPlane"/>.
		///
		/// <code>
		/// using (Draw.InScreenSpace(Camera.main)) {
		///     Draw.xy.SolidRectangle(new Rect(10, 10, 100, 100), Color.black);
		/// }
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: <see cref="WireRectangle"/>
		/// See: <see cref="Draw.WireRectangle(float3,quaternion,float2)"/>
		/// See: <see cref="Draw.SolidBox"/>
		/// </summary>
		public void SolidRectangle (Rect rect) {
			draw.SolidPlane(xy ? new float3(rect.center.x, rect.center.y, 0.0f) : new float3(rect.center.x, 0, rect.center.y), xy ? XY_TO_XZ_ROTATION : XZ_TO_XZ_ROTATION, new float2(rect.width, rect.height));
		}

		/// <summary>
		/// Draws a grid of lines.
		///
		/// <code>
		/// Draw.xz.WireGrid(Vector3.zero, new int2(3, 3), new float2(1, 1), color);
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: <see cref="Draw.WireGrid"/>
		/// </summary>
		/// <param name="center">Center of the grid</param>
		/// <param name="cells">Number of cells of the grid. Should be greater than 0.</param>
		/// <param name="totalSize">Total size of the grid along the X and Z axes.</param>
		public void WireGrid (float2 center, int2 cells, float2 totalSize) {
			draw.WireGrid(xy ? new float3(center, 0) : new float3(center.x, 0, center.y), xy ? XY_TO_XZ_ROTATION : XZ_TO_XZ_ROTATION, cells, totalSize);
		}

		/// <summary>
		/// Draws a grid of lines.
		///
		/// <code>
		/// Draw.xz.WireGrid(Vector3.zero, new int2(3, 3), new float2(1, 1), color);
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: <see cref="Draw.WireGrid"/>
		/// </summary>
		/// <param name="center">Center of the grid</param>
		/// <param name="cells">Number of cells of the grid. Should be greater than 0.</param>
		/// <param name="totalSize">Total size of the grid along the X and Z axes.</param>
		public void WireGrid (float3 center, int2 cells, float2 totalSize) {
			draw.WireGrid(center, xy ? XY_TO_XZ_ROTATION : XZ_TO_XZ_ROTATION, cells, totalSize);
		}
	}
}
// This file has been removed from the package. Since UnityPackages cannot
// delete files, only replace them, this message is left here to prevent old
// files from causing compiler errors.
// TODO: Check HDRP custom pass support, and log a warning if it is disabled
#pragma warning disable 649 // Field `Drawing.GizmoContext.activeTransform' is never assigned to, and will always have its default value `null'. Not used outside of the unity editor.
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine.Rendering;
using Unity.Profiling;
#if MODULE_RENDER_PIPELINES_UNIVERSAL
using UnityEngine.Rendering.Universal;
#endif
#if MODULE_RENDER_PIPELINES_HIGH_DEFINITION
using UnityEngine.Rendering.HighDefinition;
#endif

namespace Drawing {
/// <summary>Info about the current selection in the editor</summary>
public static class GizmoContext {
#if UNITY_EDITOR
static Transform activeTransform;
#endif

		static HashSet<Transform> selectedTransforms = new HashSet<Transform>();

		static internal bool drawingGizmos;
		static internal bool dirty;
		private static int selectionSizeInternal;

		/// <summary>Number of top-level transforms that are selected</summary>
		public static int selectionSize {
			get {
				Refresh();
				return selectionSizeInternal;
			}
			private set {
				selectionSizeInternal = value;
			}
		}

		internal static void SetDirty () {
			dirty = true;
		}

		private static void Refresh () {
#if UNITY_EDITOR
if (!drawingGizmos) throw new System.Exception("Can only be used inside the ALINE library's gizmo drawing functions.");
if (dirty) {
dirty = false;
DrawingManager.MarkerRefreshSelectionCache.Begin();
activeTransform = Selection.activeTransform;
selectedTransforms.Clear();
var topLevel = Selection.transforms;
for (int i = 0; i < topLevel.Length; i++) selectedTransforms.Add(topLevel[i]);
selectionSize = topLevel.Length;
DrawingManager.MarkerRefreshSelectionCache.End();
}
#endif
}

		/// <summary>
		/// True if the component is selected.
		/// This is a deep selection: even children of selected transforms are considered to be selected.
		/// </summary>
		public static bool InSelection (Component c) {
			return InSelection(c.transform);
		}

		/// <summary>
		/// True if the transform is selected.
		/// This is a deep selection: even children of selected transforms are considered to be selected.
		/// </summary>
		public static bool InSelection (Transform tr) {
			Refresh();
			var leaf = tr;
			while (tr != null) {
				if (selectedTransforms.Contains(tr)) {
					selectedTransforms.Add(leaf);
					return true;
				}
				tr = tr.parent;
			}
			return false;
		}

		/// <summary>
		/// True if the component is shown in the inspector.
		/// The active selection is the GameObject that is currently visible in the inspector.
		/// </summary>
		public static bool InActiveSelection (Component c) {
			return InActiveSelection(c.transform);
		}

		/// <summary>
		/// True if the transform is shown in the inspector.
		/// The active selection is the GameObject that is currently visible in the inspector.
		/// </summary>
		public static bool InActiveSelection (Transform tr) {
#if UNITY_EDITOR
Refresh();
return tr.transform == activeTransform;
#else
return false;
#endif
}
}

	/// <summary>
	/// Every object that wants to draw gizmos should implement this interface.
	/// See: <see cref="Drawing.MonoBehaviourGizmos"/>
	/// </summary>
	public interface IDrawGizmos {
		void DrawGizmos();
	}

	public enum DetectedRenderPipeline {
		BuiltInOrCustom,
		HDRP,
		URP
	}

	/// <summary>
	/// Global script which draws debug items and gizmos.
	/// If a Draw.* method has been used or if any script inheriting from the <see cref="Drawing.MonoBehaviourGizmos"/> class is in the scene then an instance of this script
	/// will be created and put on a hidden GameObject.
	///
	/// It will inject drawing logic into any cameras that are rendered.
	///
	/// Usually you never have to interact with this class.
	/// </summary>
	[ExecuteAlways]
	[AddComponentMenu("")]
	[HelpURL("http://arongranberg.com/aline/documentation/stable/drawingmanager.html")]
	public class DrawingManager : MonoBehaviour {
		public DrawingData gizmos;
		static List<GizmoDrawerGroup> gizmoDrawers = new List<GizmoDrawerGroup>();
		static Dictionary<System.Type, int> gizmoDrawerIndices = new Dictionary<System.Type, int>();
		static bool ignoreAllDrawing;
		static DrawingManager _instance;
		bool framePassed;
		int lastFrameCount = int.MinValue;
		float lastFrameTime = -float.NegativeInfinity;
		int lastFilterFrame;
#if UNITY_EDITOR
bool builtGizmos;
#endif

		struct GizmoDrawerGroup {
			public System.Type type;
			public ProfilerMarker profilerMarker;
			public List<IDrawGizmos> drawers;
			public bool enabled;
		}

		/// <summary>True if OnEnable has been called on this instance and OnDisable has not</summary>
		[SerializeField]
		bool actuallyEnabled;

		RedrawScope previousFrameRedrawScope;

		/// <summary>
		/// Allow rendering to cameras that render to RenderTextures.
		/// By default cameras which render to render textures are never rendered to.
		/// You may enable this if you wish.
		///
		/// See: <see cref="Drawing.CommandBuilder.cameraTargets"/>
		/// See: advanced (view in online documentation for working links)
		/// </summary>
		public static bool allowRenderToRenderTextures = false;
		public static bool drawToAllCameras = false;

		/// <summary>
		/// Multiply all line widths by this value.
		/// This can be used to make lines thicker or thinner.
		///
		/// This is primarily useful when generating screenshots, and you want to render at a higher resolution before scaling down the image.
		///
		/// It is only read when a camera is being rendered. So it cannot be used to change line thickness on a per-item basis.
		/// Use <see cref="Draw.WithLineWidth"/> for that.
		/// </summary>
		public static float lineWidthMultiplier = 1.0f;

		CommandBuffer commandBuffer;

		[System.NonSerialized]
		DetectedRenderPipeline detectedRenderPipeline = DetectedRenderPipeline.BuiltInOrCustom;

#if MODULE_RENDER_PIPELINES_HIGH_DEFINITION_16_0_0_OR_NEWER
CustomPass hdrpGlobalPass;
#endif

#if MODULE_RENDER_PIPELINES_UNIVERSAL
HashSet<ScriptableRenderer> scriptableRenderersWithPass = new HashSet<ScriptableRenderer>();
AlineURPRenderPassFeature renderPassFeature;
#endif

		private static readonly ProfilerMarker MarkerALINE = new ProfilerMarker("ALINE");
		private static readonly ProfilerMarker MarkerCommandBuffer = new ProfilerMarker("Executing command buffer");
		private static readonly ProfilerMarker MarkerFrameTick = new ProfilerMarker("Frame Tick");
		private static readonly ProfilerMarker MarkerFilterDestroyedObjects = new ProfilerMarker("Filter destroyed objects");
		internal static readonly ProfilerMarker MarkerRefreshSelectionCache = new ProfilerMarker("Refresh Selection Cache");
		private static readonly ProfilerMarker MarkerGizmosAllowed = new ProfilerMarker("GizmosAllowed");
		private static readonly ProfilerMarker MarkerDrawGizmos = new ProfilerMarker("DrawGizmos");
		private static readonly ProfilerMarker MarkerSubmitGizmos = new ProfilerMarker("Submit Gizmos");

		public static DrawingManager instance {
			get {
				if (_instance == null) Init();
				return _instance;
			}
		}

#if UNITY_EDITOR
[InitializeOnLoadMethod]
#endif
public static void Init () {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
if (Unity.Jobs.LowLevel.Unsafe.JobsUtility.IsExecutingJob) throw new System.Exception("Draw.* methods cannot be called from inside a job. See the documentation for info about how to use drawing functions from the Unity Job System.");
#endif
if (_instance != null) return;

			// Here one might try to look for existing instances of the class that haven't yet been enabled.
			// However, this turns out to be tricky.
			// Resources.FindObjectsOfTypeAll<T>() is the only call that includes HideInInspector GameObjects.
			// But it is hard to distinguish between objects that are internal ones which will never be enabled and objects that will be enabled.
			// Checking .gameObject.scene.isLoaded doesn't work reliably (object may be enabled and working even if isLoaded is false)
			// Checking .gameObject.scene.isValid doesn't work reliably (object may be enabled and working even if isValid is false)

			// So instead we just always create a new instance. This is not a particularly heavy operation and it only happens once per game, so why not.
			// The OnEnable call will clean up duplicate managers if there are any.

			var go = new GameObject("RetainedGizmos") {
				hideFlags = HideFlags.DontSave | HideFlags.NotEditable | HideFlags.HideInInspector | HideFlags.HideInHierarchy
			};
			_instance = go.AddComponent<DrawingManager>();
			if (Application.isPlaying) DontDestroyOnLoad(go);

			if (Application.isBatchMode) {
				// In batch mode, we never want to draw anything.
				// See https://forum.arongranberg.com/t/drawingmanager-holds-on-to-memory-in-batch-mode/17765
				ignoreAllDrawing = true;
				gizmoDrawers.Clear();
				gizmoDrawerIndices.Clear();
			}
		}

		/// <summary>Detects which render pipeline is being used and configures them for rendering</summary>
		void RefreshRenderPipelineMode () {
			var pipelineType = RenderPipelineManager.currentPipeline != null? RenderPipelineManager.currentPipeline.GetType() : null;

#if MODULE_RENDER_PIPELINES_HIGH_DEFINITION
if (pipelineType == typeof(HDRenderPipeline)) {
if (detectedRenderPipeline != DetectedRenderPipeline.HDRP) {
detectedRenderPipeline = DetectedRenderPipeline.HDRP;
#if MODULE_RENDER_PIPELINES_HIGH_DEFINITION_16_0_0_OR_NEWER
UnityEngine.Assertions.Assert.IsNull(hdrpGlobalPass);
hdrpGlobalPass = new AlineHDRPCustomPass();
CustomPassVolume.RegisterGlobalCustomPass(CustomPassInjectionPoint.AfterPostProcess, hdrpGlobalPass);
#else
if (!_instance.gameObject.TryGetComponent<CustomPassVolume>(out CustomPassVolume volume)) {
volume = _instance.gameObject.AddComponent<CustomPassVolume>();
volume.isGlobal = true;
volume.injectionPoint = CustomPassInjectionPoint.AfterPostProcess;
volume.customPasses.Add(new AlineHDRPCustomPass());
}
#endif

					var asset = GraphicsSettings.defaultRenderPipeline as HDRenderPipelineAsset;
					if (asset != null) {
						if (!asset.currentPlatformRenderPipelineSettings.supportCustomPass) {
							Debug.LogWarning("ALINE: The current render pipeline has custom pass support disabled. ALINE will not be able to render anything. Please enable custom pass support on your HDRenderPipelineAsset.", asset);
						}
					}
				}

#if UNITY_ASSERTIONS && MODULE_RENDER_PIPELINES_HIGH_DEFINITION_16_0_0_OR_NEWER
var globalPasses = CustomPassVolume.GetGlobalCustomPasses(CustomPassInjectionPoint.AfterPostProcess);
bool found = false;
for (int i = 0; i < globalPasses.Count; i++) found |= globalPasses[i].instance == hdrpGlobalPass;
UnityEngine.Assertions.Assert.IsTrue(found, "Custom pass for gizmos is not registered. Have the custom passes been forcefully removed by another script?");
#endif
return;
}
#if MODULE_RENDER_PIPELINES_HIGH_DEFINITION_16_0_0_OR_NEWER
if (hdrpGlobalPass != null) {
CustomPassVolume.UnregisterGlobalCustomPass(CustomPassInjectionPoint.AfterPostProcess, hdrpGlobalPass);
hdrpGlobalPass = null;
}
#endif
#endif

#if MODULE_RENDER_PIPELINES_UNIVERSAL
if (pipelineType == typeof(UniversalRenderPipeline)) {
detectedRenderPipeline = DetectedRenderPipeline.URP;
return;
}
#endif
detectedRenderPipeline = DetectedRenderPipeline.BuiltInOrCustom;
}

#if UNITY_EDITOR
void DelayedDestroy () {
EditorApplication.update -= DelayedDestroy;
// Check if the object still exists (it might have been destroyed in some other way already).
if (gameObject) GameObject.DestroyImmediate(gameObject);
}

		void OnPlayModeStateChanged (PlayModeStateChange change) {
			if (change == PlayModeStateChange.ExitingEditMode || change == PlayModeStateChange.ExitingPlayMode) {
				gizmos.OnChangingPlayMode();
			}
		}
#endif

		void OnEnable () {
			if (_instance == null) _instance = this;

			// Ensure we don't have duplicate managers
			if (_instance != this) {
				// We cannot destroy the object while it is being enabled, so we need to delay it a bit
#if UNITY_EDITOR
// This is only important in the editor to avoid a build-up of old managers.
// In an actual game at most 1 (though in practice zero) old managers will be laying around.
// It would be nice to use a coroutine for this instead, but unfortunately they do not work for objects marked with HideAndDontSave.
EditorApplication.update += DelayedDestroy;
#endif
return;
}

			actuallyEnabled = true;
			if (gizmos == null) gizmos = new DrawingData();
			gizmos.frameRedrawScope = new RedrawScope(gizmos);
			Draw.builder = gizmos.GetBuiltInBuilder(false);
			Draw.ingame_builder = gizmos.GetBuiltInBuilder(true);
			commandBuffer = new CommandBuffer();
			commandBuffer.name = "ALINE Gizmos";

			detectedRenderPipeline = DetectedRenderPipeline.BuiltInOrCustom;

			// Callback when rendering with the built-in render pipeline
			Camera.onPostRender += PostRender;
			// Callback when rendering with a scriptable render pipeline
#if UNITY_2021_1_OR_NEWER
UnityEngine.Rendering.RenderPipelineManager.beginContextRendering += BeginContextRendering;
#else
UnityEngine.Rendering.RenderPipelineManager.beginFrameRendering += BeginFrameRendering;
#endif
UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering += BeginCameraRendering;
UnityEngine.Rendering.RenderPipelineManager.endCameraRendering += EndCameraRendering;
#if UNITY_EDITOR
EditorApplication.update += OnEditorUpdate;
EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
}

		void BeginContextRendering (ScriptableRenderContext context, List<Camera> cameras) {
			RefreshRenderPipelineMode();
		}

		void BeginFrameRendering (ScriptableRenderContext context, Camera[] cameras) {
			RefreshRenderPipelineMode();
		}

		void BeginCameraRendering (ScriptableRenderContext context, Camera camera) {
#if MODULE_RENDER_PIPELINES_UNIVERSAL
if (detectedRenderPipeline == DetectedRenderPipeline.URP) {
var data = camera.GetUniversalAdditionalCameraData();
if (data != null) {
var renderer = data.scriptableRenderer;
if (renderPassFeature == null) {
renderPassFeature = ScriptableObject.CreateInstance<AlineURPRenderPassFeature>();
}
renderPassFeature.AddRenderPasses(renderer);
}
}
#endif
}

		void OnDisable () {
			if (!actuallyEnabled) return;
			actuallyEnabled = false;
			commandBuffer.Dispose();
			commandBuffer = null;
			Camera.onPostRender -= PostRender;
#if UNITY_2021_1_OR_NEWER
UnityEngine.Rendering.RenderPipelineManager.beginContextRendering -= BeginContextRendering;
#else
UnityEngine.Rendering.RenderPipelineManager.beginFrameRendering -= BeginFrameRendering;
#endif
UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering -= BeginCameraRendering;
UnityEngine.Rendering.RenderPipelineManager.endCameraRendering -= EndCameraRendering;
#if UNITY_EDITOR
EditorApplication.update -= OnEditorUpdate;
EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif
// Gizmos can be null here if this GameObject was duplicated by a user in the hierarchy.
if (gizmos != null) {
Draw.builder.DiscardAndDisposeInternal();
Draw.ingame_builder.DiscardAndDisposeInternal();
gizmos.ClearData();
}
#if MODULE_RENDER_PIPELINES_UNIVERSAL
if (renderPassFeature != null) {
ScriptableObject.DestroyImmediate(renderPassFeature);
renderPassFeature = null;
}
#endif
#if MODULE_RENDER_PIPELINES_HIGH_DEFINITION_16_0_0_OR_NEWER
if (hdrpGlobalPass != null) {
CustomPassVolume.UnregisterGlobalCustomPass(CustomPassInjectionPoint.AfterPostProcess, hdrpGlobalPass);
hdrpGlobalPass = null;
}
#endif
}

		// When enter play mode = reload scene & reload domain
		//	editor => play mode: OnDisable -> OnEnable (same object)
		//  play mode => editor: OnApplicationQuit (note: no OnDisable/OnEnable)
		// When enter play mode = reload scene & !reload domain
		//	editor => play mode: Nothing
		//  play mode => editor: OnApplicationQuit
		// When enter play mode = !reload scene & !reload domain
		//	editor => play mode: Nothing
		//  play mode => editor: OnApplicationQuit
		// OnDestroy is never really called for this object (unless Unity or the game quits I quess)

		// TODO: Should run in OnDestroy. OnApplicationQuit runs BEFORE OnDestroy (which we do not want)
		// private void OnApplicationQuit () {
		// Debug.Log("OnApplicationQuit");
		// Draw.builder.DiscardAndDisposeInternal();
		// Draw.ingame_builder.DiscardAndDisposeInternal();
		// gizmos.ClearData();
		// Draw.builder = gizmos.GetBuiltInBuilder(false);
		// Draw.ingame_builder = gizmos.GetBuiltInBuilder(true);
		// }

		const float NO_DRAWING_TIMEOUT_SECS = 10;

		void OnEditorUpdate () {
			framePassed = true;
			CleanupIfNoCameraRendered();
		}

		void Update () {
			if (actuallyEnabled) CleanupIfNoCameraRendered();
		}

		void CleanupIfNoCameraRendered () {
			if (Time.frameCount > lastFrameCount + 1) {
				// More than one frame old
				// It is possible no camera is being rendered at all.
				// Ensure we don't get any memory leaks from drawing items being queued every frame.
				CheckFrameTicking();
				gizmos.PostRenderCleanup();

				// Note: We do not always want to call the above method here
				// because it is nicer to call it right after the cameras have been rendered.
				// Otherwise drawing items queued before Update/OnEditorUpdate or after Update/OnEditorUpdate may end up
				// in different frames (for the purposes of rendering gizmos)
			}

			if (Time.realtimeSinceStartup - lastFrameTime > NO_DRAWING_TIMEOUT_SECS) {
				// More than NO_DRAWING_TIMEOUT_SECS seconds since we drew the last frame.
				// In the editor some script could be queuing drawing commands in e.g. EditorWindow.Update without the scene
				// view or any game view being re-rendered. We discard these commands if nothing has been rendered for a long time.
				Draw.builder.DiscardAndDisposeInternal();
				Draw.ingame_builder.DiscardAndDisposeInternal();
				Draw.builder = gizmos.GetBuiltInBuilder(false);
				Draw.ingame_builder = gizmos.GetBuiltInBuilder(true);
				lastFrameTime = Time.realtimeSinceStartup;
				RemoveDestroyedGizmoDrawers();
			}

			// Avoid potential memory leak if gizmos are not being drawn
			if (lastFilterFrame - Time.frameCount > 5) {
				lastFilterFrame = Time.frameCount;
				RemoveDestroyedGizmoDrawers();
			}
		}

		internal void ExecuteCustomRenderPass (ScriptableRenderContext context, Camera camera) {
			MarkerALINE.Begin();
			commandBuffer.Clear();
			SubmitFrame(camera, new DrawingData.CommandBufferWrapper { cmd = commandBuffer }, true);
			context.ExecuteCommandBuffer(commandBuffer);
			MarkerALINE.End();
		}

#if MODULE_RENDER_PIPELINES_UNIVERSAL
internal void ExecuteCustomRenderGraphPass (DrawingData.CommandBufferWrapper cmd, Camera camera) {
MarkerALINE.Begin();
SubmitFrame(camera, cmd, true);
MarkerALINE.End();
}
#endif

		private void EndCameraRendering (ScriptableRenderContext context, Camera camera) {
			if (detectedRenderPipeline == DetectedRenderPipeline.BuiltInOrCustom) {
				// Execute the custom render pass after the camera has finished rendering.
				// For the HDRP and URP the render pass will already have been executed.
				// However for a custom render pipline we execute the rendering code here.
				// This is only best effort. It's impossible to be compatible with all custom render pipelines.
				// However it should work for most simple ones.
				// For Unity's built-in render pipeline the EndCameraRendering method will never be called.
				ExecuteCustomRenderPass(context, camera);
			}
		}

		void PostRender (Camera camera) {
			// This method is only called when using Unity's built-in render pipeline
			commandBuffer.Clear();
			SubmitFrame(camera, new DrawingData.CommandBufferWrapper { cmd = commandBuffer }, false);
			MarkerCommandBuffer.Begin();
			Graphics.ExecuteCommandBuffer(commandBuffer);
			MarkerCommandBuffer.End();
		}

		void CheckFrameTicking () {
			MarkerFrameTick.Begin();
			if (Time.frameCount != lastFrameCount) {
				framePassed = true;
				lastFrameCount = Time.frameCount;
				lastFrameTime = Time.realtimeSinceStartup;
				previousFrameRedrawScope = gizmos.frameRedrawScope;
				gizmos.frameRedrawScope = new RedrawScope(gizmos);
				Draw.builder.DisposeInternal();
				Draw.ingame_builder.DisposeInternal();
				Draw.builder = gizmos.GetBuiltInBuilder(false);
				Draw.ingame_builder = gizmos.GetBuiltInBuilder(true);
			} else if (framePassed && Application.isPlaying) {
				// Rendered frame passed without a game frame passing!
				// This might mean the game is paused.
				// Redraw gizmos while the game is paused.
				// It might also just mean that we are rendering with multiple cameras.
				previousFrameRedrawScope.Draw();
			}

			if (framePassed) {
				gizmos.TickFramePreRender();
#if UNITY_EDITOR
builtGizmos = false;
#endif
framePassed = false;
}
MarkerFrameTick.End();
}

		internal void SubmitFrame (Camera camera, DrawingData.CommandBufferWrapper cmd, bool usingRenderPipeline) {
#if UNITY_EDITOR
bool isSceneViewCamera = SceneView.currentDrawingSceneView != null && SceneView.currentDrawingSceneView.camera == camera;
#else
bool isSceneViewCamera = false;
#endif
// Do not include when rendering to a texture unless this is a scene view camera
bool allowCameraDefault = allowRenderToRenderTextures || drawToAllCameras || camera.targetTexture == null || isSceneViewCamera;

			CheckFrameTicking();

			Submit(camera, cmd, usingRenderPipeline, allowCameraDefault);

			gizmos.PostRenderCleanup();
		}

#if UNITY_EDITOR
static readonly System.Reflection.MethodInfo IsGizmosAllowedForObject = typeof(UnityEditor.EditorGUIUtility).GetMethod("IsGizmosAllowedForObject", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
readonly System.Object[] cachedObjectParameterArray = new System.Object[1];
#endif

		bool ShouldDrawGizmos (UnityEngine.Object obj) {
#if UNITY_EDITOR
// Use reflection to call EditorGUIUtility.IsGizmosAllowedForObject which is an internal method.
// It is exactly the information we want though.
// In case Unity has changed its API or something so that the method can no longer be found then just return true
cachedObjectParameterArray[0] = obj;
return IsGizmosAllowedForObject == null || (bool)IsGizmosAllowedForObject.Invoke(null, cachedObjectParameterArray);
#else
return true;
#endif
}

		static void RemoveDestroyedGizmoDrawers () {
			MarkerFilterDestroyedObjects.Begin();
			for (int i = 0; i < gizmoDrawers.Count; i++) {
				var group = gizmoDrawers[i];
				int j = 0;
				for (int k = 0; k < group.drawers.Count; k++) {
					var v = group.drawers[k];
					if (v as MonoBehaviour) {
						group.drawers[j] = v;
						j++;
					}
				}
				group.drawers.RemoveRange(j, group.drawers.Count - j);
			}
			MarkerFilterDestroyedObjects.End();
		}

#if UNITY_EDITOR
void DrawGizmos (bool usingRenderPipeline) {
GizmoContext.SetDirty();

			// Reduce overhead if there's nothing to render
			if (gizmoDrawers.Count == 0) return;

			MarkerGizmosAllowed.Begin();

			// Figure out which component types should be rendered
			for (int i = 0; i < gizmoDrawers.Count; i++) {
				var group = gizmoDrawers[i];
#if UNITY_2022_1_OR_NEWER
// In Unity 2022.1 we can use a new utility class which is more robust.
if (GizmoUtility.TryGetGizmoInfo(group.type, out var gizmoInfo)) {
group.enabled = gizmoInfo.gizmoEnabled;
} else {
group.enabled = true;
}
#else
// We take advantage of the fact that IsGizmosAllowedForObject only depends on the type of the object and if it is active and enabled
// and not the specific object instance.
// When using a render pipeline the ShouldDrawGizmos method cannot be used because it seems to occasionally crash Unity :(
// So we need these two separate cases.
if (!usingRenderPipeline) {
group.enabled = false;
for (int j = group.drawers.Count - 1; j >= 0; j--) {
// Find the first active and enabled drawer
if ((group.drawers[j] as MonoBehaviour).isActiveAndEnabled) {
group.enabled = ShouldDrawGizmos((UnityEngine.Object)group.drawers[j]);
break;
}
}
} else {
group.enabled = true;
}
#endif
gizmoDrawers[i] = group;
}

			MarkerGizmosAllowed.End();

			// Set the current frame's redraw scope to an empty scope.
			// This is because gizmos are rendered every frame anyway so we never want to redraw them.
			// The frame redraw scope is otherwise used when the game has been paused.
			var frameRedrawScope = gizmos.frameRedrawScope;
			gizmos.frameRedrawScope = default(RedrawScope);

#if UNITY_EDITOR && UNITY_2020_1_OR_NEWER
var currentStage = StageUtility.GetCurrentStage();
var isInNonMainStage = currentStage != StageUtility.GetMainStage();
var currentStageHandle = currentStage.stageHandle;
#endif

			// This would look nicer as a 'using' block, but built-in command builders
			// cannot be disposed normally to prevent user error.
			// The try-finally is equivalent to a 'using' block.
			var gizmoBuilder = gizmos.GetBuiltInBuilder();
			// Replace Draw.builder with a custom one just for gizmos
			var debugBuilder = Draw.builder;
			MarkerDrawGizmos.Begin();
			GizmoContext.drawingGizmos = true;
			try {
				Draw.builder = gizmoBuilder;

				for (int i = gizmoDrawers.Count - 1; i >= 0; i--) {
					var group = gizmoDrawers[i];
					if (group.enabled && group.drawers.Count > 0) {
						group.profilerMarker.Begin();
						for (int j = group.drawers.Count - 1; j >= 0; j--) {
							var mono = group.drawers[j] as MonoBehaviour;
							if (!mono.isActiveAndEnabled || (mono.hideFlags & HideFlags.HideInHierarchy) != 0) continue;

#if UNITY_EDITOR && UNITY_2020_1_OR_NEWER
// True if the scene is in isolation mode (e.g. focusing on a single prefab) and this object is not part of that sub-stage
var disabledDueToIsolationMode = isInNonMainStage && !currentStageHandle.Contains(mono.gameObject);
if (disabledDueToIsolationMode) continue;
#endif

							try {
								group.drawers[j].DrawGizmos();
							} catch (System.Exception e) {
								Debug.LogException(e, mono);
							}
						}
						group.profilerMarker.End();
					}
				}
			} finally {
				GizmoContext.drawingGizmos = false;
				MarkerDrawGizmos.End();
				// Revert to the original builder
				Draw.builder = debugBuilder;
				gizmoBuilder.DisposeInternal();
			}

			gizmos.frameRedrawScope = frameRedrawScope;

			// Schedule jobs that may have been scheduled while drawing gizmos
			JobHandle.ScheduleBatchedJobs();
		}
#endif

		/// <summary>Submit a camera for rendering.</summary>
		/// <param name="allowCameraDefault">Indicates if built-in command builders and custom ones without a custom CommandBuilder.cameraTargets should render to this camera.</param>
		void Submit (Camera camera, DrawingData.CommandBufferWrapper cmd, bool usingRenderPipeline, bool allowCameraDefault) {
#if UNITY_EDITOR
bool drawGizmos = Handles.ShouldRenderGizmos() || drawToAllCameras;
// Only build gizmos if a camera actually needs them.
// This is only done for the first camera that needs them each frame.
if (drawGizmos && !builtGizmos && allowCameraDefault) {
RemoveDestroyedGizmoDrawers();
lastFilterFrame = Time.frameCount;
builtGizmos = true;
DrawGizmos(usingRenderPipeline);
}
#else
bool drawGizmos = false;
#endif

			MarkerSubmitGizmos.Begin();
			Draw.builder.DisposeInternal();
			Draw.ingame_builder.DisposeInternal();
			gizmos.Render(camera, drawGizmos, cmd, allowCameraDefault);
			Draw.builder = gizmos.GetBuiltInBuilder(false);
			Draw.ingame_builder = gizmos.GetBuiltInBuilder(true);
			MarkerSubmitGizmos.End();
		}

		/// <summary>
		/// Registers an object for gizmo drawing.
		/// The DrawGizmos method on the object will be called every frame until it is destroyed (assuming there are cameras with gizmos enabled).
		/// </summary>
		public static void Register (IDrawGizmos item) {
			if (ignoreAllDrawing) return;

			var tp = item.GetType();

			int index;
			if (gizmoDrawerIndices.TryGetValue(tp, out index)) {
			} else {
				// Use reflection to figure out if the DrawGizmos method has not been overriden from the MonoBehaviourGizmos class.
				// If it hasn't, then we know that this type will never draw gizmos and we can skip it.
				// This improves performance by not having to keep track of objects and check if they are active and enabled every frame.

				var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
				// Check for a public method first, and then an explicit interface implementation.
				var m = tp.GetMethod("DrawGizmos", flags) ?? tp.GetMethod("Pathfinding.Drawing.IDrawGizmos.DrawGizmos", flags) ?? tp.GetMethod("Drawing.IDrawGizmos.DrawGizmos", flags);
				if (m == null) {
					throw new System.Exception("Could not find the DrawGizmos method in type " + tp.Name);
				}
				var mayDrawGizmos = m.DeclaringType != typeof(MonoBehaviourGizmos);
				if (mayDrawGizmos) {
					index = gizmoDrawerIndices[tp] = gizmoDrawers.Count;
					gizmoDrawers.Add(new GizmoDrawerGroup {
						type = tp,
						enabled = true,
						drawers = new List<IDrawGizmos>(),
						profilerMarker = new ProfilerMarker(ProfilerCategory.Render, "Gizmos for " + tp.Name),
					});
				} else {
					index = gizmoDrawerIndices[tp] = -1;
				}
			}
			if (index == -1) return;

			gizmoDrawers[index].drawers.Add(item);
		}

		/// <summary>
		/// Get an empty builder for queuing drawing commands.
		///
		/// <code>
		/// // Create a new CommandBuilder
		/// using (var draw = DrawingManager.GetBuilder()) {
		///     // Use the exact same API as the global Draw class
		///     draw.WireBox(Vector3.zero, Vector3.one);
		/// }
		/// </code>
		/// See: <see cref="Drawing.CommandBuilder"/>
		/// </summary>
		/// <param name="renderInGame">If true, this builder will be rendered in standalone games and in the editor even if gizmos are disabled.
		/// If false, it will only be rendered in the editor when gizmos are enabled.</param>
		public static CommandBuilder GetBuilder(bool renderInGame = false) => instance.gizmos.GetBuilder(renderInGame);

		/// <summary>
		/// Get an empty builder for queuing drawing commands.
		///
		/// See: <see cref="Drawing.CommandBuilder"/>
		/// </summary>
		/// <param name="redrawScope">Scope for this command builder. See #GetRedrawScope.</param>
		/// <param name="renderInGame">If true, this builder will be rendered in standalone games and in the editor even if gizmos are disabled.
		/// If false, it will only be rendered in the editor when gizmos are enabled.</param>
		public static CommandBuilder GetBuilder(RedrawScope redrawScope, bool renderInGame = false) => instance.gizmos.GetBuilder(redrawScope, renderInGame);

		/// <summary>
		/// Get an empty builder for queuing drawing commands.
		///
		/// <code>
		/// // Just a nice looking curve (which uses a lot of complex math)
		/// // See https://en.wikipedia.org/wiki/Butterfly_curve_(transcendental)
		/// static float2 ButterflyCurve (float t) {
		///     t *= 12 * math.PI;
		///     var k = math.exp(math.cos(t)) - 2*math.cos(4*t) - math.pow(math.sin(t/12f), 5);
		///     return new float2(k * math.sin(t), k * math.cos(t));
		/// }
		///
		/// // Make the butterfly "flap its wings" two times per second
		/// var scale = Time.time % 0.5f < 0.25f ? new float2(1, 1) : new float2(0.7f, 1);
		///
		/// // Hash all inputs that you use for drawing something complex
		/// var hasher = new DrawingData.Hasher();
		/// // The only thing making the drawing change, in this case, is the scale
		/// hasher.Add(scale);
		///
		/// // Try to draw a previously cached mesh with this hash
		/// if (!DrawingManager.TryDrawHasher(hasher)) {
		///     // If there's no cached mesh, then draw it from scratch
		///     using (var builder = DrawingManager.GetBuilder(hasher)) {
		///         // Draw a complex curve using 10000 lines
		///         var prev = ButterflyCurve(0);
		///         for (float t = 0; t < 1; t += 0.0001f) {
		///             var next = ButterflyCurve(t);
		///             builder.xy.Line(prev*scale, next*scale, Color.white);
		///             prev = next;
		///         }
		///     }
		/// }
		/// </code>
		///
		/// See: <see cref="Drawing.CommandBuilder"/>
		/// See: caching (view in online documentation for working links)
		/// </summary>
		/// <param name="hasher">Hash of whatever inputs you used to generate the drawing data.</param>
		/// <param name="redrawScope">Scope for this command builder. See #GetRedrawScope.</param>
		/// <param name="renderInGame">If true, this builder will be rendered in standalone games and in the editor even if gizmos are disabled.</param>
		public static CommandBuilder GetBuilder(DrawingData.Hasher hasher, RedrawScope redrawScope = default, bool renderInGame = false) => instance.gizmos.GetBuilder(hasher, redrawScope, renderInGame);

		/// <summary>
		/// Tries to draw a builder that was rendered during the last frame using the same hash.
		///
		/// Returns: True if the builder was found and scheduled for rendering. If false, you should draw everything again and submit a new command builder.
		///
		/// <code>
		/// // Just a nice looking curve (which uses a lot of complex math)
		/// // See https://en.wikipedia.org/wiki/Butterfly_curve_(transcendental)
		/// static float2 ButterflyCurve (float t) {
		///     t *= 12 * math.PI;
		///     var k = math.exp(math.cos(t)) - 2*math.cos(4*t) - math.pow(math.sin(t/12f), 5);
		///     return new float2(k * math.sin(t), k * math.cos(t));
		/// }
		///
		/// // Make the butterfly "flap its wings" two times per second
		/// var scale = Time.time % 0.5f < 0.25f ? new float2(1, 1) : new float2(0.7f, 1);
		///
		/// // Hash all inputs that you use for drawing something complex
		/// var hasher = new DrawingData.Hasher();
		/// // The only thing making the drawing change, in this case, is the scale
		/// hasher.Add(scale);
		///
		/// // Try to draw a previously cached mesh with this hash
		/// if (!DrawingManager.TryDrawHasher(hasher)) {
		///     // If there's no cached mesh, then draw it from scratch
		///     using (var builder = DrawingManager.GetBuilder(hasher)) {
		///         // Draw a complex curve using 10000 lines
		///         var prev = ButterflyCurve(0);
		///         for (float t = 0; t < 1; t += 0.0001f) {
		///             var next = ButterflyCurve(t);
		///             builder.xy.Line(prev*scale, next*scale, Color.white);
		///             prev = next;
		///         }
		///     }
		/// }
		/// </code>
		///
		/// See: caching (view in online documentation for working links)
		/// </summary>
		/// <param name="hasher">Hash of whatever inputs you used to generate the drawing data.</param>
		/// <param name="redrawScope">Optional redraw scope for this command builder. See #GetRedrawScope.</param>
		public static bool TryDrawHasher(DrawingData.Hasher hasher, RedrawScope redrawScope = default) => instance.gizmos.Draw(hasher, redrawScope);

		/// <summary>
		/// A scope which will persist rendered items over multiple frames until it is disposed.
		///
		/// You can use <see cref="GetBuilder(RedrawScope,bool)"/> to get a builder with a given redraw scope.
		/// Everything drawn using the redraw scope will be drawn every frame until the redraw scope is disposed.
		///
		/// <code>
		/// private RedrawScope redrawScope;
		///
		/// void Start () {
		///     redrawScope = DrawingManager.GetRedrawScope();
		///     using (var builder = DrawingManager.GetBuilder(redrawScope)) {
		///         builder.WireSphere(Vector3.zero, 1.0f, Color.red);
		///     }
		/// }
		///
		/// void OnDestroy () {
		///     redrawScope.Dispose();
		/// }
		/// </code>
		///
		/// See: caching (view in online documentation for working links)
		/// </summary>
		/// <param name="associatedGameObject">If not null, the scope will only be drawn if gizmos for the associated GameObject are drawn.
		/// 		This is useful in the unity editor when e.g. opening a prefab in isolation mode, to disable redraw scopes for objects outside the prefab. Has no effect in standalone builds.</param>
		public static RedrawScope GetRedrawScope (GameObject associatedGameObject = null) {
			var scope = new RedrawScope(instance.gizmos);
			scope.DrawUntilDispose(associatedGameObject);
			return scope;
		}
	}
}
// This file is automatically generated by a script based on the CommandBuilder API.
// This file adds additional overloads to the CommandBuilder API.
using Unity.Burst;
using UnityEngine;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using static Drawing.CommandBuilder;

namespace Drawing {
public partial struct CommandBuilder2D {
/// <summary>\copydocref{CommandBuilder.WithMatrix(Matrix4x4)}</summary>
[BurstDiscard]
public ScopeMatrix WithMatrix (Matrix4x4 matrix) {
return draw.WithMatrix(matrix);
}
/// <summary>\copydocref{CommandBuilder.WithMatrix(float3x3)}</summary>
[BurstDiscard]
public ScopeMatrix WithMatrix (float3x3 matrix) {
return draw.WithMatrix(matrix);
}
/// <summary>\copydocref{CommandBuilder.WithColor(Color)}</summary>
[BurstDiscard]
public ScopeColor WithColor (Color color) {
return draw.WithColor(color);
}
/// <summary>\copydocref{CommandBuilder.WithDuration(float)}</summary>
[BurstDiscard]
public ScopePersist WithDuration (float duration) {
return draw.WithDuration(duration);
}

		/// <summary>\copydocref{CommandBuilder.WithLineWidth(float,bool)}</summary>
		[BurstDiscard]
		public ScopeLineWidth WithLineWidth (float pixels, bool automaticJoins = true) {
			return draw.WithLineWidth(pixels, automaticJoins);
		}
		/// <summary>\copydocref{CommandBuilder.InLocalSpace(Transform)}</summary>
		[BurstDiscard]
		public ScopeMatrix InLocalSpace (Transform transform) {
			return draw.InLocalSpace(transform);
		}

		/// <summary>\copydocref{CommandBuilder.InScreenSpace(Camera)}</summary>
		[BurstDiscard]
		public ScopeMatrix InScreenSpace (Camera camera) {
			return draw.InScreenSpace(camera);
		}

		/// <summary>\copydocref{CommandBuilder.PushMatrix(Matrix4x4)}</summary>
		public void PushMatrix (Matrix4x4 matrix) {
			draw.PushMatrix(matrix);
		}
		/// <summary>\copydocref{CommandBuilder.PushMatrix(float4x4)}</summary>
		public void PushMatrix (float4x4 matrix) {
			draw.PushMatrix(matrix);
		}
		/// <summary>\copydocref{CommandBuilder.PushSetMatrix(Matrix4x4)}</summary>
		public void PushSetMatrix (Matrix4x4 matrix) {
			draw.PushSetMatrix(matrix);
		}

		/// <summary>\copydocref{CommandBuilder.PushSetMatrix(float4x4)}</summary>
		public void PushSetMatrix (float4x4 matrix) {
			draw.PushSetMatrix(matrix);
		}

		/// <summary>\copydocref{CommandBuilder.PopMatrix()}</summary>
		public void PopMatrix () {
			draw.PopMatrix();
		}
		/// <summary>\copydocref{CommandBuilder.PushColor(Color)}</summary>
		public void PushColor (Color color) {
			draw.PushColor(color);
		}

		/// <summary>\copydocref{CommandBuilder.PopColor()}</summary>
		public void PopColor () {
			draw.PopColor();
		}

		/// <summary>\copydocref{CommandBuilder.PushDuration(float)}</summary>
		public void PushDuration (float duration) {
			draw.PushDuration(duration);
		}

		/// <summary>\copydocref{CommandBuilder.PopDuration()}</summary>
		public void PopDuration () {
			draw.PopDuration();
		}

		/// <summary>\copydocref{CommandBuilder.PushPersist(float)}</summary>
		[System.Obsolete("Renamed to PushDuration for consistency")]
		public void PushPersist (float duration) {
			draw.PushPersist(duration);
		}

		/// <summary>\copydocref{CommandBuilder.PopPersist()}</summary>
		[System.Obsolete("Renamed to PopDuration for consistency")]
		public void PopPersist () {
			draw.PopPersist();
		}

		/// <summary>\copydocref{CommandBuilder.PushLineWidth(float,bool)}</summary>
		public void PushLineWidth (float pixels, bool automaticJoins = true) {
			draw.PushLineWidth(pixels, automaticJoins);
		}

		/// <summary>\copydocref{CommandBuilder.PopLineWidth()}</summary>
		public void PopLineWidth () {
			draw.PopLineWidth();
		}

		/// <summary>\copydocref{CommandBuilder.Line(Vector3,Vector3)}</summary>
		public void Line (Vector3 a, Vector3 b) {
			draw.Line(a, b);
		}
		/// <summary>\copydocref{CommandBuilder.Line(Vector3,Vector3)}</summary>
		public void Line (Vector2 a, Vector2 b) {
			Line(xy ? new Vector3(a.x, a.y, 0) : new Vector3(a.x, 0, a.y), xy ? new Vector3(b.x, b.y, 0) : new Vector3(b.x, 0, b.y));
		}
		/// <summary>\copydocref{CommandBuilder.Line(Vector3,Vector3,Color)}</summary>
		public void Line (Vector3 a, Vector3 b, Color color) {
			draw.Line(a, b, color);
		}
		/// <summary>\copydocref{CommandBuilder.Line(Vector3,Vector3,Color)}</summary>
		public void Line (Vector2 a, Vector2 b, Color color) {
			Line(xy ? new Vector3(a.x, a.y, 0) : new Vector3(a.x, 0, a.y), xy ? new Vector3(b.x, b.y, 0) : new Vector3(b.x, 0, b.y), color);
		}
		/// <summary>\copydocref{CommandBuilder.Ray(float3,float3)}</summary>
		public void Ray (float3 origin, float3 direction) {
			draw.Ray(origin, direction);
		}
		/// <summary>\copydocref{CommandBuilder.Ray(float3,float3)}</summary>
		public void Ray (float2 origin, float2 direction) {
			Ray(xy ? new float3(origin, 0) : new float3(origin.x, 0, origin.y), xy ? new float3(direction, 0) : new float3(direction.x, 0, direction.y));
		}
		/// <summary>\copydocref{CommandBuilder.Ray(Ray,float)}</summary>
		public void Ray (Ray ray, float length) {
			draw.Ray(ray, length);
		}
		/// <summary>\copydocref{CommandBuilder.Arc(float3,float3,float3)}</summary>
		public void Arc (float3 center, float3 start, float3 end) {
			draw.Arc(center, start, end);
		}
		/// <summary>\copydocref{CommandBuilder.Arc(float3,float3,float3)}</summary>
		public void Arc (float2 center, float2 start, float2 end) {
			Arc(xy ? new float3(center, 0) : new float3(center.x, 0, center.y), xy ? new float3(start, 0) : new float3(start.x, 0, start.y), xy ? new float3(end, 0) : new float3(end.x, 0, end.y));
		}
		/// <summary>\copydocref{CommandBuilder.CircleXY(float3,float,float,float)}</summary>
		[System.Obsolete("Use Draw.xy.Circle instead")]
		public void CircleXY (float3 center, float radius, float startAngle = 0f, float endAngle = 2 * Mathf.PI) {
			draw.CircleXY(center, radius, startAngle, endAngle);
		}

		/// <summary>\copydocref{CommandBuilder.CircleXY(float3,float,float,float)}</summary>
		[System.Obsolete("Use Draw.xy.Circle instead")]
		public void CircleXY (float2 center, float radius, float startAngle = 0f, float endAngle = 2 * Mathf.PI) {
			CircleXY(xy ? new float3(center, 0) : new float3(center.x, 0, center.y), radius, startAngle, endAngle);
		}

		/// <summary>\copydocref{CommandBuilder.SolidArc(float3,float3,float3)}</summary>
		public void SolidArc (float3 center, float3 start, float3 end) {
			draw.SolidArc(center, start, end);
		}

		/// <summary>\copydocref{CommandBuilder.SolidArc(float3,float3,float3)}</summary>
		public void SolidArc (float2 center, float2 start, float2 end) {
			SolidArc(xy ? new float3(center, 0) : new float3(center.x, 0, center.y), xy ? new float3(start, 0) : new float3(start.x, 0, start.y), xy ? new float3(end, 0) : new float3(end.x, 0, end.y));
		}

		/// <summary>\copydocref{CommandBuilder.Polyline(List<Vector3>,bool)}</summary>
		[BurstDiscard]
		public void Polyline (List<Vector3> points, bool cycle = false) {
			draw.Polyline(points, cycle);
		}
		/// <summary>\copydocref{CommandBuilder.Polyline(Vector3[],bool)}</summary>
		[BurstDiscard]
		public void Polyline (Vector3[] points, bool cycle = false) {
			draw.Polyline(points, cycle);
		}
		/// <summary>\copydocref{CommandBuilder.Polyline(float3[],bool)}</summary>
		[BurstDiscard]
		public void Polyline (float3[] points, bool cycle = false) {
			draw.Polyline(points, cycle);
		}
		/// <summary>\copydocref{CommandBuilder.Polyline(NativeArray<float3>,bool)}</summary>
		public void Polyline (NativeArray<float3> points, bool cycle = false) {
			draw.Polyline(points, cycle);
		}
		/// <summary>\copydocref{CommandBuilder.DashedLine(float3,float3,float,float)}</summary>
		public void DashedLine (float3 a, float3 b, float dash, float gap) {
			draw.DashedLine(a, b, dash, gap);
		}

		/// <summary>\copydocref{CommandBuilder.DashedLine(float3,float3,float,float)}</summary>
		public void DashedLine (float2 a, float2 b, float dash, float gap) {
			DashedLine(xy ? new float3(a, 0) : new float3(a.x, 0, a.y), xy ? new float3(b, 0) : new float3(b.x, 0, b.y), dash, gap);
		}

		/// <summary>\copydocref{CommandBuilder.DashedPolyline(List<Vector3>,float,float)}</summary>
		public void DashedPolyline (List<Vector3> points, float dash, float gap) {
			draw.DashedPolyline(points, dash, gap);
		}

		/// <summary>\copydocref{CommandBuilder.Cross(float3,float)}</summary>
		public void Cross (float3 position, float size = 1) {
			draw.Cross(position, size);
		}
		/// <summary>\copydocref{CommandBuilder.Bezier(float3,float3,float3,float3)}</summary>
		public void Bezier (float3 p0, float3 p1, float3 p2, float3 p3) {
			draw.Bezier(p0, p1, p2, p3);
		}
		/// <summary>\copydocref{CommandBuilder.Bezier(float3,float3,float3,float3)}</summary>
		public void Bezier (float2 p0, float2 p1, float2 p2, float2 p3) {
			Bezier(xy ? new float3(p0, 0) : new float3(p0.x, 0, p0.y), xy ? new float3(p1, 0) : new float3(p1.x, 0, p1.y), xy ? new float3(p2, 0) : new float3(p2.x, 0, p2.y), xy ? new float3(p3, 0) : new float3(p3.x, 0, p3.y));
		}
		/// <summary>\copydocref{CommandBuilder.CatmullRom(List<Vector3>)}</summary>
		public void CatmullRom (List<Vector3> points) {
			draw.CatmullRom(points);
		}

		/// <summary>\copydocref{CommandBuilder.CatmullRom(float3,float3,float3,float3)}</summary>
		public void CatmullRom (float3 p0, float3 p1, float3 p2, float3 p3) {
			draw.CatmullRom(p0, p1, p2, p3);
		}

		/// <summary>\copydocref{CommandBuilder.CatmullRom(float3,float3,float3,float3)}</summary>
		public void CatmullRom (float2 p0, float2 p1, float2 p2, float2 p3) {
			CatmullRom(xy ? new float3(p0, 0) : new float3(p0.x, 0, p0.y), xy ? new float3(p1, 0) : new float3(p1.x, 0, p1.y), xy ? new float3(p2, 0) : new float3(p2.x, 0, p2.y), xy ? new float3(p3, 0) : new float3(p3.x, 0, p3.y));
		}

		/// <summary>\copydocref{CommandBuilder.Arrow(float3,float3)}</summary>
		public void Arrow (float3 from, float3 to) {
			ArrowRelativeSizeHead(from, to, xy ? XY_UP : XZ_UP, 0.2f);
		}
		/// <summary>\copydocref{CommandBuilder.Arrow(float3,float3)}</summary>
		public void Arrow (float2 from, float2 to) {
			Arrow(xy ? new float3(from, 0) : new float3(from.x, 0, from.y), xy ? new float3(to, 0) : new float3(to.x, 0, to.y));
		}
		/// <summary>\copydocref{CommandBuilder.Arrow(float3,float3,float3,float)}</summary>
		public void Arrow (float3 from, float3 to, float3 up, float headSize) {
			draw.Arrow(from, to, up, headSize);
		}
		/// <summary>\copydocref{CommandBuilder.Arrow(float3,float3,float3,float)}</summary>
		public void Arrow (float2 from, float2 to, float2 up, float headSize) {
			Arrow(xy ? new float3(from, 0) : new float3(from.x, 0, from.y), xy ? new float3(to, 0) : new float3(to.x, 0, to.y), xy ? new float3(up, 0) : new float3(up.x, 0, up.y), headSize);
		}
		/// <summary>\copydocref{CommandBuilder.ArrowRelativeSizeHead(float3,float3,float3,float)}</summary>
		public void ArrowRelativeSizeHead (float3 from, float3 to, float3 up, float headFraction) {
			draw.ArrowRelativeSizeHead(from, to, up, headFraction);
		}
		/// <summary>\copydocref{CommandBuilder.ArrowRelativeSizeHead(float3,float3,float3,float)}</summary>
		public void ArrowRelativeSizeHead (float2 from, float2 to, float2 up, float headFraction) {
			ArrowRelativeSizeHead(xy ? new float3(from, 0) : new float3(from.x, 0, from.y), xy ? new float3(to, 0) : new float3(to.x, 0, to.y), xy ? new float3(up, 0) : new float3(up.x, 0, up.y), headFraction);
		}
		/// <summary>\copydocref{CommandBuilder.Arrowhead(float3,float3,float)}</summary>
		public void Arrowhead (float3 center, float3 direction, float radius) {
			Arrowhead(center, direction, xy ? XY_UP : XZ_UP, radius);
		}

		/// <summary>\copydocref{CommandBuilder.Arrowhead(float3,float3,float)}</summary>
		public void Arrowhead (float2 center, float2 direction, float radius) {
			Arrowhead(xy ? new float3(center, 0) : new float3(center.x, 0, center.y), xy ? new float3(direction, 0) : new float3(direction.x, 0, direction.y), radius);
		}

		/// <summary>\copydocref{CommandBuilder.Arrowhead(float3,float3,float3,float)}</summary>
		public void Arrowhead (float3 center, float3 direction, float3 up, float radius) {
			draw.Arrowhead(center, direction, up, radius);
		}

		/// <summary>\copydocref{CommandBuilder.Arrowhead(float3,float3,float3,float)}</summary>
		public void Arrowhead (float2 center, float2 direction, float2 up, float radius) {
			Arrowhead(xy ? new float3(center, 0) : new float3(center.x, 0, center.y), xy ? new float3(direction, 0) : new float3(direction.x, 0, direction.y), xy ? new float3(up, 0) : new float3(up.x, 0, up.y), radius);
		}

		/// <summary>\copydocref{CommandBuilder.ArrowheadArc(float3,float3,float,float)}</summary>
		public void ArrowheadArc (float3 origin, float3 direction, float offset, float width = 60) {
			if (!math.any(direction)) return;
			if (offset < 0) throw new System.ArgumentOutOfRangeException(nameof(offset));
			if (offset == 0) return;

			var rot = Quaternion.LookRotation(direction, xy ? XY_UP : XZ_UP);
			PushMatrix(Matrix4x4.TRS(origin, rot, Vector3.one));
			var a1 = math.PI * 0.5f - width * (0.5f * Mathf.Deg2Rad);
			var a2 = math.PI * 0.5f + width * (0.5f * Mathf.Deg2Rad);
			draw.CircleXZInternal(float3.zero, offset, a1, a2);
			var p1 = new float3(math.cos(a1), 0, math.sin(a1)) * offset;
			var p2 = new float3(math.cos(a2), 0, math.sin(a2)) * offset;
			const float sqrt2 = 1.4142f;
			var p3 = new float3(0, 0, sqrt2 * offset);
			Line(p1, p3);
			Line(p3, p2);
			PopMatrix();
		}
		/// <summary>\copydocref{CommandBuilder.ArrowheadArc(float3,float3,float,float)}</summary>
		public void ArrowheadArc (float2 origin, float2 direction, float offset, float width = 60) {
			ArrowheadArc(xy ? new float3(origin, 0) : new float3(origin.x, 0, origin.y), xy ? new float3(direction, 0) : new float3(direction.x, 0, direction.y), offset, width);
		}
		/// <summary>\copydocref{CommandBuilder.WireTriangle(float3,float3,float3)}</summary>
		public void WireTriangle (float3 a, float3 b, float3 c) {
			draw.WireTriangle(a, b, c);
		}

		/// <summary>\copydocref{CommandBuilder.WireTriangle(float3,float3,float3)}</summary>
		public void WireTriangle (float2 a, float2 b, float2 c) {
			WireTriangle(xy ? new float3(a, 0) : new float3(a.x, 0, a.y), xy ? new float3(b, 0) : new float3(b.x, 0, b.y), xy ? new float3(c, 0) : new float3(c.x, 0, c.y));
		}

		/// <summary>\copydocref{CommandBuilder.WireRectangle(float3,quaternion,float2)}</summary>
		public void WireRectangle (float3 center, quaternion rotation, float2 size) {
			draw.WireRectangle(center, rotation, size);
		}
		/// <summary>\copydocref{CommandBuilder.WireRectangle(float3,quaternion,float2)}</summary>
		public void WireRectangle (float2 center, quaternion rotation, float2 size) {
			WireRectangle(xy ? new float3(center, 0) : new float3(center.x, 0, center.y), rotation, size);
		}
		/// <summary>\copydocref{CommandBuilder.WireTriangle(float3,quaternion,float)}</summary>
		public void WireTriangle (float3 center, quaternion rotation, float radius) {
			draw.WireTriangle(center, rotation, radius);
		}

		/// <summary>\copydocref{CommandBuilder.WireTriangle(float3,quaternion,float)}</summary>
		public void WireTriangle (float2 center, quaternion rotation, float radius) {
			WireTriangle(xy ? new float3(center, 0) : new float3(center.x, 0, center.y), rotation, radius);
		}

		/// <summary>\copydocref{CommandBuilder.SolidTriangle(float3,float3,float3)}</summary>
		public void SolidTriangle (float3 a, float3 b, float3 c) {
			draw.SolidTriangle(a, b, c);
		}

		/// <summary>\copydocref{CommandBuilder.SolidTriangle(float3,float3,float3)}</summary>
		public void SolidTriangle (float2 a, float2 b, float2 c) {
			SolidTriangle(xy ? new float3(a, 0) : new float3(a.x, 0, a.y), xy ? new float3(b, 0) : new float3(b.x, 0, b.y), xy ? new float3(c, 0) : new float3(c.x, 0, c.y));
		}

		/// <summary>\copydocref{CommandBuilder.Label2D(float3,string,float)}</summary>
		public void Label2D (float3 position, string text, float sizeInPixels = 14) {
			draw.Label2D(position, text, sizeInPixels);
		}

		/// <summary>\copydocref{CommandBuilder.Label2D(float3,string,float)}</summary>
		public void Label2D (float2 position, string text, float sizeInPixels = 14) {
			Label2D(xy ? new float3(position, 0) : new float3(position.x, 0, position.y), text, sizeInPixels);
		}

		/// <summary>\copydocref{CommandBuilder.Label2D(float3,string,float,LabelAlignment)}</summary>
		public void Label2D (float3 position, string text, float sizeInPixels, LabelAlignment alignment) {
			draw.Label2D(position, text, sizeInPixels, alignment);
		}

		/// <summary>\copydocref{CommandBuilder.Label2D(float3,string,float,LabelAlignment)}</summary>
		public void Label2D (float2 position, string text, float sizeInPixels, LabelAlignment alignment) {
			Label2D(xy ? new float3(position, 0) : new float3(position.x, 0, position.y), text, sizeInPixels, alignment);
		}

		/// <summary>\copydocref{CommandBuilder.Label2D(float3,FixedString32Bytes,float)}</summary>
		public void Label2D (float3 position, ref FixedString32Bytes text, float sizeInPixels = 14) {
			draw.Label2D(position, ref text, sizeInPixels);
		}

		/// <summary>\copydocref{CommandBuilder.Label2D(float3,FixedString32Bytes,float)}</summary>
		public void Label2D (float2 position, ref FixedString32Bytes text, float sizeInPixels = 14) {
			Label2D(xy ? new float3(position, 0) : new float3(position.x, 0, position.y), ref text, sizeInPixels);
		}

		/// <summary>\copydocref{CommandBuilder.Label2D(float3,FixedString64Bytes,float)}</summary>
		public void Label2D (float3 position, ref FixedString64Bytes text, float sizeInPixels = 14) {
			draw.Label2D(position, ref text, sizeInPixels);
		}

		/// <summary>\copydocref{CommandBuilder.Label2D(float3,FixedString64Bytes,float)}</summary>
		public void Label2D (float2 position, ref FixedString64Bytes text, float sizeInPixels = 14) {
			Label2D(xy ? new float3(position, 0) : new float3(position.x, 0, position.y), ref text, sizeInPixels);
		}

		/// <summary>\copydocref{CommandBuilder.Label2D(float3,FixedString128Bytes,float)}</summary>
		public void Label2D (float3 position, ref FixedString128Bytes text, float sizeInPixels = 14) {
			draw.Label2D(position, ref text, sizeInPixels);
		}

		/// <summary>\copydocref{CommandBuilder.Label2D(float3,FixedString128Bytes,float)}</summary>
		public void Label2D (float2 position, ref FixedString128Bytes text, float sizeInPixels = 14) {
			Label2D(xy ? new float3(position, 0) : new float3(position.x, 0, position.y), ref text, sizeInPixels);
		}

		/// <summary>\copydocref{CommandBuilder.Label2D(float3,FixedString512Bytes,float)}</summary>
		public void Label2D (float3 position, ref FixedString512Bytes text, float sizeInPixels = 14) {
			draw.Label2D(position, ref text, sizeInPixels);
		}

		/// <summary>\copydocref{CommandBuilder.Label2D(float3,FixedString512Bytes,float)}</summary>
		public void Label2D (float2 position, ref FixedString512Bytes text, float sizeInPixels = 14) {
			Label2D(xy ? new float3(position, 0) : new float3(position.x, 0, position.y), ref text, sizeInPixels);
		}

		/// <summary>\copydocref{CommandBuilder.Label2D(float3,FixedString32Bytes,float,LabelAlignment)}</summary>
		public void Label2D (float3 position, ref FixedString32Bytes text, float sizeInPixels, LabelAlignment alignment) {
			draw.Label2D(position, ref text, sizeInPixels, alignment);
		}

		/// <summary>\copydocref{CommandBuilder.Label2D(float3,FixedString32Bytes,float,LabelAlignment)}</summary>
		public void Label2D (float2 position, ref FixedString32Bytes text, float sizeInPixels, LabelAlignment alignment) {
			Label2D(xy ? new float3(position, 0) : new float3(position.x, 0, position.y), ref text, sizeInPixels, alignment);
		}

		/// <summary>\copydocref{CommandBuilder.Label2D(float3,FixedString64Bytes,float,LabelAlignment)}</summary>
		public void Label2D (float3 position, ref FixedString64Bytes text, float sizeInPixels, LabelAlignment alignment) {
			draw.Label2D(position, ref text, sizeInPixels, alignment);
		}

		/// <summary>\copydocref{CommandBuilder.Label2D(float3,FixedString64Bytes,float,LabelAlignment)}</summary>
		public void Label2D (float2 position, ref FixedString64Bytes text, float sizeInPixels, LabelAlignment alignment) {
			Label2D(xy ? new float3(position, 0) : new float3(position.x, 0, position.y), ref text, sizeInPixels, alignment);
		}

		/// <summary>\copydocref{CommandBuilder.Label2D(float3,FixedString128Bytes,float,LabelAlignment)}</summary>
		public void Label2D (float3 position, ref FixedString128Bytes text, float sizeInPixels, LabelAlignment alignment) {
			draw.Label2D(position, ref text, sizeInPixels, alignment);
		}

		/// <summary>\copydocref{CommandBuilder.Label2D(float3,FixedString128Bytes,float,LabelAlignment)}</summary>
		public void Label2D (float2 position, ref FixedString128Bytes text, float sizeInPixels, LabelAlignment alignment) {
			Label2D(xy ? new float3(position, 0) : new float3(position.x, 0, position.y), ref text, sizeInPixels, alignment);
		}

		/// <summary>\copydocref{CommandBuilder.Label2D(float3,FixedString512Bytes,float,LabelAlignment)}</summary>
		public void Label2D (float3 position, ref FixedString512Bytes text, float sizeInPixels, LabelAlignment alignment) {
			draw.Label2D(position, ref text, sizeInPixels, alignment);
		}

		/// <summary>\copydocref{CommandBuilder.Label2D(float3,FixedString512Bytes,float,LabelAlignment)}</summary>
		public void Label2D (float2 position, ref FixedString512Bytes text, float sizeInPixels, LabelAlignment alignment) {
			Label2D(xy ? new float3(position, 0) : new float3(position.x, 0, position.y), ref text, sizeInPixels, alignment);
		}

		/// <summary>\copydocref{Ray(float3,float3)}</summary>
		public void Ray (float3 origin, float3 direction, Color color) {
			draw.Ray(origin, direction, color);
		}
		/// <summary>\copydocref{Ray(float2,float2)}</summary>
		public void Ray (float2 origin, float2 direction, Color color) {
			Ray(xy ? new float3(origin, 0) : new float3(origin.x, 0, origin.y), xy ? new float3(direction, 0) : new float3(direction.x, 0, direction.y), color);
		}
		/// <summary>\copydocref{Ray(Ray,float)}</summary>
		public void Ray (Ray ray, float length, Color color) {
			draw.Ray(ray, length, color);
		}
		/// <summary>\copydocref{Arc(float3,float3,float3)}</summary>
		public void Arc (float3 center, float3 start, float3 end, Color color) {
			draw.Arc(center, start, end, color);
		}
		/// <summary>\copydocref{Arc(float2,float2,float2)}</summary>
		public void Arc (float2 center, float2 start, float2 end, Color color) {
			Arc(xy ? new float3(center, 0) : new float3(center.x, 0, center.y), xy ? new float3(start, 0) : new float3(start.x, 0, start.y), xy ? new float3(end, 0) : new float3(end.x, 0, end.y), color);
		}
		/// <summary>\copydocref{CircleXY(float3,float,float,float)}</summary>
		[System.Obsolete("Use Draw.xy.Circle instead")]
		public void CircleXY (float3 center, float radius, float startAngle, float endAngle, Color color) {
			draw.CircleXY(center, radius, startAngle, endAngle, color);
		}

		/// <summary>\copydocref{CircleXY(float3,float,float,float)}</summary>
		[System.Obsolete("Use Draw.xy.Circle instead")]
		public void CircleXY (float3 center, float radius, Color color) {
			CircleXY(center, radius, 0f, 2 * Mathf.PI, color);
		}

		/// <summary>\copydocref{CircleXY(float2,float,float,float)}</summary>
		[System.Obsolete("Use Draw.xy.Circle instead")]
		public void CircleXY (float2 center, float radius, float startAngle, float endAngle, Color color) {
			CircleXY(xy ? new float3(center, 0) : new float3(center.x, 0, center.y), radius, startAngle, endAngle, color);
		}

		/// <summary>\copydocref{CircleXY(float2,float,float,float)}</summary>
		[System.Obsolete("Use Draw.xy.Circle instead")]
		public void CircleXY (float2 center, float radius, Color color) {
			CircleXY(center, radius, 0f, 2 * Mathf.PI, color);
		}

		/// <summary>\copydocref{SolidArc(float3,float3,float3)}</summary>
		public void SolidArc (float3 center, float3 start, float3 end, Color color) {
			draw.SolidArc(center, start, end, color);
		}

		/// <summary>\copydocref{SolidArc(float2,float2,float2)}</summary>
		public void SolidArc (float2 center, float2 start, float2 end, Color color) {
			SolidArc(xy ? new float3(center, 0) : new float3(center.x, 0, center.y), xy ? new float3(start, 0) : new float3(start.x, 0, start.y), xy ? new float3(end, 0) : new float3(end.x, 0, end.y), color);
		}

		/// <summary>\copydocref{Polyline(List<Vector3>,bool)}</summary>
		[BurstDiscard]
		public void Polyline (List<Vector3> points, bool cycle, Color color) {
			draw.Polyline(points, cycle, color);
		}
		/// <summary>\copydocref{Polyline(List<Vector3>,bool)}</summary>
		[BurstDiscard]
		public void Polyline (List<Vector3> points, Color color) {
			Polyline(points, false, color);
		}
		/// <summary>\copydocref{Polyline(Vector3[],bool)}</summary>
		[BurstDiscard]
		public void Polyline (Vector3[] points, bool cycle, Color color) {
			draw.Polyline(points, cycle, color);
		}
		/// <summary>\copydocref{Polyline(Vector3[],bool)}</summary>
		[BurstDiscard]
		public void Polyline (Vector3[] points, Color color) {
			Polyline(points, false, color);
		}
		/// <summary>\copydocref{Polyline(float3[],bool)}</summary>
		[BurstDiscard]
		public void Polyline (float3[] points, bool cycle, Color color) {
			draw.Polyline(points, cycle, color);
		}
		/// <summary>\copydocref{Polyline(float3[],bool)}</summary>
		[BurstDiscard]
		public void Polyline (float3[] points, Color color) {
			Polyline(points, false, color);
		}
		/// <summary>\copydocref{Polyline(NativeArray<float3>,bool)}</summary>
		public void Polyline (NativeArray<float3> points, bool cycle, Color color) {
			draw.Polyline(points, cycle, color);
		}
		/// <summary>\copydocref{Polyline(NativeArray<float3>,bool)}</summary>
		public void Polyline (NativeArray<float3> points, Color color) {
			Polyline(points, false, color);
		}
		/// <summary>\copydocref{DashedLine(float3,float3,float,float)}</summary>
		public void DashedLine (float3 a, float3 b, float dash, float gap, Color color) {
			draw.DashedLine(a, b, dash, gap, color);
		}

		/// <summary>\copydocref{DashedLine(float2,float2,float,float)}</summary>
		public void DashedLine (float2 a, float2 b, float dash, float gap, Color color) {
			DashedLine(xy ? new float3(a, 0) : new float3(a.x, 0, a.y), xy ? new float3(b, 0) : new float3(b.x, 0, b.y), dash, gap, color);
		}

		/// <summary>\copydocref{DashedPolyline(List<Vector3>,float,float)}</summary>
		public void DashedPolyline (List<Vector3> points, float dash, float gap, Color color) {
			draw.DashedPolyline(points, dash, gap, color);
		}

		/// <summary>\copydocref{Cross(float3,float)}</summary>
		public void Cross (float3 position, float size, Color color) {
			draw.Cross(position, size, color);
		}
		/// <summary>\copydocref{Cross(float3,float)}</summary>
		public void Cross (float3 position, Color color) {
			Cross(position, 1, color);
		}
		/// <summary>\copydocref{Bezier(float3,float3,float3,float3)}</summary>
		public void Bezier (float3 p0, float3 p1, float3 p2, float3 p3, Color color) {
			draw.Bezier(p0, p1, p2, p3, color);
		}
		/// <summary>\copydocref{Bezier(float2,float2,float2,float2)}</summary>
		public void Bezier (float2 p0, float2 p1, float2 p2, float2 p3, Color color) {
			Bezier(xy ? new float3(p0, 0) : new float3(p0.x, 0, p0.y), xy ? new float3(p1, 0) : new float3(p1.x, 0, p1.y), xy ? new float3(p2, 0) : new float3(p2.x, 0, p2.y), xy ? new float3(p3, 0) : new float3(p3.x, 0, p3.y), color);
		}
		/// <summary>\copydocref{CatmullRom(List<Vector3>)}</summary>
		public void CatmullRom (List<Vector3> points, Color color) {
			draw.CatmullRom(points, color);
		}

		/// <summary>\copydocref{CatmullRom(float3,float3,float3,float3)}</summary>
		public void CatmullRom (float3 p0, float3 p1, float3 p2, float3 p3, Color color) {
			draw.CatmullRom(p0, p1, p2, p3, color);
		}

		/// <summary>\copydocref{CatmullRom(float2,float2,float2,float2)}</summary>
		public void CatmullRom (float2 p0, float2 p1, float2 p2, float2 p3, Color color) {
			CatmullRom(xy ? new float3(p0, 0) : new float3(p0.x, 0, p0.y), xy ? new float3(p1, 0) : new float3(p1.x, 0, p1.y), xy ? new float3(p2, 0) : new float3(p2.x, 0, p2.y), xy ? new float3(p3, 0) : new float3(p3.x, 0, p3.y), color);
		}

		/// <summary>\copydocref{Arrow(float3,float3)}</summary>
		public void Arrow (float3 from, float3 to, Color color) {
			ArrowRelativeSizeHead(from, to, xy ? XY_UP : XZ_UP, 0.2f, color);
		}
		/// <summary>\copydocref{Arrow(float2,float2)}</summary>
		public void Arrow (float2 from, float2 to, Color color) {
			Arrow(xy ? new float3(from, 0) : new float3(from.x, 0, from.y), xy ? new float3(to, 0) : new float3(to.x, 0, to.y), color);
		}
		/// <summary>\copydocref{Arrow(float3,float3,float3,float)}</summary>
		public void Arrow (float3 from, float3 to, float3 up, float headSize, Color color) {
			draw.Arrow(from, to, up, headSize, color);
		}
		/// <summary>\copydocref{Arrow(float2,float2,float2,float)}</summary>
		public void Arrow (float2 from, float2 to, float2 up, float headSize, Color color) {
			Arrow(xy ? new float3(from, 0) : new float3(from.x, 0, from.y), xy ? new float3(to, 0) : new float3(to.x, 0, to.y), xy ? new float3(up, 0) : new float3(up.x, 0, up.y), headSize, color);
		}
		/// <summary>\copydocref{ArrowRelativeSizeHead(float3,float3,float3,float)}</summary>
		public void ArrowRelativeSizeHead (float3 from, float3 to, float3 up, float headFraction, Color color) {
			draw.ArrowRelativeSizeHead(from, to, up, headFraction, color);
		}
		/// <summary>\copydocref{ArrowRelativeSizeHead(float2,float2,float2,float)}</summary>
		public void ArrowRelativeSizeHead (float2 from, float2 to, float2 up, float headFraction, Color color) {
			ArrowRelativeSizeHead(xy ? new float3(from, 0) : new float3(from.x, 0, from.y), xy ? new float3(to, 0) : new float3(to.x, 0, to.y), xy ? new float3(up, 0) : new float3(up.x, 0, up.y), headFraction, color);
		}
		/// <summary>\copydocref{Arrowhead(float3,float3,float)}</summary>
		public void Arrowhead (float3 center, float3 direction, float radius, Color color) {
			Arrowhead(center, direction, xy ? XY_UP : XZ_UP, radius, color);
		}

		/// <summary>\copydocref{Arrowhead(float2,float2,float)}</summary>
		public void Arrowhead (float2 center, float2 direction, float radius, Color color) {
			Arrowhead(xy ? new float3(center, 0) : new float3(center.x, 0, center.y), xy ? new float3(direction, 0) : new float3(direction.x, 0, direction.y), radius, color);
		}

		/// <summary>\copydocref{Arrowhead(float3,float3,float3,float)}</summary>
		public void Arrowhead (float3 center, float3 direction, float3 up, float radius, Color color) {
			draw.Arrowhead(center, direction, up, radius, color);
		}

		/// <summary>\copydocref{Arrowhead(float2,float2,float2,float)}</summary>
		public void Arrowhead (float2 center, float2 direction, float2 up, float radius, Color color) {
			Arrowhead(xy ? new float3(center, 0) : new float3(center.x, 0, center.y), xy ? new float3(direction, 0) : new float3(direction.x, 0, direction.y), xy ? new float3(up, 0) : new float3(up.x, 0, up.y), radius, color);
		}

		/// <summary>\copydocref{ArrowheadArc(float3,float3,float,float)}</summary>
		public void ArrowheadArc (float3 origin, float3 direction, float offset, float width, Color color) {
			if (!math.any(direction)) return;
			if (offset < 0) throw new System.ArgumentOutOfRangeException(nameof(offset));
			if (offset == 0) return;
			draw.PushColor(color);

			var rot = Quaternion.LookRotation(direction, xy ? XY_UP : XZ_UP);
			PushMatrix(Matrix4x4.TRS(origin, rot, Vector3.one));
			var a1 = math.PI * 0.5f - width * (0.5f * Mathf.Deg2Rad);
			var a2 = math.PI * 0.5f + width * (0.5f * Mathf.Deg2Rad);
			draw.CircleXZInternal(float3.zero, offset, a1, a2);
			var p1 = new float3(math.cos(a1), 0, math.sin(a1)) * offset;
			var p2 = new float3(math.cos(a2), 0, math.sin(a2)) * offset;
			const float sqrt2 = 1.4142f;
			var p3 = new float3(0, 0, sqrt2 * offset);
			Line(p1, p3);
			Line(p3, p2);
			PopMatrix();
			draw.PopColor();
		}
		/// <summary>\copydocref{ArrowheadArc(float3,float3,float,float)}</summary>
		public void ArrowheadArc (float3 origin, float3 direction, float offset, Color color) {
			ArrowheadArc(origin, direction, offset, 60, color);
		}
		/// <summary>\copydocref{ArrowheadArc(float2,float2,float,float)}</summary>
		public void ArrowheadArc (float2 origin, float2 direction, float offset, float width, Color color) {
			ArrowheadArc(xy ? new float3(origin, 0) : new float3(origin.x, 0, origin.y), xy ? new float3(direction, 0) : new float3(direction.x, 0, direction.y), offset, width, color);
		}
		/// <summary>\copydocref{ArrowheadArc(float2,float2,float,float)}</summary>
		public void ArrowheadArc (float2 origin, float2 direction, float offset, Color color) {
			ArrowheadArc(origin, direction, offset, 60, color);
		}
		/// <summary>\copydocref{WireTriangle(float3,float3,float3)}</summary>
		public void WireTriangle (float3 a, float3 b, float3 c, Color color) {
			draw.WireTriangle(a, b, c, color);
		}

		/// <summary>\copydocref{WireTriangle(float2,float2,float2)}</summary>
		public void WireTriangle (float2 a, float2 b, float2 c, Color color) {
			WireTriangle(xy ? new float3(a, 0) : new float3(a.x, 0, a.y), xy ? new float3(b, 0) : new float3(b.x, 0, b.y), xy ? new float3(c, 0) : new float3(c.x, 0, c.y), color);
		}

		/// <summary>\copydocref{WireRectangle(float3,quaternion,float2)}</summary>
		public void WireRectangle (float3 center, quaternion rotation, float2 size, Color color) {
			draw.WireRectangle(center, rotation, size, color);
		}
		/// <summary>\copydocref{WireRectangle(float2,quaternion,float2)}</summary>
		public void WireRectangle (float2 center, quaternion rotation, float2 size, Color color) {
			WireRectangle(xy ? new float3(center, 0) : new float3(center.x, 0, center.y), rotation, size, color);
		}
		/// <summary>\copydocref{WireTriangle(float3,quaternion,float)}</summary>
		public void WireTriangle (float3 center, quaternion rotation, float radius, Color color) {
			draw.WireTriangle(center, rotation, radius, color);
		}

		/// <summary>\copydocref{WireTriangle(float2,quaternion,float)}</summary>
		public void WireTriangle (float2 center, quaternion rotation, float radius, Color color) {
			WireTriangle(xy ? new float3(center, 0) : new float3(center.x, 0, center.y), rotation, radius, color);
		}

		/// <summary>\copydocref{SolidTriangle(float3,float3,float3)}</summary>
		public void SolidTriangle (float3 a, float3 b, float3 c, Color color) {
			draw.SolidTriangle(a, b, c, color);
		}

		/// <summary>\copydocref{SolidTriangle(float2,float2,float2)}</summary>
		public void SolidTriangle (float2 a, float2 b, float2 c, Color color) {
			SolidTriangle(xy ? new float3(a, 0) : new float3(a.x, 0, a.y), xy ? new float3(b, 0) : new float3(b.x, 0, b.y), xy ? new float3(c, 0) : new float3(c.x, 0, c.y), color);
		}

		/// <summary>\copydocref{Label2D(float3,string,float)}</summary>
		public void Label2D (float3 position, string text, float sizeInPixels, Color color) {
			draw.Label2D(position, text, sizeInPixels, color);
		}

		/// <summary>\copydocref{Label2D(float3,string,float)}</summary>
		public void Label2D (float3 position, string text, Color color) {
			Label2D(position, text, 14, color);
		}

		/// <summary>\copydocref{Label2D(float2,string,float)}</summary>
		public void Label2D (float2 position, string text, float sizeInPixels, Color color) {
			Label2D(xy ? new float3(position, 0) : new float3(position.x, 0, position.y), text, sizeInPixels, color);
		}

		/// <summary>\copydocref{Label2D(float2,string,float)}</summary>
		public void Label2D (float2 position, string text, Color color) {
			Label2D(position, text, 14, color);
		}

		/// <summary>\copydocref{Label2D(float3,string,float,LabelAlignment)}</summary>
		public void Label2D (float3 position, string text, float sizeInPixels, LabelAlignment alignment, Color color) {
			draw.Label2D(position, text, sizeInPixels, alignment, color);
		}

		/// <summary>\copydocref{Label2D(float2,string,float,LabelAlignment)}</summary>
		public void Label2D (float2 position, string text, float sizeInPixels, LabelAlignment alignment, Color color) {
			Label2D(xy ? new float3(position, 0) : new float3(position.x, 0, position.y), text, sizeInPixels, alignment, color);
		}

		/// <summary>\copydocref{Label2D(float3,FixedString32Bytes,float)}</summary>
		public void Label2D (float3 position, ref FixedString32Bytes text, float sizeInPixels, Color color) {
			draw.Label2D(position, ref text, sizeInPixels, color);
		}

		/// <summary>\copydocref{Label2D(float3,FixedString32Bytes,float)}</summary>
		public void Label2D (float3 position, ref FixedString32Bytes text, Color color) {
			Label2D(position, ref text, 14, color);
		}

		/// <summary>\copydocref{Label2D(float2,FixedString32Bytes,float)}</summary>
		public void Label2D (float2 position, ref FixedString32Bytes text, float sizeInPixels, Color color) {
			Label2D(xy ? new float3(position, 0) : new float3(position.x, 0, position.y), ref text, sizeInPixels, color);
		}

		/// <summary>\copydocref{Label2D(float2,FixedString32Bytes,float)}</summary>
		public void Label2D (float2 position, ref FixedString32Bytes text, Color color) {
			Label2D(position, ref text, 14, color);
		}

		/// <summary>\copydocref{Label2D(float3,FixedString64Bytes,float)}</summary>
		public void Label2D (float3 position, ref FixedString64Bytes text, float sizeInPixels, Color color) {
			draw.Label2D(position, ref text, sizeInPixels, color);
		}

		/// <summary>\copydocref{Label2D(float3,FixedString64Bytes,float)}</summary>
		public void Label2D (float3 position, ref FixedString64Bytes text, Color color) {
			Label2D(position, ref text, 14, color);
		}

		/// <summary>\copydocref{Label2D(float2,FixedString64Bytes,float)}</summary>
		public void Label2D (float2 position, ref FixedString64Bytes text, float sizeInPixels, Color color) {
			Label2D(xy ? new float3(position, 0) : new float3(position.x, 0, position.y), ref text, sizeInPixels, color);
		}

		/// <summary>\copydocref{Label2D(float2,FixedString64Bytes,float)}</summary>
		public void Label2D (float2 position, ref FixedString64Bytes text, Color color) {
			Label2D(position, ref text, 14, color);
		}

		/// <summary>\copydocref{Label2D(float3,FixedString128Bytes,float)}</summary>
		public void Label2D (float3 position, ref FixedString128Bytes text, float sizeInPixels, Color color) {
			draw.Label2D(position, ref text, sizeInPixels, color);
		}

		/// <summary>\copydocref{Label2D(float3,FixedString128Bytes,float)}</summary>
		public void Label2D (float3 position, ref FixedString128Bytes text, Color color) {
			Label2D(position, ref text, 14, color);
		}

		/// <summary>\copydocref{Label2D(float2,FixedString128Bytes,float)}</summary>
		public void Label2D (float2 position, ref FixedString128Bytes text, float sizeInPixels, Color color) {
			Label2D(xy ? new float3(position, 0) : new float3(position.x, 0, position.y), ref text, sizeInPixels, color);
		}

		/// <summary>\copydocref{Label2D(float2,FixedString128Bytes,float)}</summary>
		public void Label2D (float2 position, ref FixedString128Bytes text, Color color) {
			Label2D(position, ref text, 14, color);
		}

		/// <summary>\copydocref{Label2D(float3,FixedString512Bytes,float)}</summary>
		public void Label2D (float3 position, ref FixedString512Bytes text, float sizeInPixels, Color color) {
			draw.Label2D(position, ref text, sizeInPixels, color);
		}

		/// <summary>\copydocref{Label2D(float3,FixedString512Bytes,float)}</summary>
		public void Label2D (float3 position, ref FixedString512Bytes text, Color color) {
			Label2D(position, ref text, 14, color);
		}

		/// <summary>\copydocref{Label2D(float2,FixedString512Bytes,float)}</summary>
		public void Label2D (float2 position, ref FixedString512Bytes text, float sizeInPixels, Color color) {
			Label2D(xy ? new float3(position, 0) : new float3(position.x, 0, position.y), ref text, sizeInPixels, color);
		}

		/// <summary>\copydocref{Label2D(float2,FixedString512Bytes,float)}</summary>
		public void Label2D (float2 position, ref FixedString512Bytes text, Color color) {
			Label2D(position, ref text, 14, color);
		}

		/// <summary>\copydocref{Label2D(float3,FixedString32Bytes,float,LabelAlignment)}</summary>
		public void Label2D (float3 position, ref FixedString32Bytes text, float sizeInPixels, LabelAlignment alignment, Color color) {
			draw.Label2D(position, ref text, sizeInPixels, alignment, color);
		}

		/// <summary>\copydocref{Label2D(float2,FixedString32Bytes,float,LabelAlignment)}</summary>
		public void Label2D (float2 position, ref FixedString32Bytes text, float sizeInPixels, LabelAlignment alignment, Color color) {
			Label2D(xy ? new float3(position, 0) : new float3(position.x, 0, position.y), ref text, sizeInPixels, alignment, color);
		}

		/// <summary>\copydocref{Label2D(float3,FixedString64Bytes,float,LabelAlignment)}</summary>
		public void Label2D (float3 position, ref FixedString64Bytes text, float sizeInPixels, LabelAlignment alignment, Color color) {
			draw.Label2D(position, ref text, sizeInPixels, alignment, color);
		}

		/// <summary>\copydocref{Label2D(float2,FixedString64Bytes,float,LabelAlignment)}</summary>
		public void Label2D (float2 position, ref FixedString64Bytes text, float sizeInPixels, LabelAlignment alignment, Color color) {
			Label2D(xy ? new float3(position, 0) : new float3(position.x, 0, position.y), ref text, sizeInPixels, alignment, color);
		}

		/// <summary>\copydocref{Label2D(float3,FixedString128Bytes,float,LabelAlignment)}</summary>
		public void Label2D (float3 position, ref FixedString128Bytes text, float sizeInPixels, LabelAlignment alignment, Color color) {
			draw.Label2D(position, ref text, sizeInPixels, alignment, color);
		}

		/// <summary>\copydocref{Label2D(float2,FixedString128Bytes,float,LabelAlignment)}</summary>
		public void Label2D (float2 position, ref FixedString128Bytes text, float sizeInPixels, LabelAlignment alignment, Color color) {
			Label2D(xy ? new float3(position, 0) : new float3(position.x, 0, position.y), ref text, sizeInPixels, alignment, color);
		}

		/// <summary>\copydocref{Label2D(float3,FixedString512Bytes,float,LabelAlignment)}</summary>
		public void Label2D (float3 position, ref FixedString512Bytes text, float sizeInPixels, LabelAlignment alignment, Color color) {
			draw.Label2D(position, ref text, sizeInPixels, alignment, color);
		}

		/// <summary>\copydocref{Label2D(float2,FixedString512Bytes,float,LabelAlignment)}</summary>
		public void Label2D (float2 position, ref FixedString512Bytes text, float sizeInPixels, LabelAlignment alignment, Color color) {
			Label2D(xy ? new float3(position, 0) : new float3(position.x, 0, position.y), ref text, sizeInPixels, alignment, color);
		}

		/// <summary>\copydocref{Line(float3,float3)}</summary>
		public void Line (float3 a, float3 b, Color color) {
			draw.Line(a, b, color);
		}
		/// <summary>\copydocref{Circle(float2,float,float,float)}</summary>
		/// <param name="color">Color of the object</param>
		public void Circle (float2 center, float radius, float startAngle, float endAngle, Color color) {
			Circle(xy ? new float3(center, 0) : new float3(center.x, 0, center.y), radius, startAngle, endAngle, color);
		}
		/// <summary>\copydocref{Circle(float2,float,float,float)}</summary>
		/// <param name="color">Color of the object</param>
		public void Circle (float2 center, float radius, Color color) {
			Circle(center, radius, 0f, 2 * math.PI, color);
		}
		/// <summary>\copydocref{Circle(float3,float,float,float)}</summary>
		/// <param name="color">Color of the object</param>
		public void Circle (float3 center, float radius, float startAngle, float endAngle, Color color) {
			draw.PushColor(color);
			if (xy) {
				draw.PushMatrix(XZ_TO_XY_MATRIX);
				draw.CircleXZInternal(new float3(center.x, center.z, center.y), radius, startAngle, endAngle);
				draw.PopMatrix();
			} else {
				draw.CircleXZInternal(center, radius, startAngle, endAngle);
			}
			draw.PopColor();
		}
		/// <summary>\copydocref{Circle(float3,float,float,float)}</summary>
		/// <param name="color">Color of the object</param>
		public void Circle (float3 center, float radius, Color color) {
			Circle(center, radius, 0f, 2 * math.PI, color);
		}
		/// <summary>\copydocref{SolidCircle(float2,float,float,float)}</summary>
		public void SolidCircle (float2 center, float radius, float startAngle, float endAngle, Color color) {
			SolidCircle(xy ? new float3(center, 0) : new float3(center.x, 0, center.y), radius, startAngle, endAngle, color);
		}

		/// <summary>\copydocref{SolidCircle(float2,float,float,float)}</summary>
		public void SolidCircle (float2 center, float radius, Color color) {
			SolidCircle(center, radius, 0f, 2 * math.PI, color);
		}

		/// <summary>\copydocref{SolidCircle(float3,float,float,float)}</summary>
		/// <param name="color">Color of the object</param>
		public void SolidCircle (float3 center, float radius, float startAngle, float endAngle, Color color) {
			draw.PushColor(color);
			if (xy) draw.PushMatrix(XZ_TO_XY_MATRIX);
			draw.SolidCircleXZInternal(xy ? new float3(center.x, center.z, center.y) : center, radius, startAngle, endAngle);
			if (xy) draw.PopMatrix();
			draw.PopColor();
		}

		/// <summary>\copydocref{SolidCircle(float3,float,float,float)}</summary>
		/// <param name="color">Color of the object</param>
		public void SolidCircle (float3 center, float radius, Color color) {
			SolidCircle(center, radius, 0f, 2 * math.PI, color);
		}

		/// <summary>\copydocref{WirePill(float2,float2,float)}</summary>
		/// <param name="color">Color of the object</param>
		public void WirePill (float2 a, float2 b, float radius, Color color) {
			WirePill(a, b - a, math.length(b - a), radius, color);
		}
		/// <summary>\copydocref{WirePill(float2,float2,float,float)}</summary>
		/// <param name="color">Color of the object</param>
		public void WirePill (float2 position, float2 direction, float length, float radius, Color color) {
			draw.PushColor(color);
			direction = math.normalizesafe(direction);

			if (radius <= 0) {
				Line(position, position + direction * length);
			} else if (length <= 0 || math.all(direction == 0)) {
				Circle(position, radius);
			} else {
				float4x4 m;
				if (xy) {
					m = new float4x4(
						new float4(direction, 0, 0),
						new float4(math.cross(new float3(direction, 0), XY_UP), 0),
						new float4(0, 0, 1, 0),
						new float4(position, 0, 1)
						);
				} else {
					m = new float4x4(
						new float4(direction.x, 0, direction.y, 0),
						new float4(0, 1, 0, 0),
						new float4(math.cross(new float3(direction.x, 0, direction.y), XZ_UP), 0),
						new float4(position.x, 0, position.y, 1)
						);
				}
				draw.PushMatrix(m);
				Circle(new float2(0, 0), radius, 0.5f * math.PI, 1.5f * math.PI);
				Line(new float2(0, -radius), new float2(length, -radius));
				Circle(new float2(length, 0), radius, -0.5f * math.PI, 0.5f * math.PI);
				Line(new float2(0, radius), new float2(length, radius));
				draw.PopMatrix();
			}
			draw.PopColor();
		}
		/// <summary>\copydocref{Polyline(List<Vector2>,bool)}</summary>
		[BurstDiscard]
		public void Polyline (List<Vector2> points, bool cycle, Color color) {
			draw.PushColor(color);
			for (int i = 0; i < points.Count - 1; i++) {
				Line(points[i], points[i+1]);
			}
			if (cycle && points.Count > 1) Line(points[points.Count - 1], points[0]);
			draw.PopColor();
		}
		/// <summary>\copydocref{Polyline(List<Vector2>,bool)}</summary>
		[BurstDiscard]
		public void Polyline (List<Vector2> points, Color color) {
			Polyline(points, false, color);
		}
		/// <summary>\copydocref{Polyline(Vector2[],bool)}</summary>
		[BurstDiscard]
		public void Polyline (Vector2[] points, bool cycle, Color color) {
			draw.PushColor(color);
			for (int i = 0; i < points.Length - 1; i++) {
				Line(points[i], points[i+1]);
			}
			if (cycle && points.Length > 1) Line(points[points.Length - 1], points[0]);
			draw.PopColor();
		}
		/// <summary>\copydocref{Polyline(Vector2[],bool)}</summary>
		[BurstDiscard]
		public void Polyline (Vector2[] points, Color color) {
			Polyline(points, false, color);
		}
		/// <summary>\copydocref{Polyline(float2[],bool)}</summary>
		[BurstDiscard]
		public void Polyline (float2[] points, bool cycle, Color color) {
			draw.PushColor(color);
			for (int i = 0; i < points.Length - 1; i++) {
				Line(points[i], points[i+1]);
			}
			if (cycle && points.Length > 1) Line(points[points.Length - 1], points[0]);
			draw.PopColor();
		}
		/// <summary>\copydocref{Polyline(float2[],bool)}</summary>
		[BurstDiscard]
		public void Polyline (float2[] points, Color color) {
			Polyline(points, false, color);
		}
		/// <summary>\copydocref{Polyline(NativeArray<float2>,bool)}</summary>
		public void Polyline (NativeArray<float2> points, bool cycle, Color color) {
			draw.PushColor(color);
			for (int i = 0; i < points.Length - 1; i++) {
				Line(points[i], points[i+1]);
			}
			if (cycle && points.Length > 1) Line(points[points.Length - 1], points[0]);
			draw.PopColor();
		}
		/// <summary>\copydocref{Polyline(NativeArray<float2>,bool)}</summary>
		public void Polyline (NativeArray<float2> points, Color color) {
			Polyline(points, false, color);
		}
		/// <summary>\copydocref{Cross(float2,float)}</summary>
		public void Cross (float2 position, float size, Color color) {
			draw.PushColor(color);
			size *= 0.5f;
			Line(position - new float2(size, 0), position + new float2(size, 0));
			Line(position - new float2(0, size), position + new float2(0, size));
			draw.PopColor();
		}
		/// <summary>\copydocref{Cross(float2,float)}</summary>
		public void Cross (float2 position, Color color) {
			Cross(position, 1, color);
		}
		/// <summary>\copydocref{WireRectangle(float3,float2)}</summary>
		public void WireRectangle (float3 center, float2 size, Color color) {
			draw.WirePlane(center, xy ? XY_TO_XZ_ROTATION : XZ_TO_XZ_ROTATION, size, color);
		}
		/// <summary>\copydocref{WireRectangle(Rect)}</summary>
		public void WireRectangle (Rect rect, Color color) {
			draw.PushColor(color);
			float2 min = rect.min;
			float2 max = rect.max;

			Line(new float2(min.x, min.y), new float2(max.x, min.y));
			Line(new float2(max.x, min.y), new float2(max.x, max.y));
			Line(new float2(max.x, max.y), new float2(min.x, max.y));
			Line(new float2(min.x, max.y), new float2(min.x, min.y));
			draw.PopColor();
		}
		/// <summary>\copydocref{SolidRectangle(Rect)}</summary>
		public void SolidRectangle (Rect rect, Color color) {
			draw.SolidPlane(new float3(rect.center.x, rect.center.y, 0.0f), xy ? XY_TO_XZ_ROTATION : XZ_TO_XZ_ROTATION, new float2(rect.width, rect.height), color);
		}

		/// <summary>\copydocref{WireGrid(float2,int2,float2)}</summary>
		/// <param name="color">Color of the object</param>
		public void WireGrid (float2 center, int2 cells, float2 totalSize, Color color) {
			draw.WireGrid(xy ? new float3(center, 0) : new float3(center.x, 0, center.y), xy ? XY_TO_XZ_ROTATION : XZ_TO_XZ_ROTATION, cells, totalSize, color);
		}
		/// <summary>\copydocref{WireGrid(float3,int2,float2)}</summary>
		/// <param name="color">Color of the object</param>
		public void WireGrid (float3 center, int2 cells, float2 totalSize, Color color) {
			draw.WireGrid(center, xy ? XY_TO_XZ_ROTATION : XZ_TO_XZ_ROTATION, cells, totalSize, color);
		}
	}
}
#if MODULE_RENDER_PIPELINES_HIGH_DEFINITION
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

namespace Drawing {
/// <summary>Custom High Definition Render Pipeline Render Pass for ALINE</summary>
class AlineHDRPCustomPass : CustomPass {
#if MODULE_RENDER_PIPELINES_HIGH_DEFINITION_9_0_0_OR_NEWER
bool disabledDepth = false;

		protected override void Setup (ScriptableRenderContext renderContext, CommandBuffer cmd) {
			this.targetColorBuffer = TargetBuffer.Camera;
			this.targetDepthBuffer = TargetBuffer.Camera;
			disabledDepth = false;
		}

		protected override void Execute (CustomPassContext context) {
			UnityEngine.Profiling.Profiler.BeginSample("ALINE");
			if (!disabledDepth && context.cameraColorBuffer.isMSAAEnabled != context.cameraDepthBuffer.isMSAAEnabled) {
				Debug.LogWarning("ALINE: Cannot draw depth-tested gizmos due to limitations in Unity's high-definition render pipeline combined with MSAA. Typically this is caused by enabling Camera -> Frame Setting Overrides -> MSAA Within Forward.\n\nDepth-testing for gizmos will stay disabled until you disable this type of MSAA and recompile scripts.");
				// At this point, we only get access to the MSAA depth buffer, not the resolved non-MSAA depth buffer.
				// If we try to use the depth buffer, we will get an error message from Unity:
				// "Color and Depth buffer MSAA flags doesn't match, no rendering will occur."
				// Rendering seems to somewhat work even though that error is logged, but there are a lot of rendering artifacts.
				// So we will just disable depth testing.
				//
				// In the HDRenderPipeline.RenderGraph.cs script, the resolved non-msaa depth buffer is accessible, and this is the one
				// that Unity's own gizmos rendering code uses. However, Unity does not expose this buffer to custom render passes.
				disabledDepth = true;
				this.targetDepthBuffer = TargetBuffer.None;
			}
			DrawingManager.instance.SubmitFrame(context.hdCamera.camera, new DrawingData.CommandBufferWrapper { cmd = context.cmd }, true);
			UnityEngine.Profiling.Profiler.EndSample();
		}
#else
protected override void Execute (ScriptableRenderContext context, CommandBuffer cmd, HDCamera camera, CullingResults cullingResult) {
UnityEngine.Profiling.Profiler.BeginSample("ALINE");
DrawingManager.instance.SubmitFrame(camera.camera, new DrawingData.CommandBufferWrapper { cmd = cmd }, true);
UnityEngine.Profiling.Profiler.EndSample();
}
#endif

		protected override void Cleanup () {
		}
	}
}
#endif
```

Example for ISystem staring point Don't use ```[BurstCompile]``` with Aline it's not supported (IJobEntity is not given, but you can figure it out):
```csharp
using _src.Scripts.ZBuildings.ZBuildings.Data;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;


namespace _src.Scripts.ZBuildings.ZBuildings.Editor // Or your preferred editor namespace
{
[WorldSystemFilter(WorldSystemFilterFlags.Editor | WorldSystemFilterFlags.Default)]
[UpdateInGroup(typeof(PresentationSystemGroup))] // Or another suitable editor update group
public partial struct ZRoadVisualizerSystem : ISystem
{

        public void OnCreate(ref SystemState state)
        {
            // No specific creation needed for this simple visualizer
        }

        
        public void OnUpdate(ref SystemState state)
        {
#if ALINE
// Ensure there's at least one RoadComponent to avoid unnecessary builder creation
// This is a micro-optimization, can be skipped if preferred.
var query = SystemAPI.QueryBuilder().WithAll<RoadComponent, LocalToWorld>().Build();
if (query.IsEmpty)
{
return;
}

            var builder = Drawing.DrawingManager.GetBuilder();


            // Optional: Pass editor camera rotation if you plan to add labels that need to face the camera
            // quaternion editorCamRot = quaternion.identity;
            // if (UnityEditor.SceneView.lastActiveSceneView != null)
            // {
            //    editorCamRot = UnityEditor.SceneView.lastActiveSceneView.camera.transform.rotation;
            // }

            var job = new ZRoadVisualizerJob
            {
                Drawing = builder
                // EditorCameraRotation = editorCamRot // If using labels
            };

            state.Dependency = job.ScheduleParallel(query, state.Dependency); // Schedule with query
            builder.DisposeAfter(state.Dependency);
#endif
}


        public void OnDestroy(ref SystemState state)
        {
        }
    }
}
```