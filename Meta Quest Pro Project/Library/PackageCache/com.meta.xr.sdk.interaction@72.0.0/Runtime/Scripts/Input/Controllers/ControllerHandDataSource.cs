/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * Licensed under the Oculus SDK License Agreement (the "License");
 * you may not use the Oculus SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 *
 * You may obtain a copy of the License at
 *
 * https://developer.oculus.com/licenses/oculussdk/
 *
 * Unless required by applicable law or agreed to in writing, the Oculus SDK
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using UnityEngine;

namespace Oculus.Interaction.Input
{
    public class ControllerHandDataSource : DataSource<HandDataAsset>
    {
        [SerializeField]
        private DataSource<ControllerDataAsset> _controllerSource;

        [SerializeField]
        private Transform _root;
        public Transform Root
        {
            get => _root;
            set => _root = value;
        }
        [SerializeField]
        private bool _rootIsLocal = true;
        public bool RootIsLocal
        {
            get => _rootIsLocal;
            set => _rootIsLocal = value;
        }

        [SerializeField]
        private Transform[] _bones;

        private HandDataSourceConfig _config;
        private readonly HandDataAsset _handDataAsset = new HandDataAsset();
        protected override HandDataAsset DataAsset => _handDataAsset;

        private HandDataSourceConfig Config
        {
            get
            {
                if (_config == null)
                {
                    _config = new HandDataSourceConfig();
                }

                return _config;
            }
        }

#if ISDK_OPENXR_HAND
        private static readonly HandMirroring.HandSpace _openXRRootLeft = new HandMirroring.HandSpace(
            Vector3.forward, Vector3.up, Vector3.right);
        private static readonly HandMirroring.HandSpace _openXRRootRight = new HandMirroring.HandSpace(
            Vector3.forward, Vector3.up, Vector3.left);
        private static readonly HandMirroring.HandSpace _ovrRootLeft = new HandMirroring.HandSpace(
            Vector3.left, Vector3.down, Vector3.forward);
        private static readonly HandMirroring.HandSpace _ovrRootRight = new HandMirroring.HandSpace(
            Vector3.right, Vector3.up, Vector3.forward);
        private static readonly HandMirroring.HandsSpace openXRRootHands = new HandMirroring.HandsSpace(
            _openXRRootLeft, _openXRRootRight);
        private static readonly HandMirroring.HandsSpace ovrRootHands = new HandMirroring.HandsSpace(
            _ovrRootLeft, _ovrRootRight);
        private Quaternion[] _ovrJointRotations = new Quaternion[Compatibility.OVR.Constants.NUM_HAND_JOINTS];
#endif

        protected override void Start()
        {
            this.BeginStart(ref _started, () => base.Start());

            this.AssertAspect(_controllerSource, nameof(_controllerSource));
            this.AssertAspect(_root, nameof(_root));
            this.AssertCollectionField(_bones, nameof(_bones));

            UpdateConfig();

            this.EndStart(ref _started);
        }

        private void UpdateConfig()
        {
            ControllerDataSourceConfig controllerConfig = _controllerSource.GetData().Config;

            Config.Handedness = controllerConfig.Handedness;
            Config.TrackingToWorldTransformer = controllerConfig.TrackingToWorldTransformer;

#if ISDK_OPENXR_HAND
            Config.HandSkeleton = Config.Handedness == Handedness.Left ?
                HandSkeleton.DefaultLeftSkeleton :
                HandSkeleton.DefaultRightSkeleton;
#else
            Config.HandSkeleton = HandSkeleton.FromJoints(_bones);
#endif
        }

        protected override void UpdateData()
        {
            ControllerDataAsset controllerData = _controllerSource.GetData();
            _handDataAsset.Config = Config;
            _handDataAsset.IsDataValid = controllerData.IsDataValid;
            _handDataAsset.IsConnected = controllerData.IsConnected;

            if (!_handDataAsset.IsConnected || !this.isActiveAndEnabled)
            {
                _handDataAsset.IsTracked = default;
                _handDataAsset.RootPoseOrigin = default;
                _handDataAsset.PointerPoseOrigin = default;
                _handDataAsset.IsHighConfidence = default;
                for (var fingerIdx = 0; fingerIdx < Constants.NUM_FINGERS; fingerIdx++)
                {
                    _handDataAsset.IsFingerPinching[fingerIdx] = default;
                    _handDataAsset.IsFingerHighConfidence[fingerIdx] = default;
                }
                return;
            }

            _handDataAsset.IsTracked = controllerData.IsTracked;
            _handDataAsset.IsHighConfidence = true;
            _handDataAsset.IsDominantHand = controllerData.IsDominantHand;

            float pinchStrength = controllerData.Input.Trigger;
            float gripStrength = controllerData.Input.Grip;

            bool isPinching = controllerData.Input.TriggerButton;
            bool isGripping = controllerData.Input.GripButton;

            _handDataAsset.IsFingerHighConfidence[(int)HandFinger.Thumb] = true;
            _handDataAsset.IsFingerPinching[(int)HandFinger.Thumb] = isPinching || isGripping;
            _handDataAsset.FingerPinchStrength[(int)HandFinger.Thumb] = Mathf.Max(pinchStrength, gripStrength);

            _handDataAsset.IsFingerHighConfidence[(int)HandFinger.Index] = true;
            _handDataAsset.IsFingerPinching[(int)HandFinger.Index] = isPinching;
            _handDataAsset.FingerPinchStrength[(int)HandFinger.Index] = pinchStrength;

            _handDataAsset.IsFingerHighConfidence[(int)HandFinger.Middle] = true;
            _handDataAsset.IsFingerPinching[(int)HandFinger.Middle] = isGripping;
            _handDataAsset.FingerPinchStrength[(int)HandFinger.Middle] = gripStrength;

            _handDataAsset.IsFingerHighConfidence[(int)HandFinger.Ring] = true;
            _handDataAsset.IsFingerPinching[(int)HandFinger.Ring] = false;
            _handDataAsset.FingerPinchStrength[(int)HandFinger.Ring] = 0f;

            _handDataAsset.IsFingerHighConfidence[(int)HandFinger.Pinky] = true;
            _handDataAsset.IsFingerPinching[(int)HandFinger.Pinky] = false;
            _handDataAsset.FingerPinchStrength[(int)HandFinger.Pinky] = 0f;

            _handDataAsset.PointerPoseOrigin = PoseOrigin.FilteredTrackedPose;
            _handDataAsset.PointerPose = controllerData.PointerPose;

#if ISDK_OPENXR_HAND

            // Populate ovr rotations from bone transforms
            for (int i = 0; i < Compatibility.OVR.Constants.NUM_HAND_JOINTS; i++)
            {
                _ovrJointRotations[i] = _bones[i].localRotation;
            }

            // Set wrist poses to identity
            _handDataAsset.JointPoses[(int)HandJointId.HandWristRoot] = Pose.identity;
            _ovrJointRotations[(int)Compatibility.OVR.HandJointId.HandWristRoot] = Quaternion.identity;

            // Translate ovr rotations to OpenXR joint poses
            HandTranslationUtils.OVRHandRotationsToOpenXRPoses(_ovrJointRotations, _config.Handedness,
                ref _handDataAsset.JointPoses);

#pragma warning disable 0618
            // Populate legacy Joints array
            HandJointUtils.WristJointPosesToLocalRotations(_handDataAsset.JointPoses, ref _handDataAsset.Joints);
#pragma warning restore 0618
#else
            for (int i = 0; i < _bones.Length; i++)
            {
#pragma warning disable 0618
                _handDataAsset.Joints[i] = _bones[i].localRotation;
#pragma warning restore 0618
            }
#endif

            if (_rootIsLocal)
            {
                Pose offset = _root.GetPose(Space.Self);
#if ISDK_OPENXR_HAND
                Handedness transformHandedness = Config.Handedness;
                HandMirroring.HandSpace fromHand = ovrRootHands[transformHandedness];
                HandMirroring.HandSpace toHand = openXRRootHands[transformHandedness];
                Vector3 forward = HandMirroring.TransformPosition(offset.rotation * Vector3.forward, fromHand, toHand);
                Vector3 up = HandMirroring.TransformPosition(offset.rotation * Vector3.up, fromHand, toHand);
                offset.rotation = Quaternion.LookRotation(forward, up);
#endif
                Pose controllerPose = controllerData.RootPose;
                PoseUtils.Multiply(controllerPose, offset, ref _handDataAsset.Root);
                _handDataAsset.HandScale = _root.localScale.x;
            }
            else
            {
                _handDataAsset.Root = _root.GetPose(Space.World);
                _handDataAsset.HandScale = _root.lossyScale.x;
            }

            _handDataAsset.RootPoseOrigin = PoseOrigin.FilteredTrackedPose;
        }

        #region Inject

        public void InjectAllControllerHandDataSource(UpdateModeFlags updateMode, IDataSource updateAfter,
            DataSource<ControllerDataAsset> controllerSource, Transform[] bones)
        {
            base.InjectAllDataSource(updateMode, updateAfter);
            InjectControllerSource(controllerSource);
            InjectBones(bones);
        }

        public void InjectControllerSource(DataSource<ControllerDataAsset> controllerSource)
        {
            _controllerSource = controllerSource;
        }

        public void InjectBones(Transform[] bones)
        {
            _bones = bones;
        }

        #endregion
    }
}
