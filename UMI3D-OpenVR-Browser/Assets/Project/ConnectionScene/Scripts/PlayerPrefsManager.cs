/*
Copyright 2019 - 2021 Inetum
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
using UnityEngine;

public static class PlayerPrefsManager
{
    public static readonly string Umi3dIp = "umi3d-ip";

    public static readonly string Umi3dPort = "umi3d-port";

    public static readonly string Umi3dFavoriteServers = "umi3d-favorite-servers";


    /// <summary>
    /// If an ip server is stored, returns it otherwise returns an empty string.
    /// </summary>
    /// <returns></returns>
    public static string GetUmi3dIp()
    {
        string res = string.Empty;

        if (PlayerPrefs.HasKey(Umi3dIp))
            res = PlayerPrefs.GetString(Umi3dIp);

        return res;
    }

    public static void SaveUmi3dIp(string ip)
    {
        PlayerPrefs.SetString(Umi3dIp, ip);
    }

    /// <summary>
    /// If a port server is stored, returns it otherwise returns an empty string.
    /// </summary>
    /// <returns></returns>
    public static string GetUmi3DPort()
    {
        string res = string.Empty;

        if (PlayerPrefs.HasKey(Umi3dPort))
            res = PlayerPrefs.GetString(Umi3dPort);

        return res;
    }

    public static void SaveUmi3dPort(string port)
    {
        PlayerPrefs.SetString(Umi3dPort, port);
    }

    /// <summary>
    /// Returns true if at least one server was set as favorite by users.
    /// </summary>
    /// <returns></returns>
    public static bool HasFavoriteServersStored()
    {
        bool res = false;

        if (PlayerPrefs.HasKey(Umi3dFavoriteServers))
        {
            var data = JsonUtility.FromJson<FavoriteServers>(PlayerPrefs.GetString(Umi3dFavoriteServers));
            if (data != null)
            {
                res = data.favorites.Count > 0;
            }
        }

        return res;
    }

    /// <summary>
    /// Returns a
    /// </summary>
    /// <returns></returns>
    public static List<FavoriteServerData> GetFavoriteServers()
    {
        List<FavoriteServerData> fav = new List<FavoriteServerData>();

        if (HasFavoriteServersStored())
        {
            string dataStr = PlayerPrefs.GetString(Umi3dFavoriteServers);

            var data = JsonUtility.FromJson<FavoriteServers>(PlayerPrefs.GetString(Umi3dFavoriteServers));

            if (data == null)
            {
                Debug.LogError("Interal error, impossible to read favorite servers from preferences");
            }
            else
            {
                fav = data.favorites;
            }
        }
        else
        {
            Debug.Log("No favorite server stored, you should use HasFavoriteServersStored() before calling this method");
        }

        return fav;
    }

    /// <summary>
    /// Add a new server to favorites if there is a server already stored with the same url.
    /// </summary>
    /// <param name="url"></param>
    /// <param name="name"></param>
    public static void AddServerToFavorite(string url, string name)
    {
        FavoriteServers data;

        if (HasFavoriteServersStored())
        {
            data = JsonUtility.FromJson<FavoriteServers>(PlayerPrefs.GetString(Umi3dFavoriteServers));

            if (data == null)
            {
                Debug.LogError("Interal error, impossible to read favorite servers from preferences");
                return;
            }
            else if (data.favorites.Find(d => d.serverUrl == url) != null)
            {
                Debug.LogError("Impossible to add this server to favorites because there is already a server stored with this url");
                return;
            }
            else
            {
                data.favorites.Add(new FavoriteServerData { serverName = name, serverUrl = url });
            }
        }
        else
        {
            data = new FavoriteServers { favorites = new List<FavoriteServerData> { new FavoriteServerData { serverName = name, serverUrl = url } } };
        }
        PlayerPrefs.SetString(Umi3dFavoriteServers, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    /// <summary>
    /// If a server with <see cref="url"/> as url is currently stored, removes it.
    /// </summary>
    /// <param name="url"></param>
    public static void RemoveServerFromFavorite(string url)
    {
        FavoriteServers data;

        if (HasFavoriteServersStored())
        {
            data = JsonUtility.FromJson<FavoriteServers>(PlayerPrefs.GetString(Umi3dFavoriteServers));

            if (data == null)
            {
                return;
            }

            var entryToDelete = data.favorites.Find(d => d.serverUrl == url);

            if (entryToDelete != null)
            {
                data.favorites.Remove(entryToDelete);
                PlayerPrefs.SetString(Umi3dFavoriteServers, JsonUtility.ToJson(data));
                PlayerPrefs.Save();
            }

        }

    }

    [System.Serializable]
    public class FavoriteServerData
    {
        public string serverName;

        public string serverUrl;
    }

    [System.Serializable]
    public class FavoriteServers
    {
        public List<FavoriteServerData> favorites = new List<FavoriteServerData>();
    }
}
