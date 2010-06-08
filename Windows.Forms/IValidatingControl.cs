/*
 * Self Validating Control Interface
 * Copyright (C) 2009-2010 Ryan Harlamert (harlam357)
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace harlam357.Windows.Forms
{
   public interface IValidatingControl : IBindableComponent
   {
      /// <summary>
      /// Custom Validation Event
      /// </summary>
      event EventHandler<ValidatingControlCustomValidationEventArgs> CustomValidation;

      /// <summary>
      /// Attached ToolTip Instance
      /// </summary>
      ToolTip ErrorToolTip { get; set; }

      /// <summary>
      /// ToolTip Display Point
      /// </summary>
      Point ErrorToolTipPoint { get; set; }

      /// <summary>
      /// ToolTip Display Duration
      /// </summary>
      int ErrorToolTipDuration { get; set; }

      /// <summary>
      /// ToolTip Text for Display
      /// </summary>
      string ErrorToolTipText { get; set; }

      /// <summary>
      /// Validation Type
      /// </summary>
      ValidationType ValidationType { get; set; }

      /// <summary>
      /// Control Error BackColor
      /// </summary>
      Color ErrorBackColor { get; set; }

      /// <summary>
      /// Control Error State
      /// </summary>
      bool ErrorState { get; set; }

      /// <summary>
      /// Companion Controls List
      /// </summary>
      List<Control> CompanionControls { get; }

      /// <summary>
      /// Validate the Control Text Property
      /// </summary>
      void ValidateControlText();

      /// <summary>
      /// Show or Hide the Configured ToolTip based on Error State
      /// </summary>
      void ShowToolTip();
   }
}
