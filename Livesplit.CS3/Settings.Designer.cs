﻿using System.ComponentModel;

namespace Livesplit.CS3
{
    partial class Settings
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.skipButtonBox = new System.Windows.Forms.CheckBox();
            this.battleIDSplitsCollection = new System.Windows.Forms.CheckedListBox();
            this.SuspendLayout();
            // 
            // skipButtonBox
            // 
            this.skipButtonBox.Location = new System.Drawing.Point(15, 21);
            this.skipButtonBox.Name = "skipButtonBox";
            this.skipButtonBox.Size = new System.Drawing.Size(204, 21);
            this.skipButtonBox.TabIndex = 0;
            this.skipButtonBox.Text = "Automatically skip battle animations";
            this.skipButtonBox.UseVisualStyleBackColor = true;
            // 
            // battleIDSplitsCollection
            // 
            this.battleIDSplitsCollection.FormattingEnabled = true;
            this.battleIDSplitsCollection.Items.AddRange(new object[] {"Split a", "Split b ", "Split c", "Split d"});
            this.battleIDSplitsCollection.Location = new System.Drawing.Point(15, 101);
            this.battleIDSplitsCollection.Name = "battleIDSplitsCollection";
            this.battleIDSplitsCollection.Size = new System.Drawing.Size(355, 319);
            this.battleIDSplitsCollection.TabIndex = 1;
            this.battleIDSplitsCollection.SelectedIndexChanged += new System.EventHandler(this.battleIDSplitsCollection_SelectedIndexChanged);
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.battleIDSplitsCollection);
            this.Controls.Add(this.skipButtonBox);
            this.Name = "Settings";
            this.Size = new System.Drawing.Size(399, 576);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.CheckedListBox battleIDSplitsCollection;
        private System.Windows.Forms.CheckBox skipButtonBox;

        #endregion
    }
}