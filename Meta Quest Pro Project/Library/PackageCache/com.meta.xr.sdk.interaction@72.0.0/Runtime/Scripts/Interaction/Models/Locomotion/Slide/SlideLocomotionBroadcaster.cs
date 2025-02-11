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

using Oculus.Interaction.Input;
using System;
using UnityEngine;

namespace Oculus.Interaction.Locomotion
{
    /// <summary>
    /// This component will emit LocomotionEvent.Translation.Velocity every update
    /// in the direction and magnitude specified by the provided Axis and Aiming transform.
    /// Generally the aiming transform forward will be flattened in the Y plane, but when
    /// the angle is too extreme (near +-90, specified by the _dotSafeDirectionThreshold) it
    /// will resort to .up or .back to ensure the direction is stable.
    /// </summary>
    public class SlideLocomotionBroadcaster : MonoBehaviour
        , ILocomotionEventBroadcaster
    {
        /// <summary>
        /// The Axis that will provide the relative direction to the aiming transform
        /// and its magnitude
        /// </summary>
        [SerializeField, Interface(typeof(IAxis2D))]
        private UnityEngine.Object _axis2D;
        private IAxis2D Axis2D;

        /// <summary>
        /// The transform to use as a reference for the movement. Typically set to the
        /// hand or the head of the player
        /// </summary>
        [SerializeField, Optional]
        private Transform _aiming;
        public Transform Aiming
        {
            get => _aiming;
            set => _aiming = value;
        }

        /// <summary>
        /// When the aiming transform points too far up or down (based on this value)
        /// the final forward direction will be slerped between the aiming.forward and
        /// the aiming.up or .down to ensure that the final forward is stable.
        /// </summary>
        [SerializeField]
        private Vector2 _dotSafeDirectionThreshold = new Vector2(0.8f, 0.9f);
        public Vector2 DotSafeDirectionThreshold
        {
            get => _dotSafeDirectionThreshold;
            set => _dotSafeDirectionThreshold = value;
        }

        /// <summary>
        /// Deadzone applied to the vertical axis
        /// </summary>
        [SerializeField, Optional]
        private AnimationCurve _verticalDeadZone = AnimationCurve.Linear(-1f, -1f, 1f, 1f);
        public AnimationCurve VerticalDeadZone
        {
            get => _verticalDeadZone;
            set => _verticalDeadZone = value;
        }

        /// <summary>
        /// Deadzone applied to the horizontal axis
        /// </summary>
        [SerializeField, Optional]
        private AnimationCurve _horizontalDeadZone = AnimationCurve.Linear(-1f, -1f, 1f, 1f);
        public AnimationCurve HorizontalDeadZone
        {
            get => _horizontalDeadZone;
            set => _horizontalDeadZone = value;

        }

        private Action<LocomotionEvent> _whenLocomotionPerformed = delegate { };
        public event Action<LocomotionEvent> WhenLocomotionPerformed
        {
            add
            {
                _whenLocomotionPerformed += value;
            }
            remove
            {
                _whenLocomotionPerformed -= value;
            }
        }

        private UniqueIdentifier _identifier;
        public int Identifier => _identifier.ID;

        protected bool _started = false;

        protected virtual void Awake()
        {
            _identifier = UniqueIdentifier.Generate(Context.Global.GetInstance(), this);
            Axis2D = _axis2D as IAxis2D;
        }

        protected virtual void Start()
        {
            this.BeginStart(ref _started);
            this.AssertField(Axis2D, nameof(_axis2D));
            this.EndStart(ref _started);
        }

        protected virtual void Update()
        {
            Vector2 axis = ProcessAxisSensitivity();
            Vector3 step = StepDirection(axis);

            if (!Mathf.Approximately(step.sqrMagnitude, 0f))
            {
                var locomotionEvent = new LocomotionEvent(this.Identifier,
                    step, LocomotionEvent.TranslationType.Velocity);
                _whenLocomotionPerformed.Invoke(locomotionEvent);
            }
        }

        private Vector2 ProcessAxisSensitivity()
        {
            Vector2 value = Axis2D.Value();
            if (_horizontalDeadZone != null)
            {
                value.x = _horizontalDeadZone.Evaluate(value.x);
            }
            if (_verticalDeadZone != null)
            {
                value.y = _verticalDeadZone.Evaluate(value.y);
            }
            return value;
        }

        private Vector3 StepDirection(Vector2 axisValue)
        {
            if (_aiming == null)
            {
                return new Vector3(axisValue.x, 0f, axisValue.y);
            }

            Vector3 forward = _aiming.forward;
            Vector3 up = Vector3.up;
            float dot = Vector3.Dot(forward, up);

            if (Mathf.Abs(dot) > _dotSafeDirectionThreshold.x)
            {
                Vector3 safeForward = _aiming.up * -Mathf.Sign(dot);
                float t = Mathf.InverseLerp(_dotSafeDirectionThreshold.x, _dotSafeDirectionThreshold.y, Mathf.Abs(dot));
                forward = Vector3.Slerp(forward, safeForward, t);
            }

            Vector2 normalisedAxis = axisValue.normalized;
            float angle = Mathf.Atan2(normalisedAxis.y, -normalisedAxis.x) * Mathf.Rad2Deg - 90f;
            Vector3 worldForward = Vector3.ProjectOnPlane(forward, up).normalized;
            Vector3 direction = Quaternion.AngleAxis(angle, up) * worldForward * axisValue.magnitude;

            return direction;
        }

        #region Inject

        public void InjectAllSlideLocomotionBroadcaster(IAxis2D axis2D)
        {
            InjectAxis2D(axis2D);
        }

        public void InjectAxis2D(IAxis2D axis2D)
        {
            _axis2D = axis2D as UnityEngine.Object;
            Axis2D = axis2D;
        }

        #endregion
    }
}
