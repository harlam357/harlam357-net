/*
 * Self Validating TextBox Control
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
using System.Drawing.Design;
using System.Windows.Forms;

namespace harlam357.Windows.Forms
{
   public partial class ValidatingTextBox : TextBox, IValidatingControl
   {
      #region Events
      public event EventHandler<ValidatingControlCustomValidationEventArgs> CustomValidation;
      #endregion
      
      #region Properties
      [Browsable(true)]
      [EditorBrowsable(EditorBrowsableState.Always)]
      [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
      public new bool DoubleBuffered
      {
         get { return base.DoubleBuffered; }
         set { base.DoubleBuffered = value; }
      }
      
      [Browsable(true)]
      [EditorBrowsable(EditorBrowsableState.Always)]
      [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
      public ToolTip ErrorToolTip
      {
         get { return _logic.ErrorToolTip; }
         set { _logic.ErrorToolTip = value; }
      }
      
      [Browsable(true)]
      [EditorBrowsable(EditorBrowsableState.Always)]
      [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
      public Point ErrorToolTipPoint
      {
         get { return _logic.ErrorToolTipPoint; }
         set { _logic.ErrorToolTipPoint = value; }
      }

      [Browsable(true)]
      [EditorBrowsable(EditorBrowsableState.Always)]
      [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
      public int ErrorToolTipDuration
      {
         get { return _logic.ErrorToolTipDuration; }
         set { _logic.ErrorToolTipDuration = value; }
      }

      [Browsable(true)]
      [EditorBrowsable(EditorBrowsableState.Always)]
      [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design", typeof(UITypeEditor)), Localizable(true)]
      [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
      public string ErrorToolTipText
      {
         get { return _logic.ErrorToolTipText; }
         set { _logic.ErrorToolTipText = value; }
      }

      [Browsable(true)]
      [EditorBrowsable(EditorBrowsableState.Always)]
      [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
      public ValidationType ValidationType
      {
         get { return _logic.ValidationType; }
         set { _logic.ValidationType = value; }
      }

      [Browsable(true)]
      [EditorBrowsable(EditorBrowsableState.Always)]
      [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
      public Color ErrorBackColor
      {
         get { return _logic.ErrorBackColor; }
         set { _logic.ErrorBackColor = value; }
      }

      [Browsable(false)]
      public bool ErrorState
      {
         get { return _logic.ErrorState; }
         set { _logic.ErrorState = value; }
      }
      
      [Browsable(false)]
      public List<Control> CompanionControls
      {
         get { return _logic.CompanionControls; }
      }
      #endregion

      #region Members
      private readonly ValidatingControlLogic _logic;
      #endregion

      #region Constructor
      public ValidatingTextBox()
      {
         InitializeComponent();

         DoubleBuffered = true;
         _logic = new ValidatingControlLogic(this);
         _logic.CustomValidation += delegate(object sender, ValidatingControlCustomValidationEventArgs e)
         {
            if (CustomValidation != null)
            {
               CustomValidation(sender, e);
            }
         };
         DataBindings.CollectionChanged += _logic.DataBindings_CollectionChanged;
      }
      #endregion

      #region Event Handlers
      protected override void OnValidating(CancelEventArgs e)
      {
         _logic.OnValidating(e);
         base.OnValidating(e);
      }

      protected override void OnEnabledChanged(EventArgs e)
      {
         _logic.OnEnabledChanged(e);
         base.OnEnabledChanged(e);
      }

      protected override void OnReadOnlyChanged(EventArgs e)
      {
         _logic.OnReadOnlyChanged(e);
         base.OnReadOnlyChanged(e);
      }
      #endregion

      #region Methods
      public void ValidateControlText()
      {
         _logic.ValidateControlText();
      }

      public void ShowToolTip()
      {
         _logic.ShowToolTip();
      }
      #endregion
   }
}
