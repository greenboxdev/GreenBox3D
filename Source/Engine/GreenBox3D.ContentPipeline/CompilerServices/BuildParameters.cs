﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenBox3D.ContentPipeline.CompilerServices
{
    public class BuildParameters : Dictionary<string, object>
    {
        public BuildParameters Filter(string prefix)
        {
            BuildParameters parameters = new BuildParameters();

            foreach (KeyValuePair<string, object> kvp in this)
                if (kvp.Key.StartsWith(prefix))
                    parameters.Add(kvp.Key.Remove(0, prefix.Length), kvp.Value);

            return parameters;
        }

        public T GetValue<T>(string key, T defaultValue = default(T))
        {
            object value;

            if (TryGetValue(key, out value))
                return (T)value;

            return defaultValue;
        }
    }
}
