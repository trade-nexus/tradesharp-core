/***************************************************************************** 
* Copyright 2016 Aurora Solutions 
* 
*    http://www.aurorasolutions.io 
* 
* Aurora Solutions is an innovative services and product company at 
* the forefront of the software industry, with processes and practices 
* involving Domain Driven Design(DDD), Agile methodologies to build 
* scalable, secure, reliable and high performance products.
* 
* TradeSharp is a C# based data feed and broker neutral Algorithmic 
* Trading Platform that lets trading firms or individuals automate 
* any rules based trading strategies in stocks, forex and ETFs. 
* TradeSharp allows users to connect to providers like Tradier Brokerage, 
* IQFeed, FXCM, Blackwood, Forexware, Integral, HotSpot, Currenex, 
* Interactive Brokers and more. 
* Key features: Place and Manage Orders, Risk Management, 
* Generate Customized Reports etc 
* 
* Licensed under the Apache License, Version 2.0 (the "License"); 
* you may not use this file except in compliance with the License. 
* You may obtain a copy of the License at 
* 
*    http://www.apache.org/licenses/LICENSE-2.0 
* 
* Unless required by applicable law or agreed to in writing, software 
* distributed under the License is distributed on an "AS IS" BASIS, 
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
* See the License for the specific language governing permissions and 
* limitations under the License. 
*****************************************************************************/


﻿using System;
using System.Windows;
using System.Windows.Input;
using TraceSourceLogger;

namespace TradeHub.DataDownloader.UserInterface.Shell.Shell
{
    /// <summary>
    /// Interaction logic for ConfirmationMessageWindow.xaml
    /// </summary>
    public partial class ConfirmationMessageWindow : Window
    {
        private Type _oType = typeof(ConfirmationMessageWindow);

        private Point _origCursorLocation;
        private bool _isMaximized = false;
        private WindowState _windowState;

        private bool _selection = false;

        /// <summary>
        /// Sets Message Box Content
        /// </summary>
        public string MessageBoxContent
        {
            set
            {
                MessageBoxLabel.Content = value;
            }
        }

        /// <summary>
        /// Gets/Set Message Box Selection
        /// </summary>
        public bool Selection
        {
            get { return _selection; }
            set { _selection = value; }
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ConfirmationMessageWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Called when "NO" button is Clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NoClicked(object sender, RoutedEventArgs e)
        {
            _selection = false;
            this.Hide();
        }

        /// <summary>
        /// Called when "YES" button is Clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void YesClicked(object sender, RoutedEventArgs e)
        {
            _selection = true;
            this.Hide();
        }

        /// <summary>
        /// Close Window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Minimize Window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Maximize Window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MaximizeWindow(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_isMaximized)
                {
                    _windowState = this.WindowState;
                    this.WindowState = WindowState.Maximized;
                    _isMaximized = true;
                }
                else
                {
                    this.WindowState = _windowState;
                    _isMaximized = false;
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "MaximizeWindow");
            }
        }

        /// <summary>
        /// Called When Application is Closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {

            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "OnFormClosing");
            }
        }

        /// <summary>
        /// Mouse Down event for Canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanvasMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                _origCursorLocation = e.GetPosition(this);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "CanvasMouseDown");
            }
        }

        /// <summary>
        /// Mouse Up event for Canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanvasMouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {

            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "CanvasMouseUp");
            }
        }

        /// <summary>
        /// Mouse Move event for Canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanvasMouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                var currentPoint = e.GetPosition(this);
                if (e.LeftButton == MouseButtonState.Pressed &&
                    (Math.Abs(currentPoint.X - _origCursorLocation.X) >
                        SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(currentPoint.Y - _origCursorLocation.Y) >
                        SystemParameters.MinimumVerticalDragDistance))
                {
                    // Prevent Click from firing
                    this.ReleaseMouseCapture();
                    DragMove();
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "CanvasMouseMove");
            }
        }
    }
}
