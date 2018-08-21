﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Inventor;

namespace FieldExporter.Components
{
    public partial class JointPropertiesForm : UserControl
    {
        private BXDVector3 _center = new BXDVector3(0, 0, 0);
        /// <summary>
        /// Center point of the joint's rotational motion
        /// </summary>
        public BXDVector3 Center
        {
            get => _center;
            set
            {
                if (InvokeRequired)
                {
                    Invoke((Action<BXDVector3>)((BXDVector3 v) => Center = v), value);
                    return;
                }

                _center = value;
                centerLabel.Text = "Center: [" + value.x.ToString("N1") + ',' + value.y.ToString("N1") + ',' + value.z.ToString("N1") + ']';
            }
        }

        private BXDVector3 _axis = new BXDVector3(1, 0, 0);
        /// <summary>
        /// Axis about which the joint rotates
        /// </summary>
        public BXDVector3 Axis
        {
            get => _axis;
            set
            {
                if (InvokeRequired)
                {
                    Invoke((Action<BXDVector3>)((BXDVector3 v) => Axis = v), value);
                    return;
                }

                _axis = value;
                axisLabel.Text = "Axis: [" + value.x.ToString("N1") + ',' + value.y.ToString("N1") + ',' + value.z.ToString("N1") + ']';
            }
        }

        public JointPropertiesForm()
        {
            InitializeComponent();
        }
        
        /// <summary>
        /// True if the joint checkbox is selected.
        /// </summary>
        public bool IsJointed
        {
            get => jointCheckBox.Checked;
            set => jointCheckBox.Checked = value;
        }

        /// <summary>
        /// Returns the configured joint settings, or null if not jointed.
        /// </summary>
        /// <returns></returns>
        public PropertySet.FieldJoint Joint
        {
            get
            {
                if (jointCheckBox.Checked)
                    return new PropertySet.FieldJoint(Center, Axis);
                else
                    return null;
            }
            set
            {
                if (value == null)
                    jointCheckBox.Checked = false;
                else
                {
                    jointCheckBox.Checked = true;
                    Center = value.Center;
                    Axis = value.Axis;
                }
            }
        }

        InteractionEvents interactionEvents;
        SelectEvents selectEvents;

        private void interactionEvents_OnActivate()
        {
            selectEvents = interactionEvents.SelectEvents;
            selectEvents.OnSelect += selectEvents_OnSelect;

            if (selectingCenter)
                selectEvents.AddSelectionFilter(SelectionFilterEnum.kSketchPointFilter); // Select sketch points for center point
            else
                selectEvents.AddSelectionFilter(SelectionFilterEnum.kSketchCurveLinearFilter); // Select sketch lines for axis
        }

        bool selectingCenter = false;
        bool selectingAxis = false;

        private void selectEvents_OnSelect(ObjectsEnumerator justSelectedEntities, SelectionDeviceEnum selectionDevice, Inventor.Point modelPosition, Point2d viewPosition, Inventor.View view)
        {
            foreach (dynamic selectedEntity in justSelectedEntities)
            {
                if (selectingCenter && selectedEntity is SketchPoint point)
                {
                    Center = new BXDVector3(point.Geometry3d.X, point.Geometry3d.Y, point.Geometry3d.Z);
                    DisableInteractionEvents();
                    break;
                }
                else if (selectingAxis && selectedEntity is SketchLine axis)
                {
                    Axis = new BXDVector3(axis.Geometry3d.Direction.X, axis.Geometry3d.Direction.Y, axis.Geometry3d.Direction.Z);
                    DisableInteractionEvents();
                    break;
                }
            }
        }

        /// <summary>
        /// Enables interaction events with Inventor.
        /// </summary>
        public void EnableInteractionEvents()
        {
            if (Program.INVENTOR_APPLICATION.ActiveDocument == Program.ASSEMBLY_DOCUMENT)
            {
                try
                {
                    interactionEvents = Program.INVENTOR_APPLICATION.CommandManager.CreateInteractionEvents();
                    interactionEvents.OnActivate += interactionEvents_OnActivate;
                    interactionEvents.Start();

                    // Block other select button from being pressed.
                    if (selectingCenter)
                    {
                        selectCenterButton.Text = "Cancel";
                        selectAxisButton.Enabled = false;
                    }
                    else
                    {
                        selectAxisButton.Text = "Cancel";
                        selectCenterButton.Enabled = false;
                    }
                }
                catch
                {
                    MessageBox.Show("Cannot enter select mode.", "Document not found.");
                }
            }
            else
            {
                MessageBox.Show("Can only enter select mode for " + Program.ASSEMBLY_DOCUMENT.DisplayName);
            }
        }

        /// <summary>
        /// Disables interaction events with Inventor.
        /// </summary>
        public void DisableInteractionEvents()
        {
            interactionEvents.Stop();
            selectingCenter = false;
            selectingAxis = false;

            if (InvokeRequired)
            {
                Invoke((Action)(() =>
                {
                    selectCenterButton.Text = "Select";
                    selectAxisButton.Text = "Select";
                    selectCenterButton.Enabled = true;
                    selectAxisButton.Enabled = true;
                }));
            }
            else
            {
                selectCenterButton.Text = "Select";
                selectAxisButton.Text = "Select";
                selectCenterButton.Enabled = true;
                selectAxisButton.Enabled = true;
            }
        }

        private void selectCenterButton_Click(object sender, EventArgs e)
        {
            if (!selectingAxis && !selectingCenter)
            {
                selectingCenter = true;
                EnableInteractionEvents();
            }
            else
            {
                DisableInteractionEvents();
            }
        }

        private void selectAxisButton_Click(object sender, EventArgs e)
        {
            if (!selectingAxis && !selectingCenter)
            {
                selectingAxis = true;
                EnableInteractionEvents();
            }
            else
            {
                DisableInteractionEvents();
            }
        }

        private void jointCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (jointCheckBox.Checked)
            {
                centerLabel.Enabled = true;
                selectCenterButton.Enabled = true;
                axisLabel.Enabled = true;
                selectAxisButton.Enabled = true;
            }
            else
            {
                if (selectingCenter || selectingAxis)
                    DisableInteractionEvents();

                // Reset selected info when joint is disabled.
                Center = new BXDVector3(0, 0, 0);
                Axis = new BXDVector3(1, 0, 0);

                centerLabel.Enabled = false;
                selectCenterButton.Enabled = false;
                axisLabel.Enabled = false;
                selectAxisButton.Enabled = false;
            }
        }
    }
}
