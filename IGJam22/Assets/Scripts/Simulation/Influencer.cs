using System.Collections.Generic;
using UnityEngine;

namespace Simulation
{
    public enum Influence
    {
        None,
        Weight,
        Population,
        Pollution
    }
    
    [CreateAssetMenu(fileName = "Influencer", menuName = "Simulation/Influencer", order = 1)]
    public class Influencer : ScriptableObject
    {
        public Influence targetID;
        public Influence sourceAID;
        public Influence sourceBID;
        public List<string> argDescription;
        public List<float> args;

        public ComputeShader shader;
        public string kernel;
        public int kernelID
        {
            get
            {
                _kernelID ??= shader.FindKernel(kernel);
                return _kernelID ?? -1;
            }
        }

        private int? _kernelID;
    }
}