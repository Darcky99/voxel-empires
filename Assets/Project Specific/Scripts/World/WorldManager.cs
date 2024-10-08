using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using Stateless;
using System.Threading.Tasks;
using Unity.Jobs;
using System.Collections;
using System;
using Unity.Mathematics;
using System.Linq;
using Cysharp.Threading.Tasks;
using Unity.Collections;

namespace World
{
    public class WorldManager : Singleton<WorldManager>
    {
        public GameConfig _GameConfig => GameConfig.Instance;

        public event EventHandler<WorldState> StateChanged;

        public Dictionary<int3, Chunk> LoadedChunks => _LoadedChunks;
        public Vector3 CameraPosition => _CameraTransform.position;

        private StateMachine<WorldState, WorldTrigger> _fsm;
        private Dictionary<int3, Chunk> _LoadedChunks;
        private List<int3> _ChunksToDraw;
        private Task _CheckDraw;
        //private Dictionary<int, >

        [Title("Handlers")]
        [SerializeField] private WorldController _ChunkRenderer;
        [Title("Configuration")]
        [SerializeField] private Transform _CameraTransform;

        public void Initialize()
        {
            _LoadedChunks = new Dictionary<int3, Chunk>();
            _ChunksToDraw = new List<int3>();

            _fsm = new StateMachine<WorldState, WorldTrigger>(WorldState.Waiting);

            _fsm.Configure(WorldState.Waiting)
            .Permit(WorldTrigger.Load, WorldState.Loading);

            _fsm.Configure(WorldState.Loading)
            .Permit(WorldTrigger.Draw, WorldState.Drawing);

            _fsm.Configure(WorldState.Drawing)
            .Permit(WorldTrigger.Wait, WorldState.Waiting);

            _fsm.OnTransitionCompleted(WorldState_OnTransitionCompleted);
        }

        private void WorldState_OnTransitionCompleted(StateMachine<WorldState, WorldTrigger>.Transition transition)
        {
            StateChanged?.Invoke(this, transition.Destination);
        }

        //We need methods to generate a list of chunks to load (ring based, or whatever)
        //Loading methods, schedules Jobs.
        //Methods to instance new chunks, assign them this data.
        //Methods to get chunk data and generate meshes.
        //Method to set that mesh on a chunk.

        public async UniTask Load(NativeList<int3> toLoad)
        {
            _fsm.Fire(WorldTrigger.Load);
            for (int i = 0; i < toLoad.Length; i++)
            {
                ITerrainGeneration terrainJob = new ITerrainGeneration(toLoad[i]);
                JobHandle handler = terrainJob.Schedule();
                handler.Complete();
                int3 id = toLoad[i];
                Chunk chunk = new Chunk(id);
                if (terrainJob.IsEmpty[0])
                {
                    chunk.SetVoxelMap(new byte[] { 0 });
                }
                else
                {
                    chunk.SetVoxelMap(terrainJob.FlatVoxelMap.ToArray());
                }
                _LoadedChunks[id] = chunk;
                terrainJob.Dispose();
                if (i % 60 == 0 && i != 0)
                {
                    await Task.Yield();
                }
            }
        }
        public bool TryGetChunk(int3 chunkID, out Chunk chunk)
        {
            bool exists = LoadedChunks.TryGetValue(chunkID, out chunk);
            if (!exists)
            {
                chunk = new Chunk(chunkID);
                chunk.SetVoxelMap(new byte[] { 0 });
            }
            return exists;
        }

        private async Task DrawRenderArea()
        {
            //float time = Time.realtimeSinceStartup;
            int renderDistance = _GameConfig.GraphicsConfiguration.RenderDistance;
            for (int i = 0; i <= renderDistance; i++)
            {
                _ChunksToDraw = ChunkUtils.GetChunkByRing(CameraPosition, i).ToList();
                if (_ChunksToDraw.Count == 0)
                    continue;

                for (int j = 0; j < _ChunksToDraw.Count; j++)
                {
                    //int3 key = new int3(_ChunksToDraw[j].x, _ChunksToDraw[j].y, _ChunksToDraw[j].z);
                    bool exists = TryGetChunk(_ChunksToDraw[j], out Chunk chunk);
                    if (exists && chunk.ChunkState != eChunkState.Drawn)
                    {
                        chunk.RequestMesh();
                    }
                    if (j != 0 && j % 30 == 0)
                    {
                        await Task.Yield();
                    }
                }
                _ChunksToDraw.Clear();
            }
            //Debug.Log($"Time to draw everything: {Time.realtimeSinceStartup - time}");
        }
        public void CheckToDraw()
        {
            if (_CheckDraw != null && !_CheckDraw.IsCompleted)
            {
                // _StarOverFlag = true;
            }
            else
            {
                _CheckDraw = DrawRenderArea();
            }
        }
    }
}