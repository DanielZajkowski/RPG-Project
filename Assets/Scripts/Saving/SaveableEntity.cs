using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using RPG.Core;
using System.Collections.Generic;
using System;

namespace RPG.Saving
{
    [ExecuteAlways]
    public class SaveableEntity : MonoBehaviour
    {
        [SerializeField] string uniqueIdentifier = "";
        static Dictionary<string, SaveableEntity> globalLookup = new Dictionary<string, SaveableEntity>();

        public string GetUniqueIdentifier()
        {
            return uniqueIdentifier;
        }

        public object CaptureState()
        {
            Dictionary<string, object> state = new Dictionary<string, object>();
            foreach (ISaveable saveable in GetComponents<ISaveable>())
            {
                state[saveable.GetType().ToString()] = saveable.CaptureState();
            }
            return state;
        }

        public void RestoreState(object state)
        {
            Dictionary<string, object> stateDict = (Dictionary<string, object>)state;
            foreach (ISaveable saveable in GetComponents<ISaveable>())
            {
                string typeString = saveable.GetType().ToString();
                if (stateDict.ContainsKey(typeString))
                {
                    saveable.RestoreState(stateDict[typeString]);
                }
            }
        }

// If project has been packages up, this cose is essentailly removed entirely
#if UNITY_EDITOR
        private void Update() 
        {
            // Check if we playing and if we are in scene file vs being in a prefab
            if (Application.IsPlaying(gameObject))  return;
            if (string.IsNullOrEmpty(gameObject.scene.path))    return;

            // In order to change value that are stored into the scene file or prefab, it's need to use SerializedObject and SerializedProperty
            SerializedObject serializeObject = new SerializedObject(this);
            SerializedProperty property = serializeObject.FindProperty("uniqueIdentifier");

            if(string.IsNullOrEmpty(property.stringValue) || !IsUnique(property.stringValue))
            {
                property.stringValue = System.Guid.NewGuid().ToString();
                serializeObject.ApplyModifiedProperties();
            }

            globalLookup[property.stringValue] = this;
        }
#endif
        private bool IsUnique(string candidate)
        {
            // Check key exists in the dictionary
            if(!globalLookup.ContainsKey(candidate))
                return true;
            
            // Not pointing to ourselves
            if(globalLookup[candidate] == this) 
                return true;

            if(globalLookup[candidate] == null)
            {
                globalLookup.Remove(candidate);
                return true;
            }

            if(globalLookup[candidate].GetUniqueIdentifier() != candidate)
            {
                globalLookup.Remove(candidate);
                return true;
            }

            return false;
        }
    }
}