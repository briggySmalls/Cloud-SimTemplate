﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using TemplateBuilder.Helpers;
using TemplateBuilder.Model;

namespace TemplateBuilder.ViewModel.MainWindow
{
    public partial class TemplateBuilderViewModel
    {
        abstract public class Templating : Initialised
        {
            #region Constructor

            public Templating(TemplateBuilderViewModel outer, StateManager stateMgr) : base(outer, stateMgr)
            { }

            #endregion

            public override void OnEnteringState()
            {
                base.OnEnteringState();

                // Ensure UI controls active
                if (!Outer.IsSaveTemplatePermitted)
                {
                    Outer.IsSaveTemplatePermitted = true;
                }
                if (!Outer.IsInputMinutiaTypePermitted)
                {
                    Outer.IsInputMinutiaTypePermitted = true;
                }
            }

            public override void SkipFile()
            {
                StateMgr.TransitionTo(typeof(Idle));
            }
        }
    }
}
