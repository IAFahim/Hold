// using CinemachineLink.CinemachineLink.Data;
// using Focuses.Focuses.Data;
// using Unity.Cinemachine;
// using Unity.Entities;
// using Unity.Transforms;
//
// namespace CinemachineLink.CinemachineLink
// {
//     public partial struct CinemachineLinkerSystem : ISystem
//     {
//         public void OnUpdate(ref SystemState state)
//         {
//             var entity = SystemAPI.GetSingleton<FocusSingletonComponent>().Entity;
//             var ltwCurrent = SystemAPI.GetComponent<LocalToWorld>(entity); 
//             CinemachineLinkerSingleton.Transform.SetLocalPositionAndRotation(
//                 ltwCurrent.Position,
//                 ltwCurrent.Rotation
//             );
//         }
//     }
// }