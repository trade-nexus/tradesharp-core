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


using System;

namespace TradeSharp.UI.Common.ApplicationSecurity
{
    /// <summary>
    /// License granted to components.
    /// </summary>
    public class TradeSharpLicense : IDisposable
    {
        #region Fields

        private bool _disposed = false;

        private string _clientDetails = String.Empty;

        private readonly LicenseState _licenseState;
        private LicenseType _licenseType;

        private DateTime _expirationDate;

        #endregion

        /// <summary>
        /// Gets if this license is currently active or not.
        /// </summary>
        public bool IsActive
        {
            get { return _licenseState.Equals(LicenseState.Active); }
        }

        /// <summary>
        /// Gets if this component is running in demo mode.
        /// </summary>
        public bool IsDemo
        {
            get { return _licenseType.Equals(LicenseType.Demo); }
        }

        /// <summary>
        /// Contains client details extracted from license file
        /// </summary>
        public string ClientDetails
        {
            get { return _clientDetails; }
            private set { _clientDetails = value; }
        }

        /// <summary>
        /// Contains license subscription type
        /// </summary>
        public string LicenseSubscriptionType
        {
            get { return _licenseType.ToString(); }
        }

        /// <summary>
        /// License expiration date
        /// </summary>
        public DateTime ExpirationDate
        {
            get { return _expirationDate.Date; }
        }

        /// <summary>
        /// Creates a new <see cref="TradeSharpLicense"/> object.
        /// </summary>
        private TradeSharpLicense()
        {
            _licenseState = VerifyLicense() ? LicenseState.Active : LicenseState.Expired;
        }

        /// <summary>
        /// Creates a new <see cref="TradeSharpLicense"/> object.
        /// </summary>
        private TradeSharpLicense(LicenseType licenseType, LicenseState licenseState)
        {
            _licenseType = licenseType;
            _licenseState = licenseState;
        }

        /// <summary>
        /// Attempts to create a new <see cref="TradeSharpLicense"/> with the specified key.
        /// </summary>
        /// <returns><see cref="TradeSharpLicense"/> with the specified fields set.</returns>
        internal static TradeSharpLicense CreateLicense()
        {
            return new TradeSharpLicense();
        }

        /// <summary>
        /// Attempts to create a new <see cref="TradeSharpLicense"/> with the specified key.
        /// </summary>
        /// <returns><see cref="TradeSharpLicense"/> with the specified fields set.</returns>
        internal static TradeSharpLicense CreateInvalidLicense()
        {
            return CreateLicenseTemplate.InvalidLicense();
        }

        /// <summary>
        /// Verifies license details
        /// </summary>
        /// <returns></returns>
        private bool VerifyLicense()
        {
            LicenseKeyAnalyzer licenseKeyAnalyzer = new LicenseKeyAnalyzer();

            // Extract License information
            var licenseInformation = licenseKeyAnalyzer.GetLicenseInformation();

            // Save details
            _licenseType = licenseInformation.Item1;
            _clientDetails = licenseInformation.Item2;
            _expirationDate = licenseInformation.Item3;

            // Find if license expiration date has reached
            if (licenseInformation.Item3 < DateTime.Now)
            {
                return false;
            }

            return true;
        }

        #region Dispose

        /// <summary>
        /// Disposes this object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Disposes this object.
        /// </summary>
        /// <param name="disposing">true if the object is disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!_disposed)
                {
                    //Custom disposing here.
                }
                _disposed = true;
            }
        }

        #endregion

        /// <summary>
        /// Provides dummy license
        /// </summary>
        private static class CreateLicenseTemplate
        {
            internal static TradeSharpLicense InvalidLicense()
            {
                return new TradeSharpLicense(LicenseType.Invalid, LicenseState.Expired);
            }
        }
    }
}
