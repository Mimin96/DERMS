//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FTN {
    using System;
    using FTN;
    
    
    /// Connectivity nodes are points where terminals of conducting equipment are connected together with zero impedance.
    public class ConnectivityNode : IdentifiedObject {
        
        /// Container of this connectivity node.
        private ConnectivityNodeContainer cim_ConnectivityNodeContainer;
        
        private const bool isConnectivityNodeContainerMandatory = true;
        
        private const string _ConnectivityNodeContainerPrefix = "cim";
        
        public virtual ConnectivityNodeContainer ConnectivityNodeContainer {
            get {
                return this.cim_ConnectivityNodeContainer;
            }
            set {
                this.cim_ConnectivityNodeContainer = value;
            }
        }
        
        public virtual bool ConnectivityNodeContainerHasValue {
            get {
                return this.cim_ConnectivityNodeContainer != null;
            }
        }
        
        public static bool IsConnectivityNodeContainerMandatory {
            get {
                return isConnectivityNodeContainerMandatory;
            }
        }
        
        public static string ConnectivityNodeContainerPrefix {
            get {
                return _ConnectivityNodeContainerPrefix;
            }
        }
    }
}