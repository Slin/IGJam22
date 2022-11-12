using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Simulation
{
    public class Simulation : MonoBehaviour, ISimulation
    {
        public int width;
        public Texture2D worldMask;
        public ComputeShader baseShader;
        // Weight, Pollution, population, nature?
        private Dictionary<Influence, RenderTexture> _values;

        private RenderTexture _tmpTex;

        public RenderTexture debugTexture;
        public Material debugMaterial;

        private Texture2D _debugTextureCPU;
        private float[] _rawDebugTextureData;
        private RenderTexture _mask;

        public List<Influencer> influencers;

        private List<Influence> requestedBuffers;
        private Dictionary<Influence, (float[], float)> _outputBuffers;
        private Texture2D _copyBuffer;
        private NativeArray<float> _copyBufferArray;

        private int _sourceABufferID;
        private int _sourceBBufferID;
        private int _targetBufferID;
        private int _maskBufferID;

        private int _timeStepPropID;
        private List<int> _propIDs;
        private int _copyKernelID;
        
        private bool _loadInProgress;
        private float _t1;

        private void Awake()
        {
            if (width % 8 != 0)
            {
                Debug.LogError("Simulationsize needs to be a multiple of 8!");
            }
            _outputBuffers = new Dictionary<Influence, (float[], float)>();
            _sourceABufferID = Shader.PropertyToID("sourceA");
            _sourceBBufferID = Shader.PropertyToID("sourceB");
            _targetBufferID = Shader.PropertyToID("target");
            _maskBufferID = Shader.PropertyToID("mask");
            _timeStepPropID = Shader.PropertyToID("timeStep");
            _propIDs = new List<int>()
            {
                Shader.PropertyToID("a"),
                Shader.PropertyToID("b"),
                Shader.PropertyToID("c"),
                Shader.PropertyToID("d")
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

            _values = new Dictionary<Influence, RenderTexture>();
            foreach(Influence influence in Enum.GetValues(typeof(Influence)))
            {
                RenderTexture rt = new RenderTexture(width, width, GraphicsFormat.R32_SFloat, GraphicsFormat.None);
                rt.enableRandomWrite = true;
                _values.Add(influence, rt);
            }
            
            _tmpTex = new RenderTexture(width, width, GraphicsFormat.R32_SFloat, GraphicsFormat.None);
            _tmpTex.enableRandomWrite = true;
            debugTexture = new RenderTexture(width, width, GraphicsFormat.R32_SFloat, GraphicsFormat.None);
            debugTexture.enableRandomWrite = true;
            debugMaterial.mainTexture = debugTexture;

            _rawDebugTextureData = new float[width * width];
            _debugTextureCPU = new Texture2D(width, width, TextureFormat.RFloat, false);

            InitMap();
            // SimulationStep(1.0f);
            ReadDebugTexture();
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
            
            foreach (Influence influence in requestedBuffers)
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
            if (!requestedBuffers.Contains(influence))
            {
                requestedBuffers.Add(influence);
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
        }

        private float GetPixelValue(int x, int y)
        {
            return _rawDebugTextureData[y * width + x];
        }

        private void InitMap()
        {
            List<Vector4> vectors = new List<Vector4>() { new Vector4(40, 40, 5.0f, 0) };
            int setVectorKernel = baseShader.FindKernel("setVectorValues");
            baseShader.SetInt(Shader.PropertyToID("ai"), 1);
            baseShader.SetVectorArray(Shader.PropertyToID("vectors"), vectors.ToArray());
            baseShader.SetTexture(setVectorKernel, _targetBufferID, _values[Influence.Population]);
            baseShader.Dispatch(setVectorKernel, 1, 1, 1);
            
            CopyTo(_values[Influence.Population], debugTexture, 1f);
        }

        private Vector2Int GetDispatchSize(int kernelID)
        {
            uint dimx, dimy, dimz;
            baseShader.GetKernelThreadGroupSizes(kernelID, out dimx, out dimy, out dimz);
            return new Vector2Int((int)(width / dimx), (int)(width / dimy));
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
            CopyTo(_values[Influence.Population], debugTexture, 1f);
            ReadDebugTexture();
        }
        
        private void CopyTo(RenderTexture source, RenderTexture target, float scale=1.0f)
        {
            Vector2Int dispatchSize = GetDispatchSize(_copyKernelID);
            baseShader.SetFloat(_propIDs[0], scale);
            baseShader.SetTexture(_copyKernelID, _sourceABufferID, source);
            baseShader.SetTexture(_copyKernelID, _targetBufferID, target);
            baseShader.Dispatch(_copyKernelID, dispatchSize.x, dispatchSize.y, 1);
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
            shader.SetFloat(_timeStepPropID, timestep);
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
    }
}