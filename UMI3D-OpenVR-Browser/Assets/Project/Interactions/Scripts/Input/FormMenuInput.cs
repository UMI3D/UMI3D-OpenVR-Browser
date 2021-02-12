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
using System.Collections;
using System.Collections.Generic;
using umi3d.cdk;
using umi3d.cdk.interaction;
using umi3d.cdk.menu;
using umi3d.cdk.menu.interaction;
using umi3d.common.interaction;
using umi3d.common.userCapture;
using UnityEngine;

public class FormMenuInput : AbstractUMI3DInput
{
    /// <summary>
    /// Oculus input observer binded to this input.
    /// </summary>
    public AbstractController oculusInput;

    /// <summary>
    /// Associtated form (if any).
    /// </summary>
    public FormDto associatedForm { get; protected set; }

    /// <summary>
    /// Associated menu item.
    /// </summary>
    public FormMenuItem menuItem;

    /// <summary>
    /// Hovered object id.
    /// </summary>
    string hoveredObjectId;

    /// <summary>
    /// Avatar bone linked to this input.
    /// </summary>
    public string bone = BoneType.None;

    public override void Associate(AbstractInteractionDto interaction, string toolId, string hoveredObjectId)
    {
        if (associatedForm != null)
        {
            throw new System.Exception("This input is already binded to a interaction ! (" + associatedForm + ")");
        }

        if (IsCompatibleWith(interaction))
        {
            associatedForm = interaction as FormDto;
        }
        else
        {
            throw new System.Exception("Trying to associate an uncompatible interaction !");
        }
    }

    public override void Associate(ManipulationDto manipulation, DofGroupEnum dofs, string toolId, string hoveredObjectId)
    {
        throw new System.Exception("This input is can not be associated with a manipulation");
    }

    public override AbstractInteractionDto CurrentInteraction()
    {
        return associatedForm;
    }

    public override void Dissociate()
    {
        associatedForm = null;
        menuItem = null;
    }

    public override bool IsAvailable()
    {
        return associatedForm == null;
    }

    public override bool IsCompatibleWith(AbstractInteractionDto interaction)
    {
        return interaction is FormDto;
    }

    public override void UpdateHoveredObjectId(string hoveredObjectId)
    {
        this.hoveredObjectId = hoveredObjectId;
    }
}