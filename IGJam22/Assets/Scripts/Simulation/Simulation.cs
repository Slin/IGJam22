using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace Simulation
{
    public class Simulation : MonoBehaviour, ISimulation
    {
        public int width;
        public Texture2D worldMask;
        public ComputeShader baseShader;
        public float migrationStart = 1500f;
        public float noiseSize = 0.1f;
        public float noiseScale = 30.0f;
        public float timeScale = 0.1f;
        
        // Weight, Pollution, population, nature?
        private Dictionary<Influence, RenderTexture> _values;

        private RenderTexture _tmpTex;
        private RenderTexture _tmpTex2;
        private RenderTexture _tmpTex3;
        private RenderTexture _tmpTex4;
        
        public RenderTexture _miniBuffer;
        private Texture2D _miniBufferTexture;

        public RenderTexture debugTexture;
        public Material debugMaterial;

        private Texture2D _debugTextureCPU;
        private float[] _rawDebugTextureData;
        private RenderTexture _mask;

        public List<Influencer> influencers;

        private List<Influence> _requestedBuffers;
        private Dictionary<Influence, (float[], float)> _outputBuffers;
        private Texture2D _copyBuffer;
        private NativeArray<float> _copyBufferArray;
        private NativeArray<float> _miniCopyBufferArray;
        private float[] _miniValBuffer = new float[GroupSize1D];

        private int _sourceABufferID;
        private int _sourceBBufferID;
        private int _targetBufferID;
        private int _maskBufferID;

        private int _timeStepPropID;
        private List<int> _propIDs;
        private List<int> _propiIDs;
        private int _copyKernelID;
        private int _setKernelID;
        private int _reduceKernelID;
        private int _combineReduceKernelID;
        private int _subtractAndClampID;
        private int _subtractID;
        private int _multiplyID;
        private int _setPointID;
        private int _subtractSourceID;
        private int _minID;
        private int _cubeID;
        private int _scaleID;
        private int _discretiseID;
        private int _subtract2ID;
        
        private bool _loadInProgress;
        private float _t1;

        private static readonly int GroupSize1D = 256;

        private void Awake()
        {
            if (width % 2 != 0)
            {
                Debug.LogError("Simulationsize needs to be a multiple of 2!");
            }
            _outputBuffers = new Dictionary<Influence, (float[], float)>();
            _sourceABufferID = Shader.PropertyToID("sourceA");
            _sourceBBufferID = Shader.PropertyToID("sourceB");
            _targetBufferID = Shader.PropertyToID("target");
            _maskBufferID = Shader.PropertyToID("mask");
            _timeStepPropID = Shader.PropertyToID("timeStep");
            _requestedBuffers = new List<Influence>();
            _propIDs = new List<int>()
            {
                Shader.PropertyToID("a"),
                Shader.PropertyToID("b"),
                Shader.PropertyToID("c"),
                Shader.PropertyToID("d")
            };
            _propiIDs = new List<int>()
            {
                Shader.PropertyToID("ai"),
                Shader.PropertyToID("bi"),
                Shader.PropertyToID("ci")
            };
            baseShader.SetInt(Shader.PropertyToID("width"), width);
            _copyBuffer = new Texture2D(width, width, TextureFormat.RFloat, false);
            _copyBufferArray = _copyBuffer.GetRawTextureData<float>();

            _mask = new RenderTexture(width, width, GraphicsFormat.R32_SFloat, GraphicsFormat.None);
            _mask.enableRandomWrite = true;
            int copyKernel = baseShader.FindKernel("sampleTexture");
            int textureID = Shader.PropertyToID("inTexture");
            baseShader.SetTexture(copyKernel, textureID, worldMask);
            baseShader.SetTexture(copyKernel, _targetBufferID, _mask);
            Vector2Int dispatchSize = GetDispatchSize(copyKernel);
            baseShader.Dispatch(copyKernel, dispatchSize.x, dispatchSize.y, 1);

            _copyKernelID = baseShader.FindKernel("copy");
            _setKernelID = baseShader.FindKernel("set");
            _reduceKernelID = baseShader.FindKernel("reduceGroups");
            _combineReduceKernelID = baseShader.FindKernel("combineReduceResults");
            _subtractAndClampID = baseShader.FindKernel("subtractAndClamp");
            _subtractID = baseShader.FindKernel("subtract");
            _multiplyID = baseShader.FindKernel("multiply");
            _setPointID = baseShader.FindKernel("setPoint");
            _subtractSourceID = baseShader.FindKernel("subtractSource");
            _minID = baseShader.FindKernel("minSource");
            _scaleID = baseShader.FindKernel("scale");
            _cubeID = baseShader.FindKernel("cube");
            _discretiseID = baseShader.FindKernel("discretise");
            _subtract2ID = baseShader.FindKernel("subtract2");

            _values = new Dictionary<Influence, RenderTexture>();
            foreach(Influence influence in Enum.GetValues(typeof(Influence)))
            {
                RenderTexture rt = new RenderTexture(width, width, GraphicsFormat.R32_SFloat, GraphicsFormat.None);
                rt.enableRandomWrite = true;
                _values.Add(influence, rt);
            }
            
            _tmpTex = new RenderTexture(width, width, GraphicsFormat.R32_SFloat, GraphicsFormat.None);
            _tmpTex.enableRandomWrite = true;
            _tmpTex2 = new RenderTexture(width, width, GraphicsFormat.R32_SFloat, GraphicsFormat.None);
            _tmpTex2.enableRandomWrite = true;
            _tmpTex3 = new RenderTexture(width, width, GraphicsFormat.R32_SFloat, GraphicsFormat.None);
            _tmpTex3.enableRandomWrite = true;
            _tmpTex4 = new RenderTexture(width, width, GraphicsFormat.R32_SFloat, GraphicsFormat.None);
            _tmpTex4.enableRandomWrite = true;
            
            _miniBuffer = new RenderTexture(GroupSize1D, 1, GraphicsFormat.R32_SFloat, GraphicsFormat.None);
            _miniBuffer.enableRandomWrite = true;
            _miniBufferTexture = new Texture2D(GroupSize1D, 1, TextureFormat.RFloat, false);
            _miniCopyBufferArray = _miniBufferTexture.GetRawTextureData<float>();
            
            debugTexture = new RenderTexture(width, width, GraphicsFormat.R32_SFloat, GraphicsFormat.None);
            debugTexture.enableRandomWrite = true;
            if(debugMaterial) debugMaterial.mainTexture = debugTexture;

            _rawDebugTextureData = new float[width * width];
            _debugTextureCPU = new Texture2D(width, width, TextureFormat.RFloat, false);

            InitMap();
            //ReadDebugTexture();
        }

        private void Update()
        {
            if (Input.GetMouseButton(0))
            {
                RaycastHit hit;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
                {
                    Vector3 hitpoint = hit.collider.transform.InverseTransformPoint(hit.point) / 10 + new Vector3(0.5f, 0, 0.5f);
                    Vector2Int samplePoint = new Vector2Int((int)(hitpoint.x * width), (int)(hitpoint.z * width));
                    Debug.Log($"Local hitpoint ({samplePoint.x}, {samplePoint.y}) " +
                              $"value: {GetPixelValue(samplePoint.x, samplePoint.y)}");
                }
            }
            
            foreach (Influence influence in _requestedBuffers)
            {
                QueryTexture(influence);
            }
        }

        private void QueryTexture(Influence influence)
        {
            if (!_outputBuffers.ContainsKey(influence))
            {
                _outputBuffers.Add(influence, (new float[width * width], -1.0f));
            }

            if (!_loadInProgress)
            {
                _t1 = Time.time;
                AsyncGPUReadback.RequestIntoNativeArray(ref _copyBufferArray, _values[Influence.Population], 0,
                    (req) =>
                    {
                        // Debug.Log($"Frame received {(Time.time - _t1) * 1000f}ms");
                        _copyBufferArray.CopyTo(_outputBuffers[influence].Item1);
                        _outputBuffers[influence] = (_outputBuffers[influence].Item1, Time.time);
                        _loadInProgress = false;
                    });
                _loadInProgress = true;
            }
        }

        /// <summary>
        /// Query a value from the simulation system
        /// </summary>
        /// <param name="influence">Which simulated influence to load</param>
        /// <param name="x">x coord (Island Coordinate space)</param>
        /// <param name="y">y coord (Island Coordinate space)</param>
        /// <param name="value">Output float value</param>
        /// <returns>Returns true if a value is currently available.</returns>
        public bool GetValue(Influence influence, int x, int y, out float value)
        {
            value = 0.0f;
            if (!_requestedBuffers.Contains(influence))
            {
                _requestedBuffers.Add(influence);
                return false;
            }

            float[] buffer;
            float staleness;
            (buffer, staleness) = _outputBuffers[influence];
            if (staleness < 0)
            {
                return false;
            }

            if (Time.time - staleness > 1f)
            {
                Debug.LogWarning($"Outputbuffer is old ({Time.time - staleness}s).");
            }

            Vector2Int simCoords = IslandCoordsToSimulationSpace(x, y);
            value = buffer[simCoords.y * width + simCoords.x];
            return true;
        }

        public void SetValue(Influence influence, int x, int y, float strength, float radius)
        {
            Vector2Int coords = IslandCoordsToSimulationSpace(x, y);
            Vector2Int dispatch = GetDispatchSize(_setPointID);
            baseShader.SetInt(_propiIDs[0], coords.x);
            baseShader.SetInt(_propiIDs[1], coords.y);
            baseShader.SetFloat(_propIDs[0], strength);
            baseShader.SetFloat(_propIDs[1], radius);
            baseShader.SetTexture(_setPointID, _targetBufferID, _values[influence]);
            baseShader.Dispatch(_setPointID, dispatch.x, dispatch.y, 1);
        }

        public RenderTexture GetTexture(Influence influence) => _values[influence];

        public Vector2Int IslandCoordsToSimulationSpace(int x, int y)
        {
            return new Vector2Int(x + width / 2, y + width / 2);
        }

        private void FixedUpdate()
        {
            SimulationStep(Time.deltaTime);
        }

        private void ReadDebugTexture()
        {
            RenderTexture.active = debugTexture;
            _debugTextureCPU.ReadPixels(new Rect(0, 0, width, width), 0, 0);
            _debugTextureCPU.Apply();
            RenderTexture.active = null;
            NativeArray<float> debugData = _debugTextureCPU.GetRawTextureData<float>();
            debugData.CopyTo(_rawDebugTextureData);
            // Debug.Log($"Debug Data: [{string.Join(',', debugData)}]");
        }

        private float GetPixelValue(int x, int y)
        {
            return _rawDebugTextureData[y * width + x];
        }

        private void InitMap()
        {
            List<Vector4> vectors = new List<Vector4>() { new Vector4(50, 80, 1000.0f, 0) };
            int setVectorKernel = baseShader.FindKernel("setVectorValues");
            baseShader.SetInt(_propiIDs[0], 1);
            baseShader.SetVectorArray(Shader.PropertyToID("vectors"), vectors.ToArray());
            baseShader.SetTexture(setVectorKernel, _targetBufferID, _values[Influence.Population]);
            baseShader.Dispatch(setVectorKernel, 1, 1, 1);
            
            //CopyTo(_values[Influence.Population], debugTexture, 1f);

            int noiseWidth = (int)(width * noiseSize);
            float[] noise = new float[noiseWidth * noiseWidth];
            for (var i = 0; i < noise.Length; i++)
            {
                noise[i] = Random.Range(-noiseScale, noiseScale);
            }

            Texture2D noiseTexture = new Texture2D(noiseWidth, noiseWidth, TextureFormat.RFloat, false);
            NativeArray<float> noiseBuffer = noiseTexture.GetRawTextureData<float>();
            noiseBuffer.CopyFrom(noise);
            noiseTexture.SetPixelData(noiseBuffer, 0);
            noiseTexture.Apply();
            int copyKernel = baseShader.FindKernel("sampleTexture");
            int textureID = Shader.PropertyToID("inTexture");
            baseShader.SetTexture(copyKernel, textureID, noiseTexture);
            baseShader.SetTexture(copyKernel, _targetBufferID, _values[Influence.Spirit]);
            Vector2Int dispatchSize = GetDispatchSize(copyKernel);
            baseShader.Dispatch(copyKernel, dispatchSize.x, dispatchSize.y, 1);
            
            
            // SetValue(Influence.Spirit, 20, 30, -1000, 5);

            // CopyTo(_values[Influence.Spirit], debugTexture);
            
            // Graphics.CopyTexture(_copyBuffer, _values[Influence.Spirit]);
            // _copyBuffer.Apply();
            // CopyTo(_copyBuffer, _values[Influence.Spirit]);
        }

        private Vector2Int GetDispatchSize(int kernelID)
        {
            uint dimx, dimy, dimz;
            baseShader.GetKernelThreadGroupSizes(kernelID, out dimx, out dimy, out dimz);
            return new Vector2Int(Mathf.CeilToInt(width / (float)dimx), Mathf.CeilToInt(width / (float)dimy));
        }
        
        private void SimulationStep(float timestep)
        {
            foreach (Influencer influencer in influencers)
            {
               Apply(timestep, influencer.shader, influencer.kernelID, 
                   influencer.sourceAID != Influence.None ? _values[influencer.sourceAID] : null, 
                   influencer.sourceBID != Influence.None ? _values[influencer.sourceBID] : null, 
                   _values[influencer.targetID], influencer.args);
            }

            Migrate(migrationStart);
            AddNoise();
            // Reduce(_values[Influence.Population]);
            //CopyTo(_values[Influence.Population], debugTexture, 1f);
            //ReadDebugTexture();
        }

        private int ctr = 0;
        private Texture2D noiseTexture;
        private void AddNoise()
        {
            int noiseWidth = (int)(width * 0.5f);
            float[] noise = new float[noiseWidth * noiseWidth];
            

            Texture2D noiseTexture = new Texture2D(noiseWidth, noiseWidth, TextureFormat.RFloat, false);
            NativeArray<float> noiseBuffer = noiseTexture.GetRawTextureData<float>();
            if (ctr == 0)
            {
                for (var i = 0; i < noise.Length; i++)
                {
                    noise[i] = Random.Range(-noiseScale, noiseScale);
                }
                noiseBuffer.CopyFrom(noise);
                noiseTexture.SetPixelData(noiseBuffer, 0);
                noiseTexture.Apply();
                ctr = 1000;
                Debug.Log("Noise Regen");
            }
            else
            {
                ctr--;
            }

            int copyKernel = baseShader.FindKernel("addConstant");
            int textureID = Shader.PropertyToID("inTexture");
            baseShader.SetFloat(_propIDs[0], 0.01f);
            baseShader.SetTexture(copyKernel, textureID, noiseTexture);
            baseShader.SetTexture(copyKernel, _targetBufferID, _values[Influence.Spirit]);
            Vector2Int dispatchSize = GetDispatchSize(copyKernel);
            baseShader.Dispatch(copyKernel, dispatchSize.x, dispatchSize.y, 1);
        }
        
        private void CopyTo(Texture source, RenderTexture target, float scale=1.0f, int length=0)
        {
            Vector2Int dispatchSize = new Vector2Int(length, 1);
            if (length == 0)
            {
                length = width * width;
                dispatchSize = GetDispatchSize(_copyKernelID);
            }
            
            baseShader.SetInt(_propiIDs[0], length);
            baseShader.SetFloat(_propIDs[0], scale);
            baseShader.SetTexture(_copyKernelID, _sourceABufferID, source);
            baseShader.SetTexture(_copyKernelID, _targetBufferID, target);
            baseShader.Dispatch(_copyKernelID, dispatchSize.x, dispatchSize.y, 1);
        }
        
        private void Set(RenderTexture target, float value)
        {
            Vector2Int dispatchSize = GetDispatchSize(_copyKernelID);
            
            baseShader.SetFloat(_propIDs[0], value);
            baseShader.SetTexture(_setKernelID, _targetBufferID, target);
            baseShader.Dispatch(_setKernelID, dispatchSize.x, dispatchSize.y, 1);
        }

        private void Apply(float timestep, ComputeShader shader, int kernelID, RenderTexture sourceA, RenderTexture sourceB, RenderTexture target, List<float> values)
        {
            bool useTmpTexture = sourceA == target;
            if (useTmpTexture)
            {
                CopyTo(target, _tmpTex);
            }

            shader.SetTexture(kernelID, _targetBufferID, useTmpTexture ? _tmpTex : target);
            Vector2Int dispatchSize = GetDispatchSize(kernelID);
            shader.SetTexture(kernelID, _sourceABufferID, sourceA);
            if (sourceB != null)
            {
                shader.SetTexture(kernelID, _sourceBBufferID, sourceB);
            }
            shader.SetTexture(kernelID, _maskBufferID, _mask);
            shader.SetFloat(_timeStepPropID, timestep * timeScale);
            for (int i = 0; i < values.Count; i++)
            {
                if (i < _propIDs.Count)
                {
                    shader.SetFloat(_propIDs[i], values[i]);
                }
            }
            shader.Dispatch(kernelID, dispatchSize.x, dispatchSize.y, 1);
            if (useTmpTexture)
            {
                CopyTo(_tmpTex, target);
            }
        }

        private void Migrate(float migrationThreshold)
        {
            Vector2Int dispatch = GetDispatchSize(_subtractAndClampID);
            
            baseShader.SetTexture(_subtractSourceID, _sourceABufferID, _values[Influence.Population]);
            baseShader.SetTexture(_subtractSourceID, _sourceBBufferID, _values[Influence.Spirit]);
            baseShader.SetTexture(_subtractSourceID, _targetBufferID, _tmpTex);
            baseShader.Dispatch(_subtractSourceID, dispatch.x, dispatch.y, 1);
            
            baseShader.SetFloat(_propIDs[0], migrationThreshold);
            baseShader.SetTexture(_subtractAndClampID, _targetBufferID, _tmpTex2);
            baseShader.SetTexture(_subtractAndClampID, _sourceABufferID, _tmpTex);
            baseShader.Dispatch(_subtractAndClampID, dispatch.x, dispatch.y, 1);
            
            baseShader.SetTexture(_minID, _sourceABufferID, _tmpTex2);
            baseShader.SetTexture(_minID, _sourceBBufferID, _values[Influence.Population]);
            baseShader.SetTexture(_minID, _targetBufferID, _tmpTex3);
            baseShader.Dispatch(_minID, dispatch.x, dispatch.y, 1);
            ApplyMask(_tmpTex3);
            CopyTo(_tmpTex3, _tmpTex);
            
            float volumeToMigrate = Reduce(_tmpTex);
            if (volumeToMigrate < 10)
            {
                return;
            }
            //Debug.Log($"Volume to migrate: {volumeToMigrate}");

            float x = -10000.0f;
            for (int i = 0; i < 10; i++)
            {
                CopyTo(_values[Influence.Spirit], _tmpTex);
                ApplyMask(_tmpTex);
                float p1 = GetVolumeAt(x);
                if (Mathf.Abs(p1 - volumeToMigrate) < 5)
                {
                    // Debug.Log($"Found offset: {x} ({Mathf.Abs(p1 - volumeToMigrate)})");
                    break;
                }
                else
                {
                    // Debug.Log($"Offset at {x}: {p1}");
                }
                CopyTo(_values[Influence.Spirit], _tmpTex);
                ApplyMask(_tmpTex);
                float p2 = GetVolumeAt(x + 1);
                x = x + (p1 - volumeToMigrate) / (p1 - p2);
                // Debug.Log($"(p1-p2): {(p1-p2)}, x: {x}, facr: {(p1 - volumeToMigrate)}");
            }

            // if (Mathf.Abs(x) < 10)
            // {
            //     return;
            // }
            // Debug.Log("Done Newton optimisation.");
            
            dispatch = GetDispatchSize(_subtractAndClampID);
            baseShader.SetFloat(_propIDs[0], x);
            baseShader.SetTexture(_subtractAndClampID, _targetBufferID, _tmpTex);
            baseShader.SetTexture(_subtractAndClampID, _sourceABufferID, _values[Influence.Spirit]);
            baseShader.Dispatch(_subtractAndClampID, dispatch.x, dispatch.y, 1);

            ApplyMask(_tmpTex);
            
            CopyTo(_tmpTex, _tmpTex4);
            float beforeCube = Reduce(_tmpTex);
            if (!float.IsNormal(beforeCube) || Mathf.Abs(beforeCube) < 10.0f)
            {
                return;
            }
            
            // Subtract from population
            dispatch = GetDispatchSize(_subtractID);
            baseShader.SetFloat(_propIDs[0], 1);
            baseShader.SetTexture(_subtractID, _sourceABufferID, _tmpTex3);
            baseShader.SetTexture(_subtractID, _targetBufferID, _values[Influence.Population]);
            baseShader.Dispatch(_subtractID, dispatch.x, dispatch.y, 1);
            
            baseShader.SetTexture(_cubeID, _targetBufferID, _tmpTex4);
            baseShader.Dispatch(_cubeID, dispatch.x, dispatch.y, 1);
            baseShader.SetTexture(_discretiseID, _targetBufferID, _tmpTex4);
            baseShader.Dispatch(_discretiseID, dispatch.x, dispatch.y, 1);
            
            CopyTo(_tmpTex4, _tmpTex);
            float afterCube = Reduce(_tmpTex);
            // Debug.Log($"Before: {beforeCube}, after: {afterCube}");
            
            baseShader.SetFloat(_propIDs[0], (beforeCube / afterCube));
            baseShader.SetTexture(_scaleID, _targetBufferID, _tmpTex4);
            baseShader.Dispatch(_scaleID, dispatch.x, dispatch.y, 1);
            
            // add to population
            dispatch = GetDispatchSize(_subtract2ID);
            baseShader.SetFloat(_propIDs[0], 1f);
            baseShader.SetTexture(_subtract2ID, _sourceABufferID, _tmpTex4);
            baseShader.SetTexture(_subtract2ID, _targetBufferID, _values[Influence.Population]);
            baseShader.Dispatch(_subtract2ID, dispatch.x, dispatch.y, 1);
            // // CopyTo(_values[Influence.Population], debugTexture);
        }

        private float GetVolumeAt(float offset)
        {
            Vector2Int dispatch = GetDispatchSize(_subtractAndClampID);
            baseShader.SetFloat(_propIDs[0], offset);
            baseShader.SetTexture(_subtractAndClampID, _sourceABufferID, _tmpTex);
            baseShader.SetTexture(_subtractAndClampID, _targetBufferID, _tmpTex2);
            baseShader.Dispatch(_subtractAndClampID, dispatch.x, dispatch.y, 1);
            return Reduce(_tmpTex2);
        }

        private float Reduce(RenderTexture target)
        {
            ApplyMask(target);
            // CopyTo(target, _tmpTex);
            int size = width * width;
            int reducedNumResults = Mathf.CeilToInt(size / (float)(GroupSize1D * 2.0f));
            baseShader.SetInt(_propiIDs[0], size);
            baseShader.SetTexture(_reduceKernelID, _targetBufferID, target);
            baseShader.Dispatch(_reduceKernelID, Mathf.CeilToInt(size / (float)GroupSize1D), 1, 1);
            
            CopyTo(target, _tmpTex2);
            baseShader.SetTexture(_combineReduceKernelID, _sourceABufferID, _tmpTex2);
            baseShader.SetTexture(_combineReduceKernelID, _targetBufferID, target);
            baseShader.Dispatch(_combineReduceKernelID, Mathf.CeilToInt(reducedNumResults / (float)GroupSize1D), 1, 1);
            
            baseShader.SetInt(_propiIDs[0], reducedNumResults);
            baseShader.Dispatch(_reduceKernelID, 1, 1, 1);
            CopyTo(target, _miniBuffer, 1.0f, GroupSize1D);
            
            RenderTexture.active = _miniBuffer;
            _miniBufferTexture.ReadPixels(new Rect(0, 0, GroupSize1D, 1), 0, 0);
            _miniBufferTexture.Apply();
            RenderTexture.active = null;
            _miniCopyBufferArray = _miniBufferTexture.GetRawTextureData<float>();
            _miniCopyBufferArray.CopyTo(_miniValBuffer);
            float res = _miniValBuffer[0];
            return res; //val[0];
        }

        private void ApplyMask(RenderTexture target)
        {
            Vector2Int dispatch = GetDispatchSize(_multiplyID);
            baseShader.SetTexture(_multiplyID, _maskBufferID, _mask);
            baseShader.SetTexture(_multiplyID, _targetBufferID, target);
            baseShader.Dispatch(_multiplyID, dispatch.x, dispatch.y, 1);
        }
    }
}