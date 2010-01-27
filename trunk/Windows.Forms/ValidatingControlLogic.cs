/*
 * Self Validating Control Logic
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
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace harlam357.Windows.Forms
{
   public enum ValidationType
   {
      None = 0,
      Empty,
      Custom
   }

   internal class ValidatingControlLogic
   {
      #region Events
      /// <summary>
      /// Custom Validation Event
      /// </summary>
      public event EventHandler<ValidatingControlCustomValidationEventArgs> CustomValidation;
      #endregion

      #region Properties
      private ToolTip _ErrorToolTip = null;
      /// <summary>
      /// Attached ToolTip Instance
      /// </summary>
      public ToolTip ErrorToolTip
      {
         get { return _ErrorToolTip; }
         set { _ErrorToolTip = value; }
      }

      private Point _ErrorToolTipPoint = new Point(10, -20);
      /// <summary>
      /// ToolTip Display Point
      /// </summary>
      public Point ErrorToolTipPoint
      {
         get { return _ErrorToolTipPoint; }
         set { _ErrorToolTipPoint = value; }
      }

      private int _ErrorToolTipDuration = 5000;
      /// <summary>
      /// ToolTip Display Duration
      /// </summary>
      public int ErrorToolTipDuration
      {
         get { return _ErrorToolTipDuration; }
         set { _ErrorToolTipDuration = value; }
      }

      private string _ErrorToolTipText = String.Empty;
      /// <summary>
      /// ToolTip Text for Display
      /// </summary>
      public string ErrorToolTipText
      {
         get { return _ErrorToolTipText; }
         set { _ErrorToolTipText = value; }
      }

      private ValidationType _ValidationType = ValidationType.None;
      /// <summary>
      /// Validation Type
      /// </summary>
      public ValidationType ValidationType
      {
         get { return _ValidationType; }
         set { _ValidationType = value; }
      }

      private Color _ErrorBackColor = Color.Yellow;
      /// <summary>
      /// Control Error Color
      /// </summary>
      public Color ErrorBackColor
      {
         get { return _ErrorBackColor; }
         set { _ErrorBackColor = value; }
      }

      private bool _ErrorState = false;
      /// <summary>
      /// Control Error State
      /// </summary>
      public bool ErrorState
      {
         get
         {
            if (_control.Enabled)
            {
               return _ErrorState;
            }
            return false;
         }
         set { _ErrorState = value; }
      }

      private bool ReadOnly
      {
         get
         {
            if (_control is TextBoxBase)
            {
               PropertyInfo readOnly = typeof (TextBoxBase).GetProperty("ReadOnly");
               return (bool)readOnly.GetValue(_control, null);
            }

            return false;
         }
         set
         {
            if (_control is TextBoxBase)
            {
               PropertyInfo readOnly = typeof (TextBoxBase).GetProperty("ReadOnly");
               readOnly.SetValue(_control, value, null);
            }
         }
      }

      private readonly List<Control> _CompanionControls = new List<Control>();
      /// <summary>
      /// Companion Controls List
      /// </summary>
      public List<Control> CompanionControls
      {
         get { return _CompanionControls; }
      }
      #endregion

      #region Members
      private readonly Control _control;
      #endregion

      #region Constructor
      public ValidatingControlLogic(Control control)
      {
         _control = control;
      } 
      #endregion

      #region Event Handlers
      public void DataBindings_CollectionChanged(object sender, CollectionChangeEventArgs e)
      {
         if (_control.Enabled)
         {
            if (_control is TextBoxBase)
            {
               if (ReadOnly == false)
               {
                  ValidateControlText();
               }
               return;
            }

            ValidateControlText();
         }
      }

      public void OnValidating(CancelEventArgs e)
      {
         if (_control.Enabled)
         {
            if (_control is TextBoxBase)
            {
               if (ReadOnly == false)
               {
                  ValidateControlText();
                  ShowToolTip();
               }
               return;
            }

            ValidateControlText();
            ShowToolTip();
         }
      }

      private void ShowToolTip()
      {
         if (ErrorToolTip != null)
         {
            if (ErrorState && ErrorToolTipText.Length != 0)
            {
               ErrorToolTip.RemoveAll();
               ErrorToolTip.Tag = _control.Name;
               ErrorToolTip.Show(ErrorToolTipText, _control, ErrorToolTipPoint, ErrorToolTipDuration);
            }
            else
            {
               ErrorToolTip.RemoveAll();
            }
         }
      }

      public void OnEnabledChanged(EventArgs e)
      {
         ReadOnly = !_control.Enabled;
         _control.CausesValidation = _control.Enabled;

         if (_control.Enabled)
         {
            ValidateControlText();
         }
         else
         {
            if (ErrorToolTip != null && ErrorToolTip.Tag != null)
            {
               if (ErrorToolTip.Tag.Equals(_control.Name))
               {
                  ErrorToolTip.RemoveAll();
               }
            }
         }
      }

      public void OnReadOnlyChanged(EventArgs e)
      {
         if (ReadOnly)
         {
            _control.BackColor = SystemColors.Control;
         }
         else
         {
            _control.BackColor = SystemColors.Window;
         }
      }
      #endregion

      #region Methods
      /// <summary>
      /// Validate the Control Text Property
      /// </summary>
      public void ValidateControlText()
      {
         Color NewColor = SystemColors.Window;
         bool NewError = false;

         if (DoValidation() == false)
         {
            NewColor = ErrorBackColor;
            NewError = true;
         }

         _control.BackColor = NewColor;
         ErrorState = NewError;
         foreach (Control ctrl in CompanionControls)
         {
            ctrl.BackColor = NewColor;
            IValidatingControl validatingControl = ctrl as IValidatingControl;
            if (validatingControl != null)
            {
               validatingControl.ErrorState = NewError;
            }
         }
      }

      private bool DoValidation()
      {
         switch (ValidationType)
         {
            case ValidationType.None:
               return true;
            case ValidationType.Empty:
               if (_control.Text.Length != 0)
               {
                  return true;
               }
               return false;
            case ValidationType.Custom:
               if (CustomValidation == null)
               {
                  // Revert to behavior of ValidationType.Empty
                  if (_control.Text.Length != 0)
                  {
                     return true;
                  }
                  return false;
               }
               ValidatingControlCustomValidationEventArgs e = 
                  new ValidatingControlCustomValidationEventArgs(_control.Text, ErrorToolTipText);
               CustomValidation(this, e);
               _control.Text = e.ControlText;
               ErrorToolTipText = e.ErrorToolTipText;
               return e.ValidationResult;
            default:
               throw new NotImplementedException();
         }
      }
      #endregion
   }

   public class ValidatingControlCustomValidationEventArgs : EventArgs
   {
      #region Properties
      private string _ControlText;
      public string ControlText
      {
         get { return _ControlText; }
         set { _ControlText = value; }
      }

      private string _ErrorToolTipText;
      public string ErrorToolTipText
      {
         get { return _ErrorToolTipText; }
         set { _ErrorToolTipText = value; }
      }

      private bool _ValidationResult;
      public bool ValidationResult
      {
         get { return _ValidationResult; }
         set { _ValidationResult = value; }
      } 
      #endregion

      #region Constructor
      public ValidatingControlCustomValidationEventArgs(string TextValue, string ToolTipTextValue)
      {
         ControlText = TextValue;
         ErrorToolTipText = ToolTipTextValue;
      }
      #endregion
   }
}
