/*
Copyright 2019 - 2022 Inetum

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace umi3dVRBrowsersBase.connection
{
    /// <summary>
    /// Class which all users' preferences.
    /// </summary>
    public static class PlayerPrefsManager
    {
        #region Fields

        public static readonly string Umi3dIp = "umi3d-ip";

        public static readonly string Umi3dPort = "umi3d-port";

        public static readonly string Umi3dVirtualWorlds = "umi3d-virtual-worlds";

        #endregion

        #region Methods

        /// <summary>
        /// If an ip VirtualWorld is stored, returns it otherwise returns an empty string.
        /// </summary>
        /// <returns></returns>
        public static string GetUmi3dIp()
        {
            string res = string.Empty;

            if (PlayerPrefs.HasKey(Umi3dIp))
                res = PlayerPrefs.GetString(Umi3dIp);

            return res;
        }

        /// <summary>
        /// Stores last environment ip used.
        /// </summary>
        /// <param name="ip"></param>
        public static void SaveUmi3dIp(string ip)
        {
            PlayerPrefs.SetString(Umi3dIp, ip);
        }

        /// <summary>
        /// If a port VirtualWorld is stored, returns it otherwise returns an empty string.
        /// </summary>
        /// <returns></returns>
        public static string GetUmi3DPort()
        {
            string res = string.Empty;

            if (PlayerPrefs.HasKey(Umi3dPort))
                res = PlayerPrefs.GetString(Umi3dPort);

            return res;
        }

        /// <summary>
        /// Stored last environment port used.
        /// </summary>
        /// <param name="port"></param>
        public static void SaveUmi3dPort(string port)
        {
            PlayerPrefs.SetString(Umi3dPort, port);
        }

        /// <summary>
        /// Returns true if their is VirtualWorld store.
        /// </summary>
        /// <returns></returns>
        public static bool HasVirtualWorldsStored()
        {
            if (PlayerPrefs.HasKey(Umi3dVirtualWorlds))
            {
                VirtualWorlds data = JsonUtility.FromJson<VirtualWorlds>(PlayerPrefs.GetString(Umi3dVirtualWorlds));
                if (data != null)
                {
                    return data.worlds.Count > 0;
                }
            }

            return false;
        }

        /// <summary>
        /// Return true if at least one VirtualWorld is set to favorite;
        /// </summary>
        /// <returns></returns>
        public static bool HasFavoriteVirtualWorldsStored()
        {
            if (!HasFavoriteVirtualWorldsStored()) return false;

            if (PlayerPrefs.HasKey(Umi3dVirtualWorlds))
            {
                VirtualWorlds data = JsonUtility.FromJson<VirtualWorlds>(PlayerPrefs.GetString(Umi3dVirtualWorlds));
                if (data != null)
                {
                    return data.worlds.Where(d => d.isFavorite).Count() > 0;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns a list of all VirtualWorld.
        /// </summary>
        /// <returns></returns>
        public static List<VirtualWorldData> GetVirtualWorlds()
        {
            if (HasVirtualWorldsStored())
            {
                string dataStr = PlayerPrefs.GetString(Umi3dVirtualWorlds);

                VirtualWorlds data = JsonUtility.FromJson<VirtualWorlds>(PlayerPrefs.GetString(Umi3dVirtualWorlds));

                if (data == null)
                {
                    Debug.LogError("Interal error, impossible to read VirtualWorlds from preferences");
                }
                else
                {
                    return data.worlds;
                }
            }
            else
            {
                Debug.Log("No VirtualWorlds stored, you should use HasVirtualWorldsStored() before calling this method");
            }

            return new();
        }

        /// <summary>
        /// Returns a list of all favorite VirtualWorld.
        /// </summary>
        /// <returns></returns>
        public static List<VirtualWorldData> GetFavoriteVirtualWorlds()
        {
            if (HasVirtualWorldsStored())
                return GetVirtualWorlds().Where(d => d.isFavorite)
                                         .OrderBy(d => d.indexFavorite)
                                         .ToList();

            return new ();
        }

        /// <summary>
        /// Add a new VirtualWorld, if there is a VirtualWorld already stored with the same url update isFavorite and DateLastConnection.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="name"></param>
        public static void SaveVirtualWorld(VirtualWorldData data)
        {
            VirtualWorlds virtualWorlds;

            if (HasVirtualWorldsStored())
            {
                virtualWorlds = JsonUtility.FromJson<VirtualWorlds>(PlayerPrefs.GetString(Umi3dVirtualWorlds));

                if (virtualWorlds == null)
                {
                    Debug.LogError("Interal error, impossible to read virtual world from preferences");
                    return;
                }
                else if (virtualWorlds.worlds.Find(d => d.worldUrl == data.worldUrl) is VirtualWorldData d && d != null)
                {
                    if (data.isFavorite)
                    {
                        d.isFavorite = true;
                        d.indexFavorite = GetFavoriteVirtualWorlds().Count;
                    }
                    d.dateLastConnection = data.dateLastConnection;
                }
                else
                {
                    data.dateFirstConnection = data.dateLastConnection;
                    virtualWorlds.worlds.Add(data);
                }
            }
            else
            {
                data.dateFirstConnection = data.dateLastConnection;
                if (data.isFavorite)
                    data.indexFavorite = 0;
                virtualWorlds = new VirtualWorlds { worlds = new List<VirtualWorldData> { data } };
            }
            PlayerPrefs.SetString(Umi3dVirtualWorlds, JsonUtility.ToJson(virtualWorlds));
            PlayerPrefs.Save();
        }

        /// <summary>
        /// If a VirtualWorld with <see cref="url"/> as url is currently stored, remove from favorite.
        /// </summary>
        /// <param name="url"></param>
        public static void RemoveServerFromFavorite(string url)
        {
            if (!HasFavoriteVirtualWorldsStored()) return;
            
            var virtualWorlds = GetFavoriteVirtualWorlds();
            var favoriteFound = false;

            for (int i = 0; i < virtualWorlds.Count; i++)
            {
                if (!favoriteFound)
                {
                    if (virtualWorlds[i].worldUrl != url) continue;
                    favoriteFound = true;

                    virtualWorlds[i].isFavorite = false;
                }
                else
                {
                    virtualWorlds[i].indexFavorite -= 1;
                }
            }
            PlayerPrefs.SetString(Umi3dVirtualWorlds, JsonUtility.ToJson(virtualWorlds));
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Increment Favorite VirtualWorld index, if a favorite VirtualWorld with <see cref="url"/> as url is currently stored
        /// </summary>
        /// <param name="url"></param>
        public static void IncrementFavorite(string url)
        {
            if (!HasFavoriteVirtualWorldsStored()) return;

            var favoriteVirtualWorlds = GetFavoriteVirtualWorlds();

            for (int i = 0; i < favoriteVirtualWorlds.Count; i++)
            {
                if (favoriteVirtualWorlds[i].worldUrl != url) continue;

                if (i != favoriteVirtualWorlds.Count - 1)
                {
                    favoriteVirtualWorlds[i].indexFavorite += 1;
                    favoriteVirtualWorlds[i+1].indexFavorite -= 1;
                }
                return;
            }
        }

        /// <summary>
        /// Decrement Favorite VirtualWorld index, if a favorite VirtualWorld with <see cref="url"/> as url is currently stored
        /// </summary>
        /// <param name="url"></param>
        public static void DecrementFavorite(string url)
        {
            if (!HasFavoriteVirtualWorldsStored()) return;

            var favoriteVirtualWorlds = GetFavoriteVirtualWorlds();

            for (int i = 0; i < favoriteVirtualWorlds.Count; i++)
            {
                if (favoriteVirtualWorlds[i].worldUrl != url) continue;

                if (i != 0)
                {
                    favoriteVirtualWorlds[i].indexFavorite -= 1;
                    favoriteVirtualWorlds[i - 1].indexFavorite += 1;
                }
                return;
            }
        }

        /// <summary>
        /// Contains : environment name, ip and port.
        /// </summary>
        [System.Serializable]
        public class Data
        {
            public string environmentName;
            public string ip;
            public string port;

            public override string ToString() => $"name = {environmentName}, ip = {ip}, port = {port}";
        }

        /// <summary>
        /// Stores data about a VirtualWorld.
        /// </summary>
        [System.Serializable]
        public class VirtualWorldData
        {
            public string worldName;
            public string worldUrl;

            public bool isFavorite;
            public int indexFavorite;

            public string dateFirstConnection;
            public string dateLastConnection;
        }

        /// <summary>
        /// Stores all VirtualWorlds.
        /// </summary>
        [System.Serializable]
        public class VirtualWorlds
        {
            public List<VirtualWorldData> worlds = new ();
        }

        #endregion
    }
}