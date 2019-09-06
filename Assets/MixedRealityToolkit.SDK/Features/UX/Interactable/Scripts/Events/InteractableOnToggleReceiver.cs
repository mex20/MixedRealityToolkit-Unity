﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities.Editor;
using UnityEngine.Events;

namespace Microsoft.MixedReality.Toolkit.UI
{
    /// <summary>
    /// A receiver that listens to toggle events
    /// </summary>
    public class InteractableOnToggleReceiver : ReceiverBase
    {
        /// <summary>
        /// Invoked when toggle is deselected
        /// </summary>
        [InspectorField(Type = InspectorField.FieldTypes.Event, Label = "On Deselect", Tooltip = "The toggle is deselected")]
        public UnityEvent OnDeselect = new UnityEvent();

        /// <summary>
        /// Invoked when toggle is checked
        /// </summary>
        public UnityEvent OnSelect => uEvent;

        /// <summary>
        /// Creates a receiver that raises events for toggle button states
        /// </summary>
        public InteractableOnToggleReceiver(UnityEvent ev) : base(ev, "OnSelect") { }

        /// <summary>
        /// Creates a receiver that raises events for toggle button states
        /// </summary>
        public InteractableOnToggleReceiver() : this(new UnityEvent()) { }

        /// <inheritdoc />
        public override void OnUpdate(InteractableStates state, Interactable source)
        {
            // using onClick 
        }

        /// <inheritdoc />
        public override void OnClick(InteractableStates state, Interactable source, IMixedRealityPointer pointer = null)
        {
            int currentIndex = source.GetDimensionIndex();
            
            if (currentIndex % 2 == 0)
            {
                OnDeselect.Invoke();
            }
            else
            {
                uEvent.Invoke();
            }
        }
    }
}
