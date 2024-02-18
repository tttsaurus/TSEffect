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
        
        private enum RECallbackType
        {
            Remove = 1,
            Add = 2,
            Suspend = 3,
            Desuspend = 4,
        }
        private struct RECallback
        {
            public ThreadExecutor Executor;
            public RECallbackType CallbackType;
            public RECallback(ThreadExecutor executor, RECallbackType callback_type)
            {
                Executor = executor;
                CallbackType = callback_type;
            }
        }
        public static SortedSet<ThreadExecutor> RuntimeExecutors { get {  return _RuntimeExecutors; } }
        private static SortedSet<ThreadExecutor> _RuntimeExecutors = new SortedSet<ThreadExecutor>(new ThreadExecutorComparer());
        private static List<RECallback> _RECallbacks = new List<RECallback>();
        private static Dictionary<Guid, ThreadExecutor> _SuspendedExecutors = new Dictionary<Guid, ThreadExecutor>();

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

            bool flag_a = false;
            Dictionary<string, List<Component>> a;
            if (!_Subject.TryGetValue(channel, out a))
            {
                a = new Dictionary<string, List<Component>>();
                _Subject.Add(channel, a);
                flag_a = true;
            }

            bool flag_b = false;
            List<Component> b;
            if (flag_a)
            {
                b = new List<Component>();
                _Subject[channel].Add(target_type, b);
                flag_b = true;
            }
            else if (!a.TryGetValue(target_type, out b))
            {
                b = new List<Component>();
                _Subject[channel].Add(target_type, b);
                flag_b = true;
            }

            if (flag_b)
            {
                b.Add(com);
                var etor = _RuntimeExecutors.GetEnumerator();
                while(etor.MoveNext())
                {
                    etor.Current.AddRuntimeTarget(com, channel);
                }
            }
            else if (!b.Contains(com))
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
        /// <param name="e_id">The ID is the key for the corresponding effects.</param>
        /// <param name="cont">The container stores the effects.</param>
        /// <returns>It returns whether the behavior succeed.</returns>
        public static bool AddExecutables(string e_id, EffectsContainer cont)
        {
            if (_ExeCoreDict.ContainsKey(e_id))
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
                        var reg = Metadata.GetEffectReg(effect.GetType());
                        if (reg.Exist)
                        {
                            var etor = reg.ThreadReg.GetEnumerator();
                            while (etor.MoveNext())
                            {
                                var field_name = etor.Current.Key;
                                var list_thread = ThreadUtil.GetThreadsFromEffect(effect, field_name);
                                
                                List<NormalExeFuncBuilder> a = new List<NormalExeFuncBuilder>();
                                List<NormalExeFuncBuilder> b = new List<NormalExeFuncBuilder>();
                                List<TimeExeFuncBuilder> c = new List<TimeExeFuncBuilder>();
                                
                                effect.GenLogicBuilders(field_name, out a, out b, out c);

                                if (list_thread.Count == a.Count && list_thread.Count == b.Count && list_thread.Count == c.Count)
                                {
                                    for (int j = 0; j < list_thread.Count; j++)
                                    {
                                        if (list_thread[j].Enable)
                                            exes.Add(new ExecutableThreadCore(etor.Current.Value.DisplayName, effect.TargetType, list_thread[j], a[j], b[j], c[j]));
                                    }
                                }
                                else
                                {
                                    Debug.LogError(string.Format("Exception from {0}: field {1} contains {2} thread(s) but GenLogicBuilders generates {3}, {4} and {5} logic builders.", effect.GetType().FullName, field_name, list_thread.Count, a.Count, b.Count, c.Count));
                                }
                            }
                        }
                    }
                }

                if (exes.Count == 0)
                    return false;
                else
                    _ExeCoreDict.Add(e_id, exes);
                return true;
            }       
        }
        /// <summary>
        /// Call this function to execute effects.
        /// </summary>
        /// <param name="e_id">The ID determines what effects are going to be executed.</param>
        /// <param name="channel">The channel determines what components are going to listen to the execution.</param>
        /// <returns>It returns whether the behavior succeed.</returns>
        public static bool Execute(string e_id, int channel)
        {
            List<ExecutableThreadCore> list;
            if (_ExeCoreDict.TryGetValue(e_id, out list))
            {
                Dictionary<string, List<Component>> a = null;
                _Subject.TryGetValue(channel, out a);
                
                for (int i = 0; i < list.Count; i++)
                {
                    ThreadExecutor executor;
                    if (_SuspendedExecutors.TryGetValue(list[i].Thread.GUID, out executor))
                    {
                        _SuspendedExecutors.Remove(list[i].Thread.GUID);
                        
                        List<Component> b;
                        if (a != null)
                        {
                            if (a.TryGetValue(list[i].TargetType, out b))
                            {
                                executor.ResetRuntimeTargets(b);
                            }
                        }

                        _RECallbacks.Add(new RECallback(executor, RECallbackType.Desuspend));
                    }
                    else
                    {
                        executor = new ThreadExecutor(list[i], channel);
                        executor.SetRemoveCallback(() => { _RECallbacks.Add(new RECallback(executor, RECallbackType.Remove)); });
                        executor.SetSuspendCallback(() => { _RECallbacks.Add(new RECallback(executor, RECallbackType.Suspend)); });

                        List<Component> b;
                        if (a != null)
                        {
                            if (a.TryGetValue(list[i].TargetType, out b))
                            {
                                executor.ResetRuntimeTargets(b);
                            }
                        }

                        _RECallbacks.Add(new RECallback(executor, RECallbackType.Add));
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
        public static void ExecuteRECallbacks()
        {
            if (_RECallbacks.Count > 0)
            {
                for (int i = 0; i < _RECallbacks.Count; i++)
                {
                    switch (_RECallbacks[i].CallbackType)
                    {
                        case RECallbackType.Remove:
                            _RuntimeExecutors.Remove(_RECallbacks[i].Executor);
                            break;
                        case RECallbackType.Add:
                            _RuntimeExecutors.Add(_RECallbacks[i].Executor);
                            break;
                        case RECallbackType.Suspend:
                            Guid guid = _RECallbacks[i].Executor.ExeThreadCore.Thread.GUID;
                            if (_SuspendedExecutors.ContainsKey(guid))
                                _RuntimeExecutors.Remove(_RECallbacks[i].Executor);
                            else
                                _SuspendedExecutors.Add(guid, _RECallbacks[i].Executor);
                            break;
                        case RECallbackType.Desuspend:
                            _RECallbacks[i].Executor.Desuspend();
                            break;
                    }
                }
                _RECallbacks.Clear();
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
        /// This function will be called automatically at runtime.
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
        /// This function will be called automatically at runtime.
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
