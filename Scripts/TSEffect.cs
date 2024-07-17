using System.Collections.Generic;
using System.IO;
using System;
using TS.TSEffect.Thread;
using TS.TSEffect.Container;
using TS.TSEffect.Util;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TS.TSEffect
{
    public static class TSEffect
    {
        private static List<Action> _PostDeserializations = new List<Action>();

        private static Dictionary<string, List<ExecutableThreadCore>> _ExeCoreDict = new Dictionary<string, List<ExecutableThreadCore>>();
        private static Dictionary<int, Dictionary<string, List<Component>>> _Subject = new Dictionary<int, Dictionary<string, List<Component>>>();
        
        private enum RECollectionOperationType
        {
            Remove = 1,
            Add = 2,
            Suspend = 3
        }
        private struct RECollectionLateOperation
        {
            public ThreadExecutor Executor;
            public RECollectionOperationType OperationType;
            public RECollectionLateOperation(ThreadExecutor executor, RECollectionOperationType callback_type)
            {
                Executor = executor;
                OperationType = callback_type;
            }
        }

        public static SortedSet<ThreadExecutor> RuntimeExecutors { get {  return _RuntimeExecutors; } }
        private static SortedSet<ThreadExecutor> _RuntimeExecutors = new SortedSet<ThreadExecutor>(new ThreadExecutorComparer());
        private static Dictionary<string, ThreadExecutor> _SuspendedExecutors = new Dictionary<string, ThreadExecutor>();
        private static List<RECollectionLateOperation> _RELateOperations = new List<RECollectionLateOperation>();

        public static TSEffectMetadata Metadata { get { return _Metadata; } }
        private static TSEffectMetadata _Metadata;
        public static bool IsMetadataLoaded { get { return _IsMetadataLoaded; } }
        private static bool _IsMetadataLoaded = false;

        /// <summary>
        /// Call this function to attach a component to the given channel.
        /// </summary>
        /// <typeparam name="T">T represents the type of the component.</typeparam>
        /// <param name="channel">The channel divides the component into a group.</param>
        /// <param name="com">The component listens to the execution of effects.</param>
        public static void AttachComponent<T>(int channel, T com) where T : Component
        {
            string target_type = typeof(T).FullName;

            bool has_a = true;
            Dictionary<string, List<Component>> a;
            if (!_Subject.TryGetValue(channel, out a))
            {
                a = new Dictionary<string, List<Component>>();
                _Subject.Add(channel, a);
                has_a = false;
            }

            bool has_b = has_a;
            List<Component> b;
            if (!has_a)
            {
                b = new List<Component>();
                a.Add(target_type, b);
            }
            else if (a.TryGetValue(target_type, out b))
            {
                a.Add(target_type, b);
            }
            else
            {
                b = new List<Component>();
                a.Add(target_type, b);
                has_b = false;
            }

            if (has_b)
            {
                if (!b.Contains(com))
                {
                    b.Add(com);
                    var etor = _RuntimeExecutors.GetEnumerator();
                    while (etor.MoveNext())
                    {
                        etor.Current.AddRuntimeTarget(com, channel);
                    }
                }
            }
            else
            {
                b.Add(com);
                var etor = _RuntimeExecutors.GetEnumerator();
                while (etor.MoveNext())
                {
                    etor.Current.AddRuntimeTarget(com, channel);
                }
            }
        }

        /// <summary>
        /// Call this function to detach a component from the given channel.
        /// </summary>
        /// <typeparam name="T">T represents the type of the component.</typeparam>
        /// <param name="channel">The channel divides the component into a group.</param>
        /// <param name="com">The component listens to the execution of effects.</param>
        public static void DetachComponent<T>(int channel, T com) where T : Component
        {
            string target_type = typeof(T).FullName;

            Dictionary<string, List<Component>> a;
            if (!_Subject.TryGetValue(channel, out a))
                return;

            List<Component> b;
            if (!a.TryGetValue(target_type, out b))
                return;
            
            b.Remove(com);
            var etor = _RuntimeExecutors.GetEnumerator();
            while (etor.MoveNext())
            {
                etor.Current.RemoveRuntimeTarget(com, channel);
            }
        }

        /// <summary>
        /// Call this function to compile effects and store them in the dictionary with the given ID.
        /// </summary>
        /// <param name="id">The ID is the key for the corresponding effects.</param>
        /// <param name="cont">The container stores the effects.</param>
        /// <returns>It returns whether the behavior succeed.</returns>
        public static bool AddExecutables(string id, EffectContainer cont)
        {
            if (_ExeCoreDict.ContainsKey(id))
            {
                return false;
            }
            else
            {
                List<ExecutableThreadCore> exes = new List<ExecutableThreadCore>();

                for (int i = 0; i < cont.Count; i++)
                {
                    var effect = cont[i].Effect;

                    if (!effect.Enable)
                    {
                        continue;
                    }
                    else
                    {
                        var reg = _Metadata.GetEffectReg(effect.GetType());
                        if (reg.Exist)
                        {
                            var etor = reg.ThreadReg.GetEnumerator();
                            while (etor.MoveNext())
                            {
                                var field_name = etor.Current.Key;
                                var threads = ThreadUtil.GetThreadsFromEffect(effect, field_name);
                                
                                List<NormalExeFuncBuilder> a = new List<NormalExeFuncBuilder>();
                                List<NormalExeFuncBuilder> b = new List<NormalExeFuncBuilder>();
                                List<TimeExeFuncBuilder> c = new List<TimeExeFuncBuilder>();
                                
                                effect.GetExeFuncBuilders(field_name, out a, out b, out c);

                                if (threads.Count == a.Count && threads.Count == b.Count && threads.Count == c.Count)
                                {
                                    for (int j = 0; j < threads.Count; j++)
                                    {
                                        if (threads[j].Enable)
                                            exes.Add(new ExecutableThreadCore(etor.Current.Value.DisplayName, effect.TargetType, threads[j], a[j], b[j], c[j]));
                                    }
                                }
                                else
                                {
                                    Debug.LogError(string.Format("Exception from {0}: field {1} contains {2} thread(s) but GenLogicBuilders generates {3}, {4} and {5} logic builders.", effect.GetType().FullName, field_name, threads.Count, a.Count, b.Count, c.Count));
                                }
                            }
                        }
                    }
                }

                if (exes.Count == 0)
                    return false;
                else
                    _ExeCoreDict.Add(id, exes);
                return true;
            }       
        }

        /// <summary>
        /// Call this function to execute effects.
        /// </summary>
        /// <param name="id">The ID determines what effects are going to be executed.</param>
        /// <param name="channel">The channel determines what components are going to listen to the execution.</param>
        /// <returns>It returns whether the behavior succeed.</returns>
        public static bool Execute(string id, int channel)
        {
            List<ExecutableThreadCore> list;
            if (_ExeCoreDict.TryGetValue(id, out list))
            {
                Dictionary<string, List<Component>> a = null;
                _Subject.TryGetValue(channel, out a);
                
                for (int i = 0; i < list.Count; i++)
                {
                    ThreadExecutor executor;
                    string executor_id = list[i].GetHashCode().ToString() + channel.ToString();
                    if (_SuspendedExecutors.TryGetValue(executor_id, out executor))
                    {
                        _SuspendedExecutors.Remove(executor_id);
                        executor.Desuspend();
                    }
                    else
                    {
                        executor = new ThreadExecutor(list[i], channel, executor_id);
                        executor.SetRemoveCallback(() =>
                        {
                            _RELateOperations.Add(new RECollectionLateOperation(executor, RECollectionOperationType.Remove));
                        });
                        executor.SetSuspendCallback(() => 
                        {
                            _RELateOperations.Add(new RECollectionLateOperation(executor, RECollectionOperationType.Suspend));
                        });

                        if (a != null)
                        {
                            List<Component> b;
                            if (a.TryGetValue(list[i].TargetType, out b))
                            {
                                executor.ResetRuntimeTargets(b);
                            }
                        }

                        _RELateOperations.Add(new RECollectionLateOperation(executor, RECollectionOperationType.Add));
                    }
                }
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Don't call it manually.
        /// This function will be called automatically at runtime.
        /// </summary>
        public static void ExecuteRELateOperations()
        {
            if (_RELateOperations.Count > 0)
            {
                for (int i = 0; i < _RELateOperations.Count; i++)
                {
                    switch (_RELateOperations[i].OperationType)
                    {
                        case RECollectionOperationType.Remove:
                            _RuntimeExecutors.Remove(_RELateOperations[i].Executor);
                            break;
                        case RECollectionOperationType.Add:
                            _RuntimeExecutors.Add(_RELateOperations[i].Executor);
                            break;
                        case RECollectionOperationType.Suspend:
                            string executor_id = _RELateOperations[i].Executor.ExecutorID;
                            if (_SuspendedExecutors.ContainsKey(executor_id))
                                _RuntimeExecutors.Remove(_RELateOperations[i].Executor);
                            else
                                _SuspendedExecutors.Add(executor_id, _RELateOperations[i].Executor);
                            break;
                    }
                }
                _RELateOperations.Clear();
            }
        }
        
        private static void CreateLocalMetadata(TSEffectMetadata meta)
        {
#if UNITY_EDITOR
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
                AssetDatabase.CreateFolder("Assets/Resources", "TSEffect");
            }
            else if (!AssetDatabase.IsValidFolder("Assets/Resources/TSEffect"))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "TSEffect");
            }
            AssetDatabase.CreateAsset(meta, "Assets/Resources/TSEffect/TSEffectMetadata.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif
        }

        /// <summary>
        /// Don't call it manually.
        /// This function will be called automatically at runtime when needed.
        /// </summary>
        public static void ReloadMetadata()
        {
            var res = Resources.Load<TSEffectMetadata>("TSEffect/TSEffectMetadata");
            if (res == null)
            {
                var meta = ScriptableObject.CreateInstance<TSEffectMetadata>();
                meta.RefreshTypes();
                CreateLocalMetadata(meta);
                _Metadata = meta;
            }
            else
            {
                res.RefreshTypes();
                _Metadata = res;
            }
#if UNITY_EDITOR
            _Metadata.BuiltinEffectCollectionPaths.Clear();
            string folder_path = Application.dataPath + "/Resources/TSEffect";
            if (Directory.Exists(folder_path))
            {
                string[] files = Directory.GetFiles(folder_path, "*.asset", SearchOption.AllDirectories);
                for (int i = 0; i < files.Length; i++)
                {
                    var path = files[i].Split('/', '.');
                    if (path.Length > 1)
                    {
                        string r_path = path[path.Length - 2];
                        r_path = r_path.Replace("\\", "/");
                        var coll = Resources.Load<TSEffectCollection>(r_path);
                        if (coll != null)
                        {
                            if (coll.IsBuiltin)
                            {
                                _Metadata.BuiltinEffectCollectionPaths.Add(r_path);
                                Resources.UnloadAsset(coll);
                            }
                        }
                    }
                }
            }
            
            EditorUtility.SetDirty(_Metadata);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif
            _IsMetadataLoaded = true;
        }
        private static bool LoadMetadata()
        {
            var res = Resources.Load<TSEffectMetadata>("TSEffect/TSEffectMetadata");
            _Metadata = res;
            if (res == null)
            {
                _IsMetadataLoaded = false;
                return false;
            }
            else
            {
                _IsMetadataLoaded = true;
                return true;
            }
        }

        /// <summary>
        /// Don't call it manually.
        /// This function will be called automatically before Awake() in MonoBehaviour.
        /// </summary>
        public static void LoadTSEffect()
        {
            if (!_IsMetadataLoaded)
            {
                if (!LoadMetadata())
                {
                    ReloadMetadata();
                }
            }
            _Metadata.InitCache();
            PostDeserialize();
        }

        /// <summary>
        /// Don't call it manually.
        /// This function will be called automatically before Awake() in MonoBehaviour.
        /// </summary>
        public static void LoadBuiltinEffectCollections()
        {
#if UNITY_EDITOR
            bool remove = false;
#endif
            for (int i = 0; i < _Metadata.BuiltinEffectCollectionPaths.Count; i++)
            {
                string path = _Metadata.BuiltinEffectCollectionPaths[i];
                var coll = Resources.Load<TSEffectCollection>(path);
                if (coll == null)
                {
#if UNITY_EDITOR
                    _Metadata.BuiltinEffectCollectionPaths.Remove(path);
                    remove = true;
#endif
                }
                else if (coll.IsBuiltin)
                {
                    AddExecutables(coll.ID, coll.Container);
                }
            }
#if UNITY_EDITOR
            if (remove)
            {
                EditorUtility.SetDirty(_Metadata);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
#endif
            Resources.UnloadUnusedAssets();
        }
        
        /// <summary>
        /// Don't call it manually.
        /// This is a function for the embedded serialization system.
        /// </summary>
        /// <param name="action"></param>
        public static void AddPostDeserialization(Action action)
        {
            _PostDeserializations.Add(action);
        }
        private static void PostDeserialize()
        {
            for (int i = 0; i < _PostDeserializations.Count; i++)
            {
                _PostDeserializations[i]();
            }
            _PostDeserializations.Clear();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void RuntimeInit()
        {
            LoadTSEffect();
            LoadBuiltinEffectCollections();
        }
    }
}
