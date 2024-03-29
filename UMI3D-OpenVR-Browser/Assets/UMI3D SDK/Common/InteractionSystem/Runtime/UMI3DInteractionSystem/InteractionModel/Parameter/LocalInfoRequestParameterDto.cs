﻿/*
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

namespace umi3d.common.interaction
{
    /// <summary>
    /// local info request acces parameter dto.
    /// </summary>
    [System.Serializable]
    public class LocalInfoRequestParameterValue
    {
        public bool read;
        public bool write;

        public LocalInfoRequestParameterValue(bool read, bool write)
        {
            this.read = read;
            this.write = write;
        }

        public override string ToString()
        {
            return $"LocalInfoRequestParameterValue [Read:{read}|Write:{write}]";
        }
    }

    /// <summary>
    /// local info request acces parameter dto.
    /// </summary>
    [System.Serializable]
    public class LocalInfoRequestParameterDto : AbstractParameterDto<LocalInfoRequestParameterValue> //read authorization, write authorization
    {
        public LocalInfoRequestParameterDto() : base() { }

        public string key;
        public string reason;
        public string serverName;
        public string app_id;

    }
}