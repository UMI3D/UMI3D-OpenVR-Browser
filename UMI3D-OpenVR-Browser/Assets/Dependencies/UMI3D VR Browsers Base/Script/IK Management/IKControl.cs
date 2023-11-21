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


using System;
using System.Collections.Generic;
using umi3d.cdk.userCapture.tracking;
using umi3d.common.userCapture;
using UnityEngine;

namespace umi3dVRBrowsersBase.ikManagement
{
    [RequireComponent(typeof(Animator))]

    public class IKControl : MonoBehaviour
    {
        public TrackedSubskeleton trackedSubskeleton;
        protected Animator animator;
        public bool feetIkActive
        {
            get => LeftController.isActif; 
            set
            {
                LeftController.isActif = value;
                RightController.isActif = value;
            }
        }

        public float animationSpeed = 1f;

        public VirtualObjectBodyInteraction LeftFoot;
        public VirtualObjectBodyInteraction RightFoot;

        public DistantController LeftController { get; protected set; } = new();
        public DistantController RightController { get; protected set; } = new();

        private void Start()
        {
            LeftController.isActif = false;
            RightController.isActif = false;

            LeftController.isOverrider = true;
            RightController.isOverrider = true;

            LeftController.boneType = BoneType.LeftAnkle;
            RightController.boneType = BoneType.RightAnkle;

            trackedSubskeleton.controllers.Add(LeftController);
            trackedSubskeleton.controllers.Add(RightController);
        }

        private void Update()
        {
            LeftController.position = LeftFoot.transform.position;
            LeftController.rotation = LeftFoot.transform.rotation;

            RightController.position = RightFoot.transform.position;
            RightController.rotation = RightFoot.transform.rotation;
        }
    }
}