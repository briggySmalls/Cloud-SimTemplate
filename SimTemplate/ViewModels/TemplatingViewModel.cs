﻿using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using SimTemplate.Utilities;
using SimTemplate.ViewModels;
using SimTemplate.StateMachine;
using SimTemplate.ViewModels.Commands;
using SimTemplate.Model.DataControllers.EventArguments;
using SimTemplate.DataTypes;
using SimTemplate.Model.DataControllers;
using SimTemplate.DataTypes.Enums;
using SimTemplate.DataTypes.Collections;

namespace SimTemplate.ViewModels
{
    public partial class TemplatingViewModel : ViewModel, ITemplatingViewModel
    {
        #region Constants

        private const int LINE_LENGTH = 20;

        #endregion

        private static readonly ILog m_Log = LogManager.GetLogger(typeof(TemplatingViewModel));

        private StateManager<TemplatingState> m_StateMgr;
        private object m_StateLock;

        // Parent-driven properties
        private CaptureInfo m_Capture;
        private MinutiaType m_InputMinutiaType;

        // ViewModel-driven properties

        // View and ViewModel-driven properties
        private int? m_SelectedMinutia;

        private event EventHandler<UserActionRequiredEventArgs> m_UserActionRequired;

        #region Constructor

        public TemplatingViewModel()
        {
            m_StateLock = new object();
            Minutae = new TrulyObservableCollection<MinutiaRecord>();
            m_StateMgr = new StateManager<TemplatingState>(this, typeof(Uninitialised));
        }

        #endregion

        #region Directly Bound Properties

        /// <summary>
        /// Gets or sets the minutae. Bound to the canvas.
        /// </summary>
        /// <value>
        /// The minutae.
        /// </value>
        public TrulyObservableCollection<MinutiaRecord> Minutae { get; set; }

        /// <summary>
        /// Gets or sets the capture info currently being templated.
        /// </summary>
        /// <value>
        /// The capture.
        /// </value>
        public CaptureInfo Capture
        {
            get { return m_Capture; }
            set
            {
                if (value != m_Capture)
                {
                    m_Capture = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the type of minutia being input.
        /// </summary>
        /// <value>
        /// The type of the input minutia.
        /// </value>
        public MinutiaType InputMinutiaType
        {
            get { return m_InputMinutiaType; }
            set
            {
                if (value != m_InputMinutiaType)
                {
                    lock (m_StateLock)
                    {
                        m_InputMinutiaType = value;
                        m_StateMgr.State.SetMinutiaType(m_InputMinutiaType);
                    }
                    NotifyPropertyChanged();
                }
            }
        }

        #endregion

        #region ITemplatingViewModel

        void ITemplatingViewModel.BeginInitialise()
        {
            lock (m_StateLock)
            {
                m_StateMgr.State.BeginInitialise();
            }
        }

        bool ITemplatingViewModel.IsSaveTemplatePermitted
        {
            get
            {
                return (m_StateMgr.State.GetType() == typeof(WaitLocation)) && (Minutae.Count > 0);
            }
        }

        void ITemplatingViewModel.EscapeAction()
        {
            lock (m_StateLock)
            {
                m_StateMgr.State.EscapeAction();
            }
        }

        void ITemplatingViewModel.BeginTemplating(CaptureInfo capture)
        {
            lock (m_StateLock)
            {
                m_StateMgr.State.BeginTemplating(capture);
            }
        }

        byte[] ITemplatingViewModel.FinaliseTemplate()
        {
            lock (m_StateLock)
            {
                return m_StateMgr.State.FinaliseTemplate();
            }
        }

        void ITemplatingViewModel.QuitTemplating()
        {
            Capture = null;
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                Minutae.Clear();
            }));
        }

        public event EventHandler<UserActionRequiredEventArgs> UserActionRequired
        {
            add { m_UserActionRequired += value; }
            remove { m_UserActionRequired -= value; }
        }

        #endregion

        #region Command Handlers

        /// <summary>
        /// Takes a position on the image as a user input for templating.
        /// </summary>
        /// <param name="position">The position, in pixels, on the full scale image.</param>
        public void PositionInput(Point position)
        {
            m_Log.DebugFormat(
                "PositionInput(position.X={0}, position.Y={1}) called.",
                position.X,
                position.Y);
            lock (m_StateLock)
            {
                m_StateMgr.State.PositionInput(position);
            }
        }

        /// <summary>
        /// Updates a position on the image.
        /// </summary>
        /// <param name="position">The position, in pixels, on the full scale image.</param>
        public void PositionUpdate(Point position)
        {
            // Do not log as this occurs many times per second.
            lock (m_StateLock)
            {
                m_StateMgr.State.PositionUpdate(position);
            }
        }

        /// <summary>
        /// Updates the position of a recorded minutia.
        /// </summary>
        /// <param name="position">The position, in pixels, on the full scale image.</param>
        public void MoveMinutia(Point position)
        {
            m_Log.DebugFormat(
                "MoveMinutia(position={1}) called.",
                position);
            lock (m_StateLock)
            {
                m_StateMgr.State.MoveMinutia(position);
            }
        }

        public void RemoveMinutia(int index)
        {
            m_Log.DebugFormat(
                "Ellipse_MouseRightButtonUp(index={0}) called.",
                index);
            lock (m_StateLock)
            {
                m_StateMgr.State.RemoveMinutia(index);
            }
        }

        /// <summary>
        /// Indicates a minutia is to be moved
        /// </summary>
        /// <param name="index">The index of the minutia.</param>
        public void StartMove(int index)
        {
            m_Log.DebugFormat("StartMove(index={0}) called.", index);
            lock (m_StateLock)
            {
                m_StateMgr.State.StartMove(index);
            }
        }

        #endregion
        
        private void OnUserActionRequired(UserActionRequiredEventArgs e)
        {
            EventHandler<UserActionRequiredEventArgs> temp = m_UserActionRequired;
            if (temp != null)
            {
                temp.Invoke(this, e);
            }
        }

    }
}